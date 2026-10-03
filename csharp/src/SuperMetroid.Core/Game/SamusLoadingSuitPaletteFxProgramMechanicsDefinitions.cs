namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive suit palette loaded around Samus.</summary>
public enum SamusLoadingSuitPaletteFxProgramOwner
{
    /// <summary>Power-suit colors.</summary>
    /// <remarks>
    /// Issue #856 / #625: all 144 BGR555 words in ROM program <c>$8D:DB62</c>
    /// match existing authored Samus palettes in bank <c>$9B</c>. Records
    /// 0, 2, 4, 6, 8 equal normal Power Suit <c>$9B:9400</c>; records 1 and 3
    /// equal speed-boost shade <c>$9B:9B80</c>; records 5 and 7 equal
    /// <c>$9B:9B60</c> and <c>$9B:9B40</c>. Each comparison covers all sixteen
    /// colors. These historical equality observations are research inputs, not a
    /// retention disposition under #1165; presentation review remains pending. Production
    /// indexes only records 0..8 and colors 0..15 through <c>ColorPointer</c>.
    /// </remarks>
    PowerSuit,
    /// <summary>Varia-suit colors.</summary>
    /// <remarks>
    /// Issue #857 / #625: all 144 BGR555 words in ROM program <c>$8D:DCC8</c>
    /// match existing authored Samus palettes in bank <c>$9B</c>. Records
    /// 0, 2, 4, 6, 8 equal normal Varia Suit <c>$9B:9520</c>; records 1 and 3
    /// equal speed-boost shade <c>$9B:9D80</c>; records 5 and 7 equal
    /// <c>$9B:9D60</c> and <c>$9B:9D40</c>. Each comparison covers all sixteen
    /// colors. These historical equality observations are research inputs, not a
    /// retention disposition under #1165; presentation review remains pending. Production
    /// indexes only records 0..8 and colors 0..15 through <c>ColorPointer</c>.
    /// </remarks>
    VariaSuit,
    /// <summary>Gravity-suit colors.</summary>
    /// <remarks>
    /// Issue #858 / #625: all 144 BGR555 words in ROM program <c>$8D:DE2E</c>
    /// match existing authored Samus palettes in bank <c>$9B</c>. Records
    /// 0, 2, 4, 6, 8 equal normal Gravity Suit <c>$9B:9800</c>; records 1 and 3
    /// equal speed-boost shade <c>$9B:9F80</c>; records 5 and 7 equal
    /// <c>$9B:9F60</c> and <c>$9B:9F40</c>. Each comparison covers all sixteen
    /// colors. These historical equality observations are research inputs, not a
    /// retention disposition under #1165; presentation review remains pending. Production
    /// indexes only records 0..8 and colors 0..15 through <c>ColorPointer</c>.
    /// </remarks>
    GravitySuit,
}

/// <summary>Immutable mechanics shared by the three Samus-loading palette programs.</summary>
/// <remarks>
/// Issue #855 / #625: pinned NTSC J/U v1.0 ROM definitions <c>$8D:E1F4</c>,
/// <c>$E1F8</c>, and <c>$E1FC</c> enter at <c>$8D:DB62</c>, <c>$DCC8</c>,
/// and <c>$DE2E</c>. Each starts with <c>SetColorIndex($0180)</c>, then
/// four <c>SetTimer(byte)</c> groups at bank-local entry offsets
/// <c>$0007,$0056,$00A5,$00F4</c>. Timers <c>$24,3,3,2</c> replay each
/// group's pair of <c>3, colors[16], Wait</c> records via
/// <c>DecrementTimerAndGoto(group start)</c>. One final
/// <c>1, colors[16], Wait</c> record starts at offset <c>$0140</c>;
/// <c>Delete</c> follows at <c>$0164</c>. Thus
/// <c>(36 + 3 + 3 + 2)*2*3 + 1 = 265</c> frames elapse. All 99
/// control words and 12 timer bytes match the ROM. The 432 BGR555 words
/// remain live presentation data supplied by the presentation compiler;
/// this catalog owns only placement, timing, replay, and deletion. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class SamusLoadingSuitPaletteFxProgramMechanicsDefinitions
{
    /// <summary>All programs write CGRAM from byte index <c>$0180</c>.</summary>
    public const ushort ColorByteIndex = 0x0180;

    /// <summary>Each program has four independently counted two-record groups.</summary>
    public const int GroupCount = 4;

    /// <summary>Each counted group contains two palette records.</summary>
    public const int FramesPerGroup = 2;

    /// <summary>Each complete program contains eight counted records and one final record.</summary>
    public const int FrameCount = GroupCount * FramesPerGroup + 1;

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 36;

    /// <summary>Each record in a counted group lasts three frames.</summary>
    public const ushort GroupFrameDuration = 3;

    /// <summary>The terminal record lasts one frame.</summary>
    public const ushort FinalFrameDuration = 1;

    /// <summary>Every suit-loading program lasts 265 frames.</summary>
    public const int CycleFrames = 265;

    /// <summary>Chosen replay chronology of the loading flash, retained under #1165's nonsense exception.</summary>
    /// <remarks>Native $8D:DB66 and the following timer operands repeat each
    /// six-frame palette pair 36, 3, 3, then 2 times. The first two pairs even
    /// contain identical colors: their split is script chronology, not a color
    /// interpolation parameter. $8D:C648 assigns each literal counter and
    /// $C639 decrements it without a duration, position, or brightness input.
    /// The separate fanfare at $92:ED24 counts to 360, not this program's 265.
    /// These choices specify how long the flashes look; fitting the group index
    /// or spelling the same numbers as cases would only disguise that timing.
    /// Layout and repeated palette data are reviewed separately; this exception
    /// covers only the four replay operands, not their surrounding tables.</remarks>
    private static readonly byte[] GroupTimerValues = [0x24, 0x03, 0x03, 0x02];
    /// <summary>Native Power-suit definition $8D:E1F4 selects program $DB62.</summary>
    private static readonly SamusLoadingSuitPaletteFxProgramDefinition Power =
        new(SamusLoadingSuitPaletteFxProgramOwner.PowerSuit, 0xe1f4, 0xdb62);
    /// <summary>Native Varia-suit definition $8D:E1F8 selects program $DCC8.</summary>
    private static readonly SamusLoadingSuitPaletteFxProgramDefinition Varia =
        new(SamusLoadingSuitPaletteFxProgramOwner.VariaSuit, 0xe1f8, 0xdcc8);
    /// <summary>Native Gravity-suit definition $8D:E1FC selects program $DE2E.</summary>
    private static readonly SamusLoadingSuitPaletteFxProgramDefinition Gravity =
        new(SamusLoadingSuitPaletteFxProgramOwner.GravitySuit, 0xe1fc, 0xde2e);
    private static readonly IReadOnlyList<SamusLoadingSuitPaletteFxProgramDefinition> Programs = new ProgramList();

    /// <summary>The power, Varia, and gravity programs in definition order.</summary>
    /// <remarks>Named program identities replace the former indexed descriptor array.
    /// The view preserves original order/count/index rejection; runtime mechanics
    /// dispatches directly to the same three named owners without enumeration.
    /// Native header words and all program ownership are independently checked for1165.</remarks>
    public static IReadOnlyList<SamusLoadingSuitPaletteFxProgramDefinition> All => Programs;

    private sealed class ProgramList : IReadOnlyList<SamusLoadingSuitPaletteFxProgramDefinition>
    {
        public int Count => 3;
        public SamusLoadingSuitPaletteFxProgramDefinition this[int index] => index switch
        {
            0 => Power,
            1 => Varia,
            2 => Gravity,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        public IEnumerator<SamusLoadingSuitPaletteFxProgramDefinition> GetEnumerator()
        {
            yield return Power;
            yield return Varia;
            yield return Gravity;
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Returns the bank-local offset of one counted group.</summary>
    /// <remarks>Native $8D:DB62/DCC8/DE2E programs have a four-byte color-index
    /// setup and three-byte initial timer command, so the first record starts at7.
    /// Each next group is two36-byte color records, four-byte loop command/target,
    /// and three-byte timer command later: offset=7+79*group for group0..3.
    /// Independently decoded from supported NTSC J/U v1.0 and pinned bank_8D.asm
    /// for #1165. No stored group-offset array remains.</remarks>
    public static ushort GroupStartOffset(int group)
    {
        if ((uint)group >= GroupCount)
            throw new ArgumentOutOfRangeException(nameof(group));
        return (ushort)(7 + group * (FramesPerGroup * FrameByteCount + 4 + 3));
    }

    /// <summary>Returns the cartridge-authored replay count of one group.</summary>
    public static byte GroupTimerValue(int group)
    {
        if ((uint)group >= GroupCount)
            throw new ArgumentOutOfRangeException(nameof(group));
        return GroupTimerValues[group];
    }

    /// <summary>Resolves one compiled mechanics word across all three programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value) =>
        Power.TryReadMechanicsWord(pointer, out value) ||
        Varia.TryReadMechanicsWord(pointer, out value) ||
        Gravity.TryReadMechanicsWord(pointer, out value);

    /// <summary>Resolves one compiled byte-sized group timer across the named programs.</summary>
    public static bool TryReadMechanicsByte(ushort pointer, out byte value) =>
        Power.TryReadMechanicsByte(pointer, out value) ||
        Varia.TryReadMechanicsByte(pointer, out value) ||
        Gravity.TryReadMechanicsByte(pointer, out value);
}
/// <summary>One complete Samus-loading palette control program.</summary>
public sealed class SamusLoadingSuitPaletteFxProgramDefinition
{
    private const ushort FinalFrameOffset = 0x0140;
    private const ushort DeleteInstructionOffset = 0x0164;

    internal SamusLoadingSuitPaletteFxProgramDefinition(
        SamusLoadingSuitPaletteFxProgramOwner owner,
        ushort definitionPointer,
        ushort programStart)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
    }

    /// <summary>The mutually exclusive suit palette.</summary>
    public SamusLoadingSuitPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    /// <remarks><c>$8D:E1F4</c>, <c>$8D:E1F8</c>, or <c>$8D:E1FC</c>.</remarks>
    public ushort DefinitionPointer { get; }

    /// <summary>The native instruction-list entry.</summary>
    /// <remarks><c>$8D:DB62</c>, <c>$8D:DCC8</c>, or <c>$8D:DE2E</c>.</remarks>
    public ushort ProgramStart { get; }

    /// <summary>The terminal one-frame record after all counted groups.</summary>
    public ushort FinalFramePointer => unchecked((ushort)(ProgramStart + FinalFrameOffset));

    /// <summary>The terminal <c>delete</c> command after the final record.</summary>
    public ushort DeleteInstructionPointer =>
        unchecked((ushort)(ProgramStart + DeleteInstructionOffset));

    /// <summary>Returns the first record of one counted group.</summary>
    public ushort GroupStartPointer(int group) => unchecked((ushort)(
        ProgramStart + SamusLoadingSuitPaletteFxProgramMechanicsDefinitions
            .GroupStartOffset(group)));

    /// <summary>Returns the byte operand that initializes one group's replay timer.</summary>
    public ushort GroupTimerBytePointer(int group) =>
        unchecked((ushort)(GroupStartPointer(group) - 1));

    /// <summary>Returns one of the nine timed-record pointers.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if (frame == SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount - 1)
            return FinalFramePointer;

        int group = frame / SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FramesPerGroup;
        int frameInGroup = frame %
            SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FramesPerGroup;
        return unchecked((ushort)(GroupStartPointer(group) + frameInGroup *
            SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Returns one live BGR555 color word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorsPerFrame)
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
            var item when item == ProgramStart + 2 =>
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            var item when item == DeleteInstructionPointer => PaletteFxInstructionCodes.Delete,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int group = 0;
             group < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupCount;
             group++)
        {
            ushort groupStart = GroupStartPointer(group);
            ushort setTimer = unchecked((ushort)(groupStart - 3));
            ushort decrement = unchecked((ushort)(groupStart +
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FramesPerGroup *
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameByteCount));
            value = pointer switch
            {
                var item when item == setTimer => PaletteFxInstructionCodes.SetTimer,
                var item when item == decrement =>
                    PaletteFxInstructionCodes.DecrementTimerAndGoto,
                var item when item == decrement + 2 => groupStart,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        for (int frame = 0;
             frame < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => frame == SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount - 1
                    ? SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FinalFrameDuration
                    : SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupFrameDuration,
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameByteCount -
                    sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }

    /// <summary>Resolves one compiled byte-sized group timer.</summary>
    public bool TryReadMechanicsByte(ushort pointer, out byte value)
    {
        for (int group = 0;
             group < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupCount;
             group++)
        {
            if (pointer != GroupTimerBytePointer(group))
                continue;
            value = SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupTimerValue(group);
            return true;
        }

        value = 0;
        return false;
    }
}
