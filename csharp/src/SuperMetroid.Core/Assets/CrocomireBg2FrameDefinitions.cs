namespace SuperMetroid.Core.Assets;

/// <summary>
/// Named BG2 stream identities for the 42 mixed OAM/BG2 Crocomire fight-body
/// frames. Their OAM components are authored in the extended-enemy catalog;
/// native hitbox pointers and the new-instruction-frame gate remain engine data.
/// </summary>
internal static class CrocomireBg2FrameDefinitions
{
    internal const int Version = 1;
    internal const string FileName = "crocomire-bg2-frames.json";
    internal const byte Bank = CrocomireBodyVisualDefinitions.Bank;

    private static readonly EnemyBg2FrameDefinition[] Definitions =
        CrocomireBodyVisualDefinitions.Frames.ToArray()
            .Where(CrocomireBodyVisualDefinitions.HasBg2)
            .Select(pointer => new EnemyBg2FrameDefinition(pointer,
                $"crocomire_body_bg2_{pointer:X4}"))
            .ToArray();

    internal static ReadOnlySpan<EnemyBg2FrameDefinition> Frames => Definitions;
}
