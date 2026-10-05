using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Abilities;

namespace PanoramaUiKit.Channels;

/// <summary>
/// Ability bar on one layout (contract "abilities"), per viewer.
///
/// <see cref="Update"/> is meant to be called every tick: text and classes go through the viewer's
/// <see cref="ClientState"/>, so a steady bar costs nothing and a cooldown costs one text write per
/// second. Both gauges are client-side clip transitions started once, never driven per tick; what
/// is tracked here is only what decides when to start and stop them.
/// </summary>
internal sealed class AbilityChannel : LayoutChannel<AbilityChannel.Viewer>
{
    private const string BarId = "ab_bar";

    /// <summary>Measured minimum for an off-then-on class restart to reach the client.</summary>
    private const float RestartSeconds = 0.35f;

    /// <summary>Gauge state of one tile: when each gauge last reset, and what is running now.</summary>
    internal sealed class Tile
    {
        public int Cooldown;
        public bool Holding;
        public float BarResetAt = float.MinValue;
        public float HoldResetAt = float.MinValue;
        public SafeTimer? BarRun;
        public SafeTimer? HoldRun;

        public void KillTimers()
        {
            BarRun?.Kill();
            HoldRun?.Kill();
            BarRun = null;
            HoldRun = null;
        }
    }

    internal sealed class Viewer : ViewerState
    {
        public Viewer(ClientState client, int tiles) : base(client)
        {
            Tiles = Enumerable.Range(0, tiles).Select(_ => new Tile()).ToArray();
        }

        public Tile[] Tiles { get; }

        public override void Release()
        {
            foreach (Tile tile in Tiles)
            {
                tile.KillTimers();
            }

            base.Release();
        }
    }

    private readonly AbilityLayout _layout;

    public AbilityChannel(BasePlugin plugin, AbilityLayout layout) : base(plugin, layout)
    {
        _layout = layout;
    }

    protected override Viewer CreateViewer(int slot) => new(NewClient(), _layout.Slots);

    /// <summary>Shows the bar with these slots. An empty list hides it.</summary>
    public void Update(CCSPlayerController player, AbilityBarOptions options)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return;
        }

        if (options.Slots.Count == 0)
        {
            ClearAll(player);
            return;
        }

        Viewer viewer = Ensure(player, out _);
        ClientState client = viewer.Client;

        client.Group(player, BarId, "style", Names.StyleOrNeutral(options.Style));
        client.FreeClasses(player, BarId, options.Classes);
        client.ExtraTexts(player, "ab", options.Texts);

        for (int i = 0; i < viewer.Tiles.Length; i++)
        {
            string id = $"ab_{i}";
            bool used = i < options.Slots.Count;
            if (used)
            {
                AbilitySlot state = options.Slots[i];
                client.Text(player, $"{id}_key", state.Key);
                client.Text(player, $"{id}_name", state.Name);
                client.Class(player, id, "blocked", state.Blocked);
                PaintCooldown(player, viewer, viewer.Tiles[i], id, state.CooldownRemaining);
                PaintHold(player, viewer, viewer.Tiles[i], id, state);
            }

            client.Class(player, id, "hidden", !used);
        }
    }

    private void PaintCooldown(CCSPlayerController player, Viewer viewer, Tile tile, string id, int remaining)
    {
        if (tile.Cooldown == remaining)
        {
            return;
        }

        bool started = tile.Cooldown <= 0 && remaining > 0;
        bool ended = tile.Cooldown > 0 && remaining <= 0;
        tile.Cooldown = remaining;
        ClientState client = viewer.Client;

        if (remaining > 0)
        {
            client.Text(player, $"{id}_cd", remaining.ToString());
        }

        if (started)
        {
            client.Class(player, id, "cooldown", true);

            // Fill over what is left, so the gauge always lands exactly when the ability is back.
            int seconds = Math.Clamp(remaining, 1, _layout.MaxCooldownSeconds);
            client.Group(player, $"{id}_bar", "cd", $"cd-{seconds}");
            tile.BarRun = StartAfterReset(tile.BarResetAt, () =>
            {
                tile.BarRun = null;
                if (IsCurrent(player, viewer) && tile.Cooldown > 0)
                {
                    client.Class(player, $"{id}_bar", "run", true);
                }
            });
        }
        else if (ended)
        {
            tile.BarRun?.Kill();
            tile.BarRun = null;
            client.Class(player, id, "cooldown", false);

            // Duration and run dropped together: the gauge snaps back instead of draining.
            client.Group(player, $"{id}_bar", "cd", null);
            client.Class(player, $"{id}_bar", "run", false);
            tile.BarResetAt = Server.CurrentTime;
        }
    }

    private void PaintHold(CCSPlayerController player, Viewer viewer, Tile tile, string id, AbilitySlot state)
    {
        bool holding = state.Holding && state.HoldSeconds > 0f;
        if (tile.Holding == holding)
        {
            return;
        }

        tile.Holding = holding;
        ClientState client = viewer.Client;

        if (holding)
        {
            client.Class(player, id, "holding", true);

            int step = Math.Max(1, _layout.HoldStepTenths);
            int tenths = (int)Math.Round(state.HoldSeconds * 10f / step) * step;
            client.Group(player, $"{id}_hold", "hold", $"hold-{Math.Clamp(tenths, step, _layout.MaxHoldTenths)}");
            tile.HoldRun = StartAfterReset(tile.HoldResetAt, () =>
            {
                tile.HoldRun = null;
                if (IsCurrent(player, viewer) && tile.Holding)
                {
                    client.Class(player, $"{id}_hold", "run", true);
                }
            });
            return;
        }

        tile.HoldRun?.Kill();
        tile.HoldRun = null;
        client.Class(player, id, "holding", false);
        client.Group(player, $"{id}_hold", "hold", null);
        client.Class(player, $"{id}_hold", "run", false);
        tile.HoldResetAt = Server.CurrentTime;
    }

    /// <summary>
    /// Runs now, or once the last reset of the same gauge has had time to reach the client - an
    /// off-then-on that arrives too close together is never seen and the gauge would not move.
    /// </summary>
    private SafeTimer? StartAfterReset(float resetAt, Action start)
    {
        float wait = resetAt + RestartSeconds - Server.CurrentTime;
        if (wait <= 0f)
        {
            start();
            return null;
        }

        return Later(wait, start);
    }
}
