using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Menus;

/// <summary>Contract "menu": a list of <see cref="Items"/> entries per page, ids <c>mn_*</c>. Takes the mouse.</summary>
public sealed class MenuLayout : UiLayout
{
    /// <summary>Exit default matches the bundled stylesheet's transition, plus a margin.</summary>
    public MenuLayout()
    {
        ExitSeconds = 0.2f;
    }

    public override string Component => "menu";

    /// <summary>Entries <c>mn_item_0</c>..<c>mn_item_{Items-1}</c>: the page size.</summary>
    public int Items { get; init; } = 8;

    /// <summary>Choices a select's dropdown can list: <c>mn_choice_&lt;n&gt;_0</c>..<c>_{Choices-1}</c> per entry. More are ignored.</summary>
    public int Choices { get; init; } = 6;
}
