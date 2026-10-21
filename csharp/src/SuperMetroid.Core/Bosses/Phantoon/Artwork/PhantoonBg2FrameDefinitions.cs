using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// Phantoon's bank-$A7 extended frames selected by the compiled $CC41-$CCFB
/// instruction programs. These identities select BG2 tilemap writes, not OAM.
/// Component hitbox pointers and collision rectangles are engine definitions.
/// </summary>
internal static class PhantoonBg2FrameDefinitions
{
    /// <summary>Schema version of the extracted BG2 frame artwork file.</summary>
    internal const int Version = 1;
    /// <summary>Installed filename for Phantoon's BG2 frame artwork.</summary>
    internal const string FileName = "phantoon-bg2-frames.json";
    /// <summary>Phantoon's tentacle frames have at most two BG2 components.</summary>
    internal const int MaximumComponents = 2;
    /// <summary>The blank BG2 tile word $0338 that fills Phantoon's tilemap before and after its body maps.</summary>
    internal const ushort BlankTile = 0x0338;

    /// <summary>Enumerates the selected native frames in instruction-program order.</summary>
    internal static ImmutableArray<EnemySpritemapDefinition> Frames { get; } =
    [
        new(EnemyFrameId.PhantoonBg2DEDD, "body_invulnerable"),
        new(EnemyFrameId.PhantoonBg2DEE7, "body_full_hitbox"),
        new(EnemyFrameId.PhantoonBg2DEF1, "body_eye_hitbox_only"),
        new(EnemyFrameId.PhantoonBg2DEFB, "eye_closed"),
        new(EnemyFrameId.PhantoonBg2DF05, "eye_open_0"),
        new(EnemyFrameId.PhantoonBg2DF0F, "eye_open_1"),
        new(EnemyFrameId.PhantoonBg2DF19, "eye_open_2"),
        new(EnemyFrameId.PhantoonBg2DF23, "eyeball_center"),
        new(EnemyFrameId.PhantoonBg2DF2D, "eyeball_up"),
        new(EnemyFrameId.PhantoonBg2DF37, "eyeball_down"),
        new(EnemyFrameId.PhantoonBg2DF41, "eyeball_left"),
        new(EnemyFrameId.PhantoonBg2DF4B, "eyeball_right"),
        new(EnemyFrameId.PhantoonBg2DF55, "eyeball_down_left"),
        new(EnemyFrameId.PhantoonBg2DF5F, "eyeball_down_right"),
        new(EnemyFrameId.PhantoonBg2DF69, "eyeball_up_left"),
        new(EnemyFrameId.PhantoonBg2DF73, "eyeball_up_right"),
        new(EnemyFrameId.PhantoonBg2DFB3, "tentacles_0"),
        new(EnemyFrameId.PhantoonBg2DFC5, "tentacles_1"),
        new(EnemyFrameId.PhantoonBg2DFD7, "tentacles_2"),
        new(EnemyFrameId.PhantoonBg2DFE9, "mouth_0"),
        new(EnemyFrameId.PhantoonBg2DFF3, "mouth_1"),
        new(EnemyFrameId.PhantoonBg2DFFD, "mouth_2"),
    ];

    /// <summary>Whether <paramref name="frame"/> is one of Phantoon's BG2 frames.</summary>
    internal static bool IsFrame(EnemyFrameId frame) => Frames.Any(definition => definition.Frame == frame);
}
