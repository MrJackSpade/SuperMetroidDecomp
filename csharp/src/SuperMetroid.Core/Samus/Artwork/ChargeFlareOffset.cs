using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Signed whole-pixel visual muzzle displacement from Samus's render center, applied before camera subtraction and the renderer's separate pose-graphics Y adjustment.</summary>
[SpatialComponents("External format: the placement JSON stores each muzzle displacement as signed x/y words.")]
public sealed record ChargeFlareOffset
{
    /// <summary>The displacement as one spatial value.</summary>
    [JsonIgnore]
    public Offset16<RoomPixels> Offset => new(X, Y);

    /// <summary>Horizontal displacement in signed pixels; positive values move the visual origin right.</summary>
    public required short X { get; init; }
    /// <summary>Vertical displacement in signed pixels; positive values move the visual origin down, before the separate pose-graphics Y offset is subtracted.</summary>
    public required short Y { get; init; }
}
