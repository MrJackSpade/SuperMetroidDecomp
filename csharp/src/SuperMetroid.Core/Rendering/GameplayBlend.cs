using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How a gameplay blend changes the screen (#1407, #1514): what it changes, whether it brightens or darkens it, by how
/// much, and with what. The blended pixel combines with what lies behind it, when its layer names the background or the
/// effect overlay as lying behind, or with <see cref="Colour"/>: water darkens what lies behind it by its colour, Fireflea's
/// darkness subtracts its shade from the background and the backdrop, and the X-ray halves the room outside its cone
/// toward the room's liquid.
/// </summary>
public readonly record struct GameplayBlend
{
    /// <summary>What the blend changes where it is frontmost.</summary>
    public required BlendedLayers Blended { get; init; }

    /// <summary>Whether the blend adds what it combines with, brightening, or subtracts it, darkening.</summary>
    public required BlendOperation Operation { get; init; }

    /// <summary>
    /// Whether the combined colour is halved, so that the blend shows half of the sum or the difference, where it combines
    /// with a layer lying behind or, when no layer lies behind, with the colour.
    /// </summary>
    public required bool Halved { get; init; }

    /// <summary>
    /// The colour the blend combines with when no layer lies behind, and, unhalved, wherever the layers lying behind show
    /// nothing.
    /// </summary>
    public required RgbColor Colour { get; init; }
}
