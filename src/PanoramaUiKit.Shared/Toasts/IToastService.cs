using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Toasts;

public interface IToastService
{
    IUiHandle Show(CCSPlayerController player, ToastOptions options);

    /// <summary>Same toast to every connected human.</summary>
    void ShowAll(ToastOptions options);

    /// <summary>Removes this player's toasts. Null layout = on every toast layout.</summary>
    void Clear(CCSPlayerController player, ToastLayout? layout = null);
}