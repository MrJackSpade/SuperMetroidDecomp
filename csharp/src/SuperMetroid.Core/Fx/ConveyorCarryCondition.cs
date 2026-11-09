namespace SuperMetroid.Core.Fx;

/// <summary>When a conveyor block carries Samus.</summary>
internal enum ConveyorCarryCondition
{
    /// <summary>Whenever she is in it.</summary>
    Always,

    /// <summary>
    /// Only while she is not moving vertically and, in the Wrecked Ship, only once its area boss is defeated and the ship
    /// has power.
    /// </summary>
    OnFootWithPower,
}
