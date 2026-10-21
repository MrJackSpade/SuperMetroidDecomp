using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Named BG2 stream identities for the 42 mixed OAM/BG2 Crocomire fight-body
/// frames. Their OAM components are authored in the extended-enemy catalog;
/// native hitbox pointers and the new-instruction-frame gate remain engine data.
/// </summary>
internal static class CrocomireBg2FrameDefinitions
{
    /// <summary>Schema version for the selected mixed-frame BG2 artwork file.</summary>
    internal const int Version = 1;
    /// <summary>Installed BG2 visual resource for Crocomire's mixed fight frames.</summary>
    internal const string FileName = "crocomire-bg2-frames.json";

    /// <summary>The 42 body frames that carry BG2 components, in the compiled frame order.</summary>
    internal static ImmutableArray<EnemySpritemapDefinition> Frames { get; } =
    [
        new(EnemyFrameId.CrocomireBodyOamBFC4, "crocomire_body_bg2_BFC4"),
        new(EnemyFrameId.CrocomireBodyOamBFF6, "crocomire_body_bg2_BFF6"),
        new(EnemyFrameId.CrocomireBodyOamC028, "crocomire_body_bg2_C028"),
        new(EnemyFrameId.CrocomireBodyOamC05A, "crocomire_body_bg2_C05A"),
        new(EnemyFrameId.CrocomireBodyOamC08C, "crocomire_body_bg2_C08C"),
        new(EnemyFrameId.CrocomireBodyOamC0BE, "crocomire_body_bg2_C0BE"),
        new(EnemyFrameId.CrocomireBodyOamC0F0, "crocomire_body_bg2_C0F0"),
        new(EnemyFrameId.CrocomireBodyOamC122, "crocomire_body_bg2_C122"),
        new(EnemyFrameId.CrocomireBodyOamC154, "crocomire_body_bg2_C154"),
        new(EnemyFrameId.CrocomireBodyOamC186, "crocomire_body_bg2_C186"),
        new(EnemyFrameId.CrocomireBodyOamC1B8, "crocomire_body_bg2_C1B8"),
        new(EnemyFrameId.CrocomireBodyOamC1EA, "crocomire_body_bg2_C1EA"),
        new(EnemyFrameId.CrocomireBodyOamC2EC, "crocomire_body_bg2_C2EC"),
        new(EnemyFrameId.CrocomireBodyOamC326, "crocomire_body_bg2_C326"),
        new(EnemyFrameId.CrocomireBodyOamC360, "crocomire_body_bg2_C360"),
        new(EnemyFrameId.CrocomireBodyOamC39A, "crocomire_body_bg2_C39A"),
        new(EnemyFrameId.CrocomireBodyOamC3D4, "crocomire_body_bg2_C3D4"),
        new(EnemyFrameId.CrocomireBodyOamC40E, "crocomire_body_bg2_C40E"),
        new(EnemyFrameId.CrocomireBodyOamC448, "crocomire_body_bg2_C448"),
        new(EnemyFrameId.CrocomireBodyOamC47A, "crocomire_body_bg2_C47A"),
        new(EnemyFrameId.CrocomireBodyOamC4AC, "crocomire_body_bg2_C4AC"),
        new(EnemyFrameId.CrocomireBodyOamC4DE, "crocomire_body_bg2_C4DE"),
        new(EnemyFrameId.CrocomireBodyOamC510, "crocomire_body_bg2_C510"),
        new(EnemyFrameId.CrocomireBodyOamC542, "crocomire_body_bg2_C542"),
        new(EnemyFrameId.CrocomireBodyOamC574, "crocomire_body_bg2_C574"),
        new(EnemyFrameId.CrocomireBodyOamC5AE, "crocomire_body_bg2_C5AE"),
        new(EnemyFrameId.CrocomireBodyOamC5E8, "crocomire_body_bg2_C5E8"),
        new(EnemyFrameId.CrocomireBodyOamC6A4, "crocomire_body_bg2_C6A4"),
        new(EnemyFrameId.CrocomireBodyOamC752, "crocomire_body_bg2_C752"),
        new(EnemyFrameId.CrocomireBodyOamC78C, "crocomire_body_bg2_C78C"),
        new(EnemyFrameId.CrocomireBodyOamC7C6, "crocomire_body_bg2_C7C6"),
        new(EnemyFrameId.CrocomireBodyOamC800, "crocomire_body_bg2_C800"),
        new(EnemyFrameId.CrocomireBodyOamC83A, "crocomire_body_bg2_C83A"),
        new(EnemyFrameId.CrocomireBodyOamC874, "crocomire_body_bg2_C874"),
        new(EnemyFrameId.CrocomireBodyOamC8AE, "crocomire_body_bg2_C8AE"),
        new(EnemyFrameId.CrocomireBodyOamC8E8, "crocomire_body_bg2_C8E8"),
        new(EnemyFrameId.CrocomireBodyOamC922, "crocomire_body_bg2_C922"),
        new(EnemyFrameId.CrocomireBodyOamC95C, "crocomire_body_bg2_C95C"),
        new(EnemyFrameId.CrocomireBodyOamC996, "crocomire_body_bg2_C996"),
        new(EnemyFrameId.CrocomireBodyOamC9D0, "crocomire_body_bg2_C9D0"),
        new(EnemyFrameId.CrocomireBodyOamCA0A, "crocomire_body_bg2_CA0A"),
        new(EnemyFrameId.CrocomireBodyOamCA44, "crocomire_body_bg2_CA44"),
    ];

    /// <summary>Whether <paramref name="frame"/> is one of Crocomire's BG2 body frames.</summary>
    internal static bool IsFrame(EnemyFrameId frame) => Frames.Any(definition => definition.Frame == frame);
}
