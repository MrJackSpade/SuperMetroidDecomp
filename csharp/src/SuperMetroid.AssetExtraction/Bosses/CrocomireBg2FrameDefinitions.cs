using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Named background-layer pictures of the 42 Crocomire fight-body frames drawn
/// partly as sprites and partly on the background layer. Their sprite components
/// are authored among the extended frames; hitboxes and animation timing remain
/// gameplay data.
/// </summary>
internal static class CrocomireBg2FrameDefinitions
{
    /// <summary>Schema version for the selected mixed-frame BG2 artwork file; version 3 names each frame for its pose (#1465).</summary>
    internal const int Version = 3;
    /// <summary>Installed BG2 visual resource for Crocomire's mixed fight frames.</summary>
    internal const string FileName = "crocomire-bg2-frames.json";

    /// <summary>The 42 body frames that carry BG2 components, in the compiled frame order.</summary>
    internal static ImmutableArray<EnemySpritemapDefinition> Frames { get; } =
    [
        new(EnemyFrameId.CrocomireBody0, "crocomire_body_0"),
        new(EnemyFrameId.CrocomireBody1, "crocomire_body_1"),
        new(EnemyFrameId.CrocomireBody2, "crocomire_body_2"),
        new(EnemyFrameId.CrocomireBody3, "crocomire_body_3"),
        new(EnemyFrameId.CrocomireBody4, "crocomire_body_4"),
        new(EnemyFrameId.CrocomireBody5, "crocomire_body_5"),
        new(EnemyFrameId.CrocomireBody6, "crocomire_body_6"),
        new(EnemyFrameId.CrocomireBody7, "crocomire_body_7"),
        new(EnemyFrameId.CrocomireBody8, "crocomire_body_8"),
        new(EnemyFrameId.CrocomireBody9, "crocomire_body_9"),
        new(EnemyFrameId.CrocomireBody10, "crocomire_body_10"),
        new(EnemyFrameId.CrocomireBody11, "crocomire_body_11"),
        new(EnemyFrameId.CrocomireBody12, "crocomire_body_12"),
        new(EnemyFrameId.CrocomireBody13, "crocomire_body_13"),
        new(EnemyFrameId.CrocomireBody14, "crocomire_body_14"),
        new(EnemyFrameId.CrocomireBody15, "crocomire_body_15"),
        new(EnemyFrameId.CrocomireBody16, "crocomire_body_16"),
        new(EnemyFrameId.CrocomireBody17, "crocomire_body_17"),
        new(EnemyFrameId.CrocomireBody18, "crocomire_body_18"),
        new(EnemyFrameId.CrocomireBody19, "crocomire_body_19"),
        new(EnemyFrameId.CrocomireBody20, "crocomire_body_20"),
        new(EnemyFrameId.CrocomireBody21, "crocomire_body_21"),
        new(EnemyFrameId.CrocomireBody22, "crocomire_body_22"),
        new(EnemyFrameId.CrocomireBody23, "crocomire_body_23"),
        new(EnemyFrameId.CrocomireBody24, "crocomire_body_24"),
        new(EnemyFrameId.CrocomireBody25, "crocomire_body_25"),
        new(EnemyFrameId.CrocomireBody26, "crocomire_body_26"),
        new(EnemyFrameId.CrocomireBody27, "crocomire_body_27"),
        new(EnemyFrameId.CrocomireBody28, "crocomire_body_28"),
        new(EnemyFrameId.CrocomireBody29, "crocomire_body_29"),
        new(EnemyFrameId.CrocomireBody30, "crocomire_body_30"),
        new(EnemyFrameId.CrocomireBody31, "crocomire_body_31"),
        new(EnemyFrameId.CrocomireBody32, "crocomire_body_32"),
        new(EnemyFrameId.CrocomireBody33, "crocomire_body_33"),
        new(EnemyFrameId.CrocomireBody34, "crocomire_body_34"),
        new(EnemyFrameId.CrocomireBody35, "crocomire_body_35"),
        new(EnemyFrameId.CrocomireBody36, "crocomire_body_36"),
        new(EnemyFrameId.CrocomireBody37, "crocomire_body_37"),
        new(EnemyFrameId.CrocomireBody38, "crocomire_body_38"),
        new(EnemyFrameId.CrocomireBody39, "crocomire_body_39"),
        new(EnemyFrameId.CrocomireBody40, "crocomire_body_40"),
        new(EnemyFrameId.CrocomireBody41, "crocomire_body_41"),
    ];
}
