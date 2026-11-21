using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>The frames Phantoon shows as his body's pictures (#1466): his body and eye, gaze, tentacle and mouth
/// poses.</summary>
internal static class PhantoonBodyLayerFrameDefinitions
{
    /// <summary>The frames Phantoon draws on his body layer, each a frame of his installed body sheet.</summary>
    internal static IReadOnlySet<EnemyFrameId> Frames { get; } = new HashSet<EnemyFrameId>
    {
        EnemyFrameId.PhantoonBody,
        EnemyFrameId.PhantoonEyeClosed,
        EnemyFrameId.PhantoonEyeHalfOpen,
        EnemyFrameId.PhantoonEyeballCentered,
        EnemyFrameId.PhantoonEyeLookingUp,
        EnemyFrameId.PhantoonEyeLookingDown,
        EnemyFrameId.PhantoonEyeLookingLeft,
        EnemyFrameId.PhantoonEyeLookingRight,
        EnemyFrameId.PhantoonEyeLookingDownLeft,
        EnemyFrameId.PhantoonEyeLookingDownRight,
        EnemyFrameId.PhantoonEyeLookingUpLeft,
        EnemyFrameId.PhantoonEyeLookingUpRight,
        EnemyFrameId.PhantoonTentaclesNarrow,
        EnemyFrameId.PhantoonTentaclesSpreading,
        EnemyFrameId.PhantoonTentaclesSpread,
        EnemyFrameId.PhantoonMouthOpen,
        EnemyFrameId.PhantoonMouthHalfOpen,
        EnemyFrameId.PhantoonMouthNarrow,
    };
}
