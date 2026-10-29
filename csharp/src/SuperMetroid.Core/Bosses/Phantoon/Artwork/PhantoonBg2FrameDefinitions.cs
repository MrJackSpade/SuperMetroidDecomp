using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// Phantoon's bank-$A7 extended frames selected by the compiled $CC41-$CCFB
/// instruction programs. These identities select BG2 tilemap writes, not OAM.
/// Component hitbox pointers and collision rectangles are engine definitions.
/// </summary>
internal static class PhantoonBg2FrameDefinitions
{
    /// <summary>Schema version of the extracted BG2 frame artwork file.</summary>
    internal const int Version = 2;
    /// <summary>Installed filename for Phantoon's BG2 frame artwork.</summary>
    internal const string FileName = "phantoon-bg2-frames.json";
    /// <summary>Phantoon's tentacle frames have at most two BG2 components.</summary>
    internal const int MaximumComponents = 2;

    /// <summary>Enumerates the selected native frames in instruction-program order.</summary>
    internal static ImmutableArray<EnemySpritemapDefinition> Frames { get; } =
    [
        new(EnemyFrameId.PhantoonInvulnerableBody0, "body_invulnerable"),
        new(EnemyFrameId.PhantoonFullHitboxBody0, "body_full_hitbox"),
        new(EnemyFrameId.PhantoonEyeHitboxBody0, "body_eye_hitbox_only"),
        new(EnemyFrameId.PhantoonClosedEye0, "eye_closed"),
        new(EnemyFrameId.PhantoonOpeningEye0, "eye_open_0"),
        new(EnemyFrameId.PhantoonOpeningEye1, "eye_open_1"),
        new(EnemyFrameId.PhantoonOpeningEye2, "eye_open_2"),
        new(EnemyFrameId.PhantoonCenteredEye0, "eyeball_center"),
        new(EnemyFrameId.PhantoonGaze0, "eyeball_up"),
        new(EnemyFrameId.PhantoonGaze4, "eyeball_down"),
        new(EnemyFrameId.PhantoonGaze6, "eyeball_left"),
        new(EnemyFrameId.PhantoonGaze2, "eyeball_right"),
        new(EnemyFrameId.PhantoonGaze5, "eyeball_down_left"),
        new(EnemyFrameId.PhantoonGaze3, "eyeball_down_right"),
        new(EnemyFrameId.PhantoonGaze7, "eyeball_up_left"),
        new(EnemyFrameId.PhantoonGaze1, "eyeball_up_right"),
        new(EnemyFrameId.PhantoonTentacle0, "tentacles_0"),
        new(EnemyFrameId.PhantoonTentacle1, "tentacles_1"),
        new(EnemyFrameId.PhantoonTentacle2, "tentacles_2"),
        new(EnemyFrameId.PhantoonMouth0, "mouth_0"),
        new(EnemyFrameId.PhantoonMouth1, "mouth_1"),
        new(EnemyFrameId.PhantoonMouth2, "mouth_2"),
    ];

    /// <summary>The frames Phantoon draws on BG2.</summary>
    internal static IReadOnlySet<EnemyFrameId> FrameIds { get; } = Frames.Select(definition => definition.Frame).ToHashSet();
}
