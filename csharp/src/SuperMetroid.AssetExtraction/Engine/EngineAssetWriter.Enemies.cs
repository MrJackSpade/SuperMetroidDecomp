using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Atomic;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The enemies' data files.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes every enemy's data file.</summary>
    private void WriteEnemies()
    {
        WriteAtomic();
    }

    /// <summary>The Atomic: its four ways of spinning.</summary>
    private void WriteAtomic()
    {
        EngineEnemy atomic = Enemy("enemies/atomic.json", EnemyDefinitionId.Atomic).BodyColliders("Body", "Hurtbox");
        atomic.Timeline<AtomicSegment, SuperMetroid.Core.Animation.NoAnimationCue>(
            "up_right_turning", AtomicAnimations.Segments, AtomicAnimations.StartingWith(AtomicSegment.UpRightTurning));
        atomic.Timeline<AtomicSegment, SuperMetroid.Core.Animation.NoAnimationCue>(
            "up_left_turning", AtomicAnimations.Segments, AtomicAnimations.StartingWith(AtomicSegment.UpLeftTurning));
        atomic.Timeline<AtomicSegment, SuperMetroid.Core.Animation.NoAnimationCue>(
            "up_right_turning_back", AtomicAnimations.Segments, AtomicAnimations.StartingWith(AtomicSegment.UpRightTurningBack));
        atomic.Timeline<AtomicSegment, SuperMetroid.Core.Animation.NoAnimationCue>(
            "up_left_turning_back", AtomicAnimations.Segments, AtomicAnimations.StartingWith(AtomicSegment.UpLeftTurningBack));
        atomic.Write();
    }
}
