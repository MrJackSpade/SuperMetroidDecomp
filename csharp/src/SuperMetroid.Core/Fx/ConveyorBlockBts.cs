namespace SuperMetroid.Core.Fx;

/// <summary>Conveyor BTS entries among the normal special-air entries of bank-$94's inside-block dispatcher.</summary>
internal enum ConveyorBlockBts
{
    /// <summary>$94:98EA, BTS $08: powered/grounded rightward conveyor.</summary>
    GroundedRight = 8,
    /// <summary>$94:9910, BTS $09: powered/grounded leftward conveyor.</summary>
    GroundedLeft = 9,
    /// <summary>$94:9936, BTS $0A: unconditional rightward conveyor.</summary>
    UnconditionalRight = 10,
    /// <summary>$94:9946, BTS $0B: unconditional leftward conveyor.</summary>
    UnconditionalLeft = 11,
}
