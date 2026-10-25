namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where the cartridge writes Mother Brain's room colour images in its colour memory: verification's native view of the colour sets.</summary>
internal static class MotherBrainRoomColorDestinations
{
    /// <summary>The first source slice starts at CGRAM byte offset $68.</summary>
    public const int FirstColor = 0x0068 / sizeof(ushort);
    /// <summary>The second source slice is copied to CGRAM byte offsets $A6 and $E6.</summary>
    public const int SecondColor = 0x00a6 / sizeof(ushort);
    /// <summary>$A9:D03B's mirrored destination at CGRAM word index 115 (byte $E6), BG palette 7 color 3; receives the same twelve colors as palette 5.</summary>
    public const int MirroredSecondColor = 0x00e6 / sizeof(ushort);
    /// <summary>The attack colors begin at CGRAM byte offset $0142.</summary>
    public const int PhaseTwoAttackColor = 0x0142 / sizeof(ushort);
    /// <summary>The rear-leg colors begin at CGRAM byte offset $0162.</summary>
    public const int PhaseTwoRearLegColor = 0x0162 / sizeof(ushort);
    /// <summary>Glass-shard colors initially occupy CGRAM byte offset $0162.</summary>
    public const int InitialGlassShardColor = 0x0162 / sizeof(ushort);
    /// <summary>Tube-projectile colors initially occupy CGRAM byte offset $01E2.</summary>
    public const int InitialTubeProjectileColor = 0x01e2 / sizeof(ushort);
    /// <summary>First room-light slice begins at CGRAM byte offset $0062.</summary>
    public const int RecoveryLightsFirstColor = 0x0062 / sizeof(ushort);
    /// <summary>Second room-light slice begins at CGRAM byte offset $00A2.</summary>
    public const int RecoveryLightsSecondColor = 0x00a2 / sizeof(ushort);
}
