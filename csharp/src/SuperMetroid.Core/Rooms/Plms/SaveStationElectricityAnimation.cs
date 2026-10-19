using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The save-station electricity's typed animation.</summary>
internal sealed class SaveStationElectricityAnimation(SaveStationElectricityProgram program)
    : EnemyProjectileProgramAnimation<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue> Programs => SaveStationElectricityAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(SaveStationElectricityFrame frame) =>
        SaveStationElectricityFrameDefinitions.ShownProjectileFrames[frame];
}
