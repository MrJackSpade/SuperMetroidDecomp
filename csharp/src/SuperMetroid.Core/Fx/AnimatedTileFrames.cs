using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The animations room effects, treadmills, spikes and sand show over a room's blocks (#1406): each is an animation whose
/// frames are the pictures it shows, which asset extraction pictures and the animation selects at the update the game
/// showed them.
/// </summary>
internal static class AnimatedTileFrames
{
    /// <summary>The animation <paramref name="animatedObject"/> shows.</summary>
    internal static ArtworkAnimation Of(AnimatedTileObject animatedObject) => new($"{animatedObject} tiles");

    /// <summary>Frame <paramref name="frame"/> of <paramref name="animatedObject"/>'s timed loop.</summary>
    internal static ArtworkFrame Frame(AnimatedTileObject animatedObject, int frame) =>
        new(Of(animatedObject), $"frame {Ensure.AtLeastZero(frame)}");
}
