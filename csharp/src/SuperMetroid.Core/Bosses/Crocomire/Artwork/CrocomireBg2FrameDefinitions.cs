using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

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
        new(EnemyFrameId.CrocomireBody0, "crocomire_body_bg2_BFC4"),
        new(EnemyFrameId.CrocomireBody1, "crocomire_body_bg2_BFF6"),
        new(EnemyFrameId.CrocomireBody2, "crocomire_body_bg2_C028"),
        new(EnemyFrameId.CrocomireBody3, "crocomire_body_bg2_C05A"),
        new(EnemyFrameId.CrocomireBody4, "crocomire_body_bg2_C08C"),
        new(EnemyFrameId.CrocomireBody5, "crocomire_body_bg2_C0BE"),
        new(EnemyFrameId.CrocomireBody6, "crocomire_body_bg2_C0F0"),
        new(EnemyFrameId.CrocomireBody7, "crocomire_body_bg2_C122"),
        new(EnemyFrameId.CrocomireBody8, "crocomire_body_bg2_C154"),
        new(EnemyFrameId.CrocomireBody9, "crocomire_body_bg2_C186"),
        new(EnemyFrameId.CrocomireBody10, "crocomire_body_bg2_C1B8"),
        new(EnemyFrameId.CrocomireBody11, "crocomire_body_bg2_C1EA"),
        new(EnemyFrameId.CrocomireBody12, "crocomire_body_bg2_C2EC"),
        new(EnemyFrameId.CrocomireBody13, "crocomire_body_bg2_C326"),
        new(EnemyFrameId.CrocomireBody14, "crocomire_body_bg2_C360"),
        new(EnemyFrameId.CrocomireBody15, "crocomire_body_bg2_C39A"),
        new(EnemyFrameId.CrocomireBody16, "crocomire_body_bg2_C3D4"),
        new(EnemyFrameId.CrocomireBody17, "crocomire_body_bg2_C40E"),
        new(EnemyFrameId.CrocomireBody18, "crocomire_body_bg2_C448"),
        new(EnemyFrameId.CrocomireBody19, "crocomire_body_bg2_C47A"),
        new(EnemyFrameId.CrocomireBody20, "crocomire_body_bg2_C4AC"),
        new(EnemyFrameId.CrocomireBody21, "crocomire_body_bg2_C4DE"),
        new(EnemyFrameId.CrocomireBody22, "crocomire_body_bg2_C510"),
        new(EnemyFrameId.CrocomireBody23, "crocomire_body_bg2_C542"),
        new(EnemyFrameId.CrocomireBody24, "crocomire_body_bg2_C574"),
        new(EnemyFrameId.CrocomireBody25, "crocomire_body_bg2_C5AE"),
        new(EnemyFrameId.CrocomireBody26, "crocomire_body_bg2_C5E8"),
        new(EnemyFrameId.CrocomireBody27, "crocomire_body_bg2_C6A4"),
        new(EnemyFrameId.CrocomireBody28, "crocomire_body_bg2_C752"),
        new(EnemyFrameId.CrocomireBody29, "crocomire_body_bg2_C78C"),
        new(EnemyFrameId.CrocomireBody30, "crocomire_body_bg2_C7C6"),
        new(EnemyFrameId.CrocomireBody31, "crocomire_body_bg2_C800"),
        new(EnemyFrameId.CrocomireBody32, "crocomire_body_bg2_C83A"),
        new(EnemyFrameId.CrocomireBody33, "crocomire_body_bg2_C874"),
        new(EnemyFrameId.CrocomireBody34, "crocomire_body_bg2_C8AE"),
        new(EnemyFrameId.CrocomireBody35, "crocomire_body_bg2_C8E8"),
        new(EnemyFrameId.CrocomireBody36, "crocomire_body_bg2_C922"),
        new(EnemyFrameId.CrocomireBody37, "crocomire_body_bg2_C95C"),
        new(EnemyFrameId.CrocomireBody38, "crocomire_body_bg2_C996"),
        new(EnemyFrameId.CrocomireBody39, "crocomire_body_bg2_C9D0"),
        new(EnemyFrameId.CrocomireBody40, "crocomire_body_bg2_CA0A"),
        new(EnemyFrameId.CrocomireBody41, "crocomire_body_bg2_CA44"),
    ];

    /// <summary>Whether <paramref name="frame"/> is one of Crocomire's BG2 body frames.</summary>
    internal static bool IsFrame(EnemyFrameId frame) => Frames.Any(definition => definition.Frame == frame);
}
