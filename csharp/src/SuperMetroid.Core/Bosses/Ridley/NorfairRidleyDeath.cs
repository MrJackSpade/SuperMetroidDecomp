using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Norfair Ridley's death: the roar, breakup, explosions and the drop.</summary>
[RoomLifetime]
internal sealed partial class NorfairRidleyDeath : IEnemyJoiningFactory<RidleyBreakupSource, RidleyExplosionSpawnSettings>
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
    internal static void ReleaseNorfairRidleyGrab(NorfairRidleyState state, SamusState samus)
    {
        state.GrabState = RidleyGrabState.Released;
        Ensure.NotNull(samus).Control.Unlock();
        state.Tail.Whip.Give();
        state.Tail.Action = RidleyTailAction.Neutral;
        // The hurt timer is left untouched once death owns the fight.
        if (state.FightMode != RidleyFightMode.Dying)
        {
            state.ReleaseIntangibility = BossCountdown.Lasting(RidleyGrabs.ReleaseIntangibilityFrames(SamusPoseMetadata.ReadMovementType(samus)));
        }
    }

    /// <summary>
    /// Runs the death roar. Ridley continues converging on the death point throughout the roar;
    /// expiry freezes velocity and starts lowering the room acid before entering the long
    /// small-explosion phase in the same update.
    /// </summary>
    internal void TickNorfairRidleyDeathRoar(
        RoomEnemyPlace body,
        NorfairRidleyState state,
        SamusState samus)
    {
        NorfairRidleyMovement.MoveNorfairRidleyToward(body, state.Flight, NorfairRidleyArenaDefinitions.DeathSpot,
            RidleyInertiaDefinitions.DeathDivisor, RidleyInertiaDefinitions.DeathReversalBoost);
        if (!state.PhaseTimer.StepRunsOut())
            return;

        state.Flight.Velocity = PixelVelocity<RoomPixels>.Zero;
        RidleyLiquidMotion.Publish(_roomFx, RidleyLiquidDefinitions.DrainedHeight,
            RidleyLiquidDefinitions.DrainVelocity, RidleyLiquidDefinitions.DrainDelay);
        state.DeathExplosionTimer.LastFor(1);
        state.NextDeathExplosion = NorfairRidleyState.FirstDeathExplosion;
        state.Phase = NorfairRidleyPhase.DeathExplosions;
        state.PhaseTimer.LastFor(NorfairRidleyMotionDefinitions.DeathExplosionUpdates);
        TickNorfairRidleyDeathExplosions(body, state, samus);
    }

    /// <summary>
    /// Runs the death explosions. An authored-position small explosion appears at the death's explosion interval while the
    /// acid lowers. When its countdown runs out, Samus is released and Ridley's
    /// independently moving body and tail pieces take places in the room's enemy list.
    /// </summary>
    internal void TickNorfairRidleyDeathExplosions(
        RoomEnemyPlace body,
        NorfairRidleyState state,
        SamusState samus)
    {
        SpawnSmallExplosionNearNorfairRidley(body, state);
        if (!state.PhaseTimer.StepRunsOut())
            return;

        if (state.GrabState != RidleyGrabState.Released)
            ReleaseNorfairRidleyGrab(state, samus);
        state.Phase = NorfairRidleyPhase.DeathBreakup;
        state.Flight.Velocity = PixelVelocity<RoomPixels>.Zero;
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
        state.Motion = RidleyMotion.Still;
        body.Presence = body.Presence.With(EnemyPresence.Hidden);
        state.Phase = NorfairRidleyPhase.DeathWait;
        state.PhaseTimer.LastFor(NorfairRidleyMotionDefinitions.BreakupWaitUpdates);
    }

    /// <summary>The breakup's deliberate wait before the death's final countdown.</summary>
    internal static void TickNorfairRidleyBreakupWait(NorfairRidleyState state)
    {
        if (!state.PhaseTimer.StepRunsOut())
            return;
        state.Phase = NorfairRidleyPhase.DeathFinish;
        state.PhaseTimer.LastFor(NorfairRidleyMotionDefinitions.DeathFinishUpdates);
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

        // The death drops scatter their pickups through Ridley's authored arena
        // rectangle. The body is deleted immediately afterward, so the drop actors must
        // retain Ridley's definition themselves.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Ridley,
            samus: samus,
            count: NorfairRidleyArenaDefinitions.DeathDrops,
            xBase: NorfairRidleyArenaDefinitions.DeathDropCorner.X,
            xScatter: EnemyDropScatterDefinitions.NarrowArenaHorizontalScatter,
            yBase: NorfairRidleyArenaDefinitions.DeathDropCorner.Y,
            yScatter: EnemyDropScatterDefinitions.ArenaBandVerticalScatter);
        _audio.RequestRidleyMusic(MusicRequest.SelectTrack(MusicTrack.Elevator));
        body.Presence = body.Presence.With(EnemyPresence.Deleted);
        state.Phase = NorfairRidleyPhase.DeathComplete;
    }

    /// <summary>Spawns his death's next small explosion, once its wait runs out, at the next point of the shared scatter.</summary>
    internal void SpawnSmallExplosionNearNorfairRidley(
        RoomEnemyPlace body,
        NorfairRidleyState state)
    {
        if (state.DeathExplosionTimer.StepKeepsRunning())
            return;

        state.DeathExplosionTimer.LastFor(NorfairRidleyMotionDefinitions.DeathExplosionIntervalUpdates);
        Offset16<RoomPixels> scatter = state.NextDeathExplosion.Entry;
        state.NextDeathExplosion = state.NextDeathExplosion.NextInCycle;

        _ridleyProjectiles.SpawnRidleyDust(
            body.Position.Whole + scatter,
            DustExplosionAnimation.SmallExplosion);
        _audio.RequestRidleyDeathExplosionSound();
    }

    /// <summary>
    /// Allocates the body and tail pieces that continue moving after the intact body hides; each joins the breakup of
    /// <paramref name="state"/>'s body from its setup (#2231).
    /// </summary>
    internal void SpawnNorfairRidleyBreakupActors(RoomEnemyPlace body, NorfairRidleyState state)
    {
        RidleyBreakup.SpawnInBreakupOrder(SpawnPart);

        void SpawnPart(RidleyExplosionPart part)
        {
            // Every piece spawns at the room's origin with the same properties and names its part; its setup
            // places it where that part of Ridley still stands and starts its animation.
            var settings = new RidleyExplosionSpawnSettings(part);
            RoomEnemyPlacement population = new(
                EnemyDefinitionId.RidleyBreakupPiece,
                Placing: EnemyPlacing.At(Position16<RoomPixels>.Zero),
                Contact: EnemyContact.IgnoresSamus,
                Processing: EnemyProcessing.Animates | EnemyProcessing.OffScreen,
                Presence: EnemyPresence.None,
                CollidesByFrameHitboxes: false,
                SpawnSettings: settings);
            // Each piece's setup starts it from where its part of Ridley stands. A full pool spawns no piece, and the
            // breakup goes on without it.
            _ = _lifecycle.SpawnEnemy(new JoiningEnemySpawn<RidleyBreakupSource, RidleyExplosionSpawnSettings>(
                population, settings, this, new RidleyBreakupSource(state, body.Position.Whole)));
        }
    }

    /// <summary>A piece of the breakup <paramref name="encounter"/> names, which it joins as it is built (#2231, #2554).</summary>
    public EnemyOccupant CreateJoining(RoomEnemyOccupant occupant, RidleyExplosionSpawnSettings settings, RoomEnemyPlace place,
        EnemyCommonParticipation common, RidleyBreakupSource encounter) =>
        EnemyOccupant.Running(new RidleyBreakupPieceEntity(this, occupant, place, common, settings, encounter));

    /// <summary>Sets up one of the breakup's twelve pieces from the breakup it joined.</summary>
    /// <returns>The state the piece holds from its setup.</returns>
    internal RidleyExplosionFragmentState InitializeNorfairRidleyExplosion(RidleyExplosionSpawnSettings ridleyExplosionSpawnSettings, RoomEnemyPlace fragment, RidleyBreakupSource source)
    {
        NorfairRidleyState state = source.Ridley;
        RidleyExplosionPart part = ridleyExplosionSpawnSettings.Part;

        fragment.AnimationClock.Restart();
        fragment.Shading = state.Colours.SpriteShading(flashing: false);
        int random = _room.Random.Generate();
        SignedEightEight horizontalSpeed =
            RidleyExplosionDefinitions.FragmentSpeedFraction.Of(random) + RidleyExplosionDefinitions.FragmentSpeedWholePixel.Of(random);
        // Tail fragments start from their tail link; the body fragments start from the body.
        EnemyFrameId picture;
        if (RidleyBreakup.TailLinkBecoming(part, state.Tail) is { } link)
        {
            fragment.Position = fragment.Position.WithWhole(link.Position);
            picture = RidleyBreakup.SelectTailFrame(part, state.Tail.Tip.PointingDirection);
        }
        else
        {
            // Ridley turning to face the screen breaks up as though he faced right.
            HorizontalDirection facing = state.Flight.FacingDirection == RidleyFacing.Left ? HorizontalDirection.Left : HorizontalDirection.Right;
            fragment.Position = fragment.Position.WithWhole(source.Body + RidleyBreakup.BodyPartOffset(part, facing));
            picture = RidleyBreakup.BodyPartFrame(part, facing);
        }
        var fragmentState = new RidleyExplosionFragmentState
        {
            Velocity = new(horizontalSpeed.Toward(BossRandomRolls.SideOf(random)), default),
            Picture = picture,
        };
        fragmentState.Lifetime.LastFor(RidleyExplosionDefinitions.LifetimeUpdates[part]);
        return fragmentState;
    }

    /// <summary>Runs a breakup piece's update: flicker, drag, gravity, movement, and lifetime.</summary>
    internal void RunNorfairRidleyExplosionMain(RoomEnemyOccupant piece, RidleyExplosionFragmentState fragmentState, SamusState samus)
    {
        RoomEnemyPlace fragment = piece.Place;
        if (RidleyCadenceDefinitions.BreakupFragmentHidden.IsInSecondHalfAt(fragment.FrameCounter))
            fragment.Presence = fragment.Presence.With(EnemyPresence.Hidden);
        else
            fragment.Presence = fragment.Presence.Without(EnemyPresence.Hidden);
        // A fragment falls only for its part's authored lifetime, so its gravity adds a few pixels an update at most.
        var velocity = new PixelVelocity<RoomPixels>(
            fragmentState.Velocity.X.SlowedBy(RidleyExplosionDefinitions.BreakupFragmentDrag),
            fragmentState.Velocity.Y + RidleyExplosionDefinitions.BreakupFragmentGravity);
        fragmentState.Velocity = velocity;
        fragment.Position = fragment.Position.Advance(new(velocity.X.ToSixteenSixteenDelta(), velocity.Y.ToSixteenSixteenDelta()));

        if (fragmentState.Lifetime.StepKeepsRunning())
            return;

        // A piece's lifetime ends in the small enemy death: it leaves its place at once, and the explosion it
        // becomes keeps its definition and position for the pickup it may leave.
        _deathsAndDrops.StartGenericEnemyDeath(piece, EnemyDeathExplosion.Small, samus);
    }
}
