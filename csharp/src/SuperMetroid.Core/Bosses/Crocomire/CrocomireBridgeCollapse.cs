using SuperMetroid.Core.Bosses.Crocomire.Artwork;
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
        HandleCrocomireBridgeApproach(body, state.Death);
        if (body.Position.X.Whole < CrocomireBridgeCollapseDefinitions.CollapseStartX)
            return;

        state.DeathPhase = CrocomireDeathPhase.CrumbleBridgeAndSink;
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodySegment.BridgeCollapsed);
        body.Contact = body.Contact.With(EnemyContact.SolidToSamus);
        state.StartVerticalMotion(CrocomireBridgeCollapseDefinitions.SinkStartingAcceleration);
        body.Radius = body.Radius.WithY(16);
        _access.RequestCrocomireSound(CrocomireSoundDefinitions.BridgeCollapse);
        CrocomireDeathState death = state.Death;
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
    /// Runs the bridge effects of the bridge approach, in stages a block apart from the bridge's left edge
    /// (<see cref="CrocomireBridgeCollapseDefinitions.ApproachStartX"/>): the first raises dust at the edge, the second
    /// crumbles the bridge's first block and the third its next two, each raising dust at its own stage.
    /// </summary>
    private void HandleCrocomireBridgeApproach(RoomEnemyPlace body, CrocomireDeathState death)
    {
        int x = body.Position.X.Whole;
        if (x < ApproachStage(0))
        {
            death.ApproachDustRaised = false;
            death.FirstBridgeBlockCrumbling = false;
            death.NextBridgeBlocksCrumbling = false;
            return;
        }

        if (x < ApproachStage(1))
        {
            if (!death.ApproachDustRaised)
            {
                death.ApproachDustRaised = true;
                RaiseBridgeDust(ApproachStage(0));
            }
            return;
        }

        // One block deep crumbles the first bridge block.
        if (x < ApproachStage(2))
        {
            if (!death.FirstBridgeBlockCrumbling)
            {
                death.FirstBridgeBlockCrumbling = true;
                _access.PublishCrocomireObject(CrocomireArenaDefinitions.BridgeFirstBlock, CrocomireArenaMutation.CrumbleBridgeBlock);
                RaiseBridgeDust(ApproachStage(2));
            }
            return;
        }

        // Two blocks deep crumbles the next pair.
        if (x < ApproachStage(3) &&
            !death.NextBridgeBlocksCrumbling)
        {
            death.NextBridgeBlocksCrumbling = true;
            _access.PublishCrocomireObject(CrocomireArenaDefinitions.BridgeFirstBlock + new Offset16<RoomBlocks>(1, 0), CrocomireArenaMutation.CrumbleBridgeBlock);
            _access.PublishCrocomireObject(CrocomireArenaDefinitions.BridgeFirstBlock + new Offset16<RoomBlocks>(2, 0), CrocomireArenaMutation.CrumbleBridgeBlock);
            RaiseBridgeDust(ApproachStage(3));
        }
    }

    /// <summary>Where stage <paramref name="stage"/> of his approach onto the bridge begins, the first stage at the bridge's left edge.</summary>
    private static int ApproachStage(int stage) =>
        CrocomireBridgeCollapseDefinitions.ApproachStartX + stage * CrocomireBridgeCollapseDefinitions.ApproachStageWidth;

    /// <summary>Raises a big dust cloud on the bridge at <paramref name="x"/>.</summary>
    private void RaiseBridgeDust(int x) =>
        _projectiles.SpawnDustOrExplosion(new(x, CrocomireBridgeCollapseDefinitions.DustY), DustExplosionAnimation.BigDustCloud);

    /// <summary>
    /// Clears every bridge block, left to right, and bursts dust as the bridge collapses: the clears are published in that
    /// order, then the dust, then the invisible wall behind Samus.
    /// </summary>
    private void PublishCrocomireBridgeCollapseObjects()
    {
        for (int block = 0; block < CrocomireArenaDefinitions.BridgeBlockCount; block++)
            _access.PublishCrocomireObject(
                CrocomireArenaDefinitions.BridgeFirstBlock + new Offset16<RoomBlocks>(block, 0), CrocomireArenaMutation.ClearBridgeBlock);

        for (int puff = 0; puff < CrocomireBridgeCollapseDefinitions.CollapseDustCount; puff++)
            _projectiles.SpawnDustOrExplosion(CrocomireBridgeDustPosition(puff), DustExplosionAnimation.BigDustCloud);

        _access.PublishCrocomireObject(CrocomireArenaDefinitions.BridgeWallBlock, CrocomireArenaMutation.CreateInvisibleWall);
    }

    /// <summary>
    /// The collapse's dust puffs, a stage apart along the bridge from its left edge; the first and third rise to the
    /// approach's dust row, the others lie on the lower row.
    /// </summary>
    internal static Position16<RoomPixels> CrocomireBridgeDustPosition(int puff) =>
        puff is >= 0 and < CrocomireBridgeCollapseDefinitions.CollapseDustCount
            ? new Position16<RoomPixels>(ApproachStage(puff),
                puff is 0 or 2 ? CrocomireBridgeCollapseDefinitions.DustY : CrocomireBridgeCollapseDefinitions.LowDustY)
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

    /// <summary>
    /// Runs the body and its body layer while Crocomire, melted once, sinks into the acid again (#2193): each update
    /// shows the first melt's picture of the rows still above the acid where he stands, before he sinks.
    /// </summary>
    internal void RunCrocomireFirstMeltSinkingComposite(CrocomireEnemyState state, Position16<RoomPixels> camera)
    {
        _bodyLayer.SpawnCrocomireAcidSmoke(state);
        SelectCrocomireMeltAnimation(state, CrocomireMeltingIllustration.Flesh);
        RunCrocomireSink(state, camera);
    }

    /// <summary>
    /// Runs the body as Crocomire, melted once, resurfaces (#2193): each update shows the first melt's picture of the rows
    /// above the acid where he stands, before he rises.
    /// </summary>
    internal void RunCrocomireFirstMeltRisingComposite(CrocomireEnemyState state)
    {
        _bodyLayer.SpawnCrocomireAcidSmoke(state);
        SelectCrocomireMeltAnimation(state, CrocomireMeltingIllustration.Flesh);
        RunCrocomireRise(state);
    }

    /// <summary>Queues periodic acid sounds according to the death-scene timer.</summary>
    internal void RunCrocomireAcidSoundTimer(CrocomireDeathState death)
    {
        if (death.AcidSoundTimer == 0)
            return;
        if (--death.AcidSoundTimer != 0)
            return;
        death.AcidSoundTimer = CrocomireBridgeCollapseDefinitions.AcidSoundInterval;
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
        state.VerticalSpeed = CrocomireBridgeCollapseDefinitions.MaximumSinkSpeed;
    }

    /// <summary>Crocomire's exact fixed-point sinking fall, with the camera his main update runs with.</summary>
    internal void RunCrocomireSink(CrocomireEnemyState state, Position16<RoomPixels> camera)
    {
        CrocomireBodyLayer.LevelCrocomireMeltDistortion(state);
        state.DamageReactionPending = false;
        CrocomireBodyLayer.PlaceCrocomireTongue(state);
        RoomEnemyPlace body = state.Body;

        if (body.Position.Y.Whole >= CrocomireBridgeCollapseDefinitions.SunkenY)
        {
            state.DeathPhase = state.DeathPhase.Next();
            state.DeathPhaseDelay = CrocomireBridgeCollapseDefinitions.SubmergedPause;
            // The next rise, after a submerged pause or directly after a melt, starts from no acceleration.
            state.VerticalAcceleration = SignedSixteenSixteen.Zero;
            return;
        }

        body.AnimationClock.ShowsNewFrame = false;

        // The acceleration grows, its 256ths stopping at the sink's most with its finer part kept; the speed then gains it
        // as the sinking speed gains (CrocomireSpeedGain).
        SignedSixteenSixteen acceleration = (state.VerticalAcceleration + CrocomireBridgeCollapseDefinitions.SinkAccelerationIncrement)
            .WithTwoFiftySixthsAtMost(CrocomireBridgeCollapseDefinitions.MaximumSinkAcceleration);
        state.VerticalAcceleration = acceleration;
        SignedEightEight speed = CrocomireSpeedGain.Gained(state.VerticalSpeed, acceleration, CrocomireBridgeCollapseDefinitions.MaximumSinkSpeed);
        state.VerticalSpeed = speed;

        SignedEightEight travelled = state.VerticalFraction + speed;
        state.VerticalFraction = travelled.WithoutWholePixels();
        body.Position = body.Position.WithWholeY(body.Position.Y.Whole + travelled.WholePixels);
        // The acid hides his body below its row wherever this update leaves him, which is where his body is shown.
        HideCrocomireSunkenBody(state, camera);
    }

    /// <summary>Runs Crocomire's fixed-point rise exactly as the game does.</summary>
    internal static void RunCrocomireRise(CrocomireEnemyState state)
    {
        CrocomireBodyLayer.LevelCrocomireMeltDistortion(state);
        RoomEnemyPlace body = state.Body;
        if (body.Position.Y.Whole < CrocomireBridgeCollapseDefinitions.RisenY)
        {
            state.DeathPhase = state.DeathPhase.Next();
            return;
        }

        CrocomireBodyLayer.PlaceCrocomireTongue(state);
        SignedSixteenSixteen acceleration = SignedSixteenSixteen.Min(
            state.VerticalAcceleration + CrocomireBridgeCollapseDefinitions.RiseAccelerationIncrement, CrocomireBridgeCollapseDefinitions.MaximumRiseAcceleration);
        state.VerticalAcceleration = acceleration;

        // The speed loses the acceleration, which every rise starts from none and grows by whole 256ths of a pixel; a speed
        // that would fall below zero stalls just under one pixel per update instead of reversing.
        SignedEightEight speed = state.VerticalSpeed - SignedEightEight.FromTwoHundredFiftySixthsOfPixel(acceleration.InTwoFiftySixths);
        if (speed.IsNegative)
            speed = CrocomireBridgeCollapseDefinitions.RiseSpeedAfterStalling;
        state.VerticalSpeed = speed;

        // The rise moves up: the carried fraction less the speed, its whole pixels zero or below.
        SignedEightEight remaining = state.VerticalFraction - speed;
        state.VerticalFraction = remaining.WithoutWholePixels();
        body.Position = body.Position.WithWholeY(body.Position.Y.Whole + remaining.WholePixels);
    }

    /// <summary>
    /// Starts, afresh on every update, <paramref name="illustration"/>'s melting animation for the rows of Crocomire's body
    /// still above the acid where he stands: his top row alone from <see cref="CrocomireMeltingDefinitions.TopRowAboveAcidY"/>
    /// down, his top two rows from <see cref="CrocomireMeltingDefinitions.TopTwoRowsAboveAcidY"/>, his top three from
    /// <see cref="CrocomireMeltingDefinitions.TopThreeRowsAboveAcidY"/>, and his top four above that.
    /// </summary>
    internal static void SelectCrocomireMeltAnimation(CrocomireEnemyState state, CrocomireMeltingIllustration illustration)
    {
        // Each melt row lies a row's height above the next; the top row alone shows once he has sunk.
        int y = state.Body.Position.Y.Whole;
        int rowsAboveAcid = y >= CrocomireMeltingDefinitions.TopRowAboveAcidY
            ? 1
            : y >= CrocomireMeltingDefinitions.TopTwoRowsAboveAcidY
                ? 2
                : y >= CrocomireMeltingDefinitions.TopThreeRowsAboveAcidY
                    ? 3
                    : 4;
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, MeltAnimation(illustration, rowsAboveAcid));
    }

    /// <summary>The body animation showing <paramref name="illustration"/>'s top <paramref name="rowsAboveAcid"/> rows.</summary>
    private static CrocomireBodySegment MeltAnimation(CrocomireMeltingIllustration illustration, int rowsAboveAcid) =>
        illustration switch
        {
            CrocomireMeltingIllustration.Flesh => rowsAboveAcid switch
            {
                1 => CrocomireBodySegment.MeltingFleshTopRow,
                2 => CrocomireBodySegment.MeltingFleshTopTwoRows,
                3 => CrocomireBodySegment.MeltingFleshTopThreeRows,
                4 => CrocomireBodySegment.MeltingFleshTopFourRows,
                _ => throw new ArgumentOutOfRangeException(nameof(rowsAboveAcid), rowsAboveAcid,
                    "Crocomire's first melting animations show one to four rows above the acid."),
            },
            CrocomireMeltingIllustration.BareSkull => rowsAboveAcid switch
            {
                1 => CrocomireBodySegment.MeltingBareSkullTopRow,
                2 => CrocomireBodySegment.MeltingBareSkullTopTwoRows,
                3 => CrocomireBodySegment.MeltingBareSkullTopThreeRows,
                4 => CrocomireBodySegment.MeltingBareSkullTopFourRows,
                _ => throw new ArgumentOutOfRangeException(nameof(rowsAboveAcid), rowsAboveAcid,
                    "Crocomire's second melting animations show one to four rows above the acid."),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(illustration), illustration,
                "Crocomire melts through his first and second illustrations only."),
        };

    /// <summary>Completes the acid sink and hands control to the skeleton river sequence.</summary>
    internal void RunCrocomireFinalSink(CrocomireEnemyState state, Position16<RoomPixels> camera)
    {
        SelectCrocomireMeltAnimation(state, CrocomireMeltingIllustration.BareSkull);
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
        _access.PublishCrocomireObject(CrocomireArenaDefinitions.BridgeWallBlock, CrocomireArenaMutation.ClearInvisibleWall);
        _cameraDistance.Request(CameraDistanceMode.NormalTracking);
        state.Death.SkeletonBaringElapsed = 0;
    }

    /// <summary>Picks the next crumbling bridge column and runs the bridge fragment's setup.</summary>
    internal void SpawnNextCrocomireBridgeFragment(CrocomireDeathState death)
    {
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
        projectile.Position = projectile.Position.WithWholeY(CrocomireBridgeFragmentDefinitions.Y).WithFractionX(0).WithFractionY(0);
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.Zero);
        // The fragment's fall speed reads the room's random number without drawing a new one.
        projectile.Velocity = projectile.Velocity.WithY(CrocomireBridgeFragmentDefinitions.SlowestFall +
            SignedEightEight.FromTwoHundredFiftySixthsOfPixel(CrocomireRandomDefinitions.BridgeFragmentFallSpeedSpread.Of(_room.Random.Read())));
        // The fragments draw in the wall colours Crocomire's encounter installs.
        projectile.Graphics = SpriteGraphics.SharedIn(CrocomireColours.WallShading);
    }
}
