namespace SuperMetroid.Core.Game;

/// <summary>Identifies one compiled Ceres cinematic-light palette program.</summary>
public enum CeresCinematicLightPaletteFxProgramOwner
{
    /// <summary>The cutscene gunship engine flicker.</summary>
    GunshipEngine,
    /// <summary>The navigation lights on sprite Ceres.</summary>
    SpriteNavigationLights,
    /// <summary>The navigation lights on background Ceres.</summary>
    BackgroundNavigationLights,
}

/// <summary>Immutable entry metadata for one Ceres cinematic-light palette program.</summary>
public readonly record struct CeresCinematicLightPaletteFxProgramDefinition(
    CeresCinematicLightPaletteFxProgramOwner Owner,
    ushort DefinitionPointer,
    ushort ProgramStart);

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
public static class CeresCinematicLightPaletteFxProgramMechanicsDefinitions
{
    /// <summary><c>PalFxDef_CutsceneGunshipEngine</c> at <c>$8D:E1A8</c>.</summary>
    public const ushort GunshipEngineDefinitionPointer = 0xe1a8;
    /// <summary><c>PalFxInstList_CutsceneGunshipEngine</c> at <c>$8D:C87A</c>.</summary>
    /// <remarks>
    /// The $8D:E1A8 definition selects a bounded two-record control loop.
    /// $C87A sets CGRAM word index $00BE; timed records start at
    /// $C87E + 6 * frame for frame 0..1. Each record has duration one,
    /// one live BGR555 color word, and a $C595 wait. The $C61E goto at
    /// $C88A targets $C87E, yielding a two-frame cycle without entering
    /// the adjacent $C88E program. All eight mechanics words match the
    /// pinned NTSC J/U v1.0 ROM; the two colors stay presentation-owned.
    /// </remarks>
    public const ushort GunshipEngineProgramStart = 0xc87a;
    /// <summary>The first gunship-engine timed record at <c>$8D:C87E</c>.</summary>
    public const ushort GunshipEngineFirstFramePointer = 0xc87e;
    /// <summary>The gunship-engine terminal <c>goto</c> at <c>$8D:C88A</c>.</summary>
    public const ushort GunshipEngineLoopInstructionPointer = 0xc88a;
    /// <summary>The gunship engine writes CGRAM word index <c>$00BE</c>.</summary>
    public const ushort GunshipEngineColorIndex = 0x00be;
    /// <summary>The gunship-engine loop contains two timed records.</summary>
    public const int GunshipEngineFrameCount = 2;
    /// <summary>Each gunship-engine record writes one live BGR555 color.</summary>
    public const int GunshipEngineColorsPerFrame = 1;
    /// <summary>Bytes from one gunship-engine duration through its terminal wait.</summary>
    public const int GunshipEngineFrameByteCount = 6;
    /// <summary>Each gunship-engine record lasts one frame.</summary>
    public const ushort GunshipEngineFrameDuration = 1;
    /// <summary>The complete gunship-engine loop lasts two frames.</summary>
    public const int GunshipEngineCycleFrames = 2;

    /// <summary><c>PalFxDef_CutsceneCeresNavigationLightsSprite</c> at <c>$8D:E1AC</c>.</summary>
    public const ushort SpriteNavigationLightsDefinitionPointer = 0xe1ac;
    /// <summary><c>PalFxInstList_CutsceneCeresNavigationLightsSprite</c> at <c>$8D:C88E</c>.</summary>
    public const ushort SpriteNavigationLightsProgramStart = 0xc88e;
    /// <summary>The sprite-Ceres navigation lights write CGRAM word index <c>$01DA</c>.</summary>
    public const ushort SpriteNavigationLightsColorIndex = 0x01da;

    /// <summary><c>PalFxDef_CutsceneCeresNavigationLightsBg</c> at <c>$8D:E1B8</c>.</summary>
    public const ushort BackgroundNavigationLightsDefinitionPointer = 0xe1b8;
    /// <summary><c>PalFxInstList_CutsceneCeresNavigationLightsBg</c> at <c>$8D:C906</c>.</summary>
    public const ushort BackgroundNavigationLightsProgramStart = 0xc906;
    /// <summary>The background-Ceres navigation lights write CGRAM word index <c>$00DA</c>.</summary>
    public const ushort BackgroundNavigationLightsColorIndex = 0x00da;

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
    /// <summary>The shared Ceres navigation-light terminal <c>goto</c> at <c>$8D:C902</c>.</summary>
    public const ushort NavigationLightsLoopInstructionPointer = 0xc902;
    /// <summary>The shared Ceres navigation-light loop contains fourteen timed records.</summary>
    public const int NavigationLightsFrameCount = 14;
    /// <summary>Each Ceres navigation-light record writes two live BGR555 colors.</summary>
    public const int NavigationLightsColorsPerFrame = 2;
    /// <summary>Bytes from one navigation-light duration through its terminal wait.</summary>
    public const int NavigationLightsFrameByteCount = 8;
    /// <summary>Each Ceres navigation-light record lasts four frames.</summary>
    public const ushort NavigationLightsFrameDuration = 4;
    /// <summary>The complete Ceres navigation-light loop lasts 56 frames.</summary>
    public const int NavigationLightsCycleFrames = 56;

    /// <summary>All native definitions whose mechanics are owned by this catalog.</summary>
    public static IReadOnlyList<CeresCinematicLightPaletteFxProgramDefinition> All { get; } =
    [
        new(
            CeresCinematicLightPaletteFxProgramOwner.GunshipEngine,
            GunshipEngineDefinitionPointer,
            GunshipEngineProgramStart),
        new(
            CeresCinematicLightPaletteFxProgramOwner.SpriteNavigationLights,
            SpriteNavigationLightsDefinitionPointer,
            SpriteNavigationLightsProgramStart),
        new(
            CeresCinematicLightPaletteFxProgramOwner.BackgroundNavigationLights,
            BackgroundNavigationLightsDefinitionPointer,
            BackgroundNavigationLightsProgramStart),
    ];

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
    public static ushort NavigationLightsColorPointer(int frame, int color)
    {
        if ((uint)color >= NavigationLightsColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(NavigationLightsFramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            GunshipEngineProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            GunshipEngineProgramStart + 2 => GunshipEngineColorIndex,
            GunshipEngineLoopInstructionPointer => PaletteFxInstructionCodes.Goto,
            GunshipEngineLoopInstructionPointer + 2 => GunshipEngineFirstFramePointer,

            SpriteNavigationLightsProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            SpriteNavigationLightsProgramStart + 2 => SpriteNavigationLightsColorIndex,
            NavigationLightsLoopInstructionPointer => PaletteFxInstructionCodes.Goto,
            NavigationLightsLoopInstructionPointer + 2 => NavigationLightsFirstFramePointer,

            BackgroundNavigationLightsProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            BackgroundNavigationLightsProgramStart + 2 => BackgroundNavigationLightsColorIndex,
            BackgroundNavigationLightsProgramStart + 4 => PaletteFxInstructionCodes.Goto,
            BackgroundNavigationLightsProgramStart + 6 => NavigationLightsFirstFramePointer,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0; frame < GunshipEngineFrameCount; frame++)
        {
            int offset = pointer - GunshipEngineFramePointer(frame);
            value = offset switch
            {
                0 => GunshipEngineFrameDuration,
                GunshipEngineFrameByteCount - sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        for (int frame = 0; frame < NavigationLightsFrameCount; frame++)
        {
            int offset = pointer - NavigationLightsFramePointer(frame);
            value = offset switch
            {
                0 => NavigationLightsFrameDuration,
                NavigationLightsFrameByteCount - sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
