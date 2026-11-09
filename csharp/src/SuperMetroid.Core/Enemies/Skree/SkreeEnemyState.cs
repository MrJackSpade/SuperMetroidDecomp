using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The state of a Skree.</summary>
public sealed class SkreeEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<SkreeMetareeProgram> Program { get; set; }

    /// <summary>Reloaded to 21 while diving and counted down while burrowing.</summary>
    public int BurrowTimer { get; internal set; }

    /// <summary>The current phase function.</summary>
    public SkreeEnemyFunction? Function { get; internal set; }

    /// <summary>The animation phase requested by the current AI transition.</summary>
    public SkreeMetareeAnimationPhase RequestedInstructionIndex { get; internal set; }

    /// <summary>The animation phase whose program is currently installed.</summary>
    public SkreeMetareeAnimationPhase InstalledInstructionIndex { get; internal set; }

    /// <summary>The wind-up instruction's one-shot authorization to begin diving.</summary>
    public bool AttackReady { get; internal set; }
}
