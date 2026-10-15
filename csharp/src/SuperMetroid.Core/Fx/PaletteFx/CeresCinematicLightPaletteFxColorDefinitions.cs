namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Immutable mechanics for the cutscene gunship-engine and shared Ceres navigation-light
/// palette programs.
/// </summary>
/// <remarks>
/// The two navigation-light definitions retain their distinct native entry points but
/// converge on the cartridge's single shared fourteen-record loop at <c>$8D:C892</c>.
/// Their BGR555 colors remain live presentation data; this catalog owns palette placement,
/// timing, waits, and loop control.
/// </remarks>
public static class CeresCinematicLightPaletteFxColorDefinitions
{
    /// <summary>The first gunship-engine timed record at <c>$8D:C87E</c>.</summary>
    public const ushort GunshipEngineFirstFramePointer = 0xc87e;
    /// <summary>The gunship-engine loop contains two timed records.</summary>
    public const int GunshipEngineFrameCount = 2;
    /// <summary>Each gunship-engine record writes one live BGR555 color.</summary>
    public const int GunshipEngineColorsPerFrame = 1;
    /// <summary>Bytes from one gunship-engine duration through its terminal wait.</summary>
    public const int GunshipEngineFrameByteCount = 6;

    /// <summary>The shared Ceres navigation-light loop begins at <c>$8D:C892</c>.</summary>
    /// <remarks>
    /// The $8D:E1AC sprite definition sets CGRAM index $01DA at $C88E and
    /// falls through here; the $E1B8 background definition sets $00DA at
    /// $C906 and jumps here. For frame 0..13, its record begins at
    /// $C892 + 8 * frame, lasts four ticks, contains two live BGR555 words,
    /// and ends in $C595 wait. The $C61E goto at $C902 returns to $C892,
    /// making a 56-frame cycle. All 36 mechanics words across both entries,
    /// fourteen records, and terminal branches match the pinned NTSC J/U
    /// v1.0 ROM. The 28 color words remain presentation-owned.
    /// </remarks>
    public const ushort NavigationLightsFirstFramePointer = 0xc892;
    /// <summary>The shared Ceres navigation-light loop contains fourteen timed records.</summary>
    public const int NavigationLightsFrameCount = 14;
    /// <summary>Each Ceres navigation-light record writes two live BGR555 colors.</summary>
    public const int NavigationLightsColorsPerFrame = 2;
    /// <summary>Bytes from one navigation-light duration through its terminal wait.</summary>
    public const int NavigationLightsFrameByteCount = 8;
    /// <summary>Returns one gunship-engine timed-record pointer.</summary>
    public static ushort GunshipEngineFramePointer(int frame)
    {
        if ((uint)frame >= GunshipEngineFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(GunshipEngineFirstFramePointer +
            frame * GunshipEngineFrameByteCount));
    }

    /// <summary>Returns one presentation-owned gunship-engine BGR555 word.</summary>
    /// <remarks>
    /// For the only valid frame indices 0 and 1, the pinned ROM words at
    /// $8D:C880 + 6 * frame are $7FFF and $0000 respectively: a white/black
    /// alternation repeated by the control loop. Both values are authored
    /// BGR555 presentation content, so retain the two live color reads.
    /// The frame and color bounds reject adjacent program data.
    /// </remarks>
    public static ushort GunshipEngineColorPointer(int frame, int color)
    {
        if ((uint)color >= GunshipEngineColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(GunshipEngineFramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Returns one shared Ceres navigation-light timed-record pointer.</summary>
    public static ushort NavigationLightsFramePointer(int frame)
    {
        if ((uint)frame >= NavigationLightsFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(NavigationLightsFirstFramePointer +
            frame * NavigationLightsFrameByteCount));
    }

    /// <summary>Returns one presentation-owned shared navigation-light BGR555 word.</summary>
    /// <remarks>
    /// The shared sprite/background colors at $8D:C894 + 8 * frame and
    /// the following word have an exact bounded integer reconstruction for
    /// frame 0..13: first = (31 * abs(7 - frame) + 3) / 7 using integer
    /// division, second = 31 - first. These are red-channel BGR555 values;
    /// all 28 words match the pinned NTSC J/U v1.0 ROM, including the zero
    /// crossing at frame seven and the mirrored ascent. Both owners still
    /// consume the live authored words, and frame/color bounds exclude the
    /// adjacent palette-FX program.
    /// </remarks>
    public static ushort NavigationLightsColorPointer(int frame, int color)
    {
        if ((uint)color >= NavigationLightsColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(NavigationLightsFramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
