namespace SuperMetroid.Core.Fx;

/// <summary>Conveyor BTS entries among the normal special-air entries of the inside-block dispatch.</summary>
internal enum ConveyorBlockBts
{
    /// <summary>BTS $08: powered/grounded rightward conveyor.</summary>
    GroundedRight = 8,
    /// <summary>BTS $09: powered/grounded leftward conveyor.</summary>
    GroundedLeft = 9,
    /// <summary>BTS $0A: unconditional rightward conveyor.</summary>
    UnconditionalRight = 10,
    /// <summary>BTS $0B: unconditional leftward conveyor.</summary>
    UnconditionalLeft = 11,
}
