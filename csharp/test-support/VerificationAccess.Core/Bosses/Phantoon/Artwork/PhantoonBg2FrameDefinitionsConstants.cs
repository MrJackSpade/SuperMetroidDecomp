using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>Cartridge values of <see cref="AssetExtraction.Bosses.PhantoonBg2FrameDefinitions"/> that only verification reads.</summary>
internal static class PhantoonBg2FrameDefinitionsConstants
{
    /// <summary>VRAM destination base for Phantoon's BG2 frame tiles.</summary>
    public const ushort VramBase = EnemyBg2FrameLayoutConstants.VramBase;
    /// <summary>WRAM staging base from which Phantoon's BG2 frame upload is copied.</summary>
    public const ushort WorkingRamBase = EnemyBg2FrameLayoutConstants.WorkingRamBase;

    /// <summary>$A7:DEDD, invulnerable body without active collision rectangles.</summary>
    internal static ushort InvulnerableBody => PhantoonBg2FrameDefinitionsConstants.Frame(0).Pointer;
    /// <summary>$A7:DEFB, the closed eyelid frame.</summary>
    internal static ushort ClosedEye => PhantoonBg2FrameDefinitionsConstants.Frame(3).Pointer;
    /// <summary>$A7:DF23, the centered pupil frame.</summary>
    internal static ushort CenteredEye => PhantoonBg2FrameDefinitionsConstants.Frame(7).Pointer;
    /// <summary>$A7:DF05/DF0F/DF19, three progressively opening eyelid frames.</summary>
    internal static ushort OpeningEye(int phase) => (uint)phase < 3 ? PhantoonBg2FrameDefinitionsConstants.Frame(4 + phase).Pointer : throw new ArgumentOutOfRangeException(nameof(phase));
    /// <summary>$A7:DF2D..DF73 are named compass pupil placements, selected by CC A7..CC D1 gaze programs.</summary>
    internal static ushort Gaze(PhantoonGazeDirection direction) => PhantoonBg2FrameDefinitionsConstants.Frame(direction switch
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
    internal static ushort TentaclePose(int phase) => (uint)phase < 3 ? PhantoonBg2FrameDefinitionsConstants.Frame(16 + phase).Pointer : throw new ArgumentOutOfRangeException(nameof(phase));
    /// <summary>$A7:DFE9/DFF3/DFFD, the three mouth poses from rest to flame release.</summary>
    internal static ushort MouthPose(int phase) => (uint)phase < 3 ? PhantoonBg2FrameDefinitionsConstants.Frame(19 + phase).Pointer : throw new ArgumentOutOfRangeException(nameof(phase));
    /// <summary>Phantoon's extended frames and BG2 streams reside in bank $A7.</summary>
    internal const byte Bank = 0xa7;
    /// <summary>$A7:DEDD, first one-component body/eye frame; sixteen selected roots.</summary>
    internal const ushort BodyEyeStart = 0xdedd;
    /// <summary>$A7:DFB3, first two-component tentacle frame; three selected roots.</summary>
    internal const ushort TentaclesStart = 0xdfb3;
    /// <summary>$A7:DFE9, first one-component mouth frame; three selected roots.</summary>
    internal const ushort MouthStart = 0xdfe9;
    /// <summary>Number of body, eye, tentacle, and mouth frames addressed by the instruction programs.</summary>
    internal const int FrameCount = 22;
    /// <summary>Sixteen body/eye frames at ten-byte stride, three tentacle frames
    /// at eighteen-byte stride, then three mouth frames at ten-byte stride. The
    /// count word plus eight bytes per component explains both record sizes.</summary>
    internal static SpritemapFrameDefinition Frame(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        ushort pointer = (ushort)(index < 16 ? BodyEyeStart + 10 * index
            : index < 19 ? TentaclesStart + 18 * (index - 16)
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
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        return new(pointer, name);
    }
    /// <summary>Checks whether a pointer names one of the body, tentacle, or mouth frames in this catalog.</summary>
    internal static bool IsFrame(ushort pointer) => InRun(pointer, BodyEyeStart, 16, 10) ||
        InRun(pointer, TentaclesStart, 3, 18) || InRun(pointer, MouthStart, 3, 10);
    /// <summary>Checks that a pointer falls exactly on a fixed-size native frame boundary.</summary>
    internal static bool InRun(ushort pointer, ushort start, int count, int stride)
    {
        int offset = pointer - start;
        return offset >= 0 && offset < count * stride && offset % stride == 0;
    }
}
