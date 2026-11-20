using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The elevator room's blocked door (#1953). It shows its own look over the doors' artwork and keeps the room's glow and
/// the turning floor's elevator platform cycling. Once the station starts to self-destruct it blows apart in a rumble of
/// explosions, which publishes its destruction so the shaft starts to turn.
/// </summary>
/// <param name="doors">The room's owner of the Ceres doors and walls.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the door occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="floor">The room's turning floor, whose elevator platform the door cycles (#2382).</param>
internal sealed class CeresElevatorDoorEntity(CeresDoorBehavior doors, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, RoomFloor floor)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>How many updates the door's destruction lasts, the last the one it ends on (#2281).</summary>
    private const int RumbleDuration = 49;

    /// <summary>How many updates apart the destruction's explosions burst (#2281).</summary>
    private const int RumbleInterval = 5;

    /// <summary>The door's animation, which it starts as it is built, showing the door's own look (#2554).</summary>
    internal CeresDoorAnimationState State { get; } = doors.StartElevatorDoor(place);

    /// <summary>Where the door stands in its destruction, intact as it is built.</summary>
    private readonly CeresElevatorDoorState _destruction = new() { Phase = CeresElevatorDoorPhase.Intact };

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        CeresElevatorDoorState state = _destruction;
        switch (state.Phase)
        {
            case CeresElevatorDoorPhase.Intact:
                CycleGlowAndPlatform();
                if (doors.Escape.CeresStatus is CeresEscapeStatus.SelfDestructStarted or CeresEscapeStatus.ElevatorDoorDestroyed)
                {
                    // Entering the destruction starts its three counters, but deliberately does not run its first step
                    // until the following enemy frame.
                    state.Phase = CeresElevatorDoorPhase.Exploding;
                    state.Rumble = UpdateCountdown.Lasting(RumbleDuration);
                    // The first explosion bursts on the destruction's first update.
                    state.ExplosionWait = UpdateCountdown.LastingOneUpdate();
                    state.NextExplosion = CyclingSequence<Offset16<RoomPixels>>.StartOf(CeresDoorRumbleGeometryDefinitions.ExplosionOffsets);
                }
                return;

            case CeresElevatorDoorPhase.Exploding:
                Explode();
                return;

            case CeresElevatorDoorPhase.Destroyed:
                CycleGlowAndPlatform();
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresElevatorDoorPhase)} {state.Phase}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => doors.AdvanceAnimation(Place, State, context.Samus);

    /// <summary>Runs one update of the destruction.</summary>
    private void Explode()
    {
        CeresElevatorDoorState state = _destruction;
        if (state.Rumble.Count())
        {
            // The last update ends the destruction: the door hides, keeps cycling the glow and platform from the next update
            // on, and publishes its destruction so the room's turning floor begins the rotation after Ridley's escape.
            Place.Presence = Place.Presence.With(EnemyPresence.Hidden);
            state.Phase = CeresElevatorDoorPhase.Destroyed;
            if (doors.Escape.CeresStatus != CeresEscapeStatus.RidleyNotEscaped)
                doors.Escape.CeresStatus = CeresEscapeStatus.ElevatorDoorDestroyed;
            return;
        }

        if (!state.ExplosionWait.Count())
            return;

        state.ExplosionWait.Restart(RumbleInterval);
        Position16<RoomPixels> explosion = Place.Position.Whole + state.NextExplosion.Current;
        state.NextExplosion = state.NextExplosion.Next;

        // The shared random sequence advances exactly once per explosion; its explosion chance selects the animation.
        DustExplosionAnimation animation = CeresDoorQuakeDefinitions.RumbleExplosion.IsTakenBy(doors.Room.Random.Generate())
            ? DustExplosionAnimation.Smoke
            : DustExplosionAnimation.SmallExplosion;
        doors.Projectiles.SpawnDustOrExplosion(explosion, animation);

        // The explosion sound is requested even when the finite enemy-projectile pool was full and
        // could not allocate the visual effect.
        doors.Audio.QueueEnemySound(EnemySound.BigExplosion, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Cycles the elevator room's glow and the room's turning-floor elevator frame by the door's own update count. The glow
    /// and the platform strip are only drawn, so the door clocks them itself.
    /// </summary>
    private void CycleGlowAndPlatform()
    {
        int frame = Place.FrameCounter;

        // The elevator platform's animation does not belong to the arrival's pad or platform: the elevator door alternates
        // the turning floor's platform frames for as long as the room runs, so the landed platform keeps flashing after the
        // arrival's touchdown ends. The import pictures both strips (#1467).
        floor.Show(CeresFloorAnimation.ElevatorPlatformFrameForPhase(CeresDoorCadenceDefinitions.PlatformPicture.Of(frame)));

        // The glow takes its next step at its cadence's place.
        doors.ElevatorRoomGlowStep = CeresDoorCadenceDefinitions.GlowStep.Of(frame);
    }
}
