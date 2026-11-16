using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>The state of the Blue Brinstar face block.</summary>
public sealed class BlueBrinstarFaceBlockEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<BlueBrinstarFaceBlockSegment> Playback { get; set; }

    /// <summary>
    /// Once set, the face has selected its directional animation and the main AI never performs the proximity test
    /// again.
    /// </summary>
    public bool Activated { get; internal set; }

    /// <summary>
    /// The way the face looks, as its spawn setting's facing says (#2277): it wakes only for Samus on the side it looks
    /// toward.
    /// </summary>
    public HorizontalDirection Facing { get; internal set; }
}
