using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Bosses.MotherBrain;
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
    /// <summary>Accesses the active room, enemy slots, and shared RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Spawns Crocomire's attacks, dust, smoke, and post-death drops.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Draws Crocomire's body, skeleton and melt, and holds the colours and background pictures they show.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Produces the distributed drops at skeleton collapse.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Owns camera-distance mode changes during Crocomire's arena sequence.</summary>
    private readonly ZebetiteBehavior _zebetiteBehavior;
    /// <summary>Crocomire's state lookups, body animation start, room objects, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireSkeleton(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyPresentation presentation, EnemyDeathsAndDrops deathsAndDrops, ZebetiteBehavior zebetiteBehavior, CrocomireEncounterAccess access)
    {
        _room = room;
        _projectiles = projectiles;
        _presentation = presentation;
        _deathsAndDrops = deathsAndDrops;
        _zebetiteBehavior = zebetiteBehavior;
        _access = access;
    }

    /// <summary>Moves the skeleton through the acid river toward the arena's breakable wall.</summary>
    internal static void RunCrocomireSkeletonRiver(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWrappedWholeX(body.Position.X.Whole - 2);
        if (CoordinateAxis.Difference(body.Position.X.Whole, 480) < 0)
        {
            body.Position = body.Position.WithWhole(new(480, 54));
            state.DeathPhase = CrocomireDeathPhase.WaitForSamusAtWall;
        }
        else
        {
            body.Position = body.Position.WithWholeY(220);
        }
    }

    /// <summary>Waits with the skeleton behind the wall before its rumble and tile-load sequence.</summary>
    internal void RunCrocomireWaitBehindWall(CrocomireEnemyState state, SamusState? samus)
    {
        if (samus is null || CoordinateAxis.Difference(samus.Position.X.Whole, 640) >= 0)
            return;

        _access.RequestCrocomireMusic(MusicRequest.SelectTrack(MusicTrack.FirstSetSong), MusicQueueDelay.Shortest);
        _room.Services.RoomScroll.Write(3, RoomScrollState.RedBoundary);
        _room.Services.RoomScroll.Write(4, RoomScrollState.Blue);
        _access.PublishCrocomireObject(new Position16<RoomBlocks>(48, 3), CrocomireArenaMutation.CreateInvisibleWall);
        _zebetiteBehavior.CameraDistance = CameraDistanceMode.RightEdge;
        RoomEnemySlot body = state.Body;
        body.Contact = body.Contact.Replace(EnemyContact.SolidToSamus | EnemyContact.IgnoresSamus, EnemyContact.IgnoresSamus);
        if (state.Tongue is { } tongue)
        {
            tongue.Contact = tongue.Contact.With(EnemyContact.IgnoresSamus);
            tongue.Presence = tongue.Presence.With(EnemyPresence.Hidden);
        }
        state.RumbleStep = CrocomireRumble.FirstStep;

        CrocomireDeathState death = _access.RequireCrocomireDeath();
        death.RumbleYOffset = 0;
        death.RumbleCooldown = 10;
        death.RumbleDelta = 1;
        state.FightFlags = 0;
        body.Radius = body.Radius.WithY(56);
        state.DeathPhase = state.DeathPhase.Next();
    }

    /// <summary>Shakes the arena wall before the skeleton's break-through and advances its authored delay.</summary>
    internal void RunCrocomireWallRumble(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (CrocomireRumble.IsComplete(state.RumbleStep))
        {
            state.DeathPhaseDelay = 128;
            state.Colours.ShowWallSpikes();
            state.Body.Shading = state.Colours.OverSprites(EnemyShades.HurtFlashLook);
            if (state.Tongue is { } spikedTongue)
                spikedTongue.Shading = state.Body.Shading;
            state.DeathPhase = state.DeathPhase.Next();
            return;
        }

        CrocomireRumbleDefinition definition = CrocomireRumble.At(state.RumbleStep);
        int target = definition.TargetYOffset;
        if (death.RumbleYOffset == target)
        {
            if (target < 0)
            {
                if (death.RumbleCooldown != 0)
                {
                    death.RumbleCooldown--;
                    // Repeat the oscillation: back to its return target.
                    state.RumbleStep--;
                    _access.RequestCrocomireSound(CrocomireSoundDefinitions.WallRumble);
                    return;
                }

                death.RumbleCooldown = definition.Cooldown;
                death.RumbleDelta = definition.Delta;
            }
            state.RumbleStep++;
            return;
        }

        death.RumbleYOffset = death.RumbleYOffset >= target
            ? death.RumbleYOffset - death.RumbleDelta
            : death.RumbleYOffset + death.RumbleDelta;
    }

    /// <summary>Bares the skeleton behind the wall while its wait runs, then replaces the wall blocks as the bones break through.</summary>
    internal void RunCrocomireSkeletonTileLoadAndWallBreak(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (state.DeathPhaseDelay != 0)
        {
            state.DeathPhaseDelay--;
            BareCrocomireSkeleton(death);
            return;
        }

        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWhole(new(480, 54));
        death.RumbleCooldown = 80;
        state.SkeletonVelocity = SignedSixteenSixteen.Zero;
        _access.PublishCrocomireObject(new Position16<RoomBlocks>(32, 3), CrocomireArenaMutation.ClearInvisibleWall);
        _access.PublishCrocomireObject(new Position16<RoomBlocks>(30, 3), CrocomireArenaMutation.CreateInvisibleWall);
        _access.PublishCrocomireObject(new Position16<RoomBlocks>(112, 11), CrocomireArenaMutation.ClearBridge);
        _access.RequestCrocomireSound(CrocomireSoundDefinitions.Crumble);
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodySegment.SkeletonFallsApart);
        // The skeleton draws in the common colours with its arm's laid over them (#1512).
        state.Colours.ShowSkeletonArm();
        body.Shading = state.Colours.OverSprites(EnemyShades.HurtFlashLook);

        foreach (EnemyProjectile projectile in _projectiles.EnemyProjectiles)
            projectile.Delete();
        SpawnCrocomireSpikeWallPieces();
        _access.RequestCrocomireSound(CrocomireSoundDefinitions.SpikeWallBreak);
        state.DeathPhase = state.DeathPhase.Next();
    }

    /// <summary>Holds the broken-wall pose before launching the skeleton arc.</summary>
    internal void RunCrocomireWallBreakDelay(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (CoordinateAxis.Difference(state.Body.Position.X.Whole, 224) < 0)
        {
            state.SkeletonVelocity = AccelerateCrocomireSkeleton(state.SkeletonVelocity, accelerationTwoFiftySixths: 128, maximumWhole: 2);
            state.Body.Position = state.Body.Position.WithX(
                state.SkeletonVelocity.AddTo(state.Body.Position.X.Whole, state.Body.Position.X.Fraction));
        }

        if (death.RumbleCooldown == 0)
            return;
        if (--death.RumbleCooldown != 0)
            return;
        state.SkeletonVelocity = state.SkeletonVelocity.WithoutFraction();
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodySegment.SkeletonFalling);
        state.DeathPhase = state.DeathPhase.Next();
    }

    /// <summary>Moves the skeleton through its airborne arc after the wall break.</summary>
    internal void RunCrocomireSkeletonArc(CrocomireEnemyState state)
    {
        state.SkeletonVelocity = AccelerateCrocomireSkeleton(state.SkeletonVelocity, accelerationTwoFiftySixths: 8, maximumWhole: 5);
        state.Body.Position = state.Body.Position.WithY(
            CrocomireBehavior.SkeletonArcFallSpeed.AddTo(state.Body.Position.Y.Whole, state.Body.Position.Y.Fraction));
        state.Body.Position = state.Body.Position.WithX(
            state.SkeletonVelocity.AddTo(state.Body.Position.X.Whole, state.Body.Position.X.Fraction));
        if (CoordinateAxis.Difference(state.Body.Position.X.Whole, 576) < 0)
            return;

        _access.RequestCrocomireSound(CrocomireSoundDefinitions.BigExplosion);
        if (state.Tongue is { } tongue)
            tongue.Shading = state.Body.Shading;
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodySegment.SkeletonFallsApart);
        state.DeathPhase = state.DeathPhase.Next();
    }

    /// <summary>Finishes the skeleton collapse and scatters sixteen independently chosen enemy drops.</summary>
    internal void RunCrocomireSkeletonCollapse(CrocomireEnemyState state)
    {
        if (!CrocomireBodyAnimationGroups.SkeletonAtRest.Contains(state.BodyResidentAnimation))
        {
            state.SkeletonVelocity = AccelerateCrocomireSkeleton(state.SkeletonVelocity, accelerationTwoFiftySixths: 16, maximumWhole: 6);
            return;
        }

        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodySegment.Dead);
        state.Body.Position += new Offset16<RoomPixels>(64, 21);
        state.Body.Radius = new(40, 28);
        _access.PublishCrocomireObject(new Position16<RoomBlocks>(48, 3), CrocomireArenaMutation.ClearInvisibleWall);

        // The death drops emit sixteen independent pickup projectiles across Crocomire's
        // authored arena rectangle. These are not a single pickup at the corpse position:
        // each actor advances RNG again while selecting its own drop from Crocomire's species definition.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Crocomire,
            count: 16,
            xBase: 576,
            xScatter: EnemyDropScatterDefinitions.NarrowArenaHorizontalScatter,
            yBase: 96,
            yScatter: EnemyDropScatterDefinitions.ArenaBandVerticalScatter);
        state.DeathPhase = state.DeathPhase.Next();
    }

    /// <summary>Restores arena scroll and camera tracking after the skeleton sequence.</summary>
    internal void FinishCrocomireArenaScrolls(CrocomireEnemyState state)
    {
        for (int index = 0; index < 4; index++)
            _room.Services.RoomScroll.Write(index, RoomScrollState.Blue);
        _access.PublishCrocomireObject(new Position16<RoomBlocks>(30, 3), CrocomireArenaMutation.ClearInvisibleWall);
        state.DeathPhase = state.DeathPhase.Next();
    }

    /// <summary>Marks Crocomire's boss encounter complete and releases the death-scene presentation.</summary>
    internal void CompleteCrocomireBoss(CrocomireEnemyState state)
    {
        _access.RequestCrocomireMusic(MusicRequest.SelectTrack(MusicTrack.SecondSetSong), MusicQueueDelay.Shortest);
        _zebetiteBehavior.CameraDistance = CameraDistanceMode.NormalTracking;
        _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaMiniBoss);
        _access.SpawnCrocomireDust(state, -16);
        _access.SpawnCrocomireDust(state, 16);
        state.DeathPhase = state.DeathPhase.Next();
    }

    /// <summary>
    /// Runs one update of his death baring the skeleton while it waits behind the wall (#1406, #1538): on the last of
    /// <see cref="CrocomireSkeletonLookDefinitions.BaringDuration"/> updates his frames show the skeleton look from this
    /// update's frame on; once bared, a later update bares nothing more.
    /// </summary>
    internal void BareCrocomireSkeleton(CrocomireDeathState death)
    {
        if (death.SkeletonBaringElapsed == CrocomireSkeletonLookDefinitions.BaringDuration)
            return;
        death.SkeletonBaringElapsed++;
        if (death.SkeletonBaringElapsed == CrocomireSkeletonLookDefinitions.BaringDuration)
            _presentation.ShowLook(CrocomireSkeletonLookDefinitions.SkeletonLook);
    }

    /// <summary>
    /// Adds an acceleration of <paramref name="accelerationTwoFiftySixths"/>/256 pixel per frame per frame to the
    /// skeleton's 16.16 speed and clamps its whole pixels at <paramref name="maximumWhole"/>, keeping the fraction.
    /// The speed starts at rest and only grows, so its whole pixels never leave the clamp's range.
    /// </summary>
    private static SignedSixteenSixteen AccelerateCrocomireSkeleton(
        SignedSixteenSixteen velocity,
        int accelerationTwoFiftySixths,
        int maximumWhole)
    {
        SignedSixteenSixteen accelerated =
            velocity + SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(accelerationTwoFiftySixths);
        return accelerated.WithWholePixelsAtMost(maximumWhole);
    }

    /// <summary>
    /// Spawns the wall's eight pieces, bottom piece first, and runs the spike-wall piece initializer on each. Each piece
    /// carries its place in the wall, zero for the top piece, and stands at 56 + 16 times its place, so the spawn order
    /// runs Y=168 up to 56. The caller empties the projectile pool just before, so every piece is spawned.
    /// </summary>
    internal void SpawnCrocomireSpikeWallPieces()
    {
        CrocomireEnemyState state = _access.Crocomire ??
            throw new InvalidOperationException("Crocomire spike wall spawned without an owner.");
        SpriteGraphics graphics = SpriteGraphics.SpawnedBy(state.Body);

        for (int count = 0; count < CrocomireSpikeMotionDefinitions.PieceCount; count++)
        {
            EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
            if (projectile is null)
                return;

            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.CrocomireSpikeWallPieces,
                graphics);
            projectile.StartAnimation(new CrocomireProjectileAnimation(CrocomireProjectileFrameKind.SpikeWallPiece));
            int piece = CrocomireSpikeMotionDefinitions.PieceCount - 1 - count;
            projectile.Position = projectile.Position.WithWhole(new(528, 56 + piece * 16));
            projectile.Velocity = EightEightVector<RoomPixels>.Zero;
            projectile.State = new CrocomireSpikeWallPieceState(piece);
            projectile.Position = projectile.Position.WithFractionX(0).WithFractionY(0);
        }
    }
}
