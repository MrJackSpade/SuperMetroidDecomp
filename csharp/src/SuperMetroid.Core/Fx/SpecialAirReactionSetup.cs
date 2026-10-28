namespace SuperMetroid.Core.Fx;

/// <summary>What a special-air block does to Samus when she is inside or touches it.
/// Its values are arbitrary; verification's native view names each member's cartridge word (#1415).
/// </summary>
public enum SpecialAirReactionSetup
{
    /// <summary><c>$84:B3CF RTS</c>: no reaction.</summary>
    Nothing = 1,
    /// <summary><c>$84:B3D0 Setup_ClearCarry</c>.</summary>
    ClearCarry,
    /// <summary><c>$84:B0DC Setup_BrinstarFloorPlant</c>, selected by Brinstar inside BTS $80.</summary>
    BrinstarFloorPlant,
    /// <summary><c>$84:B113 Setup_BrinstarCeilingPlant</c>, selected by Brinstar inside BTS $81.</summary>
    BrinstarCeilingPlant,
    /// <summary><c>$84:B3EB Setup_IcePhysics</c>.</summary>
    IcePhysics,
    /// <summary>$84:B408, PlmSetup_QuicksandSurface; bottom, center, and top body sampling.</summary>
    QuicksandSurface,
    /// <summary>$84:B497, PlmSetup_B71F_SubmergingQuicksand.</summary>
    SubmergingQuicksand,
    /// <summary>$84:B4A8, PlmSetup_B723_SandfallsSlow.</summary>
    SandFallsSlow,
    /// <summary>$84:B4B6, PlmSetup_B727_SandFallsFast.</summary>
    SandFallsFast,
    /// <summary>$84:B4C4, PlmSetup_QuicksandSurfaceB; carry and quicksand contact publication.</summary>
    QuicksandSurfaceCollision,
    /// <summary>$84:B541, submerging sand collision clears vertical speed and gravity.</summary>
    SubmergingQuicksandCollision,
    /// <summary>$84:B54F, sandfall collision setup that returns clear carry.</summary>
    SandFallsCollision,
    /// <summary>$84:CDEA, Speed Booster block collision setup.</summary>
    SpeedBlock,
    /// <summary>$84:D18F, Lower Norfair Chozo-hand collision check.</summary>
    LowerNorfairChozoHand,
    /// <summary>$84:D620, Wrecked Ship Chozo-hand collision check.</summary>
    WreckedShipChozoHand,
}
