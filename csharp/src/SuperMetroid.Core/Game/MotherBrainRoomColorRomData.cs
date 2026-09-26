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
}
