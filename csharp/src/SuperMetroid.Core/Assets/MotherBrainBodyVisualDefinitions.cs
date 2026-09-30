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

    private static readonly EnemyExtendedFrameDefinition[] FrameDefinitions =
    [
        new(Bank, 0x9fa0, "mother_brain_body_oam_standing"),
        new(Bank, 0x9fea, "mother_brain_body_oam_walking_0"),
        new(Bank, 0xa03c, "mother_brain_body_oam_walking_1"),
        new(Bank, 0xa08e, "mother_brain_body_oam_walking_2"),
        new(Bank, 0xa0e0, "mother_brain_body_oam_walking_3"),
        new(Bank, 0xa12a, "mother_brain_body_oam_walking_4"),
        new(Bank, 0xa174, "mother_brain_body_oam_walking_5"),
        new(Bank, 0xa1be, "mother_brain_body_oam_walking_6"),
        new(Bank, 0xa208, "mother_brain_body_oam_walking_7"),
        new(Bank, 0xa252, "mother_brain_body_oam_crouched"),
        new(Bank, 0xa28c, "mother_brain_body_oam_uncrouching"),
        new(Bank, 0xa2d6, "mother_brain_body_oam_leaning_down"),
        new(Bank, InitialDummyFrame, "mother_brain_body_oam_initial_dummy"),
        new(Bank, 0xa384, "mother_brain_body_oam_death_beam_0"),
        new(Bank, 0xa3ce, "mother_brain_body_oam_death_beam_1"),
        new(Bank, 0xa418, "mother_brain_body_oam_death_beam_2"),
        new(Bank, 0xa462, "mother_brain_body_oam_death_beam_3"),
    ];

    private static readonly EnemyBg2FrameDefinition[] Bg2Definitions = FrameDefinitions
        .Where(frame => frame.Pointer != InitialDummyFrame)
        .Select(frame => new EnemyBg2FrameDefinition(frame.Pointer,
            frame.Name.Replace("_oam_", "_bg2_", StringComparison.Ordinal))).ToArray();

    internal static ReadOnlySpan<EnemyExtendedFrameDefinition> Frames => FrameDefinitions;
    internal static ReadOnlySpan<EnemyBg2FrameDefinition> Bg2Frames => Bg2Definitions;

    internal static bool HasBg2(ushort pointer)
    {
        foreach (EnemyBg2FrameDefinition frame in Bg2Definitions)
            if (frame.Pointer == pointer) return true;
        return false;
    }
}
