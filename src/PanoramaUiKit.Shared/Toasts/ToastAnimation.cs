using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Toasts;

/// <summary>Named for the side the toast rests on while hidden, so one name serves enter and exit.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ToastAnimation>))]
public enum ToastAnimation
{
    FromLeft,
    FromRight,
    FromTop,
    FromBottom,
    Fade,
}