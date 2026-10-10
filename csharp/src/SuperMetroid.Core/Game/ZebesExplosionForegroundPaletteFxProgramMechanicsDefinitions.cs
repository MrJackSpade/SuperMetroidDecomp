using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the wide foreground part of the Zebes explosion.</summary>
/// <remarks>
/// Issue #842 / #625: the pinned NTSC J/U v1.0 ROM starts definition
/// <c>$8D:E1C8</c> at <c>$8D:CB3C</c>: <c>SetColorIndex($0002)</c>, sixteen
/// 34-byte records of <c>duration, colors[15], Wait</c>, then <c>Delete</c>
/// at <c>$CD60</c>. Durations are 4 for frames 0..2, 60 for frame 3, and 6
/// for frames 4..15: 144 frames total. All 35 control words match. For each
/// frame <c>f</c> (0..15), exactly the first <c>min(f + 1, 15)</c> colors are
/// nonzero; the rest are black, matching all 240 ROM positions (135 nonzero,
/// 105 black). Frames 0..5 fill that prefix uniformly with, respectively,
/// <c>7C00,7CA0,7DE0,7DE0,7E80,7F20</c>. Later frames have authored accents:
/// frame 6 starts <c>7FFD,7FE9</c>, frame 14 ends <c>6B40</c>, and frame 15
/// ends <c>7FF7</c>. All 240 presentation words belong to a separate color entry;
/// this cadence conversion grants no color exception. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions
{

    /// <summary>The instruction-list entry at <c>$8D:CB3C</c>.</summary>
    public const ushort ProgramStart = 0xcb3c;

    /// <summary>The first timed record at <c>$8D:CB40</c>.</summary>
    public const ushort FirstFramePointer = ProgramStart + 2 * sizeof(ushort);

    /// <summary>The terminal <c>delete</c> command at <c>$8D:CD60</c>.</summary>
    public const ushort DeleteInstructionPointer = FirstFramePointer + FrameCount * FrameByteCount;

    /// <summary>The foreground explosion writes CGRAM from byte index <c>$0002</c>.</summary>
    public const ushort ColorByteIndex = 0x0002;

    /// <summary>The one-shot foreground explosion contains sixteen timed records.</summary>
    public const int FrameCount = 16;

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = (ColorsPerFrame + 2) * sizeof(ushort);

    /// <summary>The complete one-shot foreground explosion lasts 144 frames.</summary>
    public const int CycleFrames = EndingExplosionInstructionDefinitions.RightStarInitialHoldTicks;

    /// <summary>$8D:CB40-CBA5: three initial prefix-reveal records; authored reveal-to-hold boundary.</summary>
    private const int InitialRevealRecords = 3;
    /// <summary>$8D:CB40/CB62/CB84: selected four-tick initial reveal exposure.</summary>
    private const ushort RevealTicks = 4;
    /// <summary>$8D:CBA6: remaining exposure before the synchronized $8B:EB71 finale handoff.</summary>
    private const ushort CrestTicks = CycleFrames - InitialRevealRecords * RevealTicks -
        (FrameCount - InitialRevealRecords - 1) * ExpansionTicks;
    /// <summary>$8D:CBC8-CD3E: selected six-tick exposure of each remaining expansion record.</summary>
    private const ushort ExpansionTicks = 6;

    /// <summary>Exact initial reveal, fourth-prefix hold and expansion choreography; independent colors remain separate.</summary>
    internal static ushort Duration(int frame)
    {
        if ((uint)frame >= FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        return frame < InitialRevealRecords ? RevealTicks : frame == InitialRevealRecords ? CrestTicks : ExpansionTicks;
    }


    /// <summary>Returns one timed-record pointer.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one live BGR555 color word in a timed record.</summary>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            ProgramStart => (ushort)PaletteFxInstruction.SetColorIndex,
            ProgramStart + 2 => ColorByteIndex,
            DeleteInstructionPointer => (ushort)PaletteFxInstruction.Delete,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0; frame < FrameCount; frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => Duration(frame),
                FrameByteCount - sizeof(ushort) => (ushort)PaletteFxInstruction.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
