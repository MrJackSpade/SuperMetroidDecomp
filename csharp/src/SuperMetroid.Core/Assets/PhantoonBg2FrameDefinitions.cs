namespace SuperMetroid.Core.Assets;

/// <summary>Eight mutually exclusive compass gaze directions selected at $A7:CCA7..CCD6.</summary>
internal enum PhantoonGazeDirection
{
    Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft,
}

/// <summary>
/// Phantoon's bank-$A7 extended frames selected by the compiled $CC41-$CCFB
/// instruction programs. These identities select BG2 tilemap writes, not OAM.
/// Component hitbox pointers and collision rectangles are engine definitions.
/// </summary>
internal static class PhantoonBg2FrameDefinitions
{
    internal const int Version = 1;
    internal const string FileName = "phantoon-bg2-frames.json";
    /// <summary>Phantoon's tentacle frames have at most two BG2 components.</summary>
    internal const int MaximumComponents = 2;
    /// <summary>Phantoon's extended frames and BG2 streams reside in bank $A7.</summary>
    internal const byte Bank = 0xa7;
    /// <summary>Body with its complete five-rectangle hitbox, $A7:DEE7.</summary>
    internal const ushort BodyFullHitbox = 0xdee7;
    /// <summary>Body with only the vulnerable eye hitbox, $A7:DEF1.</summary>
    internal const ushort BodyEyeHitboxOnly = 0xdef1;
    /// <summary>First two-component tentacle frame, $A7:DFB3.</summary>
    internal const ushort Tentacles0 = 0xdfb3;
    /// <summary>Second two-component tentacle frame, $A7:DFC5.</summary>
    internal const ushort Tentacles1 = 0xdfc5;
    /// <summary>Third two-component tentacle frame, $A7:DFD7.</summary>
    internal const ushort Tentacles2 = 0xdfd7;

    /// <summary>$A7:DEDD, first one-component body/eye frame; sixteen selected roots.</summary>
    private const ushort BodyEyeStart = 0xdedd;
    /// <summary>$A7:DFE9, first one-component mouth frame; three selected roots.</summary>
    private const ushort MouthStart = 0xdfe9;
    internal const int FrameCount = 22;

    internal static EnemyBg2FrameDefinitionSequence Frames => new(FrameCount, Frame);

    /// <summary>Sixteen body/eye frames at ten-byte stride, three tentacle frames
    /// at eighteen-byte stride, then three mouth frames at ten-byte stride. The
    /// count word plus eight bytes per component explains both record sizes.</summary>
    internal static EnemyBg2FrameDefinition Frame(int index)
    {
        if ((uint)index >= FrameCount) throw new IndexOutOfRangeException();
        ushort pointer = (ushort)(index < 16 ? BodyEyeStart + 10 * index
            : index < 19 ? Tentacles0 + 18 * (index - 16)
            : MouthStart + 10 * (index - 19));
        string name = index switch
        {
            0 => "body_invulnerable",
            1 => "body_full_hitbox",
            2 => "body_eye_hitbox_only",
            3 => "eye_closed",
            4 => "eye_open_0",
            5 => "eye_open_1",
            6 => "eye_open_2",
            7 => "eyeball_center",
            8 => "eyeball_up",
            9 => "eyeball_down",
            10 => "eyeball_left",
            11 => "eyeball_right",
            12 => "eyeball_down_left",
            13 => "eyeball_down_right",
            14 => "eyeball_up_left",
            15 => "eyeball_up_right",
            16 => "tentacles_0",
            17 => "tentacles_1",
            18 => "tentacles_2",
            19 => "mouth_0",
            20 => "mouth_1",
            21 => "mouth_2",
            _ => throw new IndexOutOfRangeException(),
        };
        return new(pointer, name);
    }

    /// <summary>$A7:DEDD, invulnerable body without active collision rectangles.</summary>
    internal static ushort InvulnerableBody => Frame(0).Pointer;
    /// <summary>$A7:DEFB, the closed eyelid frame.</summary>
    internal static ushort ClosedEye => Frame(3).Pointer;
    /// <summary>$A7:DF23, the centered pupil frame.</summary>
    internal static ushort CenteredEye => Frame(7).Pointer;
    /// <summary>$A7:DF05/DF0F/DF19, three progressively opening eyelid frames.</summary>
    internal static ushort OpeningEye(int phase) => (uint)phase < 3 ? Frame(4 + phase).Pointer : throw new ArgumentOutOfRangeException(nameof(phase));
    /// <summary>$A7:DF2D..DF73 are named compass pupil placements, selected by CC A7..CC D1 gaze programs.</summary>
    internal static ushort Gaze(PhantoonGazeDirection direction) => Frame(direction switch
    {
        PhantoonGazeDirection.Up => 8,
        PhantoonGazeDirection.UpRight => 15,
        PhantoonGazeDirection.Right => 11,
        PhantoonGazeDirection.DownRight => 13,
        PhantoonGazeDirection.Down => 9,
        PhantoonGazeDirection.DownLeft => 12,
        PhantoonGazeDirection.Left => 10,
        PhantoonGazeDirection.UpLeft => 14,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    }).Pointer;
    /// <summary>$A7:DFB3/DFC5/DFD7, the three two-component tentacle poses.</summary>
    internal static ushort TentaclePose(int phase) => (uint)phase < 3 ? Frame(16 + phase).Pointer : throw new ArgumentOutOfRangeException(nameof(phase));
    /// <summary>$A7:DFE9/DFF3/DFFD, the three mouth poses from rest to flame release.</summary>
    internal static ushort MouthPose(int phase) => (uint)phase < 3 ? Frame(19 + phase).Pointer : throw new ArgumentOutOfRangeException(nameof(phase));
    internal static bool IsFrame(ushort pointer) => InRun(pointer, BodyEyeStart, 16, 10) ||
        InRun(pointer, Tentacles0, 3, 18) || InRun(pointer, MouthStart, 3, 10);

    private static bool InRun(ushort pointer, ushort start, int count, int stride)
    {
        int offset = pointer - start;
        return offset >= 0 && offset < count * stride && offset % stride == 0;
    }
}