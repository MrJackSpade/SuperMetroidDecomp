using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;

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
    /// <summary>Produces the native distributed drops at skeleton collapse.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Owns camera-distance mode changes during Crocomire's arena sequence.</summary>
    private readonly ZebetiteBehavior _zebetiteBehavior;
    /// <summary>Crocomire's state lookups, body program start, PLMs, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireSkeleton(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyPresentation presentation, EnemyDeathsAndDrops deathsAndDrops, ZebetiteBehavior zebetiteBehavior, CrocomireEncounterAccess access, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
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
        body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole - 2)));
        if (unchecked((short)(body.Position.X.Whole - 480)) < 0)
        {
            body.Position = body.Position.WithWhole(new(480, 54));
            state.DeathSequenceIndex = CrocomireDeathPhase.WaitForSamusAtWall;
        }
        else
        {
            body.Position = body.Position.WithWholeY(220);
        }
    }

    /// <summary>Waits with the skeleton behind the wall before its rumble and tile-load sequence.</summary>
    internal void RunCrocomireWaitBehindWall(CrocomireEnemyState state, SamusState? samus)
    {
        if (samus is null || unchecked((short)(samus.Position.X.Whole - 640)) >= 0)
            return;

        _access.LastCrocomireMusicRequest = new DelayedMusicRequest(
            MusicRequest.SelectTrack(5),
            MusicQueueDelay.Shortest);
        _room.Services.RoomScroll.Write(3, RoomScrollState.RedBoundary);
        _room.Services.RoomScroll.Write(4, RoomScrollState.Blue);
        _access.PublishCrocomirePlm(new Position16<RoomBlocks>(48, 3), PlmHeaderId.CreateCrocomireInvisibleWall);
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.RightEdge; // $A4:97F3-97F6
        RoomEnemySlot body = state.Body;
        body.Properties = body.Properties.Replace(
            EnemyProperties.SolidToSamus | EnemyProperties.IgnoreSamusCollision,
            EnemyProperties.IgnoreSamusCollision);
        if (state.Tongue is { } tongue)
            tongue.Properties = tongue.Properties.With(
                EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        state.RumbleTableOffset = 4;

        CrocomireDeathState death = _access.RequireCrocomireDeath();
        death.RumbleYOffset = 0;
        death.RumbleCooldown = WrappingCountdown.Start(10);
        death.RumbleDelta = 1;
        state.FightFlags = 0;
        body.Radius = body.Radius.WithY(56);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Shakes the arena wall before the skeleton's break-through and advances its authored delay.</summary>
    internal void RunCrocomireWallRumble(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        CrocomireRumbleDefinition definition =
            CrocomireRumbleDefinitions.AtOffset(state.RumbleTableOffset);
        int target = definition.TargetYOffset;
        if (definition.IsTerminator)
        {
            // The rumble hands its shared word back to the melt's shake counter, with both bytes at 128.
            death.MeltShakeCounter = WrappingCountdown.FromRaw(CrocomireBehavior.MeltShakeCounterAfterRumble);
            state.DeathPhaseDelay = WrappingCountdown.Start(128);
            (_bossArtwork.Artwork?.CrocomireColors ?? throw new InvalidOperationException(
                "Crocomire wall spikes require installed colors.")).ApplyWallSpikes(_presentation.Colours);
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
            return;
        }

        if (death.RumbleYOffset == target)
        {
            if (target < 0)
            {
                if (!death.RumbleCooldown.IsZero)
                {
                    death.RumbleCooldown = death.RumbleCooldown.Decrement().Remaining;
                    state.RumbleTableOffset -= 2;
                    _access.LastCrocomireSoundEffect = CrocomireSoundDefinitions.WallRumble;
                    return;
                }

                death.RumbleCooldown = WrappingCountdown.FromRaw(definition.Cooldown);
                death.RumbleDelta = definition.Delta;
            }
            state.RumbleTableOffset = definition.NextTargetOffset;
            return;
        }

        death.RumbleYOffset = death.RumbleYOffset >= target
            ? death.RumbleYOffset - death.RumbleDelta
            : death.RumbleYOffset + death.RumbleDelta;
    }

    /// <summary>Loads skeleton graphics and replaces the wall blocks as the bones break through.</summary>
    internal void RunCrocomireSkeletonTileLoadAndWallBreak(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (!state.DeathPhaseDelay.IsZero)
        {
            state.DeathPhaseDelay = state.DeathPhaseDelay.Decrement().Remaining;
            ShowNextCrocomireSkeletonChunk(death);
            return;
        }

        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWhole(new(480, 54));
        death.RumbleCooldown = WrappingCountdown.Start(80);
        state.SkeletonVelocity = SignedSixteenSixteen.Zero;
        _access.PublishCrocomirePlm(new Position16<RoomBlocks>(32, 3), PlmHeaderId.ClearCrocomireInvisibleWall);
        _access.PublishCrocomirePlm(new Position16<RoomBlocks>(30, 3), PlmHeaderId.CreateCrocomireInvisibleWall);
        _access.PublishCrocomirePlm(new Position16<RoomBlocks>(112, 11), PlmHeaderId.ClearCrocomireBridge);
        _access.LastCrocomireSoundEffect = CrocomireSoundDefinitions.Crumble;
        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, CrocomireBodyProgram.SkeletonFallsApart);
        body.PaletteRow = 0;
        (_bossArtwork.Artwork?.CrocomireColors ?? throw new InvalidOperationException(
            "Crocomire skeleton requires installed colors.")).ApplySkeletonArm(_presentation.Colours);

        foreach (RoomEnemyProjectileSlot projectile in _projectiles.EnemyProjectiles)
            projectile.Clear();
        SpawnCrocomireSpikeWallPieces();
        _access.LastCrocomireSoundEffect = CrocomireSoundDefinitions.SpikeWallBreak;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Holds the broken-wall pose before launching the skeleton arc.</summary>
    internal void RunCrocomireWallBreakDelay(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (unchecked((short)(state.Body.Position.X.Whole - 224)) < 0)
        {
            state.SkeletonVelocity = AccelerateCrocomireSkeleton(state.SkeletonVelocity, accelerationEightEight: 128, maximumWhole: 2);
            state.Body.Position = state.Body.Position.WithX(
                state.SkeletonVelocity.AddTo(state.Body.Position.X.Whole, state.Body.Position.X.Fraction));
        }

        if (death.RumbleCooldown.IsZero)
            return;
        death.RumbleCooldown = death.RumbleCooldown.Decrement().Remaining;
        if (!death.RumbleCooldown.IsZero)
            return;
        state.SkeletonVelocity = state.SkeletonVelocity.WithFraction(0);
        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, CrocomireBodyProgram.SkeletonFalling);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Moves the skeleton through its airborne arc after the wall break.</summary>
    internal void RunCrocomireSkeletonArc(CrocomireEnemyState state)
    {
        state.SkeletonVelocity = AccelerateCrocomireSkeleton(state.SkeletonVelocity, accelerationEightEight: 8, maximumWhole: 5);
        state.Body.Position = state.Body.Position.WithY(
            CrocomireBehavior.SkeletonArcFallSpeed.AddTo(state.Body.Position.Y.Whole, state.Body.Position.Y.Fraction));
        state.Body.Position = state.Body.Position.WithX(
            state.SkeletonVelocity.AddTo(state.Body.Position.X.Whole, state.Body.Position.X.Fraction));
        if (unchecked((short)(state.Body.Position.X.Whole - 576)) < 0)
            return;

        _access.LastCrocomireSoundEffect = CrocomireSoundDefinitions.BigExplosion;
        if (state.Tongue is { } tongue)
            tongue.PaletteRow = state.Body.PaletteRow;
        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, CrocomireBodyProgram.SkeletonFallsApart);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Finishes the skeleton collapse and scatters sixteen independently chosen enemy drops.</summary>
    internal void RunCrocomireSkeletonCollapse(CrocomireEnemyState state)
    {
        if (!CrocomireBodyProgramGroups.SkeletonAtRest.Contains(state.BodyResidentProgram))
        {
            state.SkeletonVelocity = AccelerateCrocomireSkeleton(state.SkeletonVelocity, accelerationEightEight: 16, maximumWhole: 6);
            return;
        }

        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, CrocomireBodyProgram.Dead);
        state.Body.Position += new Offset16<RoomPixels>(64, 21);
        state.Body.Radius = new(40, 28);
        _access.PublishCrocomirePlm(new Position16<RoomBlocks>(48, 3), PlmHeaderId.ClearCrocomireInvisibleWall);

        // $A0:B995 emits sixteen independent $F337 projectiles across Crocomire's
        // authored arena rectangle. These are not a single pickup at the corpse position:
        // each actor advances RNG again while selecting its own drop from header $DDBF.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Crocomire,
            count: 16,
            xBase: 576,
            xMask: 127,
            yBase: 96,
            yMask: 63);
        _access.RequireCrocomireDeath().ItemDropRequested = true;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Restores arena scroll and camera tracking after the skeleton sequence.</summary>
    internal void FinishCrocomireArenaScrolls(CrocomireEnemyState state)
    {
        for (int index = 0; index < 4; index++)
            _room.Services.RoomScroll.Write(index, RoomScrollState.Blue);
        _access.PublishCrocomirePlm(new Position16<RoomBlocks>(30, 3), PlmHeaderId.ClearCrocomireInvisibleWall);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>Marks Crocomire's boss encounter complete and releases the death-scene presentation.</summary>
    internal void CompleteCrocomireBoss(CrocomireEnemyState state)
    {
        _access.LastCrocomireMusicRequest = new DelayedMusicRequest(
            MusicRequest.SelectTrack(6),
            MusicQueueDelay.Shortest);
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.NormalTracking; // $A4:9B8D
        _room.Services.Bosses.SetDefeated(BossBits.AreaMiniBoss);
        _access.RequireCrocomireDeath().BossBitSet = true;
        _access.SpawnCrocomireDust(state, -16);
        _access.SpawnCrocomireDust(state, 16);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    /// <summary>
    /// Shows, at this update, one more of the skeleton's character chunks (#1406), in their fixed order, while the skeleton
    /// waits behind the wall; once all are shown it shows nothing more. From then on his frames drawn over a chunk's
    /// characters picture it.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The installed boss artwork has no skeleton characters, or a chunk is not one chunk's characters long.
    /// </exception>
    internal void ShowNextCrocomireSkeletonChunk(CrocomireDeathState death)
    {
        var artwork = _bossArtwork.Artwork?.CrocomireSkeleton ?? throw new InvalidDataException(
            "Installed Crocomire artwork is missing its skeleton characters.");
        int chunk = death.SkeletonChunksShown;
        if (chunk == CrocomireSkeletonTransferDefinitions.FrameCount)
            return;
        // Only character pixels are editable; the chunks' order and their place among the characters are not.
        if (artwork.Chunk(chunk).Length != CrocomireSkeletonTransferDefinitions.ChunkByteCount)
            throw new InvalidDataException($"Installed Crocomire skeleton chunk {chunk} is not one chunk's characters long.");
        _presentation.ShowCharacters(CrocomireSkeletonTransferDefinitions.Sources[chunk]);
        death.SkeletonChunksShown++;
    }

    /// <summary>
    /// Adds an acceleration of <paramref name="accelerationEightEight"/>/256 pixel per frame per frame to the
    /// skeleton's 16.16 speed and clamps its whole pixels at <paramref name="maximumWhole"/>, keeping the fraction.
    /// The speed starts at rest and only grows, so its whole pixels never leave the clamp's range.
    /// </summary>
    private static SignedSixteenSixteen AccelerateCrocomireSkeleton(
        SignedSixteenSixteen velocity,
        int accelerationEightEight,
        int maximumWhole)
    {
        SignedSixteenSixteen accelerated =
            velocity + SignedSixteenSixteen.FromRaw(SixteenSixteen.FromEightEight(accelerationEightEight));
        return accelerated.WholePixels >= maximumWhole
            ? accelerated.WithWholePixels(checked((short)maximumWhole))
            : accelerated;
    }

    /// <summary>
    /// Ports the eight descending allocations at $A4:99B6 and initializer
    /// $86:90CF. Its Y-position table at $86:9105-$9114 has eight unsigned
    /// words, all exactly $0038 + $0010*i for i=0..7 in the pinned NTSC
    /// J/U v1.0 ROM. Native even byte offsets 0..14 correspond to physical
    /// projectile slots 10..17; the checked managed index is slot minus 10.
    /// Clearing the pool before this call makes the spawn order Y=$A8 down
    /// to $38. $86:9115 starts the next routine, not a ninth table word.
    /// </summary>
    internal void SpawnCrocomireSpikeWallPieces()
    {
        CrocomireEnemyState state = _access.Crocomire ??
            throw new InvalidOperationException("Crocomire spike wall spawned without an owner.");
        SpriteGraphics graphics = state.Body.Graphics;

        // The initializer indexes Y by native projectile index minus $14. Clearing the pool
        // immediately before this call guarantees slots 17..10 and Y=$38,$48,...,$A8.
        for (int count = 0; count < 8; count++)
        {
            RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
            if (projectile is null)
                return;

            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.CrocomireSpikeWallPieces,
                graphics);
            projectile.StartAnimation(new CrocomireProjectileAnimation(CrocomireProjectileFrameKind.SpikeWallPiece));
            int yTableIndex = projectile.SlotIndex - 10;
            if ((uint)yTableIndex >= 8)
            {
                throw new InvalidDataException(
                    $"Crocomire spike wall occupied unexpected projectile slot {projectile.SlotIndex}.");
            }

            projectile.Position = projectile.Position.WithWhole(new(528, unchecked((ushort)(56 + yTableIndex * 16))));
            projectile.Velocity = new(new SignedEightEight(0), SignedEightEight.FromSignedRaw(-5));
            projectile.Variable0 = 0;
            projectile.Variable1 = 34816;
            projectile.Position = projectile.Position.WithFractionX(0).WithFractionY(0);
        }
    }
}
