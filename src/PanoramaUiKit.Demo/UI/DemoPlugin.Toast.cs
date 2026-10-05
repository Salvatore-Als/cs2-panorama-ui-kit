using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Toasts;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    /// <summary>
    /// Toasts on the bundled uikit_toast layout. No argument: one toast, top right. <c>bottom</c>: one
    /// toast with every field, bottom centre. <c>multi</c>: several at once, in every style.
    /// <c>neon</c>: the same call on another layout file (neon_toast), with its own look and a footer line.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_toast", "One toast. Arguments: bottom (every field, bottom centre) | multi (several, every style) | neon (custom layout).")]
    [CommandHelper(usage: "[bottom|multi|neon]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoToast(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        string mode = command.GetArg(1).ToLowerInvariant();
        if (mode == "bottom")
        {
            ShowFullToast(kit, target);
            return;
        }

        if (mode == "multi")
        {
            ShowToastBurst(kit, target);
            return;
        }

        if (mode == "neon")
        {
            ShowNeonToasts(kit, target);
            return;
        }

        // The one-liner.
        kit.Toast(target, DemoLayouts.Toast, "Toast title", "One-liner: title, description, style.", UiStyle.Danger);
    }

    /// <summary>
    /// The same API on a layout of our own: only <c>Layout</c> changes. A top-centre toast and a
    /// bottom-left one, each with the free <c>footer</c> text slot this layout adds.
    /// </summary>
    private static void ShowNeonToasts(IPanoramaUiKit kit, CCSPlayerController target)
    {
        kit.Toasts.Show(target, new ToastOptions
        {
            Layout = DemoLayouts.NeonToast,
            Kicker = "Custom layout",
            Title = "Neon toast",
            Description = "Same call, another file: neon_toast.xml.",
            Style = UiStyle.Info,
            Seconds = 6f,
            Placement = ToastPlacement.TopCenter,
            Texts = new Dictionary<string, string> { ["footer"] = "Free text slot: footer" },
        });

        kit.Toasts.Show(target, new ToastOptions
        {
            Layout = DemoLayouts.NeonToast,
            Title = "Bottom left",
            Description = "A placement the bundled layout also has, drawn differently.",
            Style = UiStyle.Danger,
            Seconds = 6f,
            Placement = ToastPlacement.BottomLeft,
        });
    }

    /// <summary>The full record: kicker, duration, placement.</summary>
    private static void ShowFullToast(IPanoramaUiKit kit, CCSPlayerController target)
    {
        kit.Toasts.Show(target, new ToastOptions
        {
            Layout = DemoLayouts.Toast,
            Kicker = "Kicker",
            Title = "Toast with every field",
            Description = "Kicker, description, 6 seconds, bottom centre.",
            Style = UiStyle.Success,
            Seconds = 6f,
            Placement = ToastPlacement.BottomCenter,
        });
    }

    /// <summary>Five toasts: two at once, then one every 0.4 s, one style each.</summary>
    private void ShowToastBurst(IPanoramaUiKit kit, CCSPlayerController target)
    {
        kit.Toast(target, DemoLayouts.Toast, "Toast title", "One-liner: title, description, style.", UiStyle.Danger);
        ShowFullToast(kit, target);

        string[] styles = [UiStyle.Info, UiStyle.Warn, UiStyle.Neutral];
        for (int i = 0; i < styles.Length; i++)
        {
            string style = styles[i];
            Later(0.4f * (i + 1), () =>
            {
                if (!target.IsValid)
                {
                    return;
                }

                kit.Toasts.Show(target, new ToastOptions
                {
                    Layout = DemoLayouts.Toast,
                    Kicker = style,
                    Title = $"{style} toast",
                    Description = "Stacked under the previous ones.",
                    Style = style,
                });
            });
        }
    }
}
