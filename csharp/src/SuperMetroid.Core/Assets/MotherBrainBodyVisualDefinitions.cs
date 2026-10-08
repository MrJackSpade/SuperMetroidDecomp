namespace SuperMetroid.Core.Assets;

/// <summary>
/// Complete body-frame roots selected by the compiled Mother Brain locomotion,
/// posture, initial-dummy and hand-beam programs. Native roots interleave OAM
/// limbs and BG2 body streams; their hitboxes never enter either visual asset.
/// </summary>
internal static class MotherBrainBodyVisualDefinitions
{
    /// <summary>Mother Brain extended-frame bank $A9.</summary>
    internal const byte Bank = 0xa9;
    /// <summary>$A9:A320, the initial dummy root containing only ordinary OAM.</summary>
    internal const ushort InitialDummyFrame = 0xa320;
    /// <summary>Largest native root: ten components, including two BG2 streams.</summary>
    internal const int MaximumNativeComponents = 10;
    internal const int FrameCount = 17;
    internal const int Bg2FrameCount = FrameCount - 1;
    internal const int Bg2Version = 1;
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
    internal static EnemyBg2FrameDefinitionSequence Bg2Frames => new(Bg2FrameCount, Bg2Frame);

    /// <summary>Calculates the published OAM identity. Native record strides are
    /// two header bytes plus eight bytes per component; explicit run boundaries
    /// preserve the shorter crouched/dummy frames and skipped unselected roots.</summary>
    internal static EnemyExtendedFrameDefinition Frame(int index)
    {
        if ((uint)index >= FrameCount) throw new IndexOutOfRangeException();
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
    internal static EnemyBg2FrameDefinition Bg2Frame(int index)
    {
        if ((uint)index >= Bg2FrameCount) throw new IndexOutOfRangeException();
        var frame = Frame(index < 12 ? index : index + 1);
        return new(frame.Pointer, frame.Name.Replace("_oam_", "_bg2_", StringComparison.Ordinal));
    }

    internal static bool HasBg2(ushort pointer) => pointer == Standing ||
        InRun(pointer, WalkingStart, 3, 0x52) || InRun(pointer, LaterWalkingStart, 6, 0x4a) ||
        InRun(pointer, Uncrouching, 2, 0x4a) || InRun(pointer, DeathBeamStart, 4, 0x4a);

    private static bool InRun(ushort pointer, ushort start, int count, int stride)
    {
        int offset = pointer - start;
        return offset >= 0 && offset < count * stride && offset % stride == 0;
    }
}
