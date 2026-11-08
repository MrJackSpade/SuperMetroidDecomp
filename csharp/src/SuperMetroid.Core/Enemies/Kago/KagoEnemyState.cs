using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>The state of the Kago.</summary>
public sealed class KagoEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<KagoProgram> Program { get; set; }

    /// <summary>The current behaviour, installed at initialization and replaced on the first AI update.</summary>
    public KagoEnemyFunction? Function { get; internal set; }

    /// <summary>Whether the shell runs its three-frame loop rather than its ten-frame one.</summary>
    public bool UsesFastAnimation { get; internal set; }

    /// <summary>
    /// Extra enemy word four. The game decrements first and dies only when the signed result
    /// is negative, so population value ten permits eleven accepted shots.
    /// </summary>
    public int HitCounter { get; internal set; }

    /// <summary>Set after Kago requests death animation variant four.</summary>
    public bool DeathAnimationStarted { get; internal set; }

    /// <summary>Number of Kago bugs successfully allocated from this shell.</summary>
    public int SpawnedBugCount { get; internal set; }
}
