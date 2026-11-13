using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escape Dachora's state. Dachora owns no private values: all of its
/// movement is authored directly in its three animations, so its state is the
/// animation it is running.
/// </summary>
public sealed class EscapeDachoraEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<EscapeDachoraProgram> Playback { get; set; }
}
