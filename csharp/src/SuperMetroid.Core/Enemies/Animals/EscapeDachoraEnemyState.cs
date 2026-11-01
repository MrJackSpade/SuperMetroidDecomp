using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// Debugger-visible typed marker for the escape Dachora. Dachora owns no
/// private variables: all of its movement is authored directly in its three
/// animations. These read-only properties expose that live script state
/// without inventing a parallel host state machine.
/// </summary>
public sealed class EscapeDachoraEnemyState
{
    /// <summary>The actor's typed program cursor.</summary>
    internal AnimationProgramCursor<EscapeDachoraProgram> Program { get; set; }
}
