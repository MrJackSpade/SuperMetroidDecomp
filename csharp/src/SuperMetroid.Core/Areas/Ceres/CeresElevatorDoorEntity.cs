using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The elevator room's blocked door (#1953). It shows its own look over the doors' artwork and keeps the room's glow and
/// the turning floor's elevator platform cycling. Once the station starts to self-destruct it blows apart in a rumble of
/// explosions, which publishes its destruction so the shaft starts to turn.
/// </summary>
/// <param name="doors">The room's owner of the Ceres doors and walls.</param>
/// <param name="place">The place the door occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CeresElevatorDoorEntity(CeresDoorBehavior doors, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>How many updates the door's destruction lasts, the last the one it ends on (#2281).</summary>
    private const int RumbleDuration = 49;

    /// <summary>How many updates apart the destruction's explosions burst (#2281).</summary>
    private const int RumbleInterval = 5;

    /// <inheritdoc/>
    public void Initialize()
    {
        doors.Start(Place, CeresDoorSegment.ElevatorDoor);
        Place.SetSpeciesState(new CeresElevatorDoorState { Phase = CeresElevatorDoorPhase.Intact });
        // Only the elevator door shows the door's own look (#1406, #1538).
        doors.Presentation.ShowLook(CeresElevatorDoorLookDefinitions.OverlayLook);
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        CeresElevatorDoorState state = Place.RequireSpeciesState<CeresElevatorDoorState>();
        switch (state.Phase)
        {
            case CeresElevatorDoorPhase.Intact:
                CycleGlowAndPlatform();
                if (doors.Escape.CeresStatus is CeresEscapeStatus.SelfDestructStarted or CeresEscapeStatus.ElevatorDoorDestroyed)
                {
                    // Entering the destruction starts its three counters, but deliberately does not run its first step
                    // until the following enemy frame.
                    state.Phase = CeresElevatorDoorPhase.Exploding;
                    state.RumbleUpdatesLeft = RumbleDuration;
                    // The first explosion bursts on the destruction's first update.
                    state.ExplosionIntervalLeft = 1;
                    state.LastExplosionPlace = 0;
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
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => doors.AdvanceAnimation(Place, context.Samus);

    /// <summary>Runs one update of the destruction.</summary>
    internal void Explode()
    {
        CeresElevatorDoorState state = Place.RequireSpeciesState<CeresElevatorDoorState>();
        if (--state.RumbleUpdatesLeft == 0)
        {
            // The last update ends the destruction: the door hides, keeps cycling the glow and platform from the next update
            // on, and publishes its destruction so the room's turning floor begins the rotation after Ridley's escape.
            Place.Presence = Place.Presence.With(EnemyPresence.Hidden);
            state.Phase = CeresElevatorDoorPhase.Destroyed;
            if (doors.Escape.CeresStatus != CeresEscapeStatus.RidleyNotEscaped)
                doors.Escape.CeresStatus = CeresEscapeStatus.ElevatorDoorDestroyed;
            return;
        }

        if (--state.ExplosionIntervalLeft != 0)
            return;

        state.ExplosionIntervalLeft = RumbleInterval;
        IReadOnlyList<Offset16<RoomPixels>> offsets = CeresDoorRumbleGeometryDefinitions.ExplosionOffsets;
        state.LastExplosionPlace = state.LastExplosionPlace == 0 ? offsets.Count - 1 : state.LastExplosionPlace - 1;

        Position16<RoomPixels> explosion = Place.Position.Whole + offsets[state.LastExplosionPlace];

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
    /// and the platform strip are only drawn, so the door clocks them itself; the game read the room's first enemy's count,
    /// which only shifts the drawn phase where another enemy is listed first.
    /// </summary>
    internal void CycleGlowAndPlatform()
    {
        int frame = Place.FrameCounter;

        // The elevator platform's animation does not belong to either arrival projectile: the elevator door alternates the
        // turning floor's platform frames for as long as the room runs, so the landed platform keeps flashing after the
        // projectiles' touchdown deletes them. The import pictures both strips (#1467).
        doors.Room.Services.ShowFloorFrame(CeresFloorAnimation.ElevatorPlatformFrameForPhase(CeresDoorCadenceDefinitions.PlatformPicture.Of(frame)));

        // A new step of six colours every eight updates.
        doors.ElevatorRoomGlowStep = CeresDoorCadenceDefinitions.GlowStep.Of(frame);
    }
}
