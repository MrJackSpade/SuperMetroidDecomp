using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Scrolling;
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
    /// Runs his approach onto the bridge, and starts his death once his body reaches the point where its weight breaks the
    /// bridge (<see cref="CrocomireBridgeCollapseDefinitions.CollapseStartX"/>): he becomes solid to Samus and starts to sink
    /// through the bridge, which clears and bursts into dust, and his tongue hides.
    /// </summary>
    internal void HandleCrocomireBridgeThreshold(CrocomireEnemyState state)
    {
        RoomEnemyPlace body = state.Body;
        HandleCrocomireBridgeApproach(body, state.Death);
        if (body.Position.X.Whole < CrocomireBridgeCollapseDefinitions.CollapseStartX)
            return;

        state.AdvanceDeathStage();
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodyAnimation.BridgeCollapsed);
        body.Contact = body.Contact.With(EnemyContact.SolidToSamus);
        state.StartVerticalMotion(CrocomireBridgeCollapseDefinitions.SinkStartingAcceleration);
        body.Radius = body.Radius.WithY(CrocomireBridgeCollapseDefinitions.SinkingReachY);
        _access.RequestCrocomireSound(CrocomireSoundDefinitions.BridgeCollapse);
        CrocomireDeathState death = state.Death;
        death.BridgeFragmentsSpawned = 0;
        // The acid smokes and burns on his death's first update, and at their intervals after.
        death.AcidSmokeWait.LastFor(1);
        death.AcidSoundWait.LastFor(1);

        PublishCrocomireBridgeCollapseObjects();

        CrocomireBodyLayer.HideCrocomireTongue(state);
    }

    /// <summary>
    /// Runs the stage of his approach onto the bridge his body stands at (<see cref="CrocomireBridgeCollapseDefinitions.ApproachStages"/>),
    /// once as he first reaches it: its blocks start to crumble and its dust rises. Standing off the bridge, left of its
    /// edge, starts his approach over.
    /// </summary>
    private void HandleCrocomireBridgeApproach(RoomEnemyPlace body, CrocomireDeathState death)
    {
        int x = body.Position.X.Whole;
        if (x < CrocomireBridgeCollapseDefinitions.ApproachStartX)
        {
            death.ApproachStagesRun = 0;
            return;
        }

        // He moves a step at a time, less than a stage's width, so he reaches each stage only from the one before it.
        int stage = (x - CrocomireBridgeCollapseDefinitions.ApproachStartX) / CrocomireBridgeCollapseDefinitions.ApproachStageWidth;
        if (stage < death.ApproachStagesRun || stage >= CrocomireBridgeCollapseDefinitions.ApproachStages.Count)
            return;

        death.ApproachStagesRun = stage + 1;
        CrocomireBridgeApproachStage reached = CrocomireBridgeCollapseDefinitions.ApproachStages[stage];
        foreach (Position16<RoomBlocks> block in reached.CrumblingBlocks)
            _access.PublishCrocomireObject(block, CrocomireArenaMutation.CrumbleBridgeBlock);
        RaiseBridgeDust(reached.Dust);
    }

    /// <summary>Raises a big dust cloud on the bridge at <paramref name="position"/>.</summary>
    private void RaiseBridgeDust(Position16<RoomPixels> position) =>
        _projectiles.SpawnDustOrExplosion(position, DustExplosionAnimation.BigDustCloud);

    /// <summary>
    /// Clears every bridge block, left to right, and bursts dust as the bridge collapses: the clears are published in that
    /// order, then the dust, then the invisible wall behind Samus.
    /// </summary>
    private void PublishCrocomireBridgeCollapseObjects()
    {
        for (int block = 0; block < CrocomireArenaDefinitions.BridgeBlockCount; block++)
            _access.PublishCrocomireObject(
                CrocomireArenaDefinitions.BridgeFirstBlock + new Offset16<RoomBlocks>(block, 0), CrocomireArenaMutation.ClearBridgeBlock);

        foreach (Position16<RoomPixels> puff in CrocomireBridgeCollapseDefinitions.CollapseDust)
            RaiseBridgeDust(puff);

        _access.PublishCrocomireObject(CrocomireArenaDefinitions.BridgeWallBlock, CrocomireArenaMutation.CreateInvisibleWall);
    }

    /// <summary>
    /// Hides his body from the room row two hundred and eighty-eight pixels below the top of Crocomire's view down while he
    /// sinks (#1515, #2031): this is what makes his body disappear into the acid as it slides down past the view's bottom.
    /// </summary>
    private void HideCrocomireSunkenBody(CrocomireEnemyState state, Position16<RoomPixels> camera) =>
        _bodyLayer.HideCrocomireBodyBelow(state, camera.Y + CrocomireBridgeCollapseDefinitions.SinkHiddenRowDepth);

    /// <summary>
    /// Runs his body for one update of a sink into the acid as <paramref name="body"/> (#2193): the acid smokes about him
    /// and, while he is whole, burns audibly; once he has melted his body shows its melting picture of the rows still above
    /// the acid where he stands; and he sinks.
    /// </summary>
    internal void RunCrocomireSinkingComposite(CrocomireEnemyState state, CrocomireBodyInAcid body, Position16<RoomPixels> camera)
    {
        ShowCrocomireInAcid(state, body);
        RunCrocomireSink(state, camera);
    }

    /// <summary>
    /// Runs his body for one update of a rise out of the acid as <paramref name="body"/> (#2193): the acid smokes about
    /// him and, while he is whole, burns audibly; once he has melted his body shows its melting picture of the rows above
    /// the acid where he stands; and he rises.
    /// </summary>
    internal void RunCrocomireRisingComposite(CrocomireEnemyState state, CrocomireBodyInAcid body)
    {
        ShowCrocomireInAcid(state, body);
        RunCrocomireRise(state);
    }

    /// <summary>
    /// Runs his body for one update of a rest at the acid's floor as <paramref name="body"/>: while he is whole the acid
    /// burns audibly and his hidden tongue is held at his body; he starts to rise as the rest's wait runs out.
    /// </summary>
    internal void RunCrocomireSubmergedRest(CrocomireEnemyState state, CrocomireBodyInAcid body)
    {
        if (body == CrocomireBodyInAcid.Whole)
        {
            RunCrocomireAcidSounds(state.Death);
            CrocomireBodyLayer.PlaceCrocomireTongue(state);
        }
        RunCrocomireSubmergedPause(state);
    }

    /// <summary>
    /// The acid burns audibly about his body while he is whole and smokes about it, and once he has melted his body shows
    /// its melting picture of the rows above the acid where he stands.
    /// </summary>
    private void ShowCrocomireInAcid(CrocomireEnemyState state, CrocomireBodyInAcid body)
    {
        if (body == CrocomireBodyInAcid.Whole)
            RunCrocomireAcidSounds(state.Death);
        // The smoke draws a random number and rises by his position, while the picture only starts his body's animation, so
        // either may come first.
        _bodyLayer.SpawnCrocomireAcidSmoke(state);
        if (MeltingPicture(body) is { } illustration)
            SelectCrocomireMeltAnimation(state, illustration);
    }

    /// <summary>The melting illustration <paramref name="body"/> shows, or null while he is whole.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="body"/> is not a body in the acid.</exception>
    private static CrocomireMeltingIllustration? MeltingPicture(CrocomireBodyInAcid body) => body switch
    {
        CrocomireBodyInAcid.Whole => null,
        CrocomireBodyInAcid.MeltingFlesh => CrocomireMeltingIllustration.Flesh,
        CrocomireBodyInAcid.MeltingBareSkull => CrocomireMeltingIllustration.BareSkull,
        _ => throw new ArgumentOutOfRangeException(nameof(body), body, $"Undefined {nameof(CrocomireBodyInAcid)}."),
    };

    /// <summary>Plays the acid's burning sound each time its wait runs out, every <see cref="CrocomireBridgeCollapseDefinitions.AcidSoundInterval"/> updates.</summary>
    private void RunCrocomireAcidSounds(CrocomireDeathState death)
    {
        if (death.AcidSoundWait.StepKeepsRunning())
            return;
        death.AcidSoundWait.LastFor(CrocomireBridgeCollapseDefinitions.AcidSoundInterval);
        _access.RequestCrocomireSound(CrocomireSoundDefinitions.AcidBurn);
    }

    /// <summary>Rests his body submerged while the pause's wait runs; he starts to rise on the update it runs out.</summary>
    private static void RunCrocomireSubmergedPause(CrocomireEnemyState state)
    {
        if (state.DeathStageWait.StepKeepsRunning())
            return;
        state.AdvanceDeathStage();
        state.VerticalSpeed = CrocomireBridgeCollapseDefinitions.MaximumSinkSpeed;
    }

    /// <summary>Crocomire's sinking fall to a fraction of a pixel, with the camera his main update runs with.</summary>
    private void RunCrocomireSink(CrocomireEnemyState state, Position16<RoomPixels> camera)
    {
        CrocomireBodyLayer.LevelCrocomireMeltDistortion(state);
        state.StepBack.Forget();
        CrocomireBodyLayer.PlaceCrocomireTongue(state);
        RoomEnemyPlace body = state.Body;

        if (body.Position.Y.Whole >= CrocomireBridgeCollapseDefinitions.SunkenY)
        {
            state.AdvanceDeathStage();
            state.DeathStageWait.LastFor(CrocomireBridgeCollapseDefinitions.SubmergedPauseUpdates);
            // The next rise, after a submerged pause or directly after a melt, starts from no acceleration.
            state.VerticalAcceleration = SignedSixteenSixteen.Zero;
            return;
        }

        // The acceleration grows up to the sink's most; the speed then gains it (CrocomireSpeedGain).
        SignedSixteenSixteen acceleration = SignedSixteenSixteen.Min(
            state.VerticalAcceleration + CrocomireBridgeCollapseDefinitions.SinkAccelerationIncrement, CrocomireBridgeCollapseDefinitions.MaximumSinkAcceleration);
        state.VerticalAcceleration = acceleration;
        SignedEightEight speed = CrocomireSpeedGain.Gained(state.VerticalSpeed, acceleration, CrocomireBridgeCollapseDefinitions.MaximumSinkSpeed);
        state.VerticalSpeed = speed;

        body.Position = body.Position.WithWholeY(body.Position.Y.Whole + state.VerticalTravelCarry.WholePixelsCrossedBy(speed));
        state.VerticalTravelCarry = state.VerticalTravelCarry.After(speed);
        // The acid hides his body below its row wherever this update leaves him, which is where his body is shown.
        HideCrocomireSunkenBody(state, camera);
    }

    /// <summary>Runs Crocomire's rise, to a fraction of a pixel, as the game does.</summary>
    private static void RunCrocomireRise(CrocomireEnemyState state)
    {
        CrocomireBodyLayer.LevelCrocomireMeltDistortion(state);
        RoomEnemyPlace body = state.Body;
        if (body.Position.Y.Whole < CrocomireBridgeCollapseDefinitions.RisenY)
        {
            state.AdvanceDeathStage();
            return;
        }

        CrocomireBodyLayer.PlaceCrocomireTongue(state);
        SignedSixteenSixteen acceleration = SignedSixteenSixteen.Min(
            state.VerticalAcceleration + CrocomireBridgeCollapseDefinitions.RiseAccelerationIncrement, CrocomireBridgeCollapseDefinitions.MaximumRiseAcceleration);
        state.VerticalAcceleration = acceleration;

        // The speed loses the acceleration (CrocomireSpeedGain); a speed that would fall below zero stalls just under one
        // pixel an update instead of reversing.
        SignedEightEight speed = CrocomireSpeedGain.Lost(state.VerticalSpeed, acceleration);
        if (speed.IsNegative)
            speed = CrocomireBridgeCollapseDefinitions.RiseSpeedAfterStalling;
        state.VerticalSpeed = speed;

        // The rise moves up: the carried part of a pixel less the speed, its whole pixels zero or below.
        body.Position = body.Position.WithWholeY(body.Position.Y.Whole + state.VerticalTravelCarry.WholePixelsCrossedBy(-speed));
        state.VerticalTravelCarry = state.VerticalTravelCarry.After(-speed);
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
    private static CrocomireBodyAnimation MeltAnimation(CrocomireMeltingIllustration illustration, int rowsAboveAcid) =>
        illustration switch
        {
            CrocomireMeltingIllustration.Flesh => rowsAboveAcid switch
            {
                1 => CrocomireBodyAnimation.MeltingFleshTopRow,
                2 => CrocomireBodyAnimation.MeltingFleshTopTwoRows,
                3 => CrocomireBodyAnimation.MeltingFleshTopThreeRows,
                4 => CrocomireBodyAnimation.MeltingFleshTopFourRows,
                _ => throw new ArgumentOutOfRangeException(nameof(rowsAboveAcid), rowsAboveAcid,
                    "Crocomire's first melting animations show one to four rows above the acid."),
            },
            CrocomireMeltingIllustration.BareSkull => rowsAboveAcid switch
            {
                1 => CrocomireBodyAnimation.MeltingBareSkullTopRow,
                2 => CrocomireBodyAnimation.MeltingBareSkullTopTwoRows,
                3 => CrocomireBodyAnimation.MeltingBareSkullTopThreeRows,
                4 => CrocomireBodyAnimation.MeltingBareSkullTopFourRows,
                _ => throw new ArgumentOutOfRangeException(nameof(rowsAboveAcid), rowsAboveAcid,
                    "Crocomire's second melting animations show one to four rows above the acid."),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(illustration), illustration,
                "Crocomire melts through his first and second illustrations only."),
        };

    /// <summary>
    /// Runs one update of his last sink, showing his bare skull's melting picture; once he rests at the acid's floor his
    /// skeleton starts down the river, the boss tension music starts, and the bridge's screens, invisible wall and camera
    /// open behind him.
    /// </summary>
    internal void RunCrocomireFinalSink(CrocomireEnemyState state, Position16<RoomPixels> camera)
    {
        RunCrocomireSinkingComposite(state, CrocomireBodyInAcid.MeltingBareSkull, camera);
        if (state.DeathStage.Entry is not CrocomireDeathStage.FlowDownRiver)
            return;

        _access.RequestCrocomireMusic(MusicRequest.SelectTrack(MusicTrack.BossRoomTension), MusicQueueDelay.Shortest);
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodyAnimation.SkeletonFlowingDownRiver);
        _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.BridgeLeftScreen, RoomScrollState.OpenAlignedToTop);
        _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.BridgeScreen, RoomScrollState.OpenAlignedToTop);
        state.Tongue.Presence = state.Tongue.Presence.With(EnemyPresence.Deleted);
        _access.PublishCrocomireObject(CrocomireArenaDefinitions.BridgeWallBlock, CrocomireArenaMutation.ClearInvisibleWall);
        _cameraDistance.Request(CameraDistanceMode.NormalTracking);
        state.Death.SkeletonBaring = UpdateCountdown.Lasting(CrocomireSkeletonLookDefinitions.BaringDuration);
    }

    /// <summary>Picks the next crumbling bridge column and runs the bridge fragment's setup.</summary>
    internal void SpawnNextCrocomireBridgeFragment(CrocomireDeathState death)
    {
        int fragment = death.BridgeFragmentsSpawned;
        if (fragment >= CrocomireBridgeFragmentDefinitions.FragmentCount)
            return;

        // The fragment counts as launched even when no projectile place is free.
        death.BridgeFragmentsSpawned = fragment + 1;
        // The fragments draw in the wall colours Crocomire's encounter installs.
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.CrocomireBridgeCrumbling, _combat.Debris, SpriteGraphics.SharedIn(CrocomireColours.WallShading));
        if (projectile is null)
            return;

        projectile.StartAnimation(new CrocomireProjectileAnimation(_combat.Debris, CrocomireProjectileSegment.BridgeFragment));
        projectile.Position = projectile.Position.WithWholeX(CrocomireBridgeFragmentDefinitions.XPosition(fragment));
        projectile.Position = projectile.Position.WithWholeY(CrocomireBridgeFragmentDefinitions.Y).WithoutFractions();
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.Zero);
        // The fragment's fall speed reads the room's random number without drawing a new one.
        projectile.Velocity = projectile.Velocity.WithY(CrocomireRandomDefinitions.BridgeFragmentFallSpeed.Of(_room.Random.Read()));
    }
}
