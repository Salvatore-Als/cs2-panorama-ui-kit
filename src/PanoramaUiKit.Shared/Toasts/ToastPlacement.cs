using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Toasts;

[JsonConverter(typeof(JsonStringEnumConverter<ToastPlacement>))]
public enum ToastPlacement
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight,
}
