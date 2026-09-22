namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Crateria lightning palette program.</summary>
public enum CrateriaLightningPaletteOwner
{
    SurfaceLightning,
    UnusedDarkLightning,
}

/// <summary>One immutable word-sized palette-program mechanic.</summary>
public readonly record struct PaletteFxMechanicsWord(ushort Pointer, ushort Value);

/// <summary>One immutable byte-sized palette-program mechanic.</summary>
public readonly record struct PaletteFxMechanicsByte(ushort Pointer, byte Value);

/// <summary>One timed lightning color record whose BGR555 payload remains live.</summary>
public readonly record struct CrateriaLightningPaletteFrame(
    ushort Pointer,
    ushort Duration,
    int ColorCount)
{
    /// <summary>The first live BGR555 presentation word after the duration.</summary>
    public ushort FirstColorPointer => unchecked((ushort)(Pointer + sizeof(ushort)));

    /// <summary>The terminal wait command after the live colors.</summary>
    public ushort WaitInstructionPointer => unchecked((ushort)(
        FirstColorPointer + ColorCount * sizeof(ushort)));
}

/// <summary>
/// Immutable control words and byte operands for Crateria's two lightning programs.
/// </summary>
/// <remarks>
/// Definition <c>$F765</c> is the live Landing Site lightning effect. Definition
/// <c>$F769</c> is the cartridge's unused dark-lightning counterpart. Their 202 BGR555
/// words remain presentation data; timer setup, timing, branches, and targets are compiled.
/// </remarks>
public static class CrateriaLightningPaletteFxProgramMechanicsDefinitions
{
    private static readonly CrateriaLightningPaletteFxProgramDefinition[] Definitions =
    [
        CreateSurfaceLightning(),
        CreateDarkLightning(),
    ];
    private static readonly IReadOnlyList<CrateriaLightningPaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The live and unused-dark programs in cartridge definition order.</summary>
    public static IReadOnlyList<CrateriaLightningPaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one compiled word-sized mechanic across both programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (CrateriaLightningPaletteFxProgramDefinition definition in Definitions)
        {
            foreach (PaletteFxMechanicsWord word in definition.MechanicsWords)
            {
                if (word.Pointer != pointer)
                    continue;
                value = word.Value;
                return true;
            }
        }

        value = 0;
        return false;
    }

    /// <summary>Resolves one compiled byte-sized timer operand across both programs.</summary>
    public static bool TryReadMechanicsByte(ushort pointer, out byte value)
    {
        foreach (CrateriaLightningPaletteFxProgramDefinition definition in Definitions)
        {
            foreach (PaletteFxMechanicsByte item in definition.MechanicsBytes)
            {
                if (item.Pointer != pointer)
                    continue;
                value = item.Value;
                return true;
            }
        }

        value = 0;
        return false;
    }

    private static CrateriaLightningPaletteFxProgramDefinition CreateSurfaceLightning() =>
        Create(
            CrateriaLightningPaletteOwner.SurfaceLightning,
            definitionPointer: 0xf765,
            programStart: 0xeb3b,
            preInstruction: PaletteFxPreInstructionCodes.SwitchAboveY380,
            colorByteIndex: 0x00a8,
            timerTwoPointer: 0xeb57,
            repeatedFramesPointer: 0xeb5a,
            decrementTwoPointer: 0xebe6,
            neutralFrames: [new(0xebea, 0x00f0, 8)],
            timerOnePointer: 0xebfe,
            finalFramesPointer: 0xec01,
            decrementOnePointer: 0xec51,
            gotoPointer: 0xec55,
            firstFrame: new(0xeb43, 0x00f0, 8),
            repeatedFrames:
            [
                new(0xeb5a, 2, 8), new(0xeb6e, 1, 8), new(0xeb82, 1, 8),
                new(0xeb96, 1, 8), new(0xebaa, 1, 8), new(0xebbe, 1, 8),
                new(0xebd2, 2, 8),
            ],
            finalFrames:
            [
                new(0xec01, 1, 8), new(0xec15, 1, 8),
                new(0xec29, 1, 8), new(0xec3d, 2, 8),
            ],
            cycleFrames: 503,
            displayedRecordsPerCycle: 21);

    private static CrateriaLightningPaletteFxProgramDefinition CreateDarkLightning() =>
        Create(
            CrateriaLightningPaletteOwner.UnusedDarkLightning,
            definitionPointer: 0xf769,
            programStart: 0xec6e,
            preInstruction: PaletteFxPreInstructionCodes.SwitchAboveY380Second,
            colorByteIndex: 0x0082,
            timerTwoPointer: 0xec88,
            repeatedFramesPointer: 0xec8b,
            decrementTwoPointer: 0xed09,
            neutralFrames:
            [
                new(0xed0d, 0x00f0, 7),
                new(0xed1f, 0x00f0, 7),
            ],
            timerOnePointer: 0xed31,
            finalFramesPointer: 0xed34,
            decrementOnePointer: 0xed7c,
            gotoPointer: 0xed80,
            firstFrame: new(0xec76, 0x00f0, 7),
            repeatedFrames:
            [
                new(0xec8b, 2, 7), new(0xec9d, 1, 7), new(0xecaf, 1, 7),
                new(0xecc1, 1, 7), new(0xecd3, 1, 7), new(0xece5, 1, 7),
                new(0xecf7, 2, 7),
            ],
            finalFrames:
            [
                new(0xed34, 1, 7), new(0xed46, 1, 7),
                new(0xed58, 1, 7), new(0xed6a, 2, 7),
            ],
            cycleFrames: 743,
            displayedRecordsPerCycle: 22);

    private static CrateriaLightningPaletteFxProgramDefinition Create(
        CrateriaLightningPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort preInstruction,
        ushort colorByteIndex,
        ushort timerTwoPointer,
        ushort repeatedFramesPointer,
        ushort decrementTwoPointer,
        CrateriaLightningPaletteFrame[] neutralFrames,
        ushort timerOnePointer,
        ushort finalFramesPointer,
        ushort decrementOnePointer,
        ushort gotoPointer,
        CrateriaLightningPaletteFrame firstFrame,
        CrateriaLightningPaletteFrame[] repeatedFrames,
        CrateriaLightningPaletteFrame[] finalFrames,
        int cycleFrames,
        int displayedRecordsPerCycle)
    {
        var frames = new List<CrateriaLightningPaletteFrame>
        {
            firstFrame,
        };
        frames.AddRange(repeatedFrames);
        frames.AddRange(neutralFrames);
        frames.AddRange(finalFrames);

        var words = new List<PaletteFxMechanicsWord>
        {
            new(programStart, PaletteFxInstructionCodes.SetPreInstruction),
            new(unchecked((ushort)(programStart + 2)), preInstruction),
            new(unchecked((ushort)(programStart + 4)), PaletteFxInstructionCodes.SetColorIndex),
            new(unchecked((ushort)(programStart + 6)), colorByteIndex),
            new(timerTwoPointer, PaletteFxInstructionCodes.SetTimer),
            new(decrementTwoPointer, PaletteFxInstructionCodes.DecrementTimerAndGoto),
            new(unchecked((ushort)(decrementTwoPointer + 2)), repeatedFramesPointer),
            new(timerOnePointer, PaletteFxInstructionCodes.SetTimer),
            new(decrementOnePointer, PaletteFxInstructionCodes.DecrementTimerAndGoto),
            new(unchecked((ushort)(decrementOnePointer + 2)), finalFramesPointer),
            new(gotoPointer, PaletteFxInstructionCodes.Goto),
            new(unchecked((ushort)(gotoPointer + 2)), firstFrame.Pointer),
        };
        foreach (CrateriaLightningPaletteFrame frame in frames)
        {
            words.Add(new(frame.Pointer, frame.Duration));
            words.Add(new(frame.WaitInstructionPointer, PaletteFxInstructionCodes.Wait));
        }

        return new CrateriaLightningPaletteFxProgramDefinition(
            owner,
            definitionPointer,
            programStart,
            firstFrame.Pointer,
            frames.ToArray(),
            words.ToArray(),
            [new(unchecked((ushort)(timerTwoPointer + 2)), 2),
             new(unchecked((ushort)(timerOnePointer + 2)), 1)],
            cycleFrames,
            displayedRecordsPerCycle);
    }
}

/// <summary>One complete Crateria lightning palette control program.</summary>
public sealed class CrateriaLightningPaletteFxProgramDefinition
{
    internal CrateriaLightningPaletteFxProgramDefinition(
        CrateriaLightningPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort firstFramePointer,
        CrateriaLightningPaletteFrame[] frames,
        PaletteFxMechanicsWord[] mechanicsWords,
        PaletteFxMechanicsByte[] mechanicsBytes,
        int cycleFrames,
        int displayedRecordsPerCycle)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        FirstFramePointer = firstFramePointer;
        Frames = Array.AsReadOnly(frames);
        MechanicsWords = Array.AsReadOnly(mechanicsWords);
        MechanicsBytes = Array.AsReadOnly(mechanicsBytes);
        CycleFrames = cycleFrames;
        DisplayedRecordsPerCycle = displayedRecordsPerCycle;
    }

    /// <summary>The live or unused-dark lightning owner.</summary>
    public CrateriaLightningPaletteOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The setup entry for this program.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The neutral record selected by the vertical-position pre-instruction.</summary>
    public ushort FirstFramePointer { get; }

    /// <summary>Every unique timed record in cartridge order.</summary>
    public IReadOnlyList<CrateriaLightningPaletteFrame> Frames { get; }

    /// <summary>Every word-sized mechanics operand owned by this program.</summary>
    public IReadOnlyList<PaletteFxMechanicsWord> MechanicsWords { get; }

    /// <summary>The two byte-sized timer operands owned by this program.</summary>
    public IReadOnlyList<PaletteFxMechanicsByte> MechanicsBytes { get; }

    /// <summary>Frames from initial display through the next initial display.</summary>
    public int CycleFrames { get; }

    /// <summary>Number of records displayed including the repeated initial record.</summary>
    public int DisplayedRecordsPerCycle { get; }
}
