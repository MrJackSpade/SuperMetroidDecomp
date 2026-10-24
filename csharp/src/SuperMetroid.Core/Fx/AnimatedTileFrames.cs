using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The animations room effects, treadmills, spikes, sand and the Tourian statues paint their characters with (#1406):
/// each animated-tile object is an animation whose frames are the pictures it shows, which asset extraction pictures and
/// the object selects at the update the cartridge copied them.
/// </summary>
internal static class AnimatedTileFrames
{
    /// <summary>The animation <paramref name="animatedObject"/> paints its characters with.</summary>
    internal static ArtworkAnimation Of(AnimatedTileObject animatedObject) => new($"{animatedObject} tiles");

    /// <summary>Frame <paramref name="frame"/> of <paramref name="animatedObject"/>'s timed loop.</summary>
    internal static ArtworkFrame Frame(AnimatedTileObject animatedObject, int frame) =>
        new(Of(animatedObject), $"frame {Ensure.AtLeastZero(frame)}");

    /// <summary>The frame of <paramref name="statue"/>'s animation that shows <paramref name="picture"/>.</summary>
    internal static ArtworkFrame Picture(AnimatedTileObject statue, TourianStatueTilePicture picture) => new(Of(statue), $"{picture}");
}
