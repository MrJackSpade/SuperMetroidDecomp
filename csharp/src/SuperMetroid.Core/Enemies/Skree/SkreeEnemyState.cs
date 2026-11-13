using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The state of a Skree.</summary>
public sealed class SkreeEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<SkreeMetareeProgram> Playback { get; set; }

    /// <summary>Reloaded to 21 while diving and counted down while burrowing.</summary>
    public int BurrowTimer { get; internal set; }

    /// <summary>The current phase function.</summary>
    public SkreeEnemyFunction? Function { get; internal set; }

    /// <summary>The animation phase requested by the current AI transition.</summary>
    public SkreeMetareeAnimationPhase RequestedInstructionIndex { get; internal set; }

    /// <summary>The animation phase whose program is currently installed.</summary>
    public SkreeMetareeAnimationPhase InstalledInstructionIndex { get; internal set; }

    /// <summary>The wind-up cue's one-shot authorization to begin diving.</summary>
    public bool AttackReady { get; internal set; }
}
