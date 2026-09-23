namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive exploding-Zebes layer fade.</summary>
public enum ZebesExplosionLayerFadePaletteFxProgramOwner
{
    /// <summary>The planet-crust fade-out.</summary>
    Crust,
    /// <summary>The grey-cloud fade-out.</summary>
    GreyClouds,
}

/// <summary>Immutable mechanics for the exploding-Zebes crust and cloud fades.</summary>
/// <remarks>
/// Issue #844 / #625: in the pinned NTSC J/U v1.0 ROM, definitions
/// <c>$8D:E1DC</c> (crust) and <c>$8D:E1E0</c> (grey clouds) enter at
/// <c>$8D:D48E</c> and <c>$8D:D5A4</c>. Each runs
/// <c>SetColorIndex(index)</c>, eight 34-byte records of
/// <c>duration, colors[15], Wait</c>, then <c>Delete</c>. Crust uses index
/// <c>$0082</c>, duration 20, and deletes at <c>$D5A2</c> after 160 frames;
/// clouds use index <c>$00A2</c>, duration 14, and delete at <c>$D6B8</c>
/// after 112 frames. All 38 control words match. For either layer, frame
/// <c>f</c> (0..7), column <c>c</c> (0..14), and each BGR555 component
/// <c>q</c>, the color is <c>floor(q(first row, c) * (8 - f) / 8)</c>.
/// This independent channel rule matches all 240 ROM color words exactly.
/// The first rows and their scaled colors remain live presentation data;
/// the presentation compiler supplies them while this catalog supplies only
/// controls to the palette-FX runtime. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions
{
    /// <summary>Both one-shot fades contain eight timed records.</summary>
    public const int FrameCount = 8;

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 34;

    private static readonly ZebesExplosionLayerFadePaletteFxProgramDefinition[] Definitions =
    [
        new(
            ZebesExplosionLayerFadePaletteFxProgramOwner.Crust,
            definitionPointer: 0xe1dc,
            programStart: 0xd48e,
            colorByteIndex: 0x0082,
            frameDuration: 20),
        new(
            ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds,
            definitionPointer: 0xe1e0,
            programStart: 0xd5a4,
            colorByteIndex: 0x00a2,
            frameDuration: 14),
    ];
    private static readonly IReadOnlyList<ZebesExplosionLayerFadePaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The crust and grey-cloud programs in definition order.</summary>
    public static IReadOnlyList<ZebesExplosionLayerFadePaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one compiled mechanics word across both programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (ZebesExplosionLayerFadePaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}

/// <summary>One complete exploding-Zebes layer-fade control program.</summary>
public sealed class ZebesExplosionLayerFadePaletteFxProgramDefinition
{
    internal ZebesExplosionLayerFadePaletteFxProgramDefinition(
        ZebesExplosionLayerFadePaletteFxProgramOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        ushort frameDuration)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        FrameDuration = frameDuration;
    }

    /// <summary>The mutually exclusive faded layer.</summary>
    public ZebesExplosionLayerFadePaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    /// <remarks><c>$8D:E1DC</c> is crust and <c>$8D:E1E0</c> is grey clouds.</remarks>
    public ushort DefinitionPointer { get; }

    /// <summary>The native instruction-list entry.</summary>
    /// <remarks><c>$8D:D48E</c> is crust and <c>$8D:D5A4</c> is grey clouds.</remarks>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The cartridge-authored duration of each timed record.</summary>
    public ushort FrameDuration { get; }

    /// <summary>The complete one-shot fade duration in frames.</summary>
    public int CycleFrames =>
        ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount *
        FrameDuration;

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>delete</c> command after all timed records.</summary>
    public ushort DeleteInstructionPointer => unchecked((ushort)(FirstFramePointer +
        ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount *
        ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame *
            ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Returns one live BGR555 color word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions
                .ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when item == DeleteInstructionPointer => PaletteFxInstructionCodes.Delete,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0;
             frame < ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => FrameDuration,
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameByteCount -
                    sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
