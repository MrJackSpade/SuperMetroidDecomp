using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Phantoon's bank-$A7 extended frames selected by the compiled $CC41-$CCFB
/// instruction programs. These identities select BG2 tilemap writes, not OAM.
/// Component hitbox pointers and collision rectangles are engine definitions.
/// </summary>
internal static partial class PhantoonBg2FrameDefinitions
{
    /// <summary>
    /// Schema version of the extracted BG2 frame artwork file; 3 names each frame for the animation that first shows it
    /// (#1623), 4 holds each picture once (#2249): the closed eye and the open eye looking ahead, which the cartridge stores
    /// twice each, are one frame each, and 5 holds his body once (#2574): the cartridge stores it once for each hit area.
    /// </summary>
    internal const int Version = 5;
    /// <summary>Installed filename for Phantoon's BG2 frame artwork.</summary>
    internal const string FileName = "phantoon-bg2-frames.json";
    /// <summary>Phantoon's tentacle frames have at most two BG2 components.</summary>
    internal const int MaximumComponents = 2;

    /// <summary>Enumerates the selected native frames in instruction-program order.</summary>
    internal static ImmutableArray<EnemyFrameName> Frames { get; } =
    [
        new(EnemyFrameId.PhantoonBody, "body"),
        new(EnemyFrameId.PhantoonEyeClosed, "eye_closed"),
        new(EnemyFrameId.PhantoonEyeHalfOpen, "eye_half_open"),
        new(EnemyFrameId.PhantoonEyeballCentered, "eyeball_center"),
        new(EnemyFrameId.PhantoonEyeLookingUp, "eyeball_up"),
        new(EnemyFrameId.PhantoonEyeLookingDown, "eyeball_down"),
        new(EnemyFrameId.PhantoonEyeLookingLeft, "eyeball_left"),
        new(EnemyFrameId.PhantoonEyeLookingRight, "eyeball_right"),
        new(EnemyFrameId.PhantoonEyeLookingDownLeft, "eyeball_down_left"),
        new(EnemyFrameId.PhantoonEyeLookingDownRight, "eyeball_down_right"),
        new(EnemyFrameId.PhantoonEyeLookingUpLeft, "eyeball_up_left"),
        new(EnemyFrameId.PhantoonEyeLookingUpRight, "eyeball_up_right"),
        new(EnemyFrameId.PhantoonTentaclesNarrow, "tentacles_narrow"),
        new(EnemyFrameId.PhantoonTentaclesSpreading, "tentacles_spreading"),
        new(EnemyFrameId.PhantoonTentaclesSpread, "tentacles_spread"),
        new(EnemyFrameId.PhantoonMouthOpen, "mouth_open"),
        new(EnemyFrameId.PhantoonMouthHalfOpen, "mouth_half_open"),
        new(EnemyFrameId.PhantoonMouthNarrow, "mouth_narrow"),
    ];
}
