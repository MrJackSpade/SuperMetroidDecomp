namespace SuperMetroid.Core.Game;

/// <summary>Crocomire's 45-entry bridge, melting, skeleton, and completion dispatcher.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort CrocomireSinkingHighList = 0xbf64;
    private const ushort CrocomireSinkingMidHighList = 0xbf6c;
    private const ushort CrocomireSinkingMidLowList = 0xbf72;
    private const ushort CrocomireSinkingLowList = 0xbf78;
    private const ushort CrocomireRisingHighList = 0xbf7e;
    private const ushort CrocomireRisingMidHighList = 0xbf86;
    private const ushort CrocomireRisingMidLowList = 0xbf8c;
    private const ushort CrocomireRisingLowList = 0xbf92;
    private const ushort CrocomireMeltingTongueList = 0xbf98;
    private const ushort CrocomireSkeletonFallingList = 0xe14a;
    private const ushort CrocomireSkeletonFallsApartList = 0xe158;
    private const ushort CrocomireSkeletonStableList = 0xe1c6;
    private const ushort CrocomireSkeletonRiverList = 0xe1d2;
    private const int CrocomireBridgeFragmentGraphicsTable = 0xa49156;
    private const int CrocomireFirstMeltingTilemap = 0xa49c79;
    private const int CrocomireSecondMeltingTilemap = 0xa49e7b;
    private const int CrocomireRumbleTable = 0xa498ca;

    private void RunCrocomireDeathSequence(CrocomireEnemyState state, SamusState? samus)
    {
        switch (state.DeathSequenceIndex)
        {
            case 0x02: // Bridge crumbles while Crocomire sinks into the acid.
                SpawnNextCrocomireBridgeFragment(state);
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: true);
                return;
            case 0x04: // First submerged pause.
                RunCrocomireAcidSoundTimer();
                UpdateCrocomireBg2Scroll(state, includeVerticalPosition: false);
                RunCrocomireSubmergedPause(state);
                return;
            case 0x06: // Hop one: rise.
                RunCrocomireRisingComposite(state, samus, tickAcidSound: true);
                return;
            case 0x08: // Hop one: sink.
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: true);
                return;
            case 0x0a: // Second submerged pause.
                RunCrocomireAcidSoundTimer();
                UpdateCrocomireBg2Scroll(state, includeVerticalPosition: false);
                RunCrocomireSubmergedPause(state);
                return;
            case 0x0c: // Hop two: rise.
                RunCrocomireRisingComposite(state, samus, tickAcidSound: true);
                return;
            case 0x0e: // Hop two: sink.
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: true);
                return;
            case 0x10: // Install first melting tilemap and tongue/arm actor.
                InitializeCrocomireMeltingTilemap(
                    state,
                    CrocomireFirstMeltingTilemap,
                    CrocomireSinkingHighList);
                return;
            case 0x12: // Copy first melting graphics into the native scratch image.
                InitializeCrocomireMeltingGraphics(state);
                return;
            case 0x14: // Upload one first-melt graphics slice per frame.
                UploadNextCrocomireMeltingGraphicsSlice(state);
                return;
            case 0x16: // Hop three: rise.
                RunCrocomireRisingComposite(state, samus, tickAcidSound: false);
                return;
            case 0x18: // Start first column dissolve and its vertical-scroll HDMA table.
                LastCrocomireSoundEffect = 0x0077;
                BeginCrocomireMelting(state);
                return;
            case 0x1a: // Dissolve first body image.
                RunCrocomireMelting(state, samus);
                return;
            case 0x1c: // Clear BG2 after first body is gone.
                FinishCrocomireMeltingPass(state);
                return;
            case 0x1e: // Hop four: sink.
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: false);
                return;
            case 0x20: // Third submerged pause.
                RunCrocomireSubmergedPause(state);
                return;
            case 0x22: // Hop four: rise.
                RunCrocomireRisingComposite(state, samus, tickAcidSound: false);
                return;
            case 0x24: // Hop five: sink.
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: false);
                return;
            case 0x26: // Fourth submerged pause.
                RunCrocomireSubmergedPause(state);
                return;
            case 0x28: // Hop five: rise.
                RunCrocomireRisingComposite(state, samus, tickAcidSound: false);
                return;
            case 0x2a: // Hop six: sink.
                RunCrocomireSinkingComposite(state, samus, tickAcidSound: false);
                return;
            case 0x2c: // Install second melting tilemap.
                InitializeCrocomireMeltingTilemap(
                    state,
                    CrocomireSecondMeltingTilemap,
                    CrocomireRisingHighList);
                return;
            case 0x2e: // Copy second melting graphics.
                InitializeCrocomireMeltingGraphics(state);
                return;
            case 0x30: // Upload one second-melt graphics slice per frame.
                UploadNextCrocomireMeltingGraphicsSlice(state);
                return;
            case 0x32: // Shipped index-only spacer.
                state.DeathSequenceIndex += 2;
                return;
            case 0x34: // Hop six: rise.
                SelectCrocomireRisingInstruction(state.Body);
                SpawnCrocomireAcidSmoke(state, samus);
                RunCrocomireRise(state);
                return;
            case 0x36: // Start second dissolve.
                LastCrocomireSoundEffect = 0x002d;
                BeginCrocomireMelting(state);
                return;
            case 0x38: // Dissolve second body image.
                RunCrocomireMelting(state, samus);
                return;
            case 0x3a: // Clear BG2 after second body is gone.
                FinishCrocomireMeltingPass(state);
                return;
            case 0x3c: // Final sink, then select the river-skeleton detour at index $58.
                RunCrocomireFinalSink(state, samus);
                return;
            case 0x3e: // Skeleton behind the wall waits for Samus to return left.
                RunCrocomireWaitBehindWall(state, samus);
                return;
            case 0x40: // Rumble the hidden wall using the ROM's signed table.
                RunCrocomireWallRumble(state);
                return;
            case 0x42: // Load skeleton OBJ tiles, then break the spike wall.
                RunCrocomireSkeletonTileLoadAndWallBreak(state);
                return;
            case 0x44: // Eighty-frame delay before the skeleton starts falling.
                RunCrocomireWallBreakDelay(state);
                return;
            case 0x46: // Skeleton arcs back into the arena.
                RunCrocomireSkeletonArc(state);
                return;
            case 0x48: // Wait for falls-apart instruction list to reach its terminal image.
                RunCrocomireSkeletonCollapse(state);
                return;
            case 0x4a: // Reopen the four left-hand scroll cells and clear the wall.
                FinishCrocomireArenaScrolls(state);
                return;
            case 0x4c: // Wait for the stable skeleton frame.
                if (unchecked((short)(state.Body.CurrentInstruction - CrocomireSkeletonStableList)) >= 0)
                    state.DeathSequenceIndex += 2;
                return;
            case 0x4e: // Native one-frame index-only state.
                state.DeathSequenceIndex += 2;
                return;
            case 0x50: // Publish the miniboss bit and restore boss music.
                CompleteCrocomireBoss(state);
                return;
            case 0x52: // Final live-room corpse state is intentionally inert.
                return;
            case 0x54: // Already-defeated room initializer's one-frame advance.
                state.DeathSequenceIndex += 2;
                return;
            case 0x56: // Already-defeated room pins BG2 scrolls at zero.
                CrocomireBg2HorizontalScroll = 0;
                CrocomireBg2VerticalScroll = 0;
                return;
            case 0x58: // River detour sequenced between indexes $3C and $3E.
                RunCrocomireSkeletonRiver(state);
                return;
            default:
                throw new InvalidDataException(
                    $"Crocomire death-sequence index ${state.DeathSequenceIndex:X2} is outside the 45-entry ROM table.");
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

    private void RunCrocomireSubmergedPause(CrocomireEnemyState state)
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
        body.ExtraProperties &= 0x7fff;

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

    private void SelectCrocomireSinkingInstruction(RoomEnemySlot body)
    {
        ushort pointer = body.YPosition < 248
            ? CrocomireSinkingLowList
            : body.YPosition < 264
                ? CrocomireSinkingMidLowList
                : body.YPosition < 280
                    ? CrocomireSinkingMidHighList
                    : CrocomireSinkingHighList;
        InstallCrocomireInstructionList(body, pointer);
    }

    private void SelectCrocomireRisingInstruction(RoomEnemySlot body)
    {
        ushort pointer = body.YPosition < 248
            ? CrocomireRisingLowList
            : body.YPosition < 264
                ? CrocomireRisingMidLowList
                : body.YPosition < 280
                    ? CrocomireRisingMidHighList
                    : CrocomireRisingHighList;
        InstallCrocomireInstructionList(body, pointer);
    }

    private void RunCrocomireFinalSink(CrocomireEnemyState state, SamusState? samus)
    {
        SelectCrocomireRisingInstruction(state.Body);
        SpawnCrocomireAcidSmoke(state, samus);
        RunCrocomireSink(state);
        if (state.DeathSequenceIndex != 0x3e)
            return;

        LastCrocomireMusicRequest = new CrocomireMusicRequest(Track: 6, DelayFrames: 8);
        state.DeathSequenceIndex = 0x58;
        InstallCrocomireInstructionList(state.Body, CrocomireSkeletonRiverList);
        _setRoomScrollByte?.Invoke(4, 1);
        _setRoomScrollByte?.Invoke(5, 1);
        if (state.Tongue is { } tongue)
            tongue.Properties |= 0x0200;
        PublishCrocomirePlm(0x4e, 0x03, 0xb753);
        RequireCrocomireDeath().TargetHeightOrSkeletonTileIndex = 0;
    }

    private void RunCrocomireSkeletonRiver(CrocomireEnemyState state)
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

    private void RunCrocomireWaitBehindWall(CrocomireEnemyState state, SamusState? samus)
    {
        if (samus is null || unchecked((short)(samus.XPosition - 640)) >= 0)
            return;

        LastCrocomireMusicRequest = new CrocomireMusicRequest(Track: 5, DelayFrames: 8);
        _setRoomScrollByte?.Invoke(3, 0);
        _setRoomScrollByte?.Invoke(4, 1);
        PublishCrocomirePlm(0x30, 0x03, 0xb757);
        RoomEnemySlot body = state.Body;
        body.Properties = unchecked((ushort)((body.Properties & 0x7bff) | 0x0400));
        if (state.Tongue is { } tongue)
            tongue.Properties |= 0x0500;
        state.StepCounter = 4;

        CrocomireDeathState death = RequireCrocomireDeath();
        death.RumbleYOffset = 0;
        death.RumbleCooldown = 10;
        death.RumbleDelta = 1;
        state.FightFlags = 0;
        body.YRadius = 56;
        state.DeathSequenceIndex += 2;
    }

    private void RunCrocomireWallRumble(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        ushort target = ReadWord(_bus!, CrocomireRumbleTable + state.StepCounter);
        if (target == 0x8080)
        {
            death.RumbleYOffset = 0x8080;
            state.StepCounter = 0x0080;
            _cgram!.LoadFromBus(_bus!, 0xa4b91d, colorCount: 16, destinationIndex: 176);
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

                state.StepCounter += 2;
                death.RumbleCooldown = ReadWord(_bus!, CrocomireRumbleTable + state.StepCounter);
                state.StepCounter += 2;
                death.RumbleDelta = ReadWord(_bus!, CrocomireRumbleTable + state.StepCounter);
            }
            state.StepCounter += 2;
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
        PublishCrocomirePlm(0x20, 0x03, 0xb753);
        PublishCrocomirePlm(0x1e, 0x03, 0xb757);
        PublishCrocomirePlm(0x70, 0x0b, 0xb747);
        LastCrocomireSoundEffect = 0x0029;
        InstallCrocomireInstructionList(body, CrocomireSkeletonFallsApartList);
        body.PaletteIndex = 0;
        _cgram!.LoadFromBus(_bus!, 0xa4b8fd, colorCount: 16, destinationIndex: 144);

        foreach (RoomEnemyProjectileSlot projectile in _enemyProjectiles)
            projectile.Clear();
        SpawnCrocomireSpikeWallPieces();
        LastCrocomireSoundEffect = 0x0030;
        state.DeathSequenceIndex += 2;
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
        InstallCrocomireInstructionList(state.Body, CrocomireSkeletonFallingList);
        state.DeathSequenceIndex += 2;
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
        InstallCrocomireInstructionList(state.Body, CrocomireSkeletonFallsApartList);
        state.DeathSequenceIndex += 2;
    }

    private void RunCrocomireSkeletonCollapse(CrocomireEnemyState state)
    {
        if (unchecked((short)(state.Body.CurrentInstruction - CrocomireSkeletonStableList)) < 0)
        {
            (state.ReactionTimer, state.ProjectileCounter) = CalculateCrocomireVelocity(
                state.ReactionTimer,
                state.ProjectileCounter,
                0x00001000,
                6);
            return;
        }

        InstallCrocomireInstructionList(state.Body, CrocomireDeadInstructionList);
        state.Body.XPosition += 64;
        state.Body.YPosition += 21;
        state.Body.YRadius = 28;
        state.Body.XRadius = 40;
        PublishCrocomirePlm(0x30, 0x03, 0xb753);
        LastCrocomireDropRequest = new CrocomireDropRequest(
            state.Body.XPosition,
            state.Body.YPosition,
            state.Body.Definition.ItemDropChancesPointer);

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

    private void FinishCrocomireArenaScrolls(CrocomireEnemyState state)
    {
        for (int index = 0; index < 4; index++)
            _setRoomScrollByte?.Invoke(index, 1);
        PublishCrocomirePlm(0x1e, 0x03, 0xb753);
        state.DeathSequenceIndex += 2;
    }

    private void CompleteCrocomireBoss(CrocomireEnemyState state)
    {
        LastCrocomireMusicRequest = new CrocomireMusicRequest(Track: 6, DelayFrames: 8);
        _setAreaMiniBossDefeated?.Invoke();
        RequireCrocomireDeath().BossBitSet = true;
        SpawnCrocomireDust(state, -16);
        SpawnCrocomireDust(state, 16);
        state.DeathSequenceIndex += 2;
    }

    private void UploadNextCrocomireSkeletonTileChunk(CrocomireDeathState death)
    {
        int entry = death.TargetHeightOrSkeletonTileIndex >> 1;
        ushort destinationOffset = ReadWord(_bus!, 0xa499cb + entry * 2);
        if (destinationOffset == 0xffff)
            return;
        ushort source = ReadWord(_bus!, 0xa499d9 + entry * 2);
        byte[] bytes = new byte[0x0200];
        for (int index = 0; index < bytes.Length; index++)
            bytes[index] = _bus!.ReadByte(0xad0000 | unchecked((ushort)(source + index)));

        // OBSEL is $03 in ordinary gameplay, so its low-three-bit base contributes $6000
        // words before the table's authored offset, exactly as $A4:9931-$9942 computes.
        _vram!.LoadBytes((0x6000 + destinationOffset) * 2, bytes);
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
