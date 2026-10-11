using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Crocomire's 45-entry bridge, melting, skeleton, and completion dispatcher.</summary>
public sealed partial class RoomEnemySystem
{
    private void RunCrocomireDeathSequence(CrocomireEnemyState state, SamusState? samus)
    {
        switch (state.DeathSequenceIndex)
        {
            case CrocomireDeathPhase.CrumbleBridgeAndSink:
                SpawnNextCrocomireBridgeFragment();
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhase.FirstSubmergedPause:
                RunCrocomireAcidSoundTimer();
                UpdateCrocomireBg2Scroll(state, includeVerticalPosition: false);
                RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.FirstHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhase.FirstHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhase.SecondSubmergedPause:
                RunCrocomireAcidSoundTimer();
                UpdateCrocomireBg2Scroll(state, includeVerticalPosition: false);
                RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.SecondHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhase.SecondHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: true);
                return;
            case CrocomireDeathPhase.InstallFirstMeltImage:
                LoadFirstCrocomireMeltingTilemap(state);
                return;
            case CrocomireDeathPhase.CopyFirstMeltGraphics:
                InitializeCrocomireMeltingGraphics(state);
                return;
            case CrocomireDeathPhase.UploadFirstMeltGraphics:
                UploadNextCrocomireMeltingGraphicsSlice(state);
                return;
            case CrocomireDeathPhase.ThirdHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhase.StartFirstDissolve:
                LastCrocomireSoundEffect = 0x0077;
                BeginCrocomireMelting(state);
                return;
            case CrocomireDeathPhase.DissolveFirstImage:
                RunCrocomireMelting(state, samus);
                return;
            case CrocomireDeathPhase.ClearFirstMeltImage:
                FinishCrocomireMeltingPass(state);
                return;
            case CrocomireDeathPhase.FourthHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhase.ThirdSubmergedPause:
                RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.FourthHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhase.FifthHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhase.FourthSubmergedPause:
                RunCrocomireSubmergedPause(state);
                return;
            case CrocomireDeathPhase.FifthHopRise:
                RunCrocomireRisingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhase.SixthHopSink:
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: false);
                return;
            case CrocomireDeathPhase.InstallSecondMeltImage:
                LoadSecondCrocomireMeltingTilemap(state);
                return;
            case CrocomireDeathPhase.CopySecondMeltGraphics:
                InitializeCrocomireMeltingGraphics(state);
                return;
            case CrocomireDeathPhase.UploadSecondMeltGraphics:
                UploadNextCrocomireMeltingGraphicsSlice(state);
                return;
            case CrocomireDeathPhase.ShippedSpacer:
                state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
                return;
            case CrocomireDeathPhase.SixthHopRise:
                SelectCrocomireRisingInstruction(state.Body);
                SpawnCrocomireAcidSmoke(state, samus);
                RunCrocomireRise(state);
                return;
            case CrocomireDeathPhase.StartSecondDissolve:
                LastCrocomireSoundEffect = 0x002d;
                BeginCrocomireMelting(state);
                return;
            case CrocomireDeathPhase.DissolveSecondImage:
                RunCrocomireMelting(state, samus);
                return;
            case CrocomireDeathPhase.ClearSecondMeltImage:
                FinishCrocomireMeltingPass(state);
                return;
            case CrocomireDeathPhase.FinalSink:
                RunCrocomireFinalSink(state, samus);
                return;
            case CrocomireDeathPhase.WaitForSamusAtWall:
                RunCrocomireWaitBehindWall(state, samus);
                return;
            case CrocomireDeathPhase.RumbleHiddenWall:
                RunCrocomireWallRumble(state);
                return;
            case CrocomireDeathPhase.BreakSpikeWall:
                RunCrocomireSkeletonTileLoadAndWallBreak(state);
                return;
            case CrocomireDeathPhase.DelaySkeletonFall:
                RunCrocomireWallBreakDelay(state);
                return;
            case CrocomireDeathPhase.ArcSkeletonIntoArena:
                RunCrocomireSkeletonArc(state);
                return;
            case CrocomireDeathPhase.WaitForSkeletonTerminalImage:
                RunCrocomireSkeletonCollapse(state);
                return;
            case CrocomireDeathPhase.ClearWallAndOpenScrolls:
                FinishCrocomireArenaScrolls(state);
                return;
            case CrocomireDeathPhase.WaitForStableSkeleton:
                if (unchecked((short)(
                        state.Body.CurrentInstruction -
                        CrocomireInstructionProgramDefinitions.SkeletonStable)) >= 0)
                    state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
                return;
            case CrocomireDeathPhase.NativeOneFrameSpacer:
                state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
                return;
            case CrocomireDeathPhase.PublishDefeatAndRestoreMusic:
                CompleteCrocomireBoss(state);
                return;
            case CrocomireDeathPhase.InertCorpse:
                return;
            case CrocomireDeathPhase.DefeatedRoomAdvance:
                state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
                return;
            case CrocomireDeathPhase.PinDefeatedRoomBg2Scroll:
                CrocomireBg2HorizontalScroll = 0;
                CrocomireBg2VerticalScroll = 0;
                return;
            case CrocomireDeathPhase.RiverSkeletonDetour:
                RunCrocomireSkeletonRiver(state);
                return;
            case CrocomireDeathPhase.Fighting:
                throw new InvalidDataException("Crocomire's death sequence runs before its bridge collapse began.");
            default:
                throw new InvalidOperationException($"Undefined CrocomireDeathPhase {state.DeathSequenceIndex}.");
        }
    }

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

    private static void RunCrocomireSubmergedPause(CrocomireEnemyState state)
    {
        if (state.StepCounter != 0)
        {
            state.StepCounter--;
            return;
        }
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
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
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
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
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
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

    private void RunCrocomireFinalSink(CrocomireEnemyState state, SamusState? samus)
    {
        SelectCrocomireRisingInstruction(state.Body);
        SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireSink(state);
        if (state.DeathSequenceIndex != CrocomireDeathPhase.WaitForSamusAtWall)
            return;

        LastCrocomireMusicRequest = new CrocomireMusicRequest(
            MusicCommand.SelectTrack(6),
            MusicCommandDelay.EightFrames);
        state.DeathSequenceIndex = CrocomireDeathPhase.RiverSkeletonDetour;
        InstallCrocomireInstructionList(
            state.Body,
            CrocomireInstructionProgramDefinitions.SkeletonFlowingDownRiver);
        RequireSetRoomScrollState(4, RoomScrollState.Blue);
        RequireSetRoomScrollState(5, RoomScrollState.Blue);
        if (state.Tongue is { } tongue)
            tongue.Properties = tongue.Properties.With(EnemyProperties.Deleted);
        PublishCrocomirePlm(0x4e, 0x03, PlmHeaderId.ClearCrocomireInvisibleWall);
        CameraDistanceIndex = CameraDistanceMode.NormalTracking; // $A4:90D8
        RequireCrocomireDeath().TargetHeightOrSkeletonTileIndex = 0;
    }

    private static void RunCrocomireSkeletonRiver(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        body.XPosition = unchecked((ushort)(body.XPosition - 2));
        if (unchecked((short)(body.XPosition - 480)) < 0)
        {
            body.XPosition = 480;
            body.YPosition = 54;
            state.DeathSequenceIndex = CrocomireDeathPhase.WaitForSamusAtWall;
        }
        else
        {
            body.YPosition = 220;
        }
    }

    private void RunCrocomireWaitBehindWall(CrocomireEnemyState state, SamusState? samus)
    {
        if (samus is null || unchecked((short)(samus.XPosition - 640)) >= 0)
            return;

        LastCrocomireMusicRequest = new CrocomireMusicRequest(
            MusicCommand.SelectTrack(5),
            MusicCommandDelay.EightFrames);
        RequireSetRoomScrollState(3, RoomScrollState.RedBoundary);
        RequireSetRoomScrollState(4, RoomScrollState.Blue);
        PublishCrocomirePlm(0x30, 0x03, PlmHeaderId.CreateCrocomireInvisibleWall);
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
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

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
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
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
        PublishCrocomirePlm(0x20, 0x03, PlmHeaderId.ClearCrocomireInvisibleWall);
        PublishCrocomirePlm(0x1e, 0x03, PlmHeaderId.CreateCrocomireInvisibleWall);
        PublishCrocomirePlm(0x70, 0x0b, PlmHeaderId.ClearCrocomireBridge);
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
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

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
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

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
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

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
        PublishCrocomirePlm(0x30, 0x03, PlmHeaderId.ClearCrocomireInvisibleWall);
        LastCrocomireDropRequest = new CrocomireDropRequest();

        // $A0:B995 emits sixteen independent $F337 projectiles across Crocomire's
        // authored arena rectangle. These are not a single pickup at the corpse position:
        // each actor advances RNG again while selecting its own drop from header $DDBF.
        SpawnEnemyDropScatter(
            EnemyDefinitionId.Crocomire,
            count: 16,
            xBase: 576,
            xMask: 0x007f,
            yBase: 96,
            yMask: 0x3f00);
        RequireCrocomireDeath().ItemDropRequested = true;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

    private void FinishCrocomireArenaScrolls(CrocomireEnemyState state)
    {
        for (int index = 0; index < 4; index++)
            RequireSetRoomScrollState(index, RoomScrollState.Blue);
        PublishCrocomirePlm(0x1e, 0x03, PlmHeaderId.ClearCrocomireInvisibleWall);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

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
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }

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
