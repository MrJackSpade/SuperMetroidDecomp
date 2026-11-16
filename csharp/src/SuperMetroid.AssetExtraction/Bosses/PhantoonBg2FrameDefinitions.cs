using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Phantoon's bank-$A7 extended frames selected by the compiled $CC41-$CCFB
/// instruction programs. These identities select BG2 tilemap writes, not OAM.
/// Component hitbox pointers and collision rectangles are engine definitions.
/// </summary>
internal static class PhantoonBg2FrameDefinitions
{
    /// <summary>
    /// Schema version of the extracted BG2 frame artwork file; 3 names each frame for the animation that first shows it
    /// (#1623), 4 holds each picture once (#2249): the closed eye and the open eye looking ahead, which the cartridge stores
    /// twice each, are one frame each.
    /// </summary>
    internal const int Version = 4;
    /// <summary>Installed filename for Phantoon's BG2 frame artwork.</summary>
    internal const string FileName = "phantoon-bg2-frames.json";
    /// <summary>Phantoon's tentacle frames have at most two BG2 components.</summary>
    internal const int MaximumComponents = 2;

    /// <summary>Enumerates the selected native frames in instruction-program order.</summary>
    internal static ImmutableArray<EnemyFrameName> Frames { get; } =
    [
        new(EnemyFrameId.PhantoonInvulnerableBody, "body_invulnerable"),
        new(EnemyFrameId.PhantoonFullHitboxBody, "body_full_hitbox"),
        new(EnemyFrameId.PhantoonEyeHitboxBody, "body_eye_hitbox_only"),
        new(EnemyFrameId.PhantoonEyeClosed, "eye_closed"),
        new(EnemyFrameId.PhantoonEyeHalfOpen, "eye_open_1"),
        new(EnemyFrameId.PhantoonEyeballCentered, "eyeball_center"),
        new(EnemyFrameId.PhantoonEyeLookingUp, "eyeball_up"),
        new(EnemyFrameId.PhantoonEyeLookingDown, "eyeball_down"),
        new(EnemyFrameId.PhantoonEyeLookingLeft, "eyeball_left"),
        new(EnemyFrameId.PhantoonEyeLookingRight, "eyeball_right"),
        new(EnemyFrameId.PhantoonEyeLookingDownLeft, "eyeball_down_left"),
        new(EnemyFrameId.PhantoonEyeLookingDownRight, "eyeball_down_right"),
        new(EnemyFrameId.PhantoonEyeLookingUpLeft, "eyeball_up_left"),
        new(EnemyFrameId.PhantoonEyeLookingUpRight, "eyeball_up_right"),
        new(EnemyFrameId.PhantoonTentaclesNarrow, "tentacles_0"),
        new(EnemyFrameId.PhantoonTentaclesSpreading, "tentacles_1"),
        new(EnemyFrameId.PhantoonTentaclesSpread, "tentacles_2"),
        new(EnemyFrameId.PhantoonMouthOpen, "mouth_0"),
        new(EnemyFrameId.PhantoonMouthHalfOpen, "mouth_1"),
        new(EnemyFrameId.PhantoonMouthNarrow, "mouth_2"),
    ];
}
