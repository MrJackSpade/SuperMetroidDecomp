using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escape Dachora's state. Dachora owns no private variables: all of its
/// movement is authored directly in its three animations, so its state is the
/// animation it is running.
/// </summary>
public sealed class EscapeDachoraEnemyState
{
    /// <summary>The actor's typed program cursor.</summary>
    internal AnimationProgramCursor<EscapeDachoraProgram> Program { get; set; }
}
