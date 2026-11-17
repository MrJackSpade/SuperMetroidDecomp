using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The animations room effects, treadmills, spikes and sand show over a room's blocks (#1406): each is an animation whose
/// frames are the pictures it shows, which asset extraction pictures and the animation selects at the update the game
/// showed them.
/// </summary>
internal static class BlockAnimationFrames
{
    /// <summary>The animation <paramref name="animation"/> shows.</summary>
    internal static ArtworkAnimation Of(BlockAnimation animation) => new($"{animation} block animation");

    /// <summary>The frame of <paramref name="animation"/> named <paramref name="name"/>.</summary>
    internal static ArtworkFrame Frame(BlockAnimation animation, string name) => new(Of(animation), Ensure.NotNull(name));
}
