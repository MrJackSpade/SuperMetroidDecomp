using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing, control flow, and callback operands for Phantoon's four enemy
/// records. Physical frame selectors are compiled separately; installed display
/// bindings may replace their BG2/OAM art without changing mechanics or timing.
/// </summary>
internal abstract class PhantoonInstructionProgramDefinitions
{
    /// <summary><c>InstList_Phantoon_Body_Invulnerable</c> at $A7:CC41.</summary>
    public const ushort InvulnerableBody = 0xcc41;
    /// <summary><c>InstList_Phantoon_Body_FullHitbox</c> at $A7:CC47.</summary>
    public const ushort FullHitboxBody = 0xcc47;
    /// <summary><c>InstList_Phantoon_Body_EyeHitboxOnly</c> at $A7:CC4D.</summary>
    public const ushort EyeHitboxBody = 0xcc4d;
    /// <summary><c>InstList_Phantoon_Eye_Open</c> at $A7:CC53.</summary>
    public const ushort EyeOpen = 0xcc53;
    /// <summary><c>InstList_Phantoon_Eye_Closed</c> at $A7:CC7B.</summary>
    public const ushort EyeClosed = 0xcc7b;
    /// <summary><c>InstList_Phantoon_Eye_Close_PickNewPattern</c> at $A7:CC81.</summary>
    public const ushort EyeCloseAndPickNewPattern = 0xcc81;
    /// <summary><c>InstList_Phantoon_Eye_Close</c> at $A7:CC91.</summary>
    public const ushort EyeClose = 0xcc91;
    /// <summary><c>InstList_Phantoon_Eyeball_Centered</c> at $A7:CC9D.</summary>
    public const ushort EyeballCentered = 0xcc9d;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingUp</c> at $A7:CCA7.</summary>
    public const ushort EyeLookingUp = 0xcca7;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingUpRight</c> at $A7:CCAD.</summary>
    public const ushort EyeLookingUpRight = 0xccad;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingRight</c> at $A7:CCB3.</summary>
    public const ushort EyeLookingRight = 0xccb3;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingDownRight</c> at $A7:CCB9.</summary>
    public const ushort EyeLookingDownRight = 0xccb9;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingDown</c> at $A7:CCBF.</summary>
    public const ushort EyeLookingDown = 0xccbf;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingDownLeft</c> at $A7:CCC5.</summary>
    public const ushort EyeLookingDownLeft = 0xccc5;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingLeft</c> at $A7:CCCB.</summary>
    public const ushort EyeLookingLeft = 0xcccb;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingUpLeft</c> at $A7:CCD1.</summary>
    public const ushort EyeLookingUpLeft = 0xccd1;
    /// <summary><c>InstList_Phantoon_Tentacles</c> at $A7:CCD7.</summary>
    public const ushort InitialTentacles = 0xccd7;
    /// <summary><c>InstList_Phantoon_Mouth_SpawnFlame</c> at $A7:CCEB.</summary>
    public const ushort MouthFollowUp = 0xcceb;
    /// <summary><c>InstList_Phantoon_Mouth_Initial</c> at $A7:CCF7.</summary>
    public const ushort InitialMouth = 0xccf7;

    /// <summary>Eye-transition dwell at A7:CC53/CC57/CC85/CC95. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort EyeTransitionFrames = 10;
    /// <summary>Four-pose tentacle cadence at A7:CCD7..CCE3. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort TentaclePoseFrames = 8;
    /// <summary>Mouth preparation dwell at A7:CCEB/CCEF. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort MouthPreparationFrames = 5;

    public static int MechanicsWordCount => 58;
    public static int PresentationWordCount => 27;

    /// <summary>Calculates control positions from named sleep, transition, callback and loop layouts.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 6) return SleepWord((ushort)(InvulnerableBody + index / 2 * 6), index % 2);
        if (index < 14)
        {
            int word = index - 6;
            if (word < 3) return Frame(EyeOpen, word, word < 2 ? EyeTransitionFrames : (ushort)1);
            return new((ushort)(EyeOpen + 12 + (word - 3) * 2), word switch
            {
                3 or 5 => EnemyInstructionCodePointers.Instruction_CommonA7_CallFunctionInY,
                4 => (ushort)PhantoonInstruction.PlayPhantoonMaterializationSFX,
                6 => (ushort)PhantoonInstruction.SetupEyeOpenPhantoonState,
                _ => CommonEnemyInstructionCodes.Sleep,
            });
        }
        if (index < 16) return SleepWord(EyeClosed, index - 14);
        if (index < 22) return CloseWord(EyeCloseAndPickNewPattern, index - 16, true);
        if (index < 26) return CloseWord(EyeClose, index - 22, false);
        if (index < 30)
        {
            int word = index - 26;
            return word == 0 ? Frame(EyeballCentered, 0, 1)
                : new((ushort)(EyeballCentered + 4 + (word - 1) * 2), word switch
                {
                    1 => EnemyInstructionCodePointers.Instruction_CommonA7_CallFunctionInY,
                    2 => (ushort)PhantoonInstruction.PlayPhantoonMaterializationSFX,
                    _ => CommonEnemyInstructionCodes.Sleep,
                });
        }
        if (index < 46) return SleepWord((ushort)(EyeLookingUp + (index - 30) / 2 * 6), index % 2);
        if (index < 52)
        {
            int word = index - 46;
            return word < 4 ? Frame(InitialTentacles, word, TentaclePoseFrames)
                : new((ushort)(InitialTentacles + 16 + (word - 4) * 2),
                    word == 4 ? CommonEnemyInstructionCodes.Goto : InitialTentacles);
        }
        if (index < 56)
        {
            int word = index - 52;
            return word < 2 ? Frame(MouthFollowUp, word, MouthPreparationFrames)
                : new((ushort)(MouthFollowUp + 8 + (word - 2) * 2),
                    word == 2 ? EnemyInstructionCodePointers.Instruction_CommonA7_CallFunctionInY
                        : (ushort)PhantoonInstruction.SpawnCasualFlame);
        }
        return SleepWord(InitialMouth, index - 56);
    }

    private static InstructionMechanicsWord Frame(ushort start, int frame, ushort duration) =>
        new((ushort)(start + frame * 4), duration);

    private static InstructionMechanicsWord SleepWord(ushort start, int word) =>
        new((ushort)(start + word * 4), word == 0 ? (ushort)1 : CommonEnemyInstructionCodes.Sleep);

    private static InstructionMechanicsWord CloseWord(ushort start, int word, bool pickPattern)
    {
        if (word < 2) return Frame(start, word, word == 0 ? (ushort)1 : EyeTransitionFrames);
        int command = word - 2;
        ushort value;
        if (pickPattern && command < 2)
            value = command == 0 ? EnemyInstructionCodePointers.Instruction_CommonA7_CallFunctionInY
                : (ushort)PhantoonInstruction.PickNewPhantoonPattern;
        else
            value = command == (pickPattern ? 2 : 0) ? CommonEnemyInstructionCodes.Goto : EyeClosed;
        return new((ushort)(start + 8 + command * 2), value);
    }

    /// <summary>Only four-byte pose records contribute visual operands; control commands leave gaps.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 3) return (ushort)(InvulnerableBody + index * 6 + 2);
        if (index < 6) return (ushort)(EyeOpen + (index - 3) * 4 + 2);
        if (index == 6) return EyeClosed + 2;
        if (index < 9) return (ushort)(EyeCloseAndPickNewPattern + (index - 7) * 4 + 2);
        if (index < 11) return (ushort)(EyeClose + (index - 9) * 4 + 2);
        if (index == 11) return EyeballCentered + 2;
        if (index < 20) return (ushort)(EyeLookingUp + (index - 12) * 6 + 2);
        if (index < 24) return (ushort)(InitialTentacles + (index - 20) * 4 + 2);
        if (index < 26) return (ushort)(MouthFollowUp + (index - 24) * 4 + 2);
        return InitialMouth + 2;
    }

    /// <summary>
    /// $A7:CC43..CCF9 visual operands select named body modes, opening/retracting
    /// eyelids, compass gaze, mirrored tentacles and mouth release/recovery poses.
    /// Pose holds are authored animation cadence reviewed under #1165; the artwork is installed presentation.
    /// </summary>
    internal static ushort PresentationFrame(int index)
    {
        _ = PresentationWordAddress(index);
        if (index < 3) return index switch
        {
            0 => PhantoonBg2FrameDefinitions.InvulnerableBody,
            1 => (ushort)PhantoonCollisionFrame.BodyFullHitbox,
            _ => (ushort)PhantoonCollisionFrame.BodyEyeHitboxOnly,
        };
        if (index < 6) return PhantoonBg2FrameDefinitions.OpeningEye(index - 3);
        if (index == 6) return PhantoonBg2FrameDefinitions.ClosedEye;
        if (index < 11) return PhantoonBg2FrameDefinitions.OpeningEye(2 - (index - 7) % 2);
        if (index == 11) return PhantoonBg2FrameDefinitions.CenteredEye;
        if (index < 20) return PhantoonBg2FrameDefinitions.Gaze((PhantoonGazeDirection)(index - 12));
        if (index < 24) return PhantoonBg2FrameDefinitions.TentaclePose(2 - Math.Abs(index - 22));
        if (index < 26) return PhantoonBg2FrameDefinitions.MouthPose(26 - index);
        return PhantoonBg2FrameDefinitions.MouthPose(0);
    }

    internal static bool IsPresentationWord(ushort address) => PresentationIndex(address) >= 0;
    internal static ushort FrameAt(ushort address)
    {
        int index = PresentationIndex(address);
        return index >= 0 ? PresentationFrame(index)
            : throw new InvalidDataException($"Phantoon visual operand $A7:{address:X4} is not compiled.");
    }
    private static int PresentationIndex(ushort address)
    {
        int low = 0, high = PresentationWordCount - 1;
        while (low <= high)
        {
            int middle = low + (high - low) / 2;
            ushort candidate = PresentationWordAddress(middle);
            if (address == candidate) return middle;
            if (address < candidate) high = middle - 1; else low = middle + 1;
        }
        return -1;
    }
    /// <summary>Returns fixed Phantoon control data or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (address == candidate.Address)
                return candidate.Value;
            if (address < candidate.Address)
                high = middle - 1;
            else
                low = middle + 1;
        }

        throw new InvalidDataException(
            $"Phantoon instruction mechanics pointer $A7:{address:X4} is not compiled.");
    }
}
