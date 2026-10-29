using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>The frames Phantoon draws on his body layer (#1466): his body and eye, gaze, tentacle and mouth poses. Every other frame he selects is drawn as sprites.</summary>
internal static class PhantoonBodyLayerFrameDefinitions
{
    /// <summary>The frames Phantoon draws on his body layer, each a frame of his installed body sheet.</summary>
    internal static IReadOnlySet<EnemyFrameId> Frames { get; } = new HashSet<EnemyFrameId>
    {
        EnemyFrameId.PhantoonInvulnerableBody0,
        EnemyFrameId.PhantoonFullHitboxBody0,
        EnemyFrameId.PhantoonEyeHitboxBody0,
        EnemyFrameId.PhantoonClosedEye0,
        EnemyFrameId.PhantoonOpeningEye0,
        EnemyFrameId.PhantoonOpeningEye1,
        EnemyFrameId.PhantoonOpeningEye2,
        EnemyFrameId.PhantoonCenteredEye0,
        EnemyFrameId.PhantoonGaze0,
        EnemyFrameId.PhantoonGaze4,
        EnemyFrameId.PhantoonGaze6,
        EnemyFrameId.PhantoonGaze2,
        EnemyFrameId.PhantoonGaze5,
        EnemyFrameId.PhantoonGaze3,
        EnemyFrameId.PhantoonGaze7,
        EnemyFrameId.PhantoonGaze1,
        EnemyFrameId.PhantoonTentacle0,
        EnemyFrameId.PhantoonTentacle1,
        EnemyFrameId.PhantoonTentacle2,
        EnemyFrameId.PhantoonMouth0,
        EnemyFrameId.PhantoonMouth1,
        EnemyFrameId.PhantoonMouth2,
    };
}
