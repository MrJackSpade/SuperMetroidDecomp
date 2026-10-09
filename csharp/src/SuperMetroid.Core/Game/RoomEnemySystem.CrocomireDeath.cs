using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Crocomire's 45-entry bridge, melting, skeleton, and completion dispatcher.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Dispatches one entry in Crocomire's native death table, including hops, melt transitions, skeleton breakup, and room completion.</summary>
    /// <param name="state">Boss state whose death-sequence index and movement counters select the current phase.</param>
    /// <param name="samus">Optional live Samus state used for acid effects and the wall-wait trigger.</param>
    private void RunCrocomireDeathSequence(CrocomireEnemyState state, SamusState? samus)
    {
        switch (state.DeathSequenceIndex)
        {
            case CrocomireDeathPhases.CrumbleBridgeAndSink:
                SpawnNextCrocomireBridgeFragment(state);
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhases.FirstSubmergedPause:
                RunCrocomireAcidSoundTimer();
                UpdateCrocomireBg2Scroll(state, includeVerticalPosition: false);
                RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhases.FirstHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhases.FirstHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhases.SecondSubmergedPause:
                RunCrocomireAcidSoundTimer();
                UpdateCrocomireBg2Scroll(state, includeVerticalPosition: false);
                RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhases.SecondHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhases.SecondHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhases.InstallFirstMeltImage:
                LoadFirstCrocomireMeltingTilemap(state);
                return;
            case CrocomireDeathPhases.CopyFirstMeltGraphics:
                InitializeCrocomireMeltingGraphics(state);
                return;
            case CrocomireDeathPhases.UploadFirstMeltGraphics:
                UploadNextCrocomireMeltingGraphicsSlice(state);
                return;
            case CrocomireDeathPhases.ThirdHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhases.StartFirstDissolve:
                LastCrocomireSoundEffect = 0x0077;
                BeginCrocomireMelting(state);
                return;
            case CrocomireDeathPhases.DissolveFirstImage:
                RunCrocomireMelting(state, samus);
                return;
            case CrocomireDeathPhases.ClearFirstMeltImage:
                FinishCrocomireMeltingPass(state);
                return;
            case CrocomireDeathPhases.FourthHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhases.ThirdSubmergedPause:
                RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhases.FourthHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhases.FifthHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhases.FourthSubmergedPause:
                RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhases.FifthHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhases.SixthHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhases.InstallSecondMeltImage:
                LoadSecondCrocomireMeltingTilemap(state);
                return;
            case CrocomireDeathPhases.CopySecondMeltGraphics:
                InitializeCrocomireMeltingGraphics(state);
                return;
            case CrocomireDeathPhases.UploadSecondMeltGraphics:
                UploadNextCrocomireMeltingGraphicsSlice(state);
                return;
            case CrocomireDeathPhases.ShippedSpacer:
                state.DeathSequenceIndex += 2;
                return;
            case CrocomireDeathPhases.SixthHopRise:
                SelectCrocomireRisingInstruction(state.Body);
                SpawnCrocomireAcidSmoke(state, samus);
                RunCrocomireRise(state);
                return;
            case CrocomireDeathPhases.StartSecondDissolve:
                LastCrocomireSoundEffect = 0x002d;
                BeginCrocomireMelting(state);
                return;
            case CrocomireDeathPhases.DissolveSecondImage:
                RunCrocomireMelting(state, samus);
                return;
            case CrocomireDeathPhases.ClearSecondMeltImage:
                FinishCrocomireMeltingPass(state);
                return;
            case CrocomireDeathPhases.FinalSink:
                RunCrocomireFinalSink(state, samus);
                return;
            case CrocomireDeathPhases.WaitForSamusAtWall:
                RunCrocomireWaitBehindWall(state, samus);
                return;
            case CrocomireDeathPhases.RumbleHiddenWall:
                RunCrocomireWallRumble(state);
                return;
            case CrocomireDeathPhases.BreakSpikeWall:
                RunCrocomireSkeletonTileLoadAndWallBreak(state);
                return;
            case CrocomireDeathPhases.DelaySkeletonFall:
                RunCrocomireWallBreakDelay(state);
                return;
            case CrocomireDeathPhases.ArcSkeletonIntoArena:
                RunCrocomireSkeletonArc(state);
                return;
            case CrocomireDeathPhases.WaitForSkeletonTerminalImage:
                RunCrocomireSkeletonCollapse(state);
                return;
            case CrocomireDeathPhases.ClearWallAndOpenScrolls:
                FinishCrocomireArenaScrolls(state);
                return;
            case CrocomireDeathPhases.WaitForStableSkeleton:
                if (unchecked((short)(
                        state.Body.CurrentInstruction -
                        CrocomireInstructionProgramDefinitions.SkeletonStable)) >= 0)
                    state.DeathSequenceIndex += 2;
                return;
            case CrocomireDeathPhases.NativeOneFrameSpacer:
                state.DeathSequenceIndex += 2;
                return;
            case CrocomireDeathPhases.PublishDefeatAndRestoreMusic:
                CompleteCrocomireBoss(state);
                return;
            case CrocomireDeathPhases.InertCorpse:
                return;
            case CrocomireDeathPhases.DefeatedRoomAdvance:
                state.DeathSequenceIndex += 2;
                return;
            case CrocomireDeathPhases.PinDefeatedRoomBg2Scroll:
                CrocomireBg2HorizontalScroll = 0;
                CrocomireBg2VerticalScroll = 0;
                return;
            case CrocomireDeathPhases.RiverSkeletonDetour:
                RunCrocomireSkeletonRiver(state);
                return;
            default:
                throw new InvalidDataException(
                    $"Crocomire death-sequence index ${state.DeathSequenceIndex:X2} is outside the 45-entry ROM table.");
        }
    }

    /// <summary>Combines the sinking movement with acid smoke and, for early hops, the periodic acid sound timer.</summary>
    /// <param name="state">Crocomire state advanced by the sink routine.</param>
    /// <param name="samus">Optional Samus liquid state used to place acid smoke.</param>
    /// <param name="tickAcidSound">Whether this phase still emits the periodic acid sound.</param>
    private void RunCrocomireSinkingComposite(
        CrocomireEnemyState state,
        SamusState? samus,
        bool tickAcidSound)
    {
        if (tickAcidSound)
            RunCrocomireAcidSoundTimer();
        SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireSink(state);
    }

    /// <summary>Combines rising movement with acid smoke and the phase-dependent acid sound timer.</summary>
    /// <param name="state">Crocomire state advanced by the rise routine.</param>
    /// <param name="samus">Optional Samus liquid state used to place acid smoke.</param>
    /// <param name="tickAcidSound">Whether this phase still emits the periodic acid sound.</param>
    private void RunCrocomireRisingComposite(
        CrocomireEnemyState state,
        SamusState? samus,
        bool tickAcidSound)
    {
        if (tickAcidSound)
            RunCrocomireAcidSoundTimer();
        SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireRise(state);
    }

    /// <summary>Ticks the submerged acid ambience timer and requests another sound every 32 updates when it expires.</summary>
    private void RunCrocomireAcidSoundTimer()
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        if (death.AcidSoundTimer == 0)
            return;
        death.AcidSoundTimer--;
        if (death.AcidSoundTimer != 0)
            return;
        death.AcidSoundTimer = 32;
        LastCrocomireSoundEffect = 0x0022;
    }

    /// <summary>Counts down a submerged pause before advancing the death phase and starting the next reaction interval.</summary>
    /// <param name="state">Boss state holding the pause counter and next-phase timer.</param>
    private static void RunCrocomireSubmergedPause(CrocomireEnemyState state)
    {
        if (state.StepCounter != 0)
        {
            state.StepCounter--;
            return;
        }
        state.DeathSequenceIndex += 2;
        state.ReactionTimer = 0x0300;
    }

    /// <summary>Byte-for-byte fixed-point fall from <c>SinkCrocomireDown</c> at $A4:91C1.</summary>
    private void RunCrocomireSink(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        FillCrocomireBg2ScrollTable(CrocomireBg2VerticalScroll);
        state.FightFlags &= 0xf7ff;
        UpdateCrocomireBg2Scroll(state, includeVerticalPosition: true);
        RoomEnemySlot body = state.Body;

        if (unchecked((short)(body.YPosition - 280)) >= 0)
        {
            state.DeathSequenceIndex += 2;
            state.StepCounter = 48;
            return;
        }

        ClearCrocomireBg2SinkRow();
        body.ExtraProperties = body.ExtraProperties.Without(
            EnemyExtraProperties.NewInstructionFrame);

        byte accelerationFraction = (byte)state.StepCounter;
        int accelerationCarry = accelerationFraction + 0x80 > 0xff ? 1 : 0;
        accelerationFraction = unchecked((byte)(accelerationFraction + 0x80));
        int accelerationWhole = (byte)(state.StepCounter >> 8) + 3 + accelerationCarry;
        accelerationWhole = Math.Min(accelerationWhole, 48);
        state.StepCounter = unchecked((ushort)((accelerationWhole << 8) | accelerationFraction));

        byte speedFraction = (byte)state.ReactionTimer;
        int speedCarry = speedFraction + accelerationWhole > 0xff ? 1 : 0;
        speedFraction = unchecked((byte)(speedFraction + accelerationWhole));
        int speedWhole = (byte)(state.ReactionTimer >> 8) + speedCarry;
        speedWhole = Math.Min(speedWhole, 3);
        state.ReactionTimer = unchecked((ushort)((speedWhole << 8) | speedFraction));

        byte subpositionHigh = (byte)(state.ProjectileCounter >> 8);
        int subpositionSum = subpositionHigh + speedFraction;
        subpositionHigh = unchecked((byte)subpositionSum);
        state.ProjectileCounter = unchecked((ushort)(
            (state.ProjectileCounter & 0x00ff) | (subpositionHigh << 8)));
        int yDelta = speedWhole + (subpositionSum > 0xff ? 1 : 0);
        body.YPosition = unchecked((ushort)(body.YPosition + yDelta));
    }

    /// <summary>Byte-for-byte fixed-point rise from $A4:92D8.</summary>
    private void RunCrocomireRise(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        FillCrocomireBg2ScrollTable(CrocomireBg2VerticalScroll);
        RoomEnemySlot body = state.Body;
        if (unchecked((short)(body.YPosition - 218)) < 0)
        {
            state.DeathSequenceIndex += 2;
            return;
        }

        UpdateCrocomireBg2Scroll(state, includeVerticalPosition: true);
        int acceleration = state.StepCounter + 0x0100;
        state.StepCounter = unchecked((ushort)Math.Min(acceleration, 0x1f00));
        int accelerationWhole = state.StepCounter >> 8;

        byte speedFraction = (byte)state.ReactionTimer;
        bool speedBorrow = speedFraction < accelerationWhole;
        speedFraction = unchecked((byte)(speedFraction - accelerationWhole));
        int speedWhole = (byte)(state.ReactionTimer >> 8) - (speedBorrow ? 1 : 0);
        if (speedWhole < 0)
        {
            speedFraction = 0xff;
            speedWhole = 0;
        }
        state.ReactionTimer = unchecked((ushort)((speedWhole << 8) | speedFraction));

        byte subpositionHigh = (byte)(state.ProjectileCounter >> 8);
        bool subpositionBorrow = subpositionHigh < speedFraction;
        subpositionHigh = unchecked((byte)(subpositionHigh - speedFraction));
        state.ProjectileCounter = unchecked((ushort)(
            (state.ProjectileCounter & 0x00ff) | (subpositionHigh << 8)));
        int yDelta = speedWhole + (subpositionBorrow ? 1 : 0);
        body.YPosition = unchecked((ushort)(body.YPosition - yDelta));
    }

    /// <summary>Spawns an acid-smoke sprite at a randomized horizontal offset and the current acid surface.</summary>
    /// <param name="state">Crocomire state supplying the smoke's horizontal origin.</param>
    /// <param name="samus">Optional Samus state supplying the room's current liquid surface; defaults to the native fallback height.</param>
    private void SpawnCrocomireAcidSmoke(CrocomireEnemyState state, SamusState? samus)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        death.AcidSmokeTimer = unchecked((ushort)(death.AcidSmokeTimer - 1));
        if (death.AcidSmokeTimer != 0)
            return;
        death.AcidSmokeTimer = 6;

        ushort random = ReadCrocomireRandom();
        ushort xOffset = unchecked((ushort)(random & 0x003f));
        if ((random & 2) == 0)
            xOffset = unchecked((ushort)~xOffset);
        ushort acidY = samus?.LiquidPhysics.LavaAcidYPosition ?? 0x00e0;
        SpawnRoomSpriteObject(
            unchecked((ushort)(state.Body.XPosition + xOffset)),
            unchecked((ushort)(acidY + 16 - ((random & 0x1f00) >> 8))),
            RoomSpriteObjectKind.CrocomireAcidSmoke,
            graphicsIndex: 0);
    }

    /// <summary>Selects the second melt image's row-height instruction list from Crocomire's current Y coordinate.</summary>
    /// <param name="body">Crocomire body whose instruction list is replaced.</param>
    private static void SelectCrocomireRisingInstruction(RoomEnemySlot body)
    {
        ushort pointer = body.YPosition < 248
            ? CrocomireInstructionProgramDefinitions.MeltingTwoTopFourRows
            : body.YPosition < 264
                ? CrocomireInstructionProgramDefinitions.MeltingTwoTopThreeRows
                : body.YPosition < 280
                    ? CrocomireInstructionProgramDefinitions.MeltingTwoTopTwoRows
                    : CrocomireInstructionProgramDefinitions.MeltingTwoTopRow;
        InstallCrocomireInstructionList(body, pointer);
    }

    /// <summary>Runs the last sink phase and, at its terminal index, switches to the river skeleton and arena scroll setup.</summary>
    /// <param name="state">Boss state advanced through the final sink and river handoff.</param>
    /// <param name="samus">Optional Samus state used to position the accompanying acid smoke.</param>
    private void RunCrocomireFinalSink(CrocomireEnemyState state, SamusState? samus)
    {
        SelectCrocomireRisingInstruction(state.Body);
        SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireSink(state);
        if (state.DeathSequenceIndex != 0x3e)
            return;

        LastCrocomireMusicRequest = new CrocomireMusicRequest(
            MusicCommand.SelectTrack(6),
            MusicCommandDelay.EightFrames);
        state.DeathSequenceIndex = 0x58;
        InstallCrocomireInstructionList(
            state.Body,
            CrocomireInstructionProgramDefinitions.SkeletonFlowingDownRiver);
        RequireSetRoomScrollState(4, RoomScrollState.Blue);
        RequireSetRoomScrollState(5, RoomScrollState.Blue);
        if (state.Tongue is { } tongue)
            tongue.Properties = tongue.Properties.With(EnemyProperties.Deleted);
        PublishCrocomirePlm(0x4e, 0x03, RoomPlmHeaders.ClearCrocomireInvisibleWall);
        CameraDistanceIndex = CameraDistanceMode.NormalTracking; // $A4:90D8
        RequireCrocomireDeath().TargetHeightOrSkeletonTileIndex = 0;
    }

    /// <summary>Moves the flowing skeleton left until it reaches the arena boundary, then returns it to the final-sink start pose.</summary>
    /// <param name="state">Boss state whose body position and death-sequence index are updated.</param>
    private static void RunCrocomireSkeletonRiver(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        body.XPosition = unchecked((ushort)(body.XPosition - 2));
        if (unchecked((short)(body.XPosition - 480)) < 0)
        {
            body.XPosition = 480;
            body.YPosition = 54;
            state.DeathSequenceIndex = 0x3e;
        }
        else
        {
            body.YPosition = 220;
        }
    }

    /// <summary>Waits for Samus to approach the hidden wall, then installs the wall, scroll boundary, and rumble state.</summary>
    /// <param name="state">Boss state initialized for the wall-rumble phase when the trigger is reached.</param>
    /// <param name="samus">Current player position; null leaves the phase waiting.</param>
    private void RunCrocomireWaitBehindWall(CrocomireEnemyState state, SamusState? samus)
    {
        if (samus is null || unchecked((short)(samus.XPosition - 640)) >= 0)
            return;

        LastCrocomireMusicRequest = new CrocomireMusicRequest(
            MusicCommand.SelectTrack(5),
            MusicCommandDelay.EightFrames);
        RequireSetRoomScrollState(3, RoomScrollState.RedBoundary);
        RequireSetRoomScrollState(4, RoomScrollState.Blue);
        PublishCrocomirePlm(0x30, 0x03, RoomPlmHeaders.CreateCrocomireInvisibleWall);
        CameraDistanceIndex = CameraDistanceMode.RightEdge; // $A4:97F3-97F6
        RoomEnemySlot body = state.Body;
        body.Properties = body.Properties.Replace(
            EnemyProperties.SolidToSamus | EnemyProperties.IgnoreSamusCollision,
            EnemyProperties.IgnoreSamusCollision);
        if (state.Tongue is { } tongue)
            tongue.Properties = tongue.Properties.With(
                EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        state.StepCounter = 4;

        CrocomireDeathState death = RequireCrocomireDeath();
        death.RumbleYOffset = 0;
        death.RumbleCooldown = 10;
        death.RumbleDelta = 1;
        state.FightFlags = 0;
        body.YRadius = 56;
        state.DeathSequenceIndex += 2;
    }

    /// <summary>Applies one authored wall-rumble target, sound/cooldown step, or the terminal spike-presentation transition.</summary>
    /// <param name="state">Boss state carrying the rumble-table offset and death-sequence position.</param>
    private void RunCrocomireWallRumble(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        CrocomireRumbleDefinition definition =
            CrocomireRumbleDefinitions.AtOffset(state.StepCounter);
        ushort target = unchecked((ushort)definition.TargetYOffset);
        if (definition.IsTerminator)
        {
            death.RumbleYOffset = 0x8080;
            state.StepCounter = 0x0080;
            (TileArtwork?.CrocomireColors ?? throw new InvalidOperationException(
                "Crocomire wall spikes require installed colors.")).ApplyWallSpikes(_cgram!);
            state.DeathSequenceIndex += 2;
            return;
        }

        if (death.RumbleYOffset == target)
        {
            if (unchecked((short)target) < 0)
            {
                if (death.RumbleCooldown != 0)
                {
                    death.RumbleCooldown--;
                    state.StepCounter -= 2;
                    LastCrocomireSoundEffect = 0x002b;
                    return;
                }

                death.RumbleCooldown = definition.Cooldown;
                death.RumbleDelta = definition.Delta;
            }
            state.StepCounter = definition.NextTargetOffset;
            return;
        }

        death.RumbleYOffset = unchecked((short)(death.RumbleYOffset - target)) >= 0
            ? unchecked((ushort)(death.RumbleYOffset - death.RumbleDelta))
            : unchecked((ushort)(death.RumbleYOffset + death.RumbleDelta));
    }

    /// <summary>Uploads skeleton character chunks during the delay, then breaks the wall and starts the skeleton fall.</summary>
    /// <param name="state">Boss state carrying the upload delay and transition counters.</param>
    private void RunCrocomireSkeletonTileLoadAndWallBreak(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        if (state.StepCounter != 0)
        {
            state.StepCounter--;
            UploadNextCrocomireSkeletonTileChunk(death);
            return;
        }

        RoomEnemySlot body = state.Body;
        body.XPosition = 480;
        body.YPosition = 54;
        death.RumbleCooldown = 80;
        state.ReactionTimer = 0;
        state.ProjectileCounter = 0;
        PublishCrocomirePlm(0x20, 0x03, RoomPlmHeaders.ClearCrocomireInvisibleWall);
        PublishCrocomirePlm(0x1e, 0x03, RoomPlmHeaders.CreateCrocomireInvisibleWall);
        PublishCrocomirePlm(0x70, 0x0b, RoomPlmHeaders.ClearCrocomireBridge);
        LastCrocomireSoundEffect = 0x0029;
        InstallCrocomireInstructionList(
            body,
            CrocomireInstructionProgramDefinitions.SkeletonFallsApart);
        body.PaletteIndex = 0;
        (TileArtwork?.CrocomireColors ?? throw new InvalidOperationException(
            "Crocomire skeleton requires installed colors.")).ApplySkeletonArm(_cgram!);

        foreach (RoomEnemyProjectileSlot projectile in _enemyProjectiles)
            projectile.Clear();
        SpawnCrocomireSpikeWallPieces();
        LastCrocomireSoundEffect = 0x0030;
        state.DeathSequenceIndex += 2;
    }

    /// <summary>Moves Crocomire toward the wall during its cooldown, then starts the falling skeleton instruction list.</summary>
    /// <param name="state">Boss state holding skeleton position, velocity, and death-phase timing.</param>
    private void RunCrocomireWallBreakDelay(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        if (unchecked((short)(state.Body.XPosition - 224)) < 0)
        {
            (state.ReactionTimer, state.ProjectileCounter) = CalculateCrocomireVelocity(
                state.ReactionTimer,
                state.ProjectileCounter,
                0x00008000,
                2);
            (state.Body.XPosition, state.Body.XSubposition) = CalculateCrocomirePosition(
                state.Body.XPosition,
                state.Body.XSubposition,
                state.ReactionTimer,
                state.ProjectileCounter);
        }

        if (death.RumbleCooldown == 0)
            return;
        death.RumbleCooldown--;
        if (death.RumbleCooldown != 0)
            return;
        state.ReactionTimer = 0;
        InstallCrocomireInstructionList(
            state.Body,
            CrocomireInstructionProgramDefinitions.SkeletonFalling);
        state.DeathSequenceIndex += 2;
    }

    /// <summary>Advances the skeleton's ballistic arc and switches to its breakup image after crossing the arena X threshold.</summary>
    /// <param name="state">Boss state carrying arc velocity and body subpixel position.</param>
    private void RunCrocomireSkeletonArc(CrocomireEnemyState state)
    {
        (state.ReactionTimer, state.ProjectileCounter) = CalculateCrocomireVelocity(
            state.ReactionTimer,
            state.ProjectileCounter,
            0x00000800,
            5);
        (state.Body.YPosition, state.Body.YSubposition) = CalculateCrocomirePosition(
            state.Body.YPosition,
            state.Body.YSubposition,
            fraction: 0xe000,
            whole: 0);
        (state.Body.XPosition, state.Body.XSubposition) = CalculateCrocomirePosition(
            state.Body.XPosition,
            state.Body.XSubposition,
            state.ReactionTimer,
            state.ProjectileCounter);
        if (unchecked((short)(state.Body.XPosition - 576)) < 0)
            return;

        LastCrocomireSoundEffect = 0x0025;
        if (state.Tongue is { } tongue)
            tongue.PaletteIndex = state.Body.PaletteIndex;
        InstallCrocomireInstructionList(
            state.Body,
            CrocomireInstructionProgramDefinitions.SkeletonFallsApart);
        state.DeathSequenceIndex += 2;
    }

    /// <summary>Waits for the stable skeleton image, then places the corpse, opens drops, and publishes room cleanup.</summary>
    /// <param name="state">Boss state whose body animation and completion flags determine the collapse handoff.</param>
    private void RunCrocomireSkeletonCollapse(CrocomireEnemyState state)
    {
        if (unchecked((short)(
                state.Body.CurrentInstruction -
                CrocomireInstructionProgramDefinitions.SkeletonStable)) < 0)
        {
            (state.ReactionTimer, state.ProjectileCounter) = CalculateCrocomireVelocity(
                state.ReactionTimer,
                state.ProjectileCounter,
                0x00001000,
                6);
            return;
        }

        InstallCrocomireInstructionList(
            state.Body,
            CrocomireInstructionProgramDefinitions.Dead);
        state.Body.XPosition += 64;
        state.Body.YPosition += 21;
        state.Body.YRadius = 28;
        state.Body.XRadius = 40;
        PublishCrocomirePlm(0x30, 0x03, RoomPlmHeaders.ClearCrocomireInvisibleWall);
        LastCrocomireDropRequest = new CrocomireDropRequest();

        // $A0:B995 emits sixteen independent $F337 projectiles across Crocomire's
        // authored arena rectangle. These are not a single pickup at the corpse position:
        // each actor advances RNG again while selecting its own drop from header $DDBF.
        SpawnEnemyDropScatter(
            CrocomireDefinition,
            count: 16,
            xBase: 576,
            xMask: 0x007f,
            yBase: 96,
            yMask: 0x3f00);
        RequireCrocomireDeath().ItemDropRequested = true;
        state.DeathSequenceIndex += 2;
    }

    /// <summary>Restores all four arena scroll regions and removes the temporary invisible wall.</summary>
    /// <param name="state">Boss state whose death sequence advances after room cleanup.</param>
    private void FinishCrocomireArenaScrolls(CrocomireEnemyState state)
    {
        for (int index = 0; index < 4; index++)
            RequireSetRoomScrollState(index, RoomScrollState.Blue);
        PublishCrocomirePlm(0x1e, 0x03, RoomPlmHeaders.ClearCrocomireInvisibleWall);
        state.DeathSequenceIndex += 2;
    }

    /// <summary>Publishes the defeat/music transition, restores normal camera tracking, and spawns the final dust pair.</summary>
    /// <param name="state">Boss state whose completion sequence advances.</param>
    private void CompleteCrocomireBoss(CrocomireEnemyState state)
    {
        LastCrocomireMusicRequest = new CrocomireMusicRequest(
            MusicCommand.SelectTrack(6),
            MusicCommandDelay.EightFrames);
        CameraDistanceIndex = CameraDistanceMode.NormalTracking; // $A4:9B8D
        RequireSetAreaMiniBossDefeated();
        RequireCrocomireDeath().BossBitSet = true;
        SpawnCrocomireDust(state, -16);
        SpawnCrocomireDust(state, 16);
        state.DeathSequenceIndex += 2;
    }

    /// <summary>Uploads the next editable skeleton character chunk to its fixed native VRAM destination.</summary>
    /// <param name="death">Death state whose transfer index advances after a defined chunk is copied.</param>
    private void UploadNextCrocomireSkeletonTileChunk(CrocomireDeathState death)
    {
        int entry = death.TargetHeightOrSkeletonTileIndex >> 1;
        var artwork = TileArtwork?.CrocomireSkeleton ?? throw new InvalidDataException(
            "Installed Crocomire artwork is missing its skeleton characters.");
        if (!CrocomireSkeletonTransferDefinitions.TryGet(entry, out
                CrocomireSkeletonTransferDefinition frame))
            return;
        // The native OBSEL base contributes $6000 words. Only character
        // pixels are editable; the transfer order and VRAM position are not.
        _vram!.LoadBytes((CrocomireSkeletonTransferDefinitions.ObselBaseWord +
            frame.DestinationOffset) * 2, artwork.Chunk(entry).Span);
        death.TargetHeightOrSkeletonTileIndex += 2;
    }

    /// <summary>Adds a fixed-point acceleration delta and caps the whole-word velocity at its configured maximum.</summary>
    /// <param name="fraction">Low 16-bit fractional part of the velocity.</param>
    /// <param name="whole">High 16-bit whole part of the velocity.</param>
    /// <param name="delta">Unsigned 16.16 acceleration amount to add.</param>
    /// <param name="maximumWhole">Maximum whole-word speed; at the cap the whole word is clamped and the fractional word is retained.</param>
    /// <returns>The resulting fractional and whole words in the order used by Crocomire's death state.</returns>
    private static (ushort Fraction, ushort Whole) CalculateCrocomireVelocity(
        ushort fraction,
        ushort whole,
        uint delta,
        ushort maximumWhole)
    {
        uint fixedValue = ((uint)whole << 16) | fraction;
        fixedValue = unchecked(fixedValue + delta);
        if ((ushort)(fixedValue >> 16) >= maximumWhole)
            fixedValue = (fixedValue & 0xffff) | ((uint)maximumWhole << 16);
        return ((ushort)fixedValue, (ushort)(fixedValue >> 16));
    }

    /// <summary>Adds a signed-by-caller 16.16 displacement to a wrapped world position.</summary>
    /// <param name="position">Current whole-pixel coordinate.</param>
    /// <param name="subposition">Current fractional coordinate.</param>
    /// <param name="fraction">Low word of the displacement.</param>
    /// <param name="whole">High word of the displacement.</param>
    /// <returns>Updated whole and fractional position words.</returns>
    private static (ushort Position, ushort Subposition) CalculateCrocomirePosition(
        ushort position,
        ushort subposition,
        ushort fraction,
        ushort whole)
    {
        uint fixedPosition = ((uint)position << 16) | subposition;
        uint delta = ((uint)whole << 16) | fraction;
        fixedPosition = unchecked(fixedPosition + delta);
        return ((ushort)(fixedPosition >> 16), (ushort)fixedPosition);
    }
}
