using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The bridge giving way under Crocomire: its fragments, the sink into the acid, the rise and the final sink.</summary>
[RoomLifetime]
internal sealed class CrocomireBridgeCollapse
{
    /// <summary>Accesses the active room, enemy places, and shared RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Spawns Crocomire's attacks, dust, smoke, and post-death drops.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room camera's distance mode, which the fight requests.</summary>
    private readonly RoomCameraDistance _cameraDistance;
    /// <summary>Crocomire's body layer: where it is placed, the canvas his death assembles it on, the tongue placed with it and the acid smoke drawn with it.</summary>
    private readonly CrocomireBodyLayer _bodyLayer;
    /// <summary>Crocomire's state lookups, body animation start, room objects, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;
    /// <summary>Crocomire's combat, whose projectile behaviour the bridge's fragments are bound to.</summary>
    private readonly CrocomireCombat _combat;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireBridgeCollapse(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomCameraDistance cameraDistance, CrocomireBodyLayer bodyLayer, CrocomireEncounterAccess access,
        CrocomireCombat combat)
    {
        _combat = combat;
        _room = room;
        _projectiles = projectiles;
        _cameraDistance = cameraDistance;
        _bodyLayer = bodyLayer;
        _access = access;
    }

    /// <summary>
    /// Carries out the state transition. The individual bridge room objects and the
    /// subsequent melting sequence are a separate stage, but the actor/tongue state
    /// changes occur on the exact X threshold.
    /// </summary>
    internal void HandleCrocomireBridgeThreshold(CrocomireEnemyState state)
    {
        RoomEnemyPlace body = state.Body;
        HandleCrocomireBridgeApproach(body);
        if (CoordinateAxis.Difference(body.Position.X.Whole, CrocomireBehavior.CrocomireBridgeThreshold) < 0)
            return;

        state.DeathPhase = CrocomireDeathPhase.CrumbleBridgeAndSink;
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodySegment.BridgeCollapsed);
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
            CrocomireBodyLayer.StartCrocomireTongueAnimation(state, CrocomireTongueSegment.Hidden, CrocomireBridgeCollapseDefinitions.TongueHoldsUntilHidden);
            tongue.Presence = tongue.Presence.With(EnemyPresence.Hidden);
        }
    }

    /// <summary>
    /// Runs the bridge effects of the bridge approach, staged at Crocomire X 1536, 1552 and 1568.
    /// Each stage's dust lands one block ahead, and at 1584.
    /// </summary>
    private void HandleCrocomireBridgeApproach(RoomEnemyPlace body)
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
                _projectiles.SpawnDustOrExplosion(new(1536, 176), MiscDustAnimation.BigDustCloud);
            }
            return;
        }

        // One block deep crumbles the first bridge block.
        if (CoordinateAxis.Difference(body.Position.X.Whole, 1568) < 0)
        {
            if (!death.BridgeBlockAt1568Crumbling)
            {
                death.BridgeBlockAt1568Crumbling = true;
                _access.PublishCrocomireObject(
                    new Position16<RoomBlocks>(97, 11), CrocomireArenaMutation.CrumbleBridgeBlock);
                _projectiles.SpawnDustOrExplosion(new(1568, 176), MiscDustAnimation.BigDustCloud);
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
            _projectiles.SpawnDustOrExplosion(new(1584, 176), MiscDustAnimation.BigDustCloud);
        }
    }

    /// <summary>
    /// Collapses the bridge: clears its ten blocks from left to right, the blocks 97 through 106 of the row eleven blocks
    /// down, then bursts the seven dust puffs along it (<see cref="CrocomireBridgeDustPosition"/>) and puts up the
    /// invisible wall.
    /// </summary>
    private void PublishCrocomireBridgeCollapseObjects()
    {
        for (int blockX = 97; blockX <= 106; blockX++)
            _access.PublishCrocomireObject(
                new Position16<RoomBlocks>(CoordinateAxis.Wrap(blockX), 11), CrocomireArenaMutation.ClearBridgeBlock);

        for (int puff = 0; puff < CrocomireBehavior.CrocomireBridgeDustCount; puff++)
            _projectiles.SpawnDustOrExplosion(CrocomireBridgeDustPosition(puff), MiscDustAnimation.BigDustCloud);

        _access.PublishCrocomireObject(new Position16<RoomBlocks>(78, 3), CrocomireArenaMutation.CreateInvisibleWall);
    }

    /// <summary>
    /// The seven dust puffs of the collapse: sixteen pixels apart along the bridge from
    /// X 1536; the first and third rise sixteen pixels above the others' Y 192.
    /// </summary>
    internal static Position16<RoomPixels> CrocomireBridgeDustPosition(int puff) =>
        (uint)puff < CrocomireBehavior.CrocomireBridgeDustCount
            ? new Position16<RoomPixels>(1536 + 16 * puff, puff is 0 or 2 ? 176 : 192)
            : throw new ArgumentOutOfRangeException(nameof(puff));

    /// <summary>
    /// Hides his body from the room row two hundred and eighty-eight pixels below the top of Crocomire's view down while he
    /// sinks (#1515, #2031): this is what makes his body disappear into the acid as it slides down past the view's bottom.
    /// </summary>
    private void HideCrocomireSunkenBody(CrocomireEnemyState state, Position16<RoomPixels> camera) =>
        _bodyLayer.HideCrocomireBodyBelow(state, camera.Y + CrocomireBridgeCollapseDefinitions.SinkHiddenRowDepth);

    /// <summary>Runs the body and its body layer while Crocomire sinks into acid.</summary>
    internal void RunCrocomireSinkingComposite(
        CrocomireEnemyState state,
        Position16<RoomPixels> camera)
    {
        _bodyLayer.SpawnCrocomireAcidSmoke(state);
        RunCrocomireSink(state, camera);
    }

    /// <summary>Runs the body as Crocomire resurfaces.</summary>
    internal void RunCrocomireRisingComposite(CrocomireEnemyState state)
    {
        _bodyLayer.SpawnCrocomireAcidSmoke(state);
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

    /// <summary>Crocomire's exact fixed-point sinking fall, with the camera his main update runs with.</summary>
    internal void RunCrocomireSink(CrocomireEnemyState state, Position16<RoomPixels> camera)
    {
        _bodyLayer.LevelCrocomireMeltDistortion(state);
        state.DamageReactionPending = false;
        CrocomireBodyLayer.PlaceCrocomireTongue(state);
        RoomEnemyPlace body = state.Body;

        if (CoordinateAxis.Difference(body.Position.Y.Whole, 280) >= 0)
        {
            state.DeathPhase = state.DeathPhase.Next();
            state.DeathPhaseDelay = 48;
            // The next rise, after a submerged pause or directly after a melt, starts from no acceleration.
            state.VerticalAcceleration = SignedEightEight.Zero;
            return;
        }

        body.AnimationClock.ShowsNewFrame = false;

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
        // The acid hides his body below its row wherever this update leaves him, which is where his body is shown.
        HideCrocomireSunkenBody(state, camera);
    }

    /// <summary>Runs Crocomire's fixed-point rise exactly as the game does.</summary>
    internal void RunCrocomireRise(CrocomireEnemyState state)
    {
        _bodyLayer.LevelCrocomireMeltDistortion(state);
        RoomEnemyPlace body = state.Body;
        if (CoordinateAxis.Difference(body.Position.Y.Whole, 218) < 0)
        {
            state.DeathPhase = state.DeathPhase.Next();
            return;
        }

        CrocomireBodyLayer.PlaceCrocomireTongue(state);
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

    /// <summary>Selects the body melting animation for the rows still above the acid.</summary>
    internal static void SelectCrocomireRisingAnimation(CrocomireEnemyState state)
    {
        int y = state.Body.Position.Y.Whole;
        CrocomireBodySegment segment = y < 248
            ? CrocomireBodySegment.MeltingTwoTopFourRows
            : y < 264
                ? CrocomireBodySegment.MeltingTwoTopThreeRows
                : y < 280
                    ? CrocomireBodySegment.MeltingTwoTopTwoRows
                    : CrocomireBodySegment.MeltingTwoTopRow;
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, segment);
    }

    /// <summary>Completes the acid sink and hands control to the skeleton river sequence.</summary>
    internal void RunCrocomireFinalSink(CrocomireEnemyState state, Position16<RoomPixels> camera)
    {
        SelectCrocomireRisingAnimation(state);
        _bodyLayer.SpawnCrocomireAcidSmoke(state);
        RunCrocomireSink(state, camera);
        if (state.DeathPhase != CrocomireDeathPhase.WaitForSamusAtWall)
            return;

        _access.RequestCrocomireMusic(MusicRequest.SelectTrack(MusicTrack.BossRoomTension), MusicQueueDelay.Shortest);
        state.DeathPhase = CrocomireDeathPhase.RiverSkeletonDetour;
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodySegment.SkeletonFlowingDownRiver);
        _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.BridgeLeftScreen, RoomScrollState.OpenAlignedToTop);
        _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.BridgeScreen, RoomScrollState.OpenAlignedToTop);
        if (state.Tongue is { } tongue)
            tongue.Presence = tongue.Presence.With(EnemyPresence.Deleted);
        _access.PublishCrocomireObject(new Position16<RoomBlocks>(78, 3), CrocomireArenaMutation.ClearInvisibleWall);
        _cameraDistance.Request(CameraDistanceMode.NormalTracking);
        _access.RequireCrocomireDeath().SkeletonBaringElapsed = 0;
    }

    /// <summary>Picks the next crumbling bridge column and runs the bridge fragment's initializer.</summary>
    internal void SpawnNextCrocomireBridgeFragment()
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        int fragment = death.BridgeFragmentsSpawned;
        if (fragment >= CrocomireBridgeFragmentDefinitions.FragmentCount)
            return;

        // The fragment counts as launched even when no projectile place is free. Its colours are the wall's, set below.
        death.BridgeFragmentsSpawned = fragment + 1;
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireBridgeCrumbling,
            _combat.Debris,
            graphics: default);
        projectile.StartAnimation(new CrocomireProjectileAnimation(CrocomireProjectileSegment.BridgeFragment));
        projectile.Position = projectile.Position.WithWholeX(CrocomireBridgeFragmentDefinitions.XPosition(fragment));
        projectile.Position = projectile.Position.WithWholeY(187).WithFractionX(0).WithFractionY(0);
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.Zero);
        // The fragment's fall speed reads the room's random number without drawing a new one.
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(CrocomireRandomDefinitions.BridgeFragmentFallSpeed.Of(_room.Random.Read()) + 64));
        // The fragments draw in the wall colours Crocomire's encounter installs.
        projectile.Graphics = SpriteGraphics.SharedIn(CrocomireColours.WallShading);
    }
}
