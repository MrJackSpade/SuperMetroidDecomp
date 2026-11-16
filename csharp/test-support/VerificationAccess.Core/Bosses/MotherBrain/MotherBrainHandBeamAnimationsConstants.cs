using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHandBeamCue>;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values and helpers of <see cref="MotherBrainHandBeamAnimations"/> that only verification reads.</summary>
internal static class MotherBrainHandBeamAnimationsConstants
{
    /// <summary>Stages the beam runs before deleting itself.</summary>
    internal const int StageCount = 3;
}
