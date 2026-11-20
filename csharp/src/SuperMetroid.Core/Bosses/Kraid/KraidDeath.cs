using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's death: the sink below the floor, hiding his body and the background fade.</summary>
[RoomLifetime]
internal sealed class KraidDeath
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyPassEvents _passEvents;
    /// <summary>Room services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Publishes arena shake during growth, death, and heavy movement.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    private readonly KraidParts _parts;
    /// <summary>Applies Kraid's colours, background pictures, and sprite presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Applies contact damage, death state, and resulting drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>The room camera's distance mode, which the fight requests.</summary>
    private readonly RoomCameraDistance _cameraDistance;
    /// <summary>Kraid's state and part lookups, part animations, phase waits, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    private readonly KraidBackground _background;
    /// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health colours.</summary>
    private readonly KraidHead _head;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidDeath(KraidDeathDependencies dependencies)
    {
        _passEvents = dependencies.PassEvents;
        _room = dependencies.Room;
        _earthquake = dependencies.Earthquake;
        _parts = dependencies.Parts;
        _presentation = dependencies.Presentation;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _cameraDistance = dependencies.CameraDistance;
        _access = dependencies.Access;
        _background = dependencies.Background;
        _head = dependencies.Head;
    }

    /// <summary>
    /// Once the body has sunk, his whole body is hidden (#1515) and his body layer with it, so the room's own background
    /// shows from then on (#2573). A defeated room's reload hides his body here too.
    /// </summary>
    private static void HideKraidBody(KraidEnemyState state)
    {
        state.BodyAppearance = KraidBodyAppearance.Hidden;
        state.ShowsBodyLayer = false;
    }

    /// <summary>Runs the current step of Kraid's death: his staged sinking, his colour change and the room's repair.</summary>
    internal void RunKraidDeathPhase(
        RoomEnemyPlace body,
        KraidEnemyState state,
        int cameraX,
        SamusState samus)
    {
        switch (state.BodyState.DeathPhase)
        {
            case KraidDeathPhase.Initialize:
                InitializeKraidDeath(state, samus);
                return;

            case KraidDeathPhase.FadeOut:
                _head.AdvanceKraidHeadAnimation(state);
                if (_background.AdvanceKraidRoomBackgroundFade(fade: KraidBackdropFade.ToBlack))
                {
                    state.BodyState.DeathPhase = KraidDeathPhase.ShowBody;
                    state.ShowBody();
                }
                return;

            case KraidDeathPhase.ShowBody:
                _head.AdvanceKraidHeadAnimation(state);
                state.BodyState.DeathPhase = KraidDeathPhase.StartSinking;
                state.ShowBody();
                return;

            case KraidDeathPhase.StartSinking:
                _head.AdvanceKraidHeadAnimation(state);
                state.BodyState.DeathPhase = KraidDeathPhase.Sink;
                state.SinkRumbleWait.LastFor(KraidArenaDefinitions.SinkFirstSoundUpdates);
                body.Contact = body.Contact.With(EnemyContact.SolidToSamus);
                _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Vertical, EarthquakeReach.RoomBlocks), KraidArenaDefinitions.SinkShakeUpdates);
                state.ArmState.StartAnimation(KraidPartSegment.ArmRisingOrSinking);
                state.FootState.StartAnimation(KraidPartSegment.FootInitial);
                state.FootState.Phase = KraidFootPhase.Resting;
                return;

            case KraidDeathPhase.Sink:
                RunKraidDeathSink(body, state, samus);
                return;

            case KraidDeathPhase.HideBody:
                state.BodyState.DeathPhase = KraidDeathPhase.WaitBeforeFadeIn;
                state.BodyState.PhaseWait.LastFor(KraidArenaDefinitions.DeathDarkUpdates);
                HideKraidBody(state);
                return;
            case KraidDeathPhase.WaitBeforeFadeIn:
                if (state.BodyState.PhaseWait.StepRunsOut())
                    state.BodyState.DeathPhase = KraidDeathPhase.FadeInBackground;
                return;
            case KraidDeathPhase.FadeInBackground:
                if (!_background.AdvanceKraidRoomBackgroundFade(fade: KraidBackdropFade.ToKraidColours))
                    return;
                _access.RequestKraidMusic(MusicRequest.SelectTrack(MusicTrack.Elevator));
                _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaBoss);
                state.BodyState.DeathPhase = KraidDeathPhase.Finished;
                return;
            case KraidDeathPhase.Finished:
                if (HasCameraLeftRoomLeftEdge(cameraX))
                    KraidEncounterAccess.MarkKraidPartDead(body);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidDeathPhase)} {state.BodyState.DeathPhase}.");
        }
    }

    /// <summary>
    /// Whether the camera, at <paramref name="cameraX"/>, has left the room's left edge: until it does, a finished Kraid's
    /// body keeps its properties; the first update it has, the body is given a dead enemy's.
    /// </summary>
    private static bool HasCameraLeftRoomLeftEdge(int cameraX) => cameraX != 0;

    /// <summary>
    /// Starts the death animation and prepares its room mutation schedule. His health has run out, so the update's colour
    /// handling, which runs before it, has already cut his hurt flash short.
    /// </summary>
    private void InitializeKraidDeath(KraidEnemyState state, SamusState samus)
    {
        state.Colours.ShowDeathArm();
        state.ArmState.StartAnimation(KraidPartSegment.ArmDyingOrPreparingToLunge);
        state.BodyState.DeathPhase = KraidDeathPhase.FadeOut;
        state.StartHeadAnimation(KraidHeadSegment.Death);
        // The death stops the fingernails coming back once killed, then kills the
        // nails, then the three lints. The death runs without choosing an explosion, so each
        // part leaves the first death explosion.
        foreach (RoomEnemyPlace nail in _parts.Nails)
            nail.Presence = nail.Presence.Without(EnemyPresence.RespawnsIfKilled);
        foreach (RoomEnemyPlace nail in _parts.Nails)
            _deathsAndDrops.StartGenericEnemyDeath(nail, EnemyDeathExplosion.Small, samus);
        foreach (RoomEnemyPlace lint in _parts.Lints)
            _deathsAndDrops.StartGenericEnemyDeath(lint, EnemyDeathExplosion.Small, samus);
        // The live sequence crumbles successive blocks while Kraid fades/sinks.
        // It must not substitute the instantaneous already-defeated-room clear.
        _passEvents.Publish(new KraidRoomChange(KraidObjectDefinitions.LiveDeathSpikes));
    }

    /// <summary>Moves the body downward through the authored sink sequence.</summary>
    private void RunKraidDeathSink(RoomEnemyPlace body, KraidEnemyState state, SamusState samus)
    {
        _head.AdvanceKraidHeadAnimation(state);
        if (state.SinkRumbleWait.StepRunsOut())
        {
            _access.RequestKraidSound(KraidSoundDefinitions.RockSpit);
            state.SinkRumbleWait.LastFor(KraidArenaDefinitions.SinkSoundIntervalUpdates);
        }
        TakeKraidSinkStep(body);
        body.Position = body.Position.WithWholeY(body.Position.Y.Whole + 1);
        if (body.Position.Y.Whole < KraidArenaDefinitions.SunkY)
            return;

        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        RoomEnemyPlace arm = _parts.Arm;
        arm.Contact = arm.Contact.With(EnemyContact.IgnoresSamus);
        arm.Presence = arm.Presence.With(EnemyPresence.Deleted);
        // The lints and the foot are deleted with the arm (#2298).
        foreach (RoomEnemyPlace lint in _parts.Lints)
            KraidEncounterAccess.DeleteKraidPart(lint);
        KraidEncounterAccess.DeleteKraidPart(_parts.Foot);
        state.BodyState.DeathPhase = KraidDeathPhase.HideBody;
        // The death returns the camera to its normal tracking before the death drop.
        _cameraDistance.Request(CameraDistanceMode.NormalTracking);

        // The death drops share the room's enemy projectiles with other deaths. Each pickup gets a random position in
        // Kraid's floor strip and independently rolls Kraid's drop chances.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Kraid,
            count: KraidArenaDefinitions.DeathDropCount,
            xBase: KraidArenaDefinitions.DeathDropOrigin.X,
            xScatter: EnemyDropScatterDefinitions.WideArenaHorizontalScatter,
            yBase: KraidArenaDefinitions.DeathDropOrigin.Y,
            yScatter: EnemyDropScatterDefinitions.ArenaBandVerticalScatter,
            samus: samus);
        // The death continues into redrawing Kraid's room background, which restarts the shared fade step count.
        _presentation.FadeStepCount.Reset();
    }

    /// <summary>Takes the sink step the body has reached as it descends, if any: its ceiling block goes and a rock falls from it.</summary>
    private void TakeKraidSinkStep(RoomEnemyPlace body)
    {
        if (KraidSinkSchedule.StepAt(body.Position.Y.Whole) is not KraidSinkStep step)
            return;
        RoomObjectSpawnRequest<KraidRoomMutation> request = KraidObjects.ForSinkStep(step);
        // Each sinking rock starts at the left pixel edge of the removed block.
        _background.SpawnKraidCeilingRock(request.Block.ToRoomPixels().X);
        _passEvents.Publish(new KraidRoomChange(request));
    }
}
