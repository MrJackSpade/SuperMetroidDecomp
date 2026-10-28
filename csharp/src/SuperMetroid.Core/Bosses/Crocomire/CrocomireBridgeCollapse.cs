using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;

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
    /// <summary>Crocomire's BG2 body: its scroll, scroll table, the words it writes into his BG2 image and the acid smoke drawn with it.</summary>
    private readonly CrocomireBg2 _bg2;
    /// <summary>Crocomire's state lookups, body program start, PLMs, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireBridgeCollapse(EnemyRoomBinding room, EnemyProjectileEngine projectiles, ZebetiteBehavior zebetiteBehavior, CrocomireBg2 bg2, CrocomireEncounterAccess access)
    {
        _room = room;
        _projectiles = projectiles;
        _zebetiteBehavior = zebetiteBehavior;
        _bg2 = bg2;
        _access = access;
    }

    /// <summary>True after the $640 X threshold starts the native bridge-collapse phase.</summary>
    public bool CrocomireBridgeCollapseStarted { get; private set; }

    /// <summary>
    /// Ports the state transition at $A4:8D5E. The individual bridge PLMs and subsequent
    /// melting sequence remain a separate translation boundary, but the actor/tongue state
    /// changes occur on the exact native X threshold.
    /// </summary>
    internal void HandleCrocomireBridgeThreshold(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        HandleCrocomireBridgeApproach(body);
        if (unchecked((short)(body.Position.X.Whole - CrocomireBehavior.CrocomireBridgeThreshold)) < 0)
            return;

        CrocomireBridgeCollapseStarted = true;
        state.DeathSequenceIndex = CrocomireDeathPhase.CrumbleBridgeAndSink;
        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, CrocomireBodyProgram.BridgeCollapsed);
        body.Properties = body.Properties.With(EnemyProperties.SolidToSamus);
        state.StartVerticalMotion(EightEight.FromWhole(CrocomireBehavior.SinkStartingAccelerationWhole));
        body.Radius = body.Radius.WithY(16);
        _access.LastCrocomireSoundEffect = CrocomireSoundDefinitions.BridgeCollapse;
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        death.BridgeFragmentsSpawned = 0;
        death.AcidSmokeTimer = WrappingCountdown.Start(1);
        death.AcidSoundTimer = WrappingCountdown.Start(1);

        PublishCrocomireBridgeCollapsePlms();

        if (state.Tongue is { } tongue)
        {
            // The sleep is installed behind the native maximum timer, so it is reached only after 32767 updates.
            CrocomireBg2.StartCrocomireTongueProgram(state, CrocomireTongueProgram.Sleep, WrappingCountdown.Start(32767));
            tongue.Properties = tongue.Properties.With(EnemyProperties.Invisible);
        }
    }

    /// <summary>
    /// Ports the bridge effects in $A4:8D5E, staged at Crocomire X $0600, $0610 and $0620.
    /// Each stage's dust lands one block ahead, at $0600, $0620 and $0630.
    /// </summary>
    private void HandleCrocomireBridgeApproach(RoomEnemySlot body)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (unchecked((short)(body.Position.X.Whole - 1536)) < 0)
        {
            death.BridgeDustAt1536Spawned = false;
            death.BridgeBlockAt1568Crumbling = false;
            death.BridgeBlocksAt1584Crumbling = false;
            return;
        }

        if (unchecked((short)(body.Position.X.Whole - 1552)) < 0)
        {
            if (!death.BridgeDustAt1536Spawned)
            {
                death.BridgeDustAt1536Spawned = true;
                _projectiles.SpawnRoomGraphicsDustExplosion(new(1536, 176), MiscDustAnimation.BigDustCloud);
            }
            return;
        }

        // $A4:8DA4-8DDB: one block deep crumbles the first bridge block. The flag
        // resets at $8DDC and $8E2D sit behind comparisons these ranges already passed
        // and never execute.
        if (unchecked((short)(body.Position.X.Whole - 1568)) < 0)
        {
            if (!death.BridgeBlockAt1568Crumbling)
            {
                death.BridgeBlockAt1568Crumbling = true;
                _access.PublishCrocomirePlm(
                    new Position16<RoomBlocks>(97, 11), PlmHeaderId.CrumbleCrocomireBridgeBlock);
                _projectiles.SpawnRoomGraphicsDustExplosion(new(1568, 176), MiscDustAnimation.BigDustCloud);
            }
            return;
        }

        // $A4:8DED-8E2C: two blocks deep crumbles the next pair.
        if (unchecked((short)(body.Position.X.Whole - 1584)) < 0 &&
            !death.BridgeBlocksAt1584Crumbling)
        {
            death.BridgeBlocksAt1584Crumbling = true;
            _access.PublishCrocomirePlm(
                new Position16<RoomBlocks>(98, 11), PlmHeaderId.CrumbleCrocomireBridgeBlock);
            _access.PublishCrocomirePlm(
                new Position16<RoomBlocks>(99, 11), PlmHeaderId.CrumbleCrocomireBridgeBlock);
            _projectiles.SpawnRoomGraphicsDustExplosion(new(1584, 176), MiscDustAnimation.BigDustCloud);
        }
    }

    /// <summary>
    /// Ports <c>Crocomire_8EE5</c>: clear all ten bridge blocks and burst dust.
    /// The ten native PLM operand records at $A4:8EE9 + 8*i exactly use
    /// block X=$61+i, block Y=$0B, and header $B74F for i=0..9. The
    /// inclusive production loop preserves their ascending publication
    /// order; $A4:8F35 starts dust setup, not an eleventh clear record.
    /// The seven dust positions in the pinned NTSC J/U v1.0 ROM follow the
    /// exact bounded rule X=$0600+$0010*i for i=0..6; Y=$00B0 at i=0 or 2
    /// and $00C0 otherwise. Native $A4:8F35-$8FB4 encodes these as ordered
    /// immediate operands, not a contiguous table. All seven pairs and their
    /// order match the local span; there is no eighth dust spawn.
    /// </summary>
    private void PublishCrocomireBridgeCollapsePlms()
    {
        for (int blockX = 97; blockX <= 106; blockX++)
            _access.PublishCrocomirePlm(
                new Position16<RoomBlocks>((ushort)blockX, 11), PlmHeaderId.ClearCrocomireBridgeBlock);

        for (int puff = 0; puff < CrocomireBehavior.CrocomireBridgeDustCount; puff++)
            _projectiles.SpawnRoomGraphicsDustExplosion(CrocomireBridgeDustPosition(puff), MiscDustAnimation.BigDustCloud);

        _access.PublishCrocomirePlm(new Position16<RoomBlocks>(78, 3), PlmHeaderId.CreateCrocomireInvisibleWall);
    }

    /// <summary>
    /// $A4:8F35-8FC1 (seven unrolled spawns): puffs sixteen pixels apart along the bridge from
    /// X $600; the first and third rise sixteen pixels above the others' Y $C0.
    /// </summary>
    internal static Position16<RoomPixels> CrocomireBridgeDustPosition(int puff) =>
        (uint)puff < CrocomireBehavior.CrocomireBridgeDustCount
            ? new Position16<RoomPixels>((ushort)(1536 + 16 * puff), (ushort)(puff is 0 or 2 ? 176 : 192))
            : throw new ArgumentOutOfRangeException(nameof(puff));

    /// <summary>
    /// Erases the 32-tile circular-map row selected by <c>(BG2VOFS+288)&amp;$FFF8</c> while
    /// Crocomire sinks. This is the visual producer that makes the body disappear into acid;
    /// merely moving the enemy's Y word leaves a stale full-height BG2 image behind.
    /// </summary>
    private void ClearCrocomireBg2SinkRow()
    {
        int byteOffset = 8 * (unchecked((ushort)(_bg2.CrocomireBg2Scroll.Y + 288)) & ~7);
        int firstCell = byteOffset >> 1;
        Span<BackgroundTile> tilemap = _access.RequireCrocomireDeath().MutableBg2WorkingTilemap;
        if (firstCell < 0 || firstCell + 32 > tilemap.Length)
        {
            throw new InvalidDataException(
                $"Crocomire sink selected BG2 cell {firstCell}, outside the {tilemap.Length}-cell buffer.");
        }
        tilemap.Slice(firstCell, 32).Fill(CrocomireBg2.CrocomireBlankBg2Tile);
        _bg2.WriteCrocomireBg2Cells(firstCell, 32);
    }

    /// <summary>Runs the paired body and BG2 presentation while Crocomire sinks into acid.</summary>
    internal void RunCrocomireSinkingComposite(
        CrocomireEnemyState state,
        SamusState? samus)
    {
        _bg2.SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireSink(state);
    }

    /// <summary>Runs the paired body and BG2 presentation as Crocomire resurfaces.</summary>
    internal void RunCrocomireRisingComposite(
        CrocomireEnemyState state,
        SamusState? samus)
    {
        _bg2.SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireRise(state);
    }

    /// <summary>Queues periodic acid sounds according to the death-scene timer.</summary>
    internal void RunCrocomireAcidSoundTimer()
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (death.AcidSoundTimer.IsZero)
            return;
        death.AcidSoundTimer = death.AcidSoundTimer.Decrement().Remaining;
        if (!death.AcidSoundTimer.IsZero)
            return;
        death.AcidSoundTimer = WrappingCountdown.Start(32);
        _access.LastCrocomireSoundEffect = CrocomireSoundDefinitions.AcidBurn;
    }

    /// <summary>Holds the submerged skeleton for its authored delay before rising begins.</summary>
    internal static void RunCrocomireSubmergedPause(CrocomireEnemyState state)
    {
        if (!state.DeathPhaseDelay.IsZero)
        {
            state.DeathPhaseDelay = state.DeathPhaseDelay.Decrement().Remaining;
            return;
        }
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        state.VerticalSpeed = EightEight.FromWhole(CrocomireBehavior.MaximumSinkSpeedWhole);
    }

    /// <summary>Byte-for-byte fixed-point fall from <c>SinkCrocomireDown</c> at $A4:91C1.</summary>
    internal void RunCrocomireSink(CrocomireEnemyState state)
    {
        _bg2.FillCrocomireBg2ScrollTable(_bg2.CrocomireBg2Scroll.Y);
        state.FightFlags &= ~CrocomireFightFlags.DamageReactionPending;
        _bg2.UpdateCrocomireBg2Scroll(state);
        RoomEnemySlot body = state.Body;

        if (unchecked((short)(body.Position.Y.Whole - 280)) >= 0)
        {
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
            state.DeathPhaseDelay = WrappingCountdown.Start(48);
            return;
        }

        ClearCrocomireBg2SinkRow();
        body.ExtraProperties = body.ExtraProperties.Without(
            EnemyExtraProperties.NewInstructionFrame);

        // The acceleration's fraction carries into its whole part before the whole part is clamped, and the
        // speed's into its whole part before that is clamped: each clamp keeps the fraction it carried from.
        int acceleration = ClampWholePixels(
            state.VerticalAcceleration + CrocomireBehavior.SinkAccelerationIncrement, CrocomireBehavior.MaximumSinkAccelerationWhole);
        state.VerticalAcceleration = acceleration;
        // The speed grows by the acceleration's whole part read as 1/256 pixels.
        int speed = ClampWholePixels(
            state.VerticalSpeed + EightEight.WholeOf(acceleration), CrocomireBehavior.MaximumSinkSpeedWhole);
        state.VerticalSpeed = speed;

        int travelled = state.VerticalSubpixel + speed;
        state.VerticalSubpixel = EightEight.FractionOf(travelled);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(
            body.Position.Y.Whole + EightEight.WholeOf(travelled))));
    }

    /// <summary>
    /// An 8.8 quantity in 1/256 pixels with its whole part clamped to <paramref name="maximumWhole"/> and its
    /// fraction kept, as the sink's byte-wise clamps leave it.
    /// </summary>
    private static int ClampWholePixels(int value, int maximumWhole) =>
        EightEight.FromWhole(Math.Min(EightEight.WholeOf(value), maximumWhole)) +
        EightEight.FractionOf(value);

    /// <summary>Byte-for-byte fixed-point rise from $A4:92D8.</summary>
    internal void RunCrocomireRise(CrocomireEnemyState state)
    {
        _bg2.FillCrocomireBg2ScrollTable(_bg2.CrocomireBg2Scroll.Y);
        RoomEnemySlot body = state.Body;
        if (unchecked((short)(body.Position.Y.Whole - 218)) < 0)
        {
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
            return;
        }

        _bg2.UpdateCrocomireBg2Scroll(state);
        int acceleration = Math.Min(
            state.VerticalAcceleration + EightEight.FromWhole(1),
            EightEight.FromWhole(CrocomireBehavior.MaximumRiseAccelerationWhole));
        state.VerticalAcceleration = acceleration;

        // The speed loses the acceleration's whole part read as 1/256 pixels; a speed that would fall below zero
        // stalls just under one pixel per frame instead of reversing.
        int speed = state.VerticalSpeed - EightEight.WholeOf(acceleration);
        if (speed < 0)
            speed = CrocomireBehavior.RiseSpeedAfterStalling;
        state.VerticalSpeed = speed;

        int remaining = state.VerticalSubpixel - speed;
        state.VerticalSubpixel = EightEight.FractionOf(remaining);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(
            body.Position.Y.Whole + EightEight.WholeOf(remaining))));
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
        _bg2.SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireSink(state);
        if (state.DeathSequenceIndex != CrocomireDeathPhase.WaitForSamusAtWall)
            return;

        _access.LastCrocomireMusicRequest = new DelayedMusicRequest(
            MusicRequest.SelectTrack(6),
            MusicQueueDelay.Shortest);
        state.DeathSequenceIndex = CrocomireDeathPhase.RiverSkeletonDetour;
        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, CrocomireBodyProgram.SkeletonFlowingDownRiver);
        _room.Services.RoomScroll.Write(4, RoomScrollState.Blue);
        _room.Services.RoomScroll.Write(5, RoomScrollState.Blue);
        if (state.Tongue is { } tongue)
            tongue.Properties = tongue.Properties.With(EnemyProperties.Deleted);
        _access.PublishCrocomirePlm(new Position16<RoomBlocks>(78, 3), PlmHeaderId.ClearCrocomireInvisibleWall);
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.NormalTracking; // $A4:90D8
        _access.RequireCrocomireDeath().SkeletonChunksShown = 0;
    }

    /// <summary>Ports <c>Crocomire_Func_52</c> and initializer $86:9286.</summary>
    internal void SpawnNextCrocomireBridgeFragment()
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        int fragment = death.BridgeFragmentsSpawned;
        if (fragment >= CrocomireBridgeFragmentDefinitions.FragmentCount)
            return;

        // The fragment counts as launched even when no projectile slot is free. The graphics argument the
        // cartridge passes alongside it is immaterial: the definition initializer immediately replaces it.
        death.BridgeFragmentsSpawned = fragment + 1;
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireBridgeCrumbling,
            graphics: default);
        projectile.StartAnimation(new CrocomireProjectileAnimation(CrocomireProjectileFrameKind.BridgeFragment));
        projectile.Position = projectile.Position.WithWholeX(CrocomireBridgeFragmentDefinitions.XPosition(fragment));
        projectile.Position = projectile.Position.WithWholeY(187).WithFractionX(0).WithFractionY(0);
        projectile.Velocity = projectile.Velocity.WithX(new SignedEightEight(0));
        // $86:92A5 reads RandomNumberSeed; it does not advance it.
        projectile.Velocity = projectile.Velocity.WithY(new SignedEightEight(unchecked((ushort)((_room.Random.Read() & 63) + 64))));
        projectile.Graphics = SpriteGraphics.InPalette(2);
    }
}
