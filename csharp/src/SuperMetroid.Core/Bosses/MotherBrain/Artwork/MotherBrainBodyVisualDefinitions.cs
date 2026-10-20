using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Complete body-frame roots selected by the compiled Mother Brain locomotion,
/// posture, initial-dummy and hand-beam programs. Native roots interleave OAM
/// limbs and BG2 body streams; their hitboxes never enter either visual asset.
/// </summary>
internal static class MotherBrainBodyVisualDefinitions
{
    /// <summary>Mother Brain extended-frame bank $A9.</summary>
    internal const byte Bank = 0xa9;
    /// <summary>Largest native root: ten components, including two BG2 streams.</summary>
    internal const int MaximumNativeComponents = 10;
    /// <summary>Standing, walking, crouch, dummy, and death-beam body roots selected by compiled programs.</summary>
    internal const int FrameCount = 17;
    /// <summary>Every selected body root except the OAM-only initial dummy.</summary>
    internal const int Bg2FrameCount = FrameCount - 1;
    /// <summary>Schema revision of the selected BG2 body-frame document.</summary>
    internal const int Bg2Version = 1;
    /// <summary>Installed JSON filename for Mother Brain's selected BG2 body frames.</summary>
    internal const string Bg2FileName = "mother-brain-body-bg2-frames.json";

    /// <summary>$A9:9FA0, standing frame with nine components.</summary>
    private const ushort Standing = 0x9fa0;
    /// <summary>$A9:9FEA, first of three ten-component walking frames.</summary>
    private const ushort WalkingStart = 0x9fea;
    /// <summary>$A9:A0E0, five nine-component walking frames followed by crouched.</summary>
    private const ushort LaterWalkingStart = 0xa0e0;
    /// <summary>$A9:A28C, uncrouching and leaning-down nine-component frames.</summary>
    private const ushort Uncrouching = 0xa28c;
    /// <summary>$A9:A384, first of four nine-component death-beam frames.</summary>
    private const ushort DeathBeamStart = 0xa384;
    /// <summary>Ordered BG2 frames projected from the selected body-root catalog.</summary>
    internal static EnemyBg2FrameDefinitionSequence Bg2Frames => new(Bg2FrameCount, Bg2Frame);

    /// <summary>Calculates the published OAM identity. Native record strides are
    /// two header bytes plus eight bytes per component; explicit run boundaries
    /// preserve the shorter crouched/dummy frames and skipped unselected roots.</summary>
    internal static EnemySpritemapDefinition Frame(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        ushort pointer = (ushort)(index == 0 ? Standing
            : index < 4 ? WalkingStart + 0x52 * (index - 1)
            : index < 10 ? LaterWalkingStart + 0x4a * (index - 4)
            : index < 13 ? Uncrouching + 0x4a * (index - 10)
            : DeathBeamStart + 0x4a * (index - 13));
        string pose = index switch
        {
            0 => "standing",
            >= 1 and <= 8 => FormattableString.Invariant($"walking_{index - 1}"),
            9 => "crouched",
            10 => "uncrouching",
            11 => "leaning_down",
            12 => "initial_dummy",
            _ => FormattableString.Invariant($"death_beam_{index - 13}"),
        };
        return new(Bank, pointer, $"mother_brain_body_oam_{pose}");
    }

    /// <summary>BG2 uses the same pose identity with its published BG2 prefix,
    /// omitting the OAM-only dummy at OAM index twelve.</summary>
    internal static SpritemapFrameDefinition Bg2Frame(int index)
    {
        if ((uint)index >= Bg2FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        var frame = Frame(index < 12 ? index : index + 1);
        return new(frame.Pointer, frame.Name.Replace("_oam_", "_bg2_", StringComparison.Ordinal));
    }

    /// <summary>Recognizes a selected body root with a BG2 stream, excluding the OAM-only dummy.</summary>
    internal static bool HasBg2(ushort pointer) => pointer == Standing ||
        InRun(pointer, WalkingStart, 3, 0x52) || InRun(pointer, LaterWalkingStart, 6, 0x4a) ||
        InRun(pointer, Uncrouching, 2, 0x4a) || InRun(pointer, DeathBeamStart, 4, 0x4a);

    /// <summary>Recognizes a native record origin within a fixed-stride body-frame run.</summary>
    private static bool InRun(ushort pointer, ushort start, int count, int stride)
    {
        int offset = pointer - start;
        return offset >= 0 && offset < count * stride && offset % stride == 0;
    }

    /// <summary>The installed frame each <see cref="MotherBrainBodyFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainBodyFrame> ShownFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.MotherBrainBodyOamStanding },
        { new(Index: 1), EnemyFrameId.MotherBrainBodyOamWalking0 },
        { new(Index: 10), EnemyFrameId.MotherBrainBodyOamUncrouching },
        { new(Index: 11), EnemyFrameId.MotherBrainBodyOamLeaningDown },
        { new(Index: 12), EnemyFrameId.MotherBrainBodyOamInitialDummy },
        { new(Index: 13), EnemyFrameId.MotherBrainBodyOamDeathBeam0 },
        { new(Index: 14), EnemyFrameId.MotherBrainBodyOamDeathBeam1 },
        { new(Index: 15), EnemyFrameId.MotherBrainBodyOamDeathBeam2 },
        { new(Index: 16), EnemyFrameId.MotherBrainBodyOamDeathBeam3 },
        { new(Index: 2), EnemyFrameId.MotherBrainBodyOamWalking1 },
        { new(Index: 3), EnemyFrameId.MotherBrainBodyOamWalking2 },
        { new(Index: 4), EnemyFrameId.MotherBrainBodyOamWalking3 },
        { new(Index: 5), EnemyFrameId.MotherBrainBodyOamWalking4 },
        { new(Index: 6), EnemyFrameId.MotherBrainBodyOamWalking5 },
        { new(Index: 7), EnemyFrameId.MotherBrainBodyOamWalking6 },
        { new(Index: 8), EnemyFrameId.MotherBrainBodyOamWalking7 },
        { new(Index: 9), EnemyFrameId.MotherBrainBodyOamCrouched },
    };
}
