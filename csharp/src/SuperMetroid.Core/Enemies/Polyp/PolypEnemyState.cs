using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>The state of the Polyp.</summary>
public sealed class PolypEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<PolypProgram> Program { get; set; }

    /// <summary>The current Polyp behaviour.</summary>
    public PolypEnemyFunction? Function { get; internal set; }

    /// <summary>The launch cooldown, decremented through -1 before the Polyp rearms.</summary>
    public int CooldownTimer { get; internal set; }
}
