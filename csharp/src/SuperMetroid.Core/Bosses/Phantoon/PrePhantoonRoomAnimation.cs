using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.Phantoon.PrePhantoonRoomProgram, SuperMetroid.Core.Bosses.Phantoon.PrePhantoonRoomFrame, SuperMetroid.Core.Bosses.Phantoon.PrePhantoonRoomCue>;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The pre-Phantoon room projectile's typed animation: one blank frame held while the room's setup holds its projectile
/// place (<see cref="PrePhantoonRoomHeldProjectilePlace"/>), then deletion (#641).
/// </summary>
internal sealed class PrePhantoonRoomAnimation()
    : EnemyProjectileProgramAnimation<PrePhantoonRoomProgram, PrePhantoonRoomFrame, PrePhantoonRoomCue>(PrePhantoonRoomProgram.BlankThenDelete)
{
    /// <summary>How long the blank frame lasts.</summary>
    private const int HoldDuration = PrePhantoonRoomHeldProjectilePlace.HeldUpdates;

    /// <summary>The blank frame, its deletion, and the hold that follows it.</summary>
    private static readonly AnimationProgramSet<PrePhantoonRoomProgram, PrePhantoonRoomFrame, PrePhantoonRoomCue> RoomPrograms = new(
        new Dictionary<PrePhantoonRoomProgram, Step[]>
        {
            [PrePhantoonRoomProgram.BlankThenDelete] = [Step.Show(default, HoldDuration), Step.Publish(PrePhantoonRoomCue.Delete), Step.Hold],
        });

    /// <inheritdoc/>
    private protected override AnimationProgramSet<PrePhantoonRoomProgram, PrePhantoonRoomFrame, PrePhantoonRoomCue> Programs => RoomPrograms;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(PrePhantoonRoomFrame frame) =>
        EnemyProjectileFrameId.Blank;
}
