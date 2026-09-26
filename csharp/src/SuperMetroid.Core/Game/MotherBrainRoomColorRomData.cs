namespace SuperMetroid.Core.Game;

/// <summary>Native sources and CGRAM destinations for Mother Brain's fake-death room colors.</summary>
public static class MotherBrainRoomColorRomData
{
    /// <summary>The bank containing the $D046 palette program and its RGB5 payloads.</summary>
    public const int SourceBank = 0xa90000;

    /// <summary>Each timed entry selects two twelve-color source slices.</summary>
    public const int SliceColors = 12;

    /// <summary>The first source slice starts at CGRAM byte offset $68.</summary>
    public const int FirstColor = 0x0068 / sizeof(ushort);

    /// <summary>The second source slice is copied to CGRAM byte offsets $A6 and $E6.</summary>
    public const int SecondColor = 0x00a6 / sizeof(ushort);
    public const int MirroredSecondColor = 0x00e6 / sizeof(ushort);

    /// <summary>Phase-two setup copies fifteen nontransparent attack colors from $A9:94B4.</summary>
    public const int PhaseTwoAttackSource = 0xa994b4;
    /// <summary>The attack colors begin at CGRAM byte offset $0142.</summary>
    public const int PhaseTwoAttackColor = 0x0142 / sizeof(ushort);

    /// <summary>Phase-two setup copies fifteen rear-leg colors from $A9:9494.</summary>
    public const int PhaseTwoRearLegSource = 0xa99494;
    /// <summary>The rear-leg colors begin at CGRAM byte offset $0162.</summary>
    public const int PhaseTwoRearLegColor = 0x0162 / sizeof(ushort);
    /// <summary>Both phase-two setup palettes have fifteen nontransparent colors.</summary>
    public const int PhaseTwoColors = 15;

    /// <summary>Mother Brain body initialization copies glass-shard colors from $A9:9514.</summary>
    public const int InitialGlassShardSource = 0xa99514;
    /// <summary>Glass-shard colors initially occupy CGRAM byte offset $0162.</summary>
    public const int InitialGlassShardColor = 0x0162 / sizeof(ushort);

    /// <summary>Mother Brain body initialization copies tube-projectile colors from $A9:94F4.</summary>
    public const int InitialTubeProjectileSource = 0xa994f4;
    /// <summary>Tube-projectile colors initially occupy CGRAM byte offset $01E2.</summary>
    public const int InitialTubeProjectileColor = 0x01e2 / sizeof(ushort);

    /// <summary>Both room-entry palettes omit transparent color zero and copy fifteen colors.</summary>
    public const int InitialColors = 15;

    /// <summary>Seven phase-three room-light images, selected after the Baby cutscene.</summary>
    public const int RecoveryLightsFrames = 7;
    /// <summary>Each $AD:F3D3-minus-index image is two fourteen-color source slices.</summary>
    public const int RecoveryLightsColorsPerDestination = 14;
    /// <summary>The room-light source rows descend by $38 bytes in bank $AD.</summary>
    public const int RecoveryLightsFirstSource = 0xadf3d3;
    public const int RecoveryLightsByteStride = 0x38;
    /// <summary>First room-light slice begins at CGRAM byte offset $0062.</summary>
    public const int RecoveryLightsFirstColor = 0x0062 / sizeof(ushort);
    /// <summary>Second room-light slice begins at CGRAM byte offset $00A2.</summary>
    public const int RecoveryLightsSecondColor = 0x00a2 / sizeof(ushort);

    public static int RecoveryLightsSource(int index) =>
        (uint)index < RecoveryLightsFrames
            ? RecoveryLightsFirstSource - index * RecoveryLightsByteStride
            : throw new ArgumentOutOfRangeException(nameof(index));
}
