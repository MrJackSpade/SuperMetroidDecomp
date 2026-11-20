using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's skeleton: waiting behind the wall, breaking through, its arc and collapse, and the fight's end.</summary>
[RoomLifetime]
internal sealed class CrocomireSkeleton
{
    /// <summary>Accesses the active room, enemy places, and shared RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Spawns Crocomire's attacks, dust, smoke, and post-death drops.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Draws Crocomire's body, skeleton and melt, and holds the colours and background pictures they show.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Produces the distributed drops at skeleton collapse.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>The room camera's distance mode, which the fight requests.</summary>
    private readonly RoomCameraDistance _cameraDistance;
    /// <summary>Crocomire's state lookups, body animation start, room objects, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;
    /// <summary>Crocomire's combat, whose projectile behaviour the spike wall's pieces are bound to.</summary>
    private readonly CrocomireCombat _combat;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireSkeleton(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyPresentation presentation, EnemyDeathsAndDrops deathsAndDrops, RoomCameraDistance cameraDistance, CrocomireEncounterAccess access,
        CrocomireCombat combat)
    {
        _combat = combat;
        _room = room;
        _projectiles = projectiles;
        _presentation = presentation;
        _deathsAndDrops = deathsAndDrops;
        _cameraDistance = cameraDistance;
        _access = access;
    }

    /// <summary>Moves the skeleton through the acid river toward the arena's breakable wall.</summary>
    internal static void RunCrocomireSkeletonRiver(CrocomireEnemyState state)
    {
        RoomEnemyPlace body = state.Body;
        body.Position = body.Position.WithWholeX(body.Position.X.Whole - CrocomireSkeletonDefinitions.RiverDrift);
        if (body.Position.X.Whole < CrocomireSkeletonDefinitions.BehindWallPosition.X)
        {
            body.Position = body.Position.WithWhole(CrocomireSkeletonDefinitions.BehindWallPosition);
            state.AdvanceDeathStage();
        }
        else
        {
            body.Position = body.Position.WithWholeY(CrocomireSkeletonDefinitions.RiverY);
        }
    }

    /// <summary>Waits with the skeleton behind the wall before its rumble and picture-load sequence.</summary>
    internal void RunCrocomireWaitBehindWall(CrocomireEnemyState state, SamusState samus)
    {
        if (samus.Position.X.Whole >= CrocomireSkeletonDefinitions.WallApproachX)
            return;

        _access.RequestCrocomireMusic(MusicRequest.SelectTrack(MusicTrack.KraidAndPhantoonBattle), MusicQueueDelay.Shortest);
        _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.HiddenWallScreen, RoomScrollState.Closed);
        _room.Services.RoomScroll.Write(CrocomireCameraDefinitions.BridgeLeftScreen, RoomScrollState.OpenAlignedToTop);
        _access.PublishCrocomireObject(CrocomireArenaDefinitions.SkeletonWallBlock, CrocomireArenaMutation.CreateInvisibleWall);
        _cameraDistance.Request(CameraDistanceMode.RightEdge);
        RoomEnemyPlace body = state.Body;
        body.Contact = body.Contact.Replace(EnemyContact.SolidToSamus | EnemyContact.IgnoresSamus, EnemyContact.IgnoresSamus);
        state.Tongue.Contact = state.Tongue.Contact.With(EnemyContact.IgnoresSamus);
        state.Tongue.Presence = state.Tongue.Presence.With(EnemyPresence.Hidden);
        state.RumbleOscillation = ListStep<CrocomireRumbleOscillation>.FirstOf(CrocomireRumbleDefinitions.Oscillations);
        state.RumbleTowardExtreme = false;

        CrocomireDeathState death = state.Death;
        death.RumbleYOffset = 0;
        death.RumbleRepeatsLeft = UpdateCountdown.For(CrocomireRumbleDefinitions.Oscillations[0].Repeats);
        state.StepBack.Forget();
        body.Radius = body.Radius.WithY(CrocomireSkeletonDefinitions.BehindWallReachY);
        state.AdvanceDeathStage();
    }

    /// <summary>Shakes the arena wall before the skeleton's break-through and advances its authored delay.</summary>
    internal void RunCrocomireWallRumble(CrocomireEnemyState state)
    {
        CrocomireDeathState death = state.Death;
        if (state.RumbleOscillation is not { } underWay)
        {
            state.DeathStageWait.LastFor(CrocomireSkeletonDefinitions.SpikedWallUpdates);
            state.Colours.ShowWallSpikes();
            state.Body.Shading = state.Colours.OverSprites(EnemyShades.HurtFlashLook);
            state.Tongue.Shading = state.Body.Shading;
            state.AdvanceDeathStage();
            return;
        }

        CrocomireRumbleOscillation oscillation = underWay.Entry;
        int target = state.RumbleTowardExtreme ? oscillation.ExtremeOffset : oscillation.ReturnOffset;
        if (death.RumbleYOffset == target)
        {
            // Reaching an offset takes the update: the wall turns around on the next one.
            if (!state.RumbleTowardExtreme)
            {
                state.RumbleTowardExtreme = true;
                return;
            }

            state.RumbleTowardExtreme = false;
            if (death.RumbleRepeatsLeft.IsRunning)
            {
                death.RumbleRepeatsLeft.Count();
                _access.RequestCrocomireSound(CrocomireSoundDefinitions.WallRumble);
                return;
            }

            state.RumbleOscillation = underWay.Following;
            if (state.RumbleOscillation is { } next)
                death.RumbleRepeatsLeft = UpdateCountdown.For(next.Entry.Repeats);
            return;
        }

        death.RumbleYOffset = death.RumbleYOffset >= target
            ? death.RumbleYOffset - oscillation.Speed
            : death.RumbleYOffset + oscillation.Speed;
    }

    /// <summary>
    /// Bares the skeleton behind the wall on each update of its wait but the last, then replaces the wall blocks as the bones
    /// break through on the update the wait runs out.
    /// </summary>
    internal void RunCrocomireSkeletonRevealAndWallBreak(CrocomireEnemyState state)
    {
        CrocomireDeathState death = state.Death;
        if (state.DeathStageWait.StepKeepsRunning())
        {
            BareCrocomireSkeleton(death);
            return;
        }

        RoomEnemyPlace body = state.Body;
        body.Position = body.Position.WithWhole(CrocomireSkeletonDefinitions.BehindWallPosition);
        death.SkeletonFallWait.LastFor(CrocomireSkeletonDefinitions.FallDelay);
        state.SkeletonVelocity = SignedSixteenSixteen.Zero;
        _access.PublishCrocomireObject(CrocomireArenaDefinitions.FightWallBlock, CrocomireArenaMutation.ClearInvisibleWall);
        _access.PublishCrocomireObject(CrocomireArenaDefinitions.SpikeWallBlock, CrocomireArenaMutation.CreateInvisibleWall);
        _access.PublishCrocomireObject(CrocomireArenaDefinitions.BreakThroughBridgeBlock, CrocomireArenaMutation.ClearBridge);
        _access.RequestCrocomireSound(CrocomireSoundDefinitions.Crumble);
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodyAnimation.SkeletonFallsApart);
        // The skeleton draws in the common colours with its arm's laid over them (#1512).
        state.Colours.ShowSkeletonArm();
        body.Shading = state.Colours.OverSprites(EnemyShades.HurtFlashLook);

        foreach (EnemyProjectile projectile in _projectiles.EnemyProjectiles)
            projectile.Delete();
        SpawnCrocomireSpikeWallPieces(state);
        _access.RequestCrocomireSound(CrocomireSoundDefinitions.SpikeWallBreak);
        state.AdvanceDeathStage();
    }

    /// <summary>Holds the broken-wall pose before launching the skeleton arc.</summary>
    internal static void RunCrocomireWallBreakDelay(CrocomireEnemyState state)
    {
        CrocomireDeathState death = state.Death;
        if (state.Body.Position.X.Whole < CrocomireSkeletonDefinitions.BreakOutEndX)
        {
            state.SkeletonVelocity = AccelerateCrocomireSkeleton(state.SkeletonVelocity, CrocomireSkeletonDefinitions.BreakOut);
            state.Body.Position = state.Body.Position.WithX(state.Body.Position.X.MovedBy(state.SkeletonVelocity));
        }

        if (death.SkeletonFallWait.StepKeepsRunning())
            return;
        state.SkeletonVelocity = state.SkeletonVelocity.WithoutFraction();
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodyAnimation.SkeletonFalling);
        state.AdvanceDeathStage();
    }

    /// <summary>Moves the skeleton through its airborne arc after the wall break.</summary>
    internal void RunCrocomireSkeletonArc(CrocomireEnemyState state)
    {
        state.SkeletonVelocity = AccelerateCrocomireSkeleton(state.SkeletonVelocity, CrocomireSkeletonDefinitions.Arc);
        state.Body.Position = state.Body.Position.WithY(state.Body.Position.Y.MovedBy(CrocomireSkeletonDefinitions.ArcFallSpeed));
        state.Body.Position = state.Body.Position.WithX(state.Body.Position.X.MovedBy(state.SkeletonVelocity));
        if (state.Body.Position.X.Whole < CrocomireSkeletonDefinitions.ArcEndX)
            return;

        _access.RequestCrocomireSound(CrocomireSoundDefinitions.BigExplosion);
        state.Tongue.Shading = state.Body.Shading;
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodyAnimation.SkeletonFallsApart);
        state.AdvanceDeathStage();
    }

    /// <summary>Finishes the skeleton collapse and scatters sixteen independently chosen enemy drops.</summary>
    internal void RunCrocomireSkeletonCollapse(CrocomireEnemyState state, SamusState samus)
    {
        if (!state.SkeletonHasCollapsed)
        {
            state.SkeletonVelocity = AccelerateCrocomireSkeleton(state.SkeletonVelocity, CrocomireSkeletonDefinitions.Collapse);
            return;
        }

        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodyAnimation.Dead);
        state.Body.Position += CrocomireSkeletonDefinitions.CorpseOffset;
        state.Body.Radius = CrocomireArenaDefinitions.CorpseRadius;
        _access.PublishCrocomireObject(CrocomireArenaDefinitions.SkeletonWallBlock, CrocomireArenaMutation.ClearInvisibleWall);

        // The death drops emit sixteen independent pickup projectiles across Crocomire's
        // authored arena rectangle. These are not a single pickup at the corpse position:
        // each actor advances RNG again while selecting its own drop from Crocomire's species definition.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Crocomire,
            count: CrocomireSkeletonDefinitions.DropCount,
            xBase: CrocomireSkeletonDefinitions.DropLeft,
            xScatter: EnemyDropScatterDefinitions.NarrowArenaHorizontalScatter,
            yBase: CrocomireSkeletonDefinitions.DropTop,
            yScatter: EnemyDropScatterDefinitions.ArenaBandVerticalScatter,
            samus: samus);
        state.AdvanceDeathStage();
    }

    /// <summary>Restores arena scroll and camera tracking after the skeleton sequence.</summary>
    internal void FinishCrocomireArenaScrolls(CrocomireEnemyState state)
    {
        foreach (Position16<RoomScreens> screen in CrocomireCameraDefinitions.ArenaScreens)
            _room.Services.RoomScroll.Write(screen, RoomScrollState.OpenAlignedToTop);
        _access.PublishCrocomireObject(CrocomireArenaDefinitions.SpikeWallBlock, CrocomireArenaMutation.ClearInvisibleWall);
        state.AdvanceDeathStage();
    }

    /// <summary>Marks Crocomire's boss encounter complete and releases the death-scene presentation.</summary>
    internal void CompleteCrocomireBoss(CrocomireEnemyState state)
    {
        _access.RequestCrocomireMusic(MusicRequest.SelectTrack(MusicTrack.BossRoomTension), MusicQueueDelay.Shortest);
        _cameraDistance.Request(CameraDistanceMode.NormalTracking);
        _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaMiniBoss);
        _access.SpawnCrocomireDust(state, -CrocomireSkeletonDefinitions.DefeatDustOffset);
        _access.SpawnCrocomireDust(state, CrocomireSkeletonDefinitions.DefeatDustOffset);
        state.AdvanceDeathStage();
    }

    /// <summary>
    /// Runs one update of his death baring the skeleton while it waits behind the wall (#1406, #1538): on the last of
    /// <see cref="CrocomireSkeletonLookDefinitions.BaringDuration"/> updates his frames show the skeleton look from this
    /// update's frame on; once bared, a later update bares nothing more.
    /// </summary>
    private void BareCrocomireSkeleton(CrocomireDeathState death)
    {
        if (death.SkeletonBaring.StepEnds())
            _presentation.ShowLook(CrocomireSkeletonLookDefinitions.SkeletonLook);
    }

    /// <summary>
    /// Adds <paramref name="acceleration"/>'s growth per update to the skeleton's <paramref name="velocity"/> and clamps its
    /// whole pixels at the acceleration's maximum, keeping the fraction.
    /// The speed starts at rest and only grows, so its whole pixels never leave the clamp's range.
    /// </summary>
    private static SignedSixteenSixteen AccelerateCrocomireSkeleton(
        SignedSixteenSixteen velocity,
        CrocomireSkeletonAcceleration acceleration) =>
        (velocity + acceleration.PerUpdate).WithWholePixelsAtMost(acceleration.MaximumWholePixels);

    /// <summary>
    /// Spawns the wall's pieces, bottom piece first, and runs the spike-wall piece setup on each. Each piece carries
    /// its definition, which sets how far below the wall's top it stands. The melt empties the enemy projectiles just
    /// before, so every piece is spawned.
    /// </summary>
    private void SpawnCrocomireSpikeWallPieces(CrocomireEnemyState state)
    {
        SpriteGraphics graphics = SpriteGraphics.SpawnedBy(state.BodyOccupant, state.Body.Shading);

        foreach (CrocomireSpikeWallPiece piece in CrocomireSpikeMotionDefinitions.Pieces.Reverse())
        {
            EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.CrocomireSpikeWallPieces, _combat.Debris, graphics);
            if (projectile is null)
                return;

            projectile.StartAnimation(new CrocomireProjectileAnimation(_combat.Debris, CrocomireProjectileSegment.SpikeWallPiece));
            projectile.Position = projectile.Position.WithWhole(CrocomireSkeletonDefinitions.SpikeWallTop
                + new Offset16<RoomPixels>(0, piece.Depth));
            projectile.Velocity = PixelVelocity<RoomPixels>.Zero;
            _combat.Debris.SpikeWallPieces.Start(projectile, new CrocomireSpikeWallPieceState(piece));
            projectile.Position = projectile.Position.WithoutFractions();
        }
    }
}
