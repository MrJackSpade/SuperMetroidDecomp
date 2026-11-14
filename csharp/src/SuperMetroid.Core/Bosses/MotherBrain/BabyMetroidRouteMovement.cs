namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The steering behaviours the Baby Metroid route's steps name.
/// Its values are arbitrary; verification's catalog relates each member to its source (#1413).
/// </summary>
internal enum BabyMetroidRouteMovement
{
    /// <summary>Gradual acceleration with wrong-way extra eight.</summary>
    GradualAccelerationExtraEight = 1,

    /// <summary>Gradual acceleration with wrong-way extra sixteen.</summary>
    GradualAccelerationExtraSixteen,
}
