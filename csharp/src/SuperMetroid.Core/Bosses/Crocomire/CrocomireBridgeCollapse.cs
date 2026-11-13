using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The bridge giving way under Crocomire: its fragments, the sink into the acid, the rise and the final sink.</summary>
[RoomLifetime]
internal sealed class CrocomireBridgeCollapse
{
    /// <summary>Accesses the active room, enemy slots, and shared RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Spawns Crocomire's attacks, dust, smoke, and post-death drops.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Owns camera-distance mode changes during Crocomire's arena sequence.</summary>
    private readonly ZebetiteBehavior _zebetiteBehavior;
    /// <summary>Crocomire's body layer: where it is placed, the canvas his death assembles it on, the tongue placed with it and the acid smoke drawn with it.</summary>
    private readonly CrocomireBodyLayer _bodyLayer;
    /// <summary>Crocomire's state lookups, body program start, room objects, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireBridgeCollapse(EnemyRoomBinding room, EnemyProjectileEngine projectiles, ZebetiteBehavior zebetiteBehavior, CrocomireBodyLayer bodyLayer, CrocomireEncounterAccess access)
    {
        _room = room;
        _projectiles = projectiles;
        _zebetiteBehavior = zebetiteBehavior;
        _bodyLayer = bodyLayer;
        _access = access;
    }

    /// <summary>True after the X threshold of 1600 starts the bridge-collapse phase.</summary>
    public bool CrocomireBridgeCollapseStarted { get; private set; }

    /// <summary>
    /// Carries out the state transition. The individual bridge room objects and the
    /// subsequent melting sequence are a separate stage, but the actor/tongue state
    /// changes occur on the exact X threshold.
    /// </summary>
    internal void HandleCrocomireBridgeThreshold(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        HandleCrocomireBridgeApproach(body);
        if (CoordinateAxis.Difference(body.Position.X.Whole, CrocomireBehavior.CrocomireBridgeThreshold) < 0)
            return;

        CrocomireBridgeCollapseStarted = true;
        state.DeathPhase = CrocomireDeathPhase.CrumbleBridgeAndSink;
        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, CrocomireBodyProgram.BridgeCollapsed);
        body.Contact = body.Contact.With(EnemyContact.SolidToSamus);
        state.StartVerticalMotion(CrocomireBehavior.SinkStartingAcceleration);
        body.Radius = body.Radius.WithY(16);
        _access.RequestCrocomireSound(CrocomireSoundDefinitions.BridgeCollapse);
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        death.BridgeFragmentsSpawned = 0;
        death.AcidSmokeTimer = 1;
        death.AcidSoundTimer = 1;

        PublishCrocomireBridgeCollapseObjects();

        if (state.Tongue is { } tongue)
        {
            // The sleep is installed behind the maximum timer, so it is reached only after 32767 updates.
            CrocomireBodyLayer.StartCrocomireTongueProgram(state, CrocomireTongueProgram.Sleep, WrappingCountdown.Start(32767));
            tongue.Presence = tongue.Presence.With(EnemyPresence.Hidden);
        }
    }

    /// <summary>
    /// Runs the bridge effects of the bridge approach, staged at Crocomire X 1536, 1552 and 1568.
    /// Each stage's dust lands one block ahead, and at 1584.
    /// </summary>
    private void HandleCrocomireBridgeApproach(RoomEnemySlot body)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (CoordinateAxis.Difference(body.Position.X.Whole, 1536) < 0)
        {
            death.BridgeDustAt1536Spawned = false;
            death.BridgeBlockAt1568Crumbling = false;
            death.BridgeBlocksAt1584Crumbling = false;
            return;
        }

        if (CoordinateAxis.Difference(body.Position.X.Whole, 1552) < 0)
        {
            if (!death.BridgeDustAt1536Spawned)
            {
                death.BridgeDustAt1536Spawned = true;
                _projectiles.SpawnRoomGraphicsDustExplosion(new(1536, 176), MiscDustAnimation.BigDustCloud);
            }
            return;
        }

        // One block deep crumbles the first bridge block. The flag
        // resets and the collapse step sit behind comparisons these ranges already passed
        // and never execute.
        if (CoordinateAxis.Difference(body.Position.X.Whole, 1568) < 0)
        {
            if (!death.BridgeBlockAt1568Crumbling)
            {
                death.BridgeBlockAt1568Crumbling = true;
                _access.PublishCrocomireObject(
                    new Position16<RoomBlocks>(97, 11), CrocomireArenaMutation.CrumbleBridgeBlock);
                _projectiles.SpawnRoomGraphicsDustExplosion(new(1568, 176), MiscDustAnimation.BigDustCloud);
            }
            return;
        }

        // Two blocks deep crumbles the next pair.
        if (CoordinateAxis.Difference(body.Position.X.Whole, 1584) < 0 &&
            !death.BridgeBlocksAt1584Crumbling)
        {
            death.BridgeBlocksAt1584Crumbling = true;
            _access.PublishCrocomireObject(
                new Position16<RoomBlocks>(98, 11), CrocomireArenaMutation.CrumbleBridgeBlock);
            _access.PublishCrocomireObject(
                new Position16<RoomBlocks>(99, 11), CrocomireArenaMutation.CrumbleBridgeBlock);
            _projectiles.SpawnRoomGraphicsDustExplosion(new(1584, 176), MiscDustAnimation.BigDustCloud);
        }
    }

    /// <summary>
    /// Clears all ten bridge blocks and bursts dust as the bridge collapses.
    /// The ten room-object spawns exactly use
    /// block X=97+i, block Y=11, and the bridge-block clearing for i=0..9. The
    /// inclusive production loop preserves their ascending publication
    /// order; the dust setup follows, not an eleventh clear.
    /// The seven dust positions follow the
    /// exact bounded rule X=1536+16*i for i=0..6; Y=176 at i=0 or 2
    /// and 192 otherwise. The original game writes these out one by one,
    /// not as a table. All seven pairs and their
    /// order match the authored ones; there is no eighth dust spawn.
    /// </summary>
    private void PublishCrocomireBridgeCollapseObjects()
    {
        for (int blockX = 97; blockX <= 106; blockX++)
            _access.PublishCrocomireObject(
                new Position16<RoomBlocks>(CoordinateAxis.Wrap(blockX), 11), CrocomireArenaMutation.ClearBridgeBlock);

        for (int puff = 0; puff < CrocomireBehavior.CrocomireBridgeDustCount; puff++)
            _projectiles.SpawnRoomGraphicsDustExplosion(CrocomireBridgeDustPosition(puff), MiscDustAnimation.BigDustCloud);

        _access.PublishCrocomireObject(new Position16<RoomBlocks>(78, 3), CrocomireArenaMutation.CreateInvisibleWall);
    }

    /// <summary>
    /// The seven unrolled dust spawns: puffs sixteen pixels apart along the bridge from
    /// X 1536; the first and third rise sixteen pixels above the others' Y 192.
    /// </summary>
    internal static Position16<RoomPixels> CrocomireBridgeDustPosition(int puff) =>
        (uint)puff < CrocomireBehavior.CrocomireBridgeDustCount
            ? new Position16<RoomPixels>(1536 + 16 * puff, puff is 0 or 2 ? 176 : 192)
            : throw new ArgumentOutOfRangeException(nameof(puff));

    /// <summary>
    /// Hides his body from the body row two hundred and eighty-eight pixels below the top of Crocomire's view down while he
    /// sinks (#1515): this is what makes his body disappear into the acid as it slides down past the view's bottom.
    /// </summary>
    private void HideCrocomireSunkenBody() =>
        _bodyLayer.HideCrocomireBodyFrom(CoordinateAxis.Wrap(_bodyLayer.Placement.Y + SinkRowDepth));

    /// <summary>How far below the top of Crocomire's view, in pixels, the row from which his sinking hides his body lies.</summary>
    private const int SinkRowDepth = 288;

    /// <summary>Runs the body and its body layer while Crocomire sinks into acid.</summary>
    internal void RunCrocomireSinkingComposite(
        CrocomireEnemyState state,
        SamusState? samus)
    {
        _bodyLayer.SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireSink(state);
    }

    /// <summary>Runs the body and its body layer as Crocomire resurfaces.</summary>
    internal void RunCrocomireRisingComposite(
        CrocomireEnemyState state,
        SamusState? samus)
    {
        _bodyLayer.SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireRise(state);
    }

    /// <summary>Queues periodic acid sounds according to the death-scene timer.</summary>
    internal void RunCrocomireAcidSoundTimer()
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (death.AcidSoundTimer == 0)
            return;
        if (--death.AcidSoundTimer != 0)
            return;
        death.AcidSoundTimer = 32;
        _access.RequestCrocomireSound(CrocomireSoundDefinitions.AcidBurn);
    }

    /// <summary>Holds the submerged skeleton for its authored delay before rising begins.</summary>
    internal static void RunCrocomireSubmergedPause(CrocomireEnemyState state)
    {
        if (state.DeathPhaseDelay != 0)
        {
            state.DeathPhaseDelay--;
            return;
        }
        state.DeathPhase = state.DeathPhase.Next();
        state.VerticalSpeed = SignedEightEight.FromPixels(CrocomireBehavior.MaximumSinkSpeedWhole);
    }

    /// <summary>Crocomire's exact fixed-point sinking fall.</summary>
    internal void RunCrocomireSink(CrocomireEnemyState state)
    {
        _bodyLayer.LevelCrocomireMeltDistortion(_bodyLayer.Placement.Y);
        state.FightFlags &= ~CrocomireFightFlags.DamageReactionPending;
        _bodyLayer.PlaceCrocomireBodyLayer(state);
        RoomEnemySlot body = state.Body;

        if (CoordinateAxis.Difference(body.Position.Y.Whole, 280) >= 0)
        {
            state.DeathPhase = state.DeathPhase.Next();
            state.DeathPhaseDelay = 48;
            // The next rise, after a submerged pause or directly after a melt, starts from no acceleration.
            state.VerticalAcceleration = SignedEightEight.Zero;
            return;
        }

        HideCrocomireSunkenBody();
        body.NewInstructionFrame = false;

        // The acceleration's fraction carries into its whole part before the whole part is clamped, and the
        // speed's into its whole part before that is clamped: each clamp keeps the fraction it carried from.
        SignedEightEight acceleration = (state.VerticalAcceleration + CrocomireBehavior.SinkAccelerationIncrement)
            .WithWholePixelsAtMost(CrocomireBehavior.MaximumSinkAccelerationWhole);
        state.VerticalAcceleration = acceleration;
        // The speed grows by the acceleration's whole pixels read as 256ths of a pixel.
        SignedEightEight speed = (state.VerticalSpeed + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(acceleration.WholePixels))
            .WithWholePixelsAtMost(CrocomireBehavior.MaximumSinkSpeedWhole);
        state.VerticalSpeed = speed;

        SignedEightEight travelled = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(state.VerticalSubpixel) + speed;
        state.VerticalSubpixel = travelled.FractionSubpixels;
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole + travelled.WholePixels);
    }

    /// <summary>Byte-for-byte fixed-point rise.</summary>
    internal void RunCrocomireRise(CrocomireEnemyState state)
    {
        _bodyLayer.LevelCrocomireMeltDistortion(_bodyLayer.Placement.Y);
        RoomEnemySlot body = state.Body;
        if (CoordinateAxis.Difference(body.Position.Y.Whole, 218) < 0)
        {
            state.DeathPhase = state.DeathPhase.Next();
            return;
        }

        _bodyLayer.PlaceCrocomireBodyLayer(state);
        SignedEightEight acceleration = SignedEightEight.Min(
            state.VerticalAcceleration + SignedEightEight.FromPixels(1), CrocomireBehavior.MaximumRiseAcceleration);
        state.VerticalAcceleration = acceleration;

        // The speed loses the acceleration's whole pixels read as 256ths of a pixel; a speed that would fall below zero
        // stalls just under one pixel per frame instead of reversing.
        SignedEightEight speed = state.VerticalSpeed - SignedEightEight.FromTwoHundredFiftySixthsOfPixel(acceleration.WholePixels);
        if (speed.IsNegative)
            speed = CrocomireBehavior.RiseSpeedAfterStalling;
        state.VerticalSpeed = speed;

        // The rise moves up: the subpixel less the speed, its whole pixels zero or below.
        SignedEightEight remaining = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(state.VerticalSubpixel) - speed;
        state.VerticalSubpixel = remaining.FractionSubpixels;
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole + remaining.WholePixels);
    }

    /// <summary>Selects the body melting program for the rows still above the acid.</summary>
    internal static void SelectCrocomireRisingInstruction(CrocomireEnemyState state)
    {
        int y = state.Body.Position.Y.Whole;
        CrocomireBodyProgram program = y < 248
            ? CrocomireBodyProgram.MeltingTwoTopFourRows
            : y < 264
                ? CrocomireBodyProgram.MeltingTwoTopThreeRows
                : y < 280
                    ? CrocomireBodyProgram.MeltingTwoTopTwoRows
                    : CrocomireBodyProgram.MeltingTwoTopRow;
        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, program);
    }

    /// <summary>Completes the acid sink and hands control to the skeleton river sequence.</summary>
    internal void RunCrocomireFinalSink(CrocomireEnemyState state, SamusState? samus)
    {
        SelectCrocomireRisingInstruction(state);
        _bodyLayer.SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireSink(state);
        if (state.DeathPhase != CrocomireDeathPhase.WaitForSamusAtWall)
            return;

        _access.RequestCrocomireMusic(MusicRequest.SelectTrack(MusicTrack.SecondSetSong), MusicQueueDelay.Shortest);
        state.DeathPhase = CrocomireDeathPhase.RiverSkeletonDetour;
        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, CrocomireBodyProgram.SkeletonFlowingDownRiver);
        _room.Services.RoomScroll.Write(4, RoomScrollState.Blue);
        _room.Services.RoomScroll.Write(5, RoomScrollState.Blue);
        if (state.Tongue is { } tongue)
            tongue.Presence = tongue.Presence.With(EnemyPresence.Deleted);
        _access.PublishCrocomireObject(new Position16<RoomBlocks>(78, 3), CrocomireArenaMutation.ClearInvisibleWall);
        _zebetiteBehavior.CameraDistance = CameraDistanceMode.NormalTracking;
        _access.RequireCrocomireDeath().SkeletonBaringElapsed = 0;
    }

    /// <summary>Picks the next crumbling bridge column and runs the bridge fragment's initializer.</summary>
    internal void SpawnNextCrocomireBridgeFragment()
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        int fragment = death.BridgeFragmentsSpawned;
        if (fragment >= CrocomireBridgeFragmentDefinitions.FragmentCount)
            return;

        // The fragment counts as launched even when no projectile slot is free. The graphics argument the
        // passes alongside it is immaterial: the definition initializer immediately replaces it.
        death.BridgeFragmentsSpawned = fragment + 1;
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireBridgeCrumbling,
            graphics: default);
        projectile.StartAnimation(new CrocomireProjectileAnimation(CrocomireProjectileFrameKind.BridgeFragment));
        projectile.Position = projectile.Position.WithWholeX(CrocomireBridgeFragmentDefinitions.XPosition(fragment));
        projectile.Position = projectile.Position.WithWholeY(187).WithFractionX(0).WithFractionY(0);
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.Zero);
        // The fragment initializer reads RandomNumberSeed; it does not advance it.
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(CrocomireRandomDefinitions.BridgeFragmentFallSpeed.Of(_room.Random.Read()) + 64));
        // The fragments draw in the wall colours Crocomire's encounter installs.
        projectile.Graphics = SpriteGraphics.SharedIn(CrocomireColours.WallShading);
    }
}
