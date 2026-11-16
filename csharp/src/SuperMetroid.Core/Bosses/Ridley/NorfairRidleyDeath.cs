using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Norfair Ridley's death: the roar, breakup, explosions and the drop.</summary>
[RoomLifetime]
internal sealed class NorfairRidleyDeath
{
    /// <summary>Spawns the enemies Ridley's death releases into the room.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>Room services, level state, and RNG for the Norfair fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Handles common combat damage and enemy drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Coordinates Ridley's dedicated projectile effects.</summary>
    private readonly RidleyProjectiles _ridleyProjectiles;
    /// <summary>Raises the arena liquid during Ridley's reveal.</summary>
    private readonly EnemyRoomFx _roomFx;
    /// <summary>Receives the death's explosion sounds and music change.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal NorfairRidleyDeath(IEnemyLifecycle lifecycle, EnemyRoomBinding room, EnemyDeathsAndDrops deathsAndDrops, RidleyProjectiles ridleyProjectiles, EnemyRoomFx roomFx, EnemyAudioQueue audio)
    {
        _audio = audio;
        _lifecycle = lifecycle;
        _room = room;
        _deathsAndDrops = deathsAndDrops;
        _ridleyProjectiles = ridleyProjectiles;
        _roomFx = roomFx;
    }

    /// <summary>Releases Samus's script lock and grants movement-dependent intangibility outside death.</summary>
    internal static void ReleaseNorfairRidleyGrab(NorfairRidleyState state, SamusState? samus)
    {
        state.GrabState = RidleyGrabState.Released;
        samus?.Control.Unlock();
        state.Tail.WhipRequested = true;
        state.Tail.Action = RidleyTailAction.Neutral;
        // The hurt timer is left untouched once death owns the fight.
        if (state.FightMode != RidleyFightMode.Dying)
        {
            SamusMovementType movement = samus is null ? SamusMovementType.Standing : SamusPoseMetadata.ReadMovementType(samus);
            state.IntangibilityTimer = RidleyGrabs.ReleaseIntangibilityFrames(movement);
        }
    }

    /// <summary>
    /// Runs the death roar. Ridley continues converging on the death point throughout the roar;
    /// expiry freezes velocity and starts lowering the room acid before entering the long
    /// small-explosion phase in the same update.
    /// </summary>
    internal void TickNorfairRidleyDeathRoar(
        RoomEnemyPlace body,
        NorfairRidleyState state)
    {
        NorfairRidleyMovement.MoveNorfairRidleyToward(body, state.Flight, new Position16<RoomPixels>(128, 328),
            RidleyInertiaDefinitions.DeathDivisor, RidleyInertiaDefinitions.DeathReversalBoost);
        if (!state.PhaseTimer.StepRunsOut())
            return;

        state.Flight.Velocity = EightEightVector<RoomPixels>.Zero;
        RidleyLiquidMotion.Publish(_roomFx, RidleyLiquidDefinitions.DrainedHeight,
            RidleyLiquidDefinitions.DrainVelocity, RidleyLiquidDefinitions.DrainDelay);
        state.DeathExplosionTimer = 0;
        state.DeathExplosionCount = 0;
        state.Phase = NorfairRidleyPhase.DeathExplosions;
        state.PhaseTimer.Count = 160;
        TickNorfairRidleyDeathExplosions(body, state, samus: null);
    }

    /// <summary>
    /// Runs the death explosions. An authored-position small explosion appears every five calls while the
    /// acid lowers. When its countdown of 160 runs out, Samus is released and Ridley's twelve
    /// independently moving body/tail breakup enemies are allocated from the real place pool.
    /// </summary>
    internal void TickNorfairRidleyDeathExplosions(
        RoomEnemyPlace body,
        NorfairRidleyState state,
        SamusState? samus)
    {
        SpawnSmallExplosionNearNorfairRidley(body, state);
        if (!state.PhaseTimer.StepRunsOut())
            return;

        if (state.GrabState != RidleyGrabState.Released)
            ReleaseNorfairRidleyGrab(state, samus);
        state.Phase = NorfairRidleyPhase.DeathBreakup;
        state.Flight.Velocity = EightEightVector<RoomPixels>.Zero;
        SpawnNorfairRidleyBreakupActors(body, state);
    }

    /// <summary>
    /// The breakup performs one final small-explosion tick, hides the intact body, and disables
    /// shared wing/tail movement so only the breakup actors remain visible.
    /// </summary>
    internal void BeginNorfairRidleyBreakup(
        RoomEnemyPlace body,
        NorfairRidleyState state)
    {
        SpawnSmallExplosionNearNorfairRidley(body, state);
        state.MovementAnimationEnabled = false;
        body.Presence = body.Presence.With(EnemyPresence.Hidden);
        state.Phase = NorfairRidleyPhase.DeathWait;
        state.PhaseTimer.Count = 32;
    }

    /// <summary>The breakup's deliberate wait, a countdown of 32, before the death's final countdown of 256.</summary>
    internal static void TickNorfairRidleyBreakupWait(NorfairRidleyState state)
    {
        if (!state.PhaseTimer.StepRunsOut())
            return;
        state.Phase = NorfairRidleyPhase.DeathFinish;
        state.PhaseTimer.Count = 256;
    }

    /// <summary>
    /// Finishes the death. Ridley's defeat is recorded before his body is deleted, on the update his drops scatter and
    /// the elevator music is requested.
    /// </summary>
    /// <param name="body">Ridley's body.</param>
    /// <param name="state">Ridley's state.</param>
    /// <param name="samus">Samus, whose energy and ammunition each drop's roll reads.</param>
    internal void TickNorfairRidleyDeathFinish(
        RoomEnemyPlace body,
        NorfairRidleyState state,
        SamusState samus)
    {
        if (!state.PhaseTimer.StepRunsOut())
            return;

        _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaBoss);

        // The death drops scatter sixteen pickups through Ridley's authored 128x64 arena
        // rectangle. The body is deleted immediately afterward, so the drop actors must
        // retain Ridley's definition themselves.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Ridley,
            samus: samus,
            count: 16,
            xBase: 64,
            xScatter: EnemyDropScatterDefinitions.NarrowArenaHorizontalScatter,
            yBase: 320,
            yScatter: EnemyDropScatterDefinitions.ArenaBandVerticalScatter);
        _audio.RequestRidleyMusic(MusicRequest.SelectTrack(MusicTrack.Elevator));
        body.Presence = body.Presence.With(EnemyPresence.Deleted);
        state.Phase = NorfairRidleyPhase.DeathComplete;
    }

    /// <summary>Spawns a small explosion at one of its ten signed X/Y offset pairs.</summary>
    internal void SpawnSmallExplosionNearNorfairRidley(
        RoomEnemyPlace body,
        NorfairRidleyState state)
    {
        if (state.DeathExplosionTimer != 0)
        {
            state.DeathExplosionTimer--;
            return;
        }

        state.DeathExplosionTimer = 4;
        state.DeathExplosionCount++;
        if (state.DeathExplosionCount >= 10)
            state.DeathExplosionCount = 0;

        _ridleyProjectiles.SpawnRidleyDust(
            body.Position.Whole + RidleyExplosionDefinitions.DeathExplosionPlacement(state.DeathExplosionCount),
            MiscDustAnimation.SmallExplosion);
        _audio.RequestRidleyDeathExplosionSound();
    }

    /// <summary>
    /// Allocates the body and tail pieces that continue moving after the intact body hides; each joins the breakup of
    /// <paramref name="state"/>'s body from its setup (#2231).
    /// </summary>
    internal void SpawnNorfairRidleyBreakupActors(RoomEnemyPlace body, NorfairRidleyState state)
    {
        if (state.DeathBreakupSpawned)
            return;
        state.DeathBreakupSpawned = true;

        RidleyBreakup.SpawnInBreakupOrder(SpawnPart);

        void SpawnPart(RidleyExplosionPart part)
        {
            // The embedded entries all use the same zero position and properties and name one
            // body part. Initialization replaces position and animation from the still-live
            // shared Ridley joints below.
            RoomEnemyPlacement population = new(
                EnemyDefinitionId.RidleyBreakupPiece,
                Position: Position16<RoomPixels>.Zero,
                Contact: EnemyContact.IgnoresSamus,
                Processing: EnemyProcessing.Animates | EnemyProcessing.OffScreen,
                Presence: EnemyPresence.None,
                CollidesByFrameHitboxes: false,
                SpawnSettings: new RidleyExplosionSpawnSettings(part));
            // The explosion's initialization AI starts each part from the still-live shared Ridley joints.
            if (_lifecycle.SpawnEnemyJoining(population, new RidleyBreakupSource(state, body.Position.Whole)) is null)
            {
                throw new InvalidOperationException(
                    "Lower Norfair Ridley's breakup found no place left for an enemy in the room.");
            }
        }
    }

    /// <summary>Runs the breakup actor's initialization AI for one of its 23 pieces, from the breakup it joined.</summary>
    internal void InitializeNorfairRidleyExplosion(RoomEnemyPlace fragment)
    {
        RidleyBreakupSource source = fragment.RequireSpeciesState<RidleyBreakupSource>();
        NorfairRidleyState state = source.Ridley;
        RidleyExplosionPart part = fragment.Spawn.Population.SpawnSettingsOf<RidleyExplosionSpawnSettings>().Part;

        fragment.AnimationClock.Restart();
        fragment.Shading = state.Colours.SpriteShading(flashing: false);
        int random = _room.Random.Generate();
        SignedEightEight horizontalSpeed =
            SignedEightEight.FromSixteenthsOfPixel(RidleyExplosionDefinitions.FragmentSpeedSixteenths.Of(random)) +
            SignedEightEight.FromPixels(RidleyExplosionDefinitions.FragmentSpeedWholePixel.Of(random));
        var fragmentState = new RidleyExplosionFragmentState
        {
            Lifetime = { Count = RidleyExplosionDefinitions.GetPart(part).Lifetime },
            Velocity = new(BossRandomRolls.InLowerHalf(random) ? horizontalSpeed : -horizontalSpeed, default),
        };
        fragment.SetSpeciesState(fragmentState);

        // Tail fragments start from their tail link; the body fragments start from the body.
        if (RidleyBreakup.TailLinkBecoming(part, state.Tail) is { } link)
        {
            fragment.Position = fragment.Position.WithWhole(link.Position);
            fragmentState.Playback = new AnimationPlayback<RidleyBreakupFrame>(
                RidleyBreakupAnimations.StartingWith(RidleyBreakup.SelectTailFrame(part, state.Tail.Tip.PointingDirection)), AnimationPlace.StartOfAnimation);
            return;
        }

        RidleyExplosionBodyPartDefinition bodyPart =
            RidleyBreakup.SelectBodyPart(
                part,
                facingRight: state.Flight.FacingDirection != RidleyFacing.Left);
        fragment.Position = fragment.Position.WithWhole(source.Body + bodyPart.Offset);
        fragmentState.Playback = new AnimationPlayback<RidleyBreakupFrame>(RidleyBreakupAnimations.StartingWith(bodyPart.Frame), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs the breakup actor's main AI: flicker, drag, gravity, movement, and lifetime.</summary>
    internal void RunNorfairRidleyExplosionMain(RoomEnemyPlace fragment, SamusState samus)
    {
        if (RidleyCadenceDefinitions.BreakupFragmentHidden.IsSetIn(fragment.FrameCounter))
            fragment.Presence = fragment.Presence.With(EnemyPresence.Hidden);
        else
            fragment.Presence = fragment.Presence.Without(EnemyPresence.Hidden);

        RidleyExplosionFragmentState fragmentState = fragment.RequireSpeciesState<RidleyExplosionFragmentState>();
        // A fragment falls only for its part's authored lifetime, so its gravity adds a few pixels an update at most.
        var velocity = new EightEightVector<RoomPixels>(
            fragmentState.Velocity.X.SlowedBy(RidleyExplosionDefinitions.BreakupFragmentDrag),
            fragmentState.Velocity.Y + RidleyExplosionDefinitions.BreakupFragmentGravity);
        fragmentState.Velocity = velocity;
        fragment.Position = fragment.Position.Advance(new(velocity.X.ToSixteenSixteenDelta(), velocity.Y.ToSixteenSixteenDelta()));

        if (fragmentState.Lifetime.StepKeepsRunning())
            return;

        // The breakup actor runs the small enemy death: clear the place immediately and
        // retain the fragment's definition and position in the shared explosion-to-pickup actor.
        _deathsAndDrops.StartGenericEnemyDeath(fragment, EnemyDeathExplosion.Small, samus);
    }
}
