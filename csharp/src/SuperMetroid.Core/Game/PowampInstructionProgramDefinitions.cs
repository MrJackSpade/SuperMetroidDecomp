namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Powamp's body and balloon instruction programs.
/// Spritemap selections resolve compiled identities to installed artwork.
/// </summary>
internal abstract class PowampInstructionProgramDefinitions
{
    /// <summary><c>InstList_Powamp_Body_FastAnimation</c> at $A8:C163.</summary>
    internal const ushort BodyFast = 0xc163;

    /// <summary><c>InstList_Powamp_Body_SlowAnimation</c> at $A8:C173.</summary>
    internal const ushort BodySlow = 0xc173;

    /// <summary><c>InstList_Powamp_Balloon_Inflate_0</c> at $A8:C183.</summary>
    internal const ushort BalloonInflate0 = 0xc183;

    /// <summary><c>InstList_Powamp_Balloon_Inflate_1</c> at $A8:C187.</summary>
    internal const ushort BalloonInflate1 = 0xc187;

    /// <summary><c>InstList_Powamp_Balloon_StartSinking</c> at $A8:C191.</summary>
    internal const ushort BalloonStartSinking = 0xc191;

    /// <summary><c>InstList_Powamp_Balloon_Deflated</c> at $A8:C199.</summary>
    internal const ushort BalloonDeflated = 0xc199;

    /// <summary>$A8:C163/C167/C16B: selected five-tick cheek exposure in the fast loop; retained drawn-performance choice.</summary>
    private const ushort FastBodyHold = 5;
    /// <summary>$A8:C173/C177/C17B: selected nine-tick cheek exposure in the slow loop; retained drawn-performance choice.</summary>
    private const ushort SlowBodyHold = 9;
    /// <summary>$A8:C183/C191: selected one-tick initial inflation/deflation exposure; retains pose-driven collision-center timing.</summary>
    private const ushort TransitionStartHold = 1;
    /// <summary>$A8:C187/C195: selected six-tick intermediate inflation/deflation exposure; retains pose-driven collision-center timing.</summary>
    private const ushort TransitionMiddleHold = 6;
    /// <summary>$A8:C18B/C199: selected160-tick terminal exposure before Sleep; retains exact interpreter timer bookkeeping.</summary>
    private const ushort BalloonHeldDuration = 160;
    // These five reviewed choices compose the scripted cheek/balloon performance.
    // The balloon cursor also controls physical Y alignment; these are not visual-only.
    // Independent AI transition timers, velocity, geometry and artwork are not exempt.
    /// <summary>Both body loops and both balloon transitions have three duration/visual pairs.</summary>
    private const int FramesPerProgram = 3;
    /// <summary>$A8:C163..C182: each body loop has three frame controls, Goto and its loop target.</summary>
    private const int BodyMechanicsCount = FramesPerProgram + 2;
    /// <summary>$A8:C183..C19E: each balloon transition has three frame controls and Sleep.</summary>
    private const int BalloonMechanicsCount = FramesPerProgram + 1;

    public static int MechanicsWordCount => 2 * (BodyMechanicsCount + BalloonMechanicsCount);
    public static int PresentationWordCount => 4 * FramesPerProgram;

    /// <summary>Native ordered controls calculated from the two loop programs followed by the two sleeping transitions.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool body = index < 2 * BodyMechanicsCount;
        int group = body ? index / BodyMechanicsCount : 2 + (index - 2 * BodyMechanicsCount) / BalloonMechanicsCount;
        int local = body ? index % BodyMechanicsCount : (index - 2 * BodyMechanicsCount) % BalloonMechanicsCount;
        ushort start = ProgramStart(group);
        int offset = local < FramesPerProgram ? local * 4 : FramesPerProgram * 4 + (local - FramesPerProgram) * 2;
        ushort value = local < FramesPerProgram
            ? body ? group == 0 ? FastBodyHold : SlowBodyHold
                : local == 0 ? TransitionStartHold : local == 1 ? TransitionMiddleHold : BalloonHeldDuration
            : body ? local == FramesPerProgram ? CommonEnemyInstructionCodes.Goto : start : CommonEnemyInstructionCodes.Sleep;
        return new((ushort)(start + offset), value);
    }

    /// <summary>Visual operands follow each of the three frame-duration words; native source order is preserved.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(ProgramStart(index / FramesPerProgram) + 4 * (index % FramesPerProgram) + sizeof(ushort));
    }

    /// <summary>$A8:C163/C173 body programs occupy16bytes each; C183/C191 balloon programs occupy14bytes each.</summary>
    private static ushort ProgramStart(int group) => group < 2
        ? (ushort)(BodyFast + group * (FramesPerProgram * 4 + 4))
        : (ushort)(BalloonInflate0 + (group - 2) * (FramesPerProgram * 4 + 2));

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Powamp instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }
}
