using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Toasts;

namespace PanoramaUiKit.Channels;

/// <summary>
/// Toasts on one toast layout (contract "toast").
///
/// Slots, not a queue: the layout declares a fixed pool per placement. An unused card carries
/// "hidden" so it collapses out of the stack, and the cards below close the gap on their own.
///
/// Timing rules (runtime-behaviour.md):
/// - "show" goes in a later network update than "hidden" off, or the card snaps in.
/// - The bar's "run" goes at least <see cref="BarRestartSeconds"/> after the bar was last reset,
///   or the client may never see the reset and the bar does not drain again.
/// - dur-* and run are dropped together, never in the same update as a new dur-*: with a duration
///   still applied, dropping "run" refills the bar backwards over that duration.
/// </summary>
internal sealed class ToastChannel : LayoutChannel<ToastChannel.Viewer>
{
    /// <summary>Measured minimum for an off-then-on class restart to reach the client.</summary>
    private const float BarRestartSeconds = 0.35f;

    internal enum SlotState
    {
        Free,
        Active,
        Leaving,
    }

    /// <summary>One card of the pool. Its timers are its own: cards come and go independently.</summary>
    internal sealed class Slot
    {
        public required string Id;
        public SlotState State;
        public UiHandle? Handle;
        public ToastAnimation Exit;
        public float ShownAt;
        public float LeftAt;
        public float BarResetAt = float.MinValue;

        public SafeTimer? Entry;
        public SafeTimer? Run;
        public SafeTimer? Expiry;
        public SafeTimer? Release;

        public void KillTimers()
        {
            Entry?.Kill();
            Run?.Kill();
            Expiry?.Kill();
            Release?.Kill();
            Entry = null;
            Run = null;
            Expiry = null;
            Release = null;
        }
    }

    internal sealed class Viewer : ViewerState
    {
        public Viewer(ClientState client, Dictionary<ToastPlacement, Slot[]> stacks) : base(client)
        {
            Stacks = stacks;
        }

        public Dictionary<ToastPlacement, Slot[]> Stacks { get; }

        public bool Empty => Stacks.Values.All(stack => stack.All(s => s.State == SlotState.Free));

        public override void Release()
        {
            foreach (Slot slot in Stacks.Values.SelectMany(stack => stack))
            {
                slot.Handle?.MarkGone();
                slot.KillTimers();
            }

            base.Release();
        }
    }

    private readonly ToastLayout _layout;

    public ToastChannel(BasePlugin plugin, ToastLayout layout) : base(plugin, layout)
    {
        _layout = layout;
    }

    protected override Viewer CreateViewer(int slot)
    {
        Dictionary<ToastPlacement, Slot[]> stacks = _layout.Placements.ToDictionary(
            placement => placement,
            placement => Enumerable.Range(0, _layout.Depth)
                .Select(i => new Slot { Id = ToastNames.CardId(placement, i) })
                .ToArray());

        return new Viewer(NewClient(), stacks);
    }

    public IUiHandle Show(CCSPlayerController player, ToastOptions options)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        Viewer viewer = Ensure(player, out _);
        ToastPlacement placement = _layout.Placements[0];
        if (_layout.Placements.Contains(options.Placement))
        {
            placement = options.Placement;
        }

        Slot slot = PickSlot(player, viewer, viewer.Stacks[placement], out float delay);

        // Toasts shown in the same tick are queued, one every WriteGapSeconds.
        delay = Math.Max(delay, viewer.Reserve(Server.CurrentTime, WriteGapSeconds));

        UiHandle handle = new UiHandle(player);
        handle.OnClear = () => Retire(player, viewer, slot, handle);

        slot.State = SlotState.Active;
        slot.Handle = handle;
        slot.Exit = options.Exit ?? options.Enter ?? placement.DefaultAnimation();
        slot.ShownAt = Server.CurrentTime;

        if (delay <= 0f)
        {
            Begin(player, viewer, slot, handle, placement, options);
        }
        else
        {
            slot.Entry = Later(delay, () => Begin(player, viewer, slot, handle, placement, options));
        }

        return handle;
    }

    /// <summary>
    /// A free card if there is one. Otherwise one already leaving, reused once its exit has played.
    /// Otherwise the oldest card is pushed out - dropping the newest would hide an important message.
    /// </summary>
    private Slot PickSlot(CCSPlayerController player, Viewer viewer, Slot[] stack, out float delay)
    {
        delay = 0f;
        Slot? free = stack.FirstOrDefault(s => s.State == SlotState.Free);
        if (free != null)
        {
            return free;
        }

        Slot? leaving = stack.Where(s => s.State == SlotState.Leaving).MinBy(s => s.LeftAt);
        if (leaving != null)
        {
            leaving.Release?.Kill();
            leaving.Release = null;
            delay = Math.Max(0f, leaving.LeftAt + _layout.ExitSeconds - Server.CurrentTime);
            return leaving;
        }

        Slot oldest = stack.MinBy(s => s.ShownAt)!;
        StartExit(player, viewer, oldest);
        delay = _layout.ExitSeconds;
        return oldest;
    }

    private void Begin(CCSPlayerController player, Viewer viewer, Slot slot, UiHandle handle,
                       ToastPlacement placement, ToastOptions options)
    {
        slot.Entry = null;
        if (!IsCurrent(player, viewer) || slot.Handle != handle)
        {
            return;
        }

        bool progress = options.Progress && options.Seconds > 0f;
        Paint(player, viewer, slot, placement, options, progress);

        slot.Entry = Later(_layout.EntrySeconds, () =>
        {
            slot.Entry = null;
            if (!IsCurrent(player, viewer) || slot.Handle != handle)
            {
                return;
            }

            viewer.Client.Class(player, slot.Id, "show", true);
            if (progress)
            {
                StartBar(player, viewer, slot, handle);
            }
        });

        if (options.Seconds > 0f)
        {
            slot.Expiry = Later(options.Seconds, () => Retire(player, viewer, slot, handle));
        }
    }

    private void StartBar(CCSPlayerController player, Viewer viewer, Slot slot, UiHandle handle)
    {
        float wait = Math.Max(0f, slot.BarResetAt + BarRestartSeconds - Server.CurrentTime);
        if (wait <= 0f)
        {
            viewer.Client.Class(player, $"{slot.Id}_bar", "run", true);
            return;
        }

        slot.Run = Later(wait, () =>
        {
            slot.Run = null;
            if (IsCurrent(player, viewer) && slot.Handle == handle)
            {
                viewer.Client.Class(player, $"{slot.Id}_bar", "run", true);
            }
        });
    }

    private void Paint(CCSPlayerController player, Viewer viewer, Slot slot, ToastPlacement placement,
                       ToastOptions options, bool progress)
    {
        ClientState client = viewer.Client;
        string id = slot.Id;

        client.OptionalText(player, $"{id}_kicker", options.Kicker);
        client.Text(player, $"{id}_title", options.Title.Trim());
        client.OptionalText(player, $"{id}_desc", options.Description);
        client.ExtraTexts(player, id, options.Texts);

        client.Group(player, id, "style", Names.StyleOrNeutral(options.Style));
        client.Group(player, id, "anim", (options.Enter ?? placement.DefaultAnimation()).Class());
        client.FreeClasses(player, id, options.Classes);

        // The bar was reset when this card was last released or evicted, so adding the new
        // duration here cannot drag a running bar backwards.
        client.Class(player, $"{id}_track", "has-progress", progress);
        string? duration = null;
        if (progress)
        {
            duration = $"dur-{NearestDuration(options.Seconds)}";
        }

        client.Group(player, $"{id}_bar", "dur", duration);

        client.Class(player, id, "hidden", false);
    }

    /// <summary>Starts the exit animation, then frees the card once it has played.</summary>
    private void Retire(CCSPlayerController player, Viewer viewer, Slot slot, UiHandle handle)
    {
        if (!IsCurrent(player, viewer) || slot.Handle != handle || slot.State != SlotState.Active)
        {
            return;
        }

        StartExit(player, viewer, slot);
        slot.Release = Later(_layout.ExitSeconds, () => Release(player, viewer, slot));
    }

    private void StartExit(CCSPlayerController player, Viewer viewer, Slot slot)
    {
        slot.Handle?.MarkGone();
        slot.KillTimers();
        slot.State = SlotState.Leaving;
        slot.LeftAt = Server.CurrentTime;

        if (player is not { IsValid: true })
        {
            return;
        }

        // Direction first, so it can leave differently than it arrived.
        ClientState client = viewer.Client;
        client.Group(player, slot.Id, "anim", slot.Exit.Class());
        client.Class(player, slot.Id, "show", false);

        // dur-* and run together: the bar snaps back to full while the card fades.
        client.Group(player, $"{slot.Id}_bar", "dur", null);
        client.Class(player, $"{slot.Id}_bar", "run", false);
        slot.BarResetAt = Server.CurrentTime;
    }

    private void Release(CCSPlayerController player, Viewer viewer, Slot slot)
    {
        slot.Release = null;
        if (slot.State != SlotState.Leaving)
        {
            return;
        }

        slot.State = SlotState.Free;
        slot.Handle = null;
        if (!IsCurrent(player, viewer))
        {
            return;
        }

        viewer.Client.Class(player, slot.Id, "hidden", true);

        // Nothing left: close so the layout stops being drawn for this viewer.
        if (viewer.Empty)
        {
            ClearAll(player);
        }
    }

    private int NearestDuration(float seconds) => _layout.Durations.MinBy(d => Math.Abs(d - seconds));
}
