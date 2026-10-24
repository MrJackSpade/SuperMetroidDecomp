namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Native sources and CGRAM destinations for Mother Brain's fake-death room colors.</summary>
public static class MotherBrainRoomColorRomData
{
    /// <summary>The bank containing the $D046 palette program and its RGB5 payloads.</summary>
    public const int SourceBank = 0xa90000;

    /// <summary>Each timed entry selects two twelve-color source slices.</summary>
    public const int SliceColors = 12;

    /// <summary>Phase-two setup copies fifteen nontransparent attack colors from $A9:94B4.</summary>
    public const int PhaseTwoAttackSource = 0xa994b4;

    /// <summary>Phase-two setup copies fifteen rear-leg colors from $A9:9494.</summary>
    public const int PhaseTwoRearLegSource = 0xa99494;
    /// <summary>Both phase-two setup palettes have fifteen nontransparent colors.</summary>
    public const int PhaseTwoColors = 15;

    /// <summary>Mother Brain body initialization copies glass-shard colors from $A9:9514.</summary>
    public const int InitialGlassShardSource = 0xa99514;

    /// <summary>Mother Brain body initialization copies tube-projectile colors from $A9:94F4.</summary>
    public const int InitialTubeProjectileSource = 0xa994f4;

    /// <summary>Both room-entry palettes omit transparent color zero and copy fifteen colors.</summary>
    public const int InitialColors = 15;

    /// <summary>Seven phase-three room-light images, selected after the Baby cutscene.</summary>
    public const int RecoveryLightsFrames = 7;
    /// <summary>Each $AD:F3D3-minus-index image is two slices of fourteen colours.</summary>
    public const int RecoveryLightsSliceColors = 14;
    /// <summary>The room-light source rows descend by $38 bytes in bank $AD.</summary>
    public const int RecoveryLightsFirstSource = 0xadf3d3;
    /// <summary>Byte size of one $AD:F24B recovery image: two fourteen-word RGB5 slices, so playback subtracts 56 bytes between source rows.</summary>
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
    /// <summary>Calculates the full bank-$AD source selected by FadeInBackgroundForMotherBrainPhase3's $F273 pointer table, without reading or applying colors.</summary>
    /// <param name="index">Playback image 0..6, selecting native rows palette6 through palette0; excludes the following zero-pointer terminator.</param>
    /// <returns>24-bit cartridge address $AD:F3D3 minus <paramref name="index"/> times $38, not an address in <see cref="SourceBank"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside 0..6.</exception>
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
    public const int GlassDarkGrayColor = 11;
    /// <summary>$A9:94F4/94F6 repeat recovery-light neutral highlight; following twelve slots repeat final-room12..23.</summary>
    public const int TubeNeutralCount = 2;
}
