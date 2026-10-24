using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Where each animated-tile object's frames are copied (<see cref="AnimatedTileVramDestinationDefinitions"/>).</summary>
internal static class AnimatedTileVramDestinations
{
    /// <summary>The VRAM word <paramref name="animatedObject"/>'s object header copies every frame to.</summary>
    /// <exception cref="InvalidOperationException">The object copies no frames the port pictures.</exception>
    internal static ushort WordOf(AnimatedTileObject animatedObject) =>
        AnimatedTileVramDestinationDefinitions.Destinations.TryGetValue(animatedObject, out ushort word) ? word
        : Enum.IsDefined(animatedObject)
            ? throw new InvalidOperationException($"Animated-tile object {animatedObject} copies no frames the port pictures.")
            : throw new InvalidOperationException($"Undefined {nameof(AnimatedTileObject)} {animatedObject}.");
}
