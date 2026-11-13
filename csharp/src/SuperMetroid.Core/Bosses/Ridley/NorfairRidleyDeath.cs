using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
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
    /// <summary>The drag a breakup fragment's sideways speed loses each update, stopping at rest.</summary>
    private static readonly SignedEightEight BreakupFragmentDrag = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4);

    /// <summary>The gravity a breakup fragment's fall gains each update.</summary>
    private static readonly SignedEightEight BreakupFragmentGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4);

    /// <summary>Enemy dispatcher used for Ridley's registered callbacks.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Room services, level state, and RNG for the Norfair fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Handles common combat damage and enemy drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Owns fight state shared with cutscenes and room transitions.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>Coordinates Ridley's dedicated projectile effects.</summary>
    private readonly RidleyProjectiles _ridleyProjectiles;
    /// <summary>Raises the arena liquid during Ridley's reveal.</summary>
    private readonly EnemyRoomFx _roomFx;
    /// <summary>Receives the death's explosion sounds and music change.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal NorfairRidleyDeath(IEnemyDispatch dispatch, EnemyRoomBinding room, EnemyDeathsAndDrops deathsAndDrops, RidleyEncounter ridleyEncounter, RidleyProjectiles ridleyProjectiles, EnemyRoomFx roomFx, EnemyAudioQueue audio)
    {
        _audio = audio;
        _dispatch = dispatch;
        _room = room;
        _deathsAndDrops = deathsAndDrops;
        _ridleyEncounter = ridleyEncounter;
        _ridleyProjectiles = ridleyProjectiles;
        _roomFx = roomFx;
    }

    /// <summary>Releases Samus's script lock and grants movement-dependent intangibility outside death.</summary>
    internal static void ReleaseNorfairRidleyGrab(RidleyEnemyState state, SamusState? samus)
    {
        state.GrabState = RidleyGrabState.Released;
        samus?.Control.Unlock();
        state.TailWhipRequest = 1;
        state.TailFunctionIndex = RidleyTailFunction.Neutral;
        // The hurt timer is left untouched once death owns the fight.
        if (state.FightMode != RidleyFightMode.Dying)
        {
            SamusMovementType movement = samus is null ? SamusMovementType.Standing : SamusPoseMetadata.ReadMovementType(samus);
            state.IntangibilityTimer = RidleyGrabs.ReleaseIntangibilityFrames(movement);
        }
    }

    /// <summary>Runs the breakup explosion actor's initialization AI.</summary>
    internal void RunRidleyExplosionInitializationAi(RoomEnemySlot slot) =>
        InitializeNorfairRidleyExplosion(
            slot,
            _ridleyEncounter.Parts.Body,
            _ridleyEncounter.Ridley ?? throw new InvalidOperationException(
                "A Ridley breakup actor has no shared Ridley owner."));

    /// <summary>Last item-drop request published by Ridley's death.</summary>
    public bool RidleyDeathDropRequested { get; private set; }

    /// <summary>
    /// Runs the death roar. Ridley continues converging on the death point throughout the roar;
    /// expiry freezes velocity and starts lowering the room acid before entering the long
    /// small-explosion phase in the same dispatcher call.
    /// </summary>
    internal void TickNorfairRidleyDeathRoar(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        NorfairRidleyMovement.MoveNorfairRidleyToward(body, state, new Position16<RoomPixels>(128, 328),
            RidleyInertiaDefinitions.DeathDivisor, RidleyInertiaDefinitions.DeathReversalBoost);
        if (!NorfairRidleyMovement.TickRidleyFunctionTimer(state))
            return;

        state.Velocity = EightEightVector<RoomPixels>.Zero;
        RidleyLiquidMotion.Publish(_roomFx, state, RidleyLiquidDefinitions.DrainedHeight,
            RidleyLiquidDefinitions.DrainVelocity, RidleyLiquidDefinitions.DrainDelay);
        state.DeathExplosionTimer = 0;
        state.DeathExplosionCount = 0;
        state.Function = RidleyAiFunction.NorfairDeathFall;
        state.FunctionTimer = 160;
        TickNorfairRidleyDeathExplosions(body, state, samus: null);
    }

    /// <summary>
    /// Runs the death explosions. An authored-position small explosion appears every five calls while the
    /// acid lowers. When the 160-frame clock expires, Samus is released and Ridley's twelve
    /// independently moving body/tail breakup enemies are allocated from the real slot pool.
    /// </summary>
    internal void TickNorfairRidleyDeathExplosions(
        RoomEnemySlot body,
        RidleyEnemyState state,
        SamusState? samus)
    {
        SpawnSmallExplosionNearNorfairRidley(body, state);
        if (!NorfairRidleyMovement.TickRidleyFunctionTimer(state))
            return;

        if (state.GrabState != RidleyGrabState.Released)
            ReleaseNorfairRidleyGrab(state, samus);
        state.Function = RidleyAiFunction.NorfairDeathImpact;
        state.Velocity = EightEightVector<RoomPixels>.Zero;
        SpawnNorfairRidleyBreakupActors(state);
    }

    /// <summary>
    /// The breakup performs one final small-explosion tick, hides the intact body, and disables
    /// shared wing/tail movement so only the breakup actors remain visible.
    /// </summary>
    internal void BeginNorfairRidleyBreakup(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        SpawnSmallExplosionNearNorfairRidley(body, state);
        state.MovementAnimationEnabled = false;
        body.Presence = body.Presence.With(EnemyPresence.Hidden);
        state.Function = RidleyAiFunction.NorfairDeathWait;
        state.FunctionTimer = 32;
    }

    /// <summary>The breakup's deliberate 32-frame pre-wait before the final 256 frames.</summary>
    internal static void TickNorfairRidleyBreakupWait(RidleyEnemyState state)
    {
        if (!NorfairRidleyMovement.TickRidleyFunctionTimer(state))
            return;
        state.Function = RidleyAiFunction.NorfairDeathFinish;
        state.FunctionTimer = 256;
    }

    /// <summary>
    /// Finishes the death. Boss persistence is published before the slot is deleted, exactly
    /// when the retail item drop and delayed music-track-three request occur.
    /// </summary>
    internal void TickNorfairRidleyDeathFinish(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if (!NorfairRidleyMovement.TickRidleyFunctionTimer(state))
            return;

        _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
        state.BossDefeatPublished = true;

        // The death drops scatter sixteen pickups through Ridley's authored 128x64 arena
        // rectangle. The body is deleted immediately afterward, so the drop actors must
        // retain Ridley's definition themselves.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Ridley,
            count: 16,
            xBase: 64,
            xScatter: EnemyDropScatterDefinitions.NarrowArenaHorizontalScatter,
            yBase: 320,
            yScatter: EnemyDropScatterDefinitions.ArenaBandVerticalScatter);
        RidleyDeathDropRequested = true;
        _audio.RequestRidleyMusic(MusicRequest.SelectTrack(MusicTrack.Elevator));
        body.Presence = body.Presence.With(EnemyPresence.Deleted);
        state.Function = RidleyAiFunction.NorfairDeathComplete;
    }

    /// <summary>Spawns a small explosion at one of its ten signed X/Y offset pairs.</summary>
    internal void SpawnSmallExplosionNearNorfairRidley(
        RoomEnemySlot body,
        RidleyEnemyState state)
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

    /// <summary>Allocates the body and tail pieces that continue moving after the intact body hides.</summary>
    internal void SpawnNorfairRidleyBreakupActors(RidleyEnemyState state)
    {
        if (state.DeathBreakupSpawned)
            return;
        state.DeathBreakupSpawned = true;

        RidleyBreakup.SpawnInBreakupOrder(SpawnPart);

        void SpawnPart(RidleyExplosionPart part)
        {
            // The embedded entries all use the same zero position and properties and name one
            // body part. Initialization replaces position and program from the still-live
            // shared Ridley joints below.
            RoomEnemyPopulationRecord population = new(
                EnemyDefinitionId.RidleyExplosion,
                Position: Position16<RoomPixels>.Zero,
                Contact: EnemyContact.IgnoresSamus,
                Processing: EnemyProcessing.AnimationScript | EnemyProcessing.OffScreen,
                Presence: EnemyPresence.None,
                CollidesByFrameHitboxes: false,
                SpawnSettings: new RidleyExplosionSpawnSettings(part));
            // The explosion's initialization AI starts each part from the still-live shared Ridley joints.
            if (_dispatch.SpawnEnemy(population) is null)
            {
                throw new InvalidOperationException(
                    "Lower Norfair Ridley breakup exhausted the 32-slot enemy pool.");
            }
        }
    }

    /// <summary>Runs the breakup actor's initialization AI for one parameter 0..22.</summary>
    internal void InitializeNorfairRidleyExplosion(
        RoomEnemySlot fragment,
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        RidleyExplosionPart part = fragment.Spawn.Population.SpawnSettingsOf<RidleyExplosionSpawnSettings>().Part;

        fragment.FrameHold = WrappingCountdown.Start(1);
        fragment.Timer = WrappingCountdown.Start(0);
        fragment.Shading = state.Colours.SpriteShading(flashing: false);
        int random = _room.Random.Generate();
        int horizontalMagnitude =
            RidleyExplosionDefinitions.FragmentSpeedSixteenths.Of(random) * RidleyExplosionDefinitions.SixteenthPixelSpeed +
            RidleyExplosionDefinitions.FragmentSpeedWholePixel.Of(random) * RidleyExplosionDefinitions.WholePixelSpeed;
        var fragmentState = new RidleyExplosionFragmentState
        {
            Lifetime = RidleyExplosionDefinitions.GetPart(part).Lifetime,
            Velocity = new(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(BossRandomRolls.InLowerHalf(random)
                ? horizontalMagnitude
                : -horizontalMagnitude), default),
        };
        fragment.SetSpeciesState(fragmentState);

        // Tail fragments start from their tail segment; the body fragments start from the body.
        if (RidleyExplosionDefinitions.TailSegment.TryGetValue(part, out int tailIndex))
        {
            RidleyTailSegment tail = state.TailSegments[tailIndex];
            fragment.Position = fragment.Position.WithWhole(tail.Position);
            TrigonometryAngle tailTipDirection = tail.Angle.Direction.TurnedBy(state.TailSegments[5].Angle.Direction);
            fragmentState.Playback = new AnimationPlayback<RidleyBreakupFrame>(
                RidleyBreakupAnimations.StartingWith(RidleyBreakup.SelectTailFrame(part, tailTipDirection)), AnimationPlace.StartOfAnimation);
            return;
        }

        RidleyExplosionBodyPartDefinition bodyPart =
            RidleyBreakup.SelectBodyPart(
                part,
                facingRight: state.FacingDirection != RidleyFacing.Left);
        fragment.Position = fragment.Position.WithWhole(body.Position.Whole + bodyPart.Offset);
        fragmentState.Playback = new AnimationPlayback<RidleyBreakupFrame>(RidleyBreakupAnimations.StartingWith(bodyPart.Frame), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs the breakup actor's main AI: flicker, drag, gravity, movement, and lifetime.</summary>
    internal void RunNorfairRidleyExplosionMain(RoomEnemySlot fragment)
    {
        if (RidleyCadenceDefinitions.BreakupFragmentHidden.IsSetIn(fragment.FrameCounter))
            fragment.Presence = fragment.Presence.With(EnemyPresence.Hidden);
        else
            fragment.Presence = fragment.Presence.Without(EnemyPresence.Hidden);
        fragment.FrameCounter = RidleyCadenceDefinitions.BreakupFragmentHidden.Of(fragment.FrameCounter);

        RidleyExplosionFragmentState fragmentState = fragment.SpeciesState<RidleyExplosionFragmentState>()
            ?? throw new InvalidOperationException($"Ridley breakup fragment in slot {fragment.SlotIndex} has no state.");
        // A fragment falls only for its part's authored lifetime, so its gravity adds a few pixels an update at most.
        var velocity = new EightEightVector<RoomPixels>(
            fragmentState.Velocity.X.SlowedBy(BreakupFragmentDrag),
            fragmentState.Velocity.Y + BreakupFragmentGravity);
        fragmentState.Velocity = velocity;
        fragment.Position = fragment.Position.Advance(new(velocity.X.ToSixteenSixteenDelta(), velocity.Y.ToSixteenSixteenDelta()));

        if (--fragmentState.Lifetime >= 0)
            return;

        // The breakup actor runs the small enemy death: clear the slot immediately and
        // retain the fragment's definition and position in the shared explosion-to-pickup actor.
        _deathsAndDrops.StartGenericEnemyDeath(fragment, deathAnimation: EnemyDeathExplosion.Small);
    }
}
