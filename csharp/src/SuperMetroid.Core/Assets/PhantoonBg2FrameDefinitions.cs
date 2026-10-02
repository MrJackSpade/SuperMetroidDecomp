namespace SuperMetroid.Core.Assets;

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
    /// <summary>$A0:96CA identifies an extended BG2 command stream by this first word.</summary>
    internal const ushort StreamMarker = EnemyBg2FrameLayout.StreamMarker;
    internal const ushort VramBase = EnemyBg2FrameLayout.VramBase;
    internal const ushort WorkingRamBase = EnemyBg2FrameLayout.WorkingRamBase;
    internal const int TilemapWidth = EnemyBg2FrameLayout.TilemapWidth;
    internal const int TilemapHeight = EnemyBg2FrameLayout.TilemapHeight;
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

    internal static bool IsFrame(ushort pointer) => InRun(pointer, BodyEyeStart, 16, 10) ||
        InRun(pointer, Tentacles0, 3, 18) || InRun(pointer, MouthStart, 3, 10);

    private static bool InRun(ushort pointer, ushort start, int count, int stride)
    {
        int offset = pointer - start;
        return offset >= 0 && offset < count * stride && offset % stride == 0;
    }
}