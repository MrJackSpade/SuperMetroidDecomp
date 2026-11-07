namespace SuperMetroid.Core.Fx;

/// <summary>Conveyor BTS entries among the normal special-air entries of the inside-block dispatch.</summary>
internal enum ConveyorBlockBts
{
    /// <summary>A powered/grounded rightward conveyor.</summary>
    GroundedRight = 8,
    /// <summary>A powered/grounded leftward conveyor.</summary>
    GroundedLeft = 9,
    /// <summary>An unconditional rightward conveyor.</summary>
    UnconditionalRight = 10,
    /// <summary>An unconditional leftward conveyor.</summary>
    UnconditionalLeft = 11,
}
