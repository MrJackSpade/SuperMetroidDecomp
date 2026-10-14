namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable control words for the four Tourian entrance-statue grey-out palette programs.
/// </summary>
/// <remarks>
/// The four entries at $8D:E222-$E23A select a CGRAM destination and converge on the
/// shared eight-frame program at $8D:E23E. Its 64 BGR555 colors remain live presentation
/// data; only color-index setup, branches, durations, waits, and deletion are compiled.
/// Draygon, Kraid, Ridley, and Phantoon select CGRAM bytes $00C0,
/// $00E0, $0120, and $0140 respectively. The first three entries
/// explicitly goto $E23E; Phantoon falls through. Shared records
/// f=0..7 start at $E23E + 20*f, last eight frames, write eight live
/// colors, and end in $C595 wait. Frame eight reaches $C5CF delete
/// at $E2DE after 64 timed frames. All 31 mechanics words match the
/// pinned NTSC J/U v1.0 ROM; each guarded caller reaches deletion.
/// </remarks>
public static class TourianStatueGreyPaletteFxProgramMechanicsDefinitions
{
    /// <summary>$8D:F749: first of four consecutive statue palette-FX definitions.</summary>
    private const ushort FirstDefinition = 0xf749;
    /// <summary>$8D:E222: first color-index/goto entry; the final entry falls through.</summary>
    private const ushort FirstProgram = 0xe222;
    /// <summary>CGRAM byte $C0: first statue palette, with palette row eight skipped.</summary>
    private const ushort FirstColorByte = 0xc0;

    /// <summary>Four boss-specific entries sharing the eight-frame grey-out program.</summary>
    internal static readonly IReadOnlyList<TourianStatueGreyPaletteFxProgramDefinition> Definitions = new ProgramEntries();
    /// <summary>Computes consecutive definition and program addresses without storing duplicate records.</summary>
    private sealed class ProgramEntries : IReadOnlyList<TourianStatueGreyPaletteFxProgramDefinition>
    {
        /// <summary>Draygon, Kraid, Ridley, and Phantoon each have one setup entry.</summary>
        public int Count => 4;
        /// <summary>Maps a boss index to its definition, instruction start, and CGRAM row.</summary>
        public TourianStatueGreyPaletteFxProgramDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return new((TourianStatueBoss)index,
                    (ushort)(FirstDefinition + 4 * index),
                    (ushort)(FirstProgram + 8 * index),
                    (ushort)(FirstColorByte + 32 * (index + (index >= 2 ? 1 : 0))),
                    usesGoto: index < Count - 1);
            }
        }
        /// <summary>Yields the four boss entries in native table order.</summary>
        public IEnumerator<TourianStatueGreyPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Exposes the same ordered entries to non-generic collection callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary><c>InstList_PaletteFXObject_Common_GreyOutTourianStatue</c> at $8D:E23E.</summary>
    public const ushort FirstFramePointer = 0xe23e;

    /// <summary>Eight eight-frame records form the statue grey-out sequence.</summary>
    public const int FrameCount = 8;

    /// <summary>Eight BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>Bytes from one duration word through its terminal wait command.</summary>
    public const int FrameByteCount = 20;

    /// <summary>Terminal <c>delete</c> instruction at $8D:E2DE.</summary>
    public const ushort DeleteInstructionPointer = 0xe2de;

    /// <summary>Resolves one compiled mechanics word across all four statue entries.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (TourianStatueGreyPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        int frameOffset = pointer - FirstFramePointer;
        if (frameOffset is >= 0 and < (FrameCount * FrameByteCount))
        {
            int inFrame = frameOffset % FrameByteCount;
            if (inFrame == 0)
            {
                value = 8;
                return true;
            }
            if (inFrame == FrameByteCount - sizeof(ushort))
            {
                value = (ushort)PaletteFxInstruction.Wait;
                return true;
            }
        }

        if (pointer == DeleteInstructionPointer)
        {
            value = (ushort)PaletteFxInstruction.Delete;
            return true;
        }

        value = 0;
        return false;
    }

    /// <summary>Returns the timed-record pointer for one zero-based fade frame.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns the presentation-owned BGR555 word for one fade frame/color.</summary>
    /// <remarks>
    /// For frame f=0..7 and color c=0..7, the live word is at
    /// $8D:E240 + 20*f + 2*c. Color zero is $0000 at f=0 and $3800
    /// for f=1..7. For colors 1..7, initial endpoint BGR555 words are
    /// ($57FF,$2BFF,$1F3C,$0278,$01B0,$010B,$0087), and final
    /// endpoints are ($7F58,$6ED5,$5A71,$49EE,$356A,$24E7,$1083).
    /// Decode each red, green, and blue channel. With delta=end-start,
    /// each channel at f is start + sign(delta)*floor((abs(delta)*f+3)/7);
    /// re-encode BGR555. This nearest-integer interpolation matches all
    /// 64 words in the pinned NTSC J/U v1.0 ROM. Frame eight reaches
    /// delete; all four callers read these presentation colors live.
    /// </remarks>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
