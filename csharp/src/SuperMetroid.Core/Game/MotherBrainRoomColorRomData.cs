namespace SuperMetroid.Core.Game;

/// <summary>Native sources and CGRAM destinations for Mother Brain's fake-death room colors.</summary>
public static class MotherBrainRoomColorRomData
{
    /// <summary>The bank containing the $D046 palette program and its RGB5 payloads.</summary>
    public const int SourceBank = 0xa90000;

    /// <summary>Each $A9:D046 timed flash entry contains a duration word and a palette-pointer word.</summary>
    public const int TimedEntryByteCount = 2 * sizeof(ushort);
    /// <summary>The imported palette-pointer operand follows the timed entry's duration word.</summary>
    public const int PaletteOperandByteOffset = sizeof(ushort);

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

    /// <summary>$A9:94B4-B8, Palette_MotherBrain_Attacks cyan ramp, three nontransparent slots.</summary>
    public const int AttackCyanFirst = 0, AttackCyanCount = 3;
    /// <summary>$A9:94BA-C0, Palette_MotherBrain_Attacks yellow-to-red ramp, four slots.</summary>
    public const int AttackFlameFirst = 3, AttackFlameCount = 4;
    /// <summary>$A9:94C2-C6 repeats three body tissue shades from $A9:9486-948A.</summary>
    public const int AttackTissueFirst = 7, AttackBodyTissueFirst = 9;
    /// <summary>$A9:94C8-94CC and94D0 form four neutral shades; the white94CE slot interrupts their storage.</summary>
    public const int AttackGrayFirst = 10, AttackGrayLast = 14;
    /// <summary>$A9:94CE repeats the body palette's white nontransparent slot13.</summary>
    public const int WhiteColor = 13;
    public static int RecoveryLightsSource(int index) =>
        (uint)index < RecoveryLightsFrames
            ? RecoveryLightsFirstSource - index * RecoveryLightsByteStride
            : throw new ArgumentOutOfRangeException(nameof(index));
    /// <summary>$A9:D09A final-room foreground outline, repeated at $94F8 and glass ramp end$9528.</summary>
    public const int RoomOutlineColor = 12;
    /// <summary>$A9:D09C-D0A0 final-room gray shades, repeated by glass slots0..2.</summary>
    public const int RoomGrayFirst = 13;
    /// <summary>$A9:D0A2 final-room darkest gray, repeated by glass slot11.</summary>
    public const int RoomDarkGrayColor = 16;
    /// <summary>$A9:D082-D088, final-room four-color wall ramp.</summary>
    public const int RoomWallFirst = 0, RoomWallCount = 4;
    /// <summary>$A9:D08A-D090, final-room four-color darker wall ramp.</summary>
    public const int RoomShadowFirst = 4, RoomShadowCount = 4;
    /// <summary>$A9:D096, final-room BG palette3 inkE amber between black slots at D092/D094/D098; its pixel role remains unresolved.</summary>
    public const int RoomAmberColor = 10;
    /// <summary>$A9:D09C-D0A0, three neutral shades from intensity20 to8; middle14 interpolates.</summary>
    public const int RoomGrayCount = 3;
    /// <summary>$A9:D0A4-D0AA, red and blue indicator pairs sharing an active-channel shade decrease.</summary>
    public const int RoomRedFirst = 17, RoomBlueFirst = 19;
    /// <summary>$A9:D0AC repeats darkest room gray at D0A2.</summary>
    public const int RoomRepeatedDarkGrayColor = 21;
    /// <summary>$A9:D0AE, final-room warm accent.</summary>
    public const int RoomGlowColor = 22;
    /// <summary>$A9:9510/9530 terminate the room-entry palettes with the body palette's black slot14.</summary>
    public const int RoomEntryBlackColor = 14;
    /// <summary>$A9:951A-9528, Palette_MotherBrain_GlassShards eight-color RGB5 shade ramp.</summary>
    public const int GlassRampFirst = 3, GlassRampCount = 8;
    /// <summary>$A9:952A-952C, glass dark-neutral and neutral-highlight slots.</summary>
    public const int GlassDarkGrayColor = 11, GlassNeutralColor = 12;
    /// <summary>$A9:94F4/94F6 repeat recovery-light neutral highlight; following twelve slots repeat final-room12..23.</summary>
    public const int TubeNeutralCount = 2;
}
