namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// Whether a Yapping Maw's tongue is curling out or back (#2450), which decides the way each curl update adds its speed.
/// Its values are arbitrary.
/// </summary>
internal enum YappingMawCurlPhase
{
    /// <summary>Curling out toward its turn-around: each update adds the mark's speed.</summary>
    Extending,

    /// <summary>Curling back after the turn-around: each update takes the mark's speed away.</summary>
    Retracting,
}
