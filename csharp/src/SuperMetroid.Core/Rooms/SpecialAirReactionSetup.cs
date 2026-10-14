namespace SuperMetroid.Core.Rooms;

/// <summary>Bank-$84 setup callbacks stored by the special-air reaction PLM headers.</summary>
public enum SpecialAirReactionSetup : ushort
{
    /// <summary><c>$84:B3CF RTS</c>: no reaction.</summary>
    Nothing = 0xb3cf,
    /// <summary><c>$84:B3D0 Setup_ClearCarry</c>.</summary>
    ClearCarry = 0xb3d0,
    /// <summary><c>$84:B0DC Setup_BrinstarFloorPlant</c>, selected by Brinstar inside BTS $80.</summary>
    BrinstarFloorPlant = 0xb0dc,
    /// <summary><c>$84:B113 Setup_BrinstarCeilingPlant</c>, selected by Brinstar inside BTS $81.</summary>
    BrinstarCeilingPlant = 0xb113,
    /// <summary><c>$84:B3EB Setup_IcePhysics</c>.</summary>
    IcePhysics = 0xb3eb,
    /// <summary>$84:B408, PlmSetup_QuicksandSurface; bottom, center, and top body sampling.</summary>
    QuicksandSurface = 0xb408,
    /// <summary>$84:B497, PlmSetup_B71F_SubmergingQuicksand.</summary>
    SubmergingQuicksand = 0xb497,
    /// <summary>$84:B4A8, PlmSetup_B723_SandfallsSlow.</summary>
    SandFallsSlow = 0xb4a8,
    /// <summary>$84:B4B6, PlmSetup_B727_SandFallsFast.</summary>
    SandFallsFast = 0xb4b6,
    /// <summary>$84:B4C4, PlmSetup_QuicksandSurfaceB; carry and quicksand contact publication.</summary>
    QuicksandSurfaceCollision = 0xb4c4,
    /// <summary>$84:B541, submerging sand collision clears vertical speed and gravity.</summary>
    SubmergingQuicksandCollision = 0xb541,
    /// <summary>$84:B54F, sandfall collision setup that returns clear carry.</summary>
    SandFallsCollision = 0xb54f,
    /// <summary>$84:CDEA, Speed Booster block collision setup.</summary>
    SpeedBlock = 0xcdea,
    /// <summary>$84:D18F, Lower Norfair Chozo-hand collision check.</summary>
    LowerNorfairChozoHand = 0xd18f,
    /// <summary>$84:D620, Wrecked Ship Chozo-hand collision check.</summary>
    WreckedShipChozoHand = 0xd620,
}
