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
    /// Whether the face looks right, as its spawn setting's facing says. Activation needs Samus on the side opposite the one
    /// this names, so a face looking right wakes for Samus to its left.
    /// </summary>
    public bool FacesRight { get; internal set; }
}
