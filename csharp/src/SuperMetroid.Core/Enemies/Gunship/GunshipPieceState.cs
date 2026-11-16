using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The state each of the gunship's three pieces keeps of its own (#2063): the animation it plays and where it stands in
/// it (#1600). The ship's own state is the top hull's <see cref="GunshipState"/>.
/// </summary>
/// <param name="playback">The animation the piece starts with.</param>
internal sealed class GunshipPieceState(AnimationPlayback<GunshipSegment> playback)
{
    /// <summary>The animation the piece plays and where it stands in it.</summary>
    internal AnimationPlayback<GunshipSegment> Playback { get; set; } = playback;
}
