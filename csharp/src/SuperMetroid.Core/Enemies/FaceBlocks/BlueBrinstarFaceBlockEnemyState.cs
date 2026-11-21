using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>The state of the Blue Brinstar face block.</summary>
public sealed class BlueBrinstarFaceBlockEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<BlueBrinstarFaceBlockSegment> Playback { get; set; }

    /// <summary>
    /// Whether the face has woken (#2476): it plays the turn toward Samus its wake started in place of its neutral look,
    /// which it never shows again, and main AI never performs the proximity test again.
    /// </summary>
    public bool Activated { get; internal set; }
}
