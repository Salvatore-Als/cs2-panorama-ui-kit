using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Targets;

/// <summary>Text size, applied as <c>size-sm</c>, <c>size-md</c>, <c>size-lg</c> or <c>size-xl</c>.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TargetSize>))]
public enum TargetSize
{
    Small,
    Medium,
    Large,
    ExtraLarge,
}
