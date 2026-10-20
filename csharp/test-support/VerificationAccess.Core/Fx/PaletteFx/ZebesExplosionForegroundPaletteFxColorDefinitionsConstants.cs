
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="ZebesExplosionForegroundPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionForegroundPaletteFxColorDefinitionsConstants
{
    /// <summary>The palette-FX definition at <c>$8D:E1C8</c>.</summary>
    public const ushort DefinitionPointer = 0xe1c8;

    /// <summary>Provides the Zebes explosion foreground palette definition for effect checks.</summary>
    extension(ZebesExplosionForegroundPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
    /// <summary>The terminal <c>delete</c> command at <c>$8D:CD60</c>.</summary>
    internal static ushort DeleteInstructionPointer => checked((ushort)(ZebesExplosionForegroundPaletteFxColorDefinitions.FirstFramePointer + ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.FrameCount * ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.FrameByteCount));
    /// <summary>The foreground explosion writes CGRAM from byte index <c>$0002</c>.</summary>
    public const ushort ColorByteIndex = 0x0002;
    /// <summary>The complete one-shot foreground explosion lasts 144 frames, the ending's $8B:EB71 right-star exposure.</summary>
    public const int CycleFrames = 144;
    /// <summary>$8D:CB40-CBA5: three initial prefix-reveal records; authored reveal-to-hold boundary.</summary>
    internal const int InitialRevealRecords = 3;
    /// <summary>$8D:CB40/CB62/CB84: selected four-tick initial reveal exposure.</summary>
    internal const ushort RevealTicks = 4;
    /// <summary>$8D:CBA6: remaining exposure before the synchronized $8B:EB71 finale handoff.</summary>
    internal static ushort CrestTicks => (ushort)(CycleFrames - InitialRevealRecords * RevealTicks -
        (ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.FrameCount - InitialRevealRecords - 1) * ExpansionTicks);
    /// <summary>$8D:CBC8-CD3E: selected six-tick exposure of each remaining expansion record.</summary>
    internal const ushort ExpansionTicks = 6;
    /// <summary>Exact initial reveal, fourth-prefix hold and expansion choreography; independent colors remain separate.</summary>
    internal static ushort Duration(int frame)
    {
        if ((uint)frame >= ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        return frame < InitialRevealRecords ? RevealTicks : frame == InitialRevealRecords ? CrestTicks : ExpansionTicks;
    }
    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = (ZebesExplosionForegroundPaletteFxColorDefinitions.ColorsPerFrame + 2) * sizeof(ushort);
    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => ZebesExplosionForegroundPaletteFxColorDefinitions.Rows.Color(frame, color);
    /// <summary>The one-shot foreground explosion contains sixteen timed records.</summary>
    public static int FrameCount => ZebesExplosionForegroundPaletteFxColorDefinitions.Rows.FrameCount;
}
