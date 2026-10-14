namespace SuperMetroid.Core.Game;

/// <summary>Low two bits consumed by <c>$A0:A8F0</c>'s direction jump table.</summary>
public enum SamusCollisionDirection : ushort
{
    /// <summary>Native jump-table value 0 at $A0:A911: probes toward decreasing room X using the current left edge.</summary>
    Left = 0,
    /// <summary>Native jump-table value 1 at $A0:A913: probes toward increasing room X using the current right edge.</summary>
    Right = 1,
    /// <summary>Native jump-table value 2 at $A0:A915: probes toward decreasing room Y using the current top edge.</summary>
    Up = 2,
    /// <summary>Native jump-table value 3 at $A0:A917: probes toward increasing room Y using the current bottom edge.</summary>
    Down = 3,
    /// <summary>Bank-$94:96E3 pose-observation direction $F; not an actual downward contact.</summary>
    NonDirectionalProbe = 0x0f,
}
