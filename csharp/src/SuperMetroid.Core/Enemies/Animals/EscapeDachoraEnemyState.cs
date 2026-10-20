using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// Debugger-visible typed marker for escape Dachora enemy <c>$F313</c>. Dachora owns no
/// private variables: all of its movement is authored directly in the three ROM instruction
/// lists at <c>$B3:E964-$EAA7</c>. These read-only properties expose that live script state
/// without inventing a parallel host state machine.
/// </summary>
public sealed class EscapeDachoraEnemyState
{
    /// <summary>The actor's typed program cursor, in place of its native instruction-list pointer.</summary>
    internal AnimationProgramCursor<EscapeDachoraProgram> Program { get; set; }
}
