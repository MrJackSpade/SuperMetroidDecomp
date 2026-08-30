using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed lifecycle and combat regression for Hibashi/fire geyser definition $E07F.
/// Lower Norfair room $B236 supplies a real paired graphics/hitbox actor with a nonzero
/// inactive delay. The prefix wrapper inserts only the population terminator after that
/// unchanged pair; definition data, code, instruction lists, shape tables, and room assets
/// continue to come from the user's retail cartridge.
/// </summary>
internal static class HibashiAudit
{
    private const ushort RoomPointer = 0xb236;
    private const ushort ExpectedStatePointer = 0xb243;
    private const ushort DefinitionPointer = 0xe07f;
    private const ushort GraphicsInstructionList = 0x8d1b;
    private const ushort HitboxInstructionList = 0x8da9;
    private const int YOffsetTable = 0xa68dbb;
    private const int YRadiusTable = 0xa68de7;
    private const int ActivityFrameCount = 22;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        ushort pairPopulation = FindFirstPopulationRecord(
            bus,
            room.State.EnemyPopulationPointer,
            DefinitionPointer);

        LoadedHibashi lifecycle = LoadPair(bus, room, assets, pairPopulation);
        VerifyInitialization(room, lifecycle);
        (int frames, int maps) = VerifyCompleteEruption(bus, assets, lifecycle);
        VerifyContactAttack(bus, room, assets, pairPopulation);
        VerifyShotNoOp(bus, room, assets, pairPopulation);

        Console.WriteLine(
            $"Hibashi audit passed: the retail paired actor completed all " +
            $"{ActivityFrameCount} ROM shape commands across {frames} frames and {maps} " +
            "visual maps; its moving invisible hitbox dealt 30 contact damage, cleanup " +
            "restored the 80-frame inactive delay, and the authored RTL shot callback " +
            "left health and projectile state unchanged.");
        return 0;
    }

    private static void VerifyInitialization(
        CartridgeRoomHeader room,
        LoadedHibashi loaded)
    {
        RoomEnemySlot graphics = loaded.Graphics;
        RoomEnemySlot hitbox = loaded.Hitbox;
        HibashiEnemyState graphicsState = RequireState(loaded.Enemies, graphics);
        HibashiEnemyState hitboxState = RequireState(loaded.Enemies, hitbox);
        RoomEnemyDefinition definition = graphics.Definition;

        if (room.State.Pointer != ExpectedStatePointer || loaded.Enemies.EnemyCount != 2 ||
            graphics.EnemyDefinitionPointer != DefinitionPointer ||
            hitbox.EnemyDefinitionPointer != DefinitionPointer ||
            graphics.XPosition != 0x0197 || graphics.YPosition != 0x02b8 ||
            hitbox.XPosition != graphics.XPosition || hitbox.YPosition != graphics.YPosition ||
            graphics.Parameter1 != 0x0050 || graphics.Parameter2 != 0 ||
            hitbox.Parameter1 != 0 || hitbox.Parameter2 != 1 ||
            graphics.Properties != 0x2500 || hitbox.Properties != 0x2100 ||
            graphics.CurrentInstruction != GraphicsInstructionList ||
            hitbox.CurrentInstruction != HitboxInstructionList ||
            graphics.InstructionTimer != 1 || hitbox.InstructionTimer != 1 ||
            graphics.XRadius != 0 ||
            graphicsState.Function != HibashiEnemyFunction.Inactive ||
            graphicsState.InactiveTimer != 0 || graphicsState.FinishedActivityFlag != 0 ||
            graphicsState.SpawnYPosition != graphics.YPosition ||
            graphicsState.InactiveTimerResetValue != 0x0050 ||
            hitboxState.Part != 1 || definition.Bank != 0xa6 ||
            definition.InitializationAiPointer != 0x8ffc ||
            definition.MainAiPointer != 0x9023 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x804c ||
            definition.Health != 20 || definition.Damage != 30)
        {
            throw new InvalidDataException(
                $"Hibashi pair initialization mismatch: state=${room.State.Pointer:X4}, " +
                $"count={loaded.Enemies.EnemyCount}, positions=" +
                $"({graphics.XPosition:X4},{graphics.YPosition:X4})/" +
                $"({hitbox.XPosition:X4},{hitbox.YPosition:X4}), params=" +
                $"${graphics.Parameter1:X4}/${graphics.Parameter2:X4} and " +
                $"${hitbox.Parameter1:X4}/${hitbox.Parameter2:X4}, properties=" +
                $"${graphics.Properties:X4}/${hitbox.Properties:X4}, lists=" +
                $"${graphics.CurrentInstruction:X4}/${hitbox.CurrentInstruction:X4}, " +
                $"function=$A6:{(ushort)graphicsState.Function:X4}.");
        }
    }

    private static (int Frames, int Maps) VerifyCompleteEruption(
        ISnesAddressSpace bus,
        CartridgeRoomAssets assets,
        LoadedHibashi loaded)
    {
        RoomEnemySlot graphics = loaded.Graphics;
        RoomEnemySlot hitbox = loaded.Hitbox;
        HibashiEnemyState state = RequireState(loaded.Enemies, graphics);
        var maps = new HashSet<ushort>();
        int nextActivityFrame = 0;
        int elapsedFrames = 0;
        bool sawActive = false;
        bool heardEruption = false;

        while (elapsedFrames < 256)
        {
            Step(loaded, assets);
            elapsedFrames++;
            sawActive |= state.Function == HibashiEnemyFunction.Active;
            heardEruption |= loaded.Enemies.LastHibashiSoundEffect == 0x0061;
            if (graphics.SpritemapPointer is not (0 or 0x804d))
                maps.Add(graphics.SpritemapPointer);

            if (loaded.Enemies.LastHibashiActivityFrameIndex is int frameIndex)
            {
                if (frameIndex != nextActivityFrame)
                {
                    throw new InvalidDataException(
                        $"Hibashi shape command order jumped from {nextActivityFrame} " +
                        $"to {frameIndex} on actor frame {elapsedFrames}.");
                }

                ushort expectedY = unchecked((ushort)(
                    state.SpawnYPosition - ReadWord(bus, YOffsetTable + frameIndex * 2)));
                ushort expectedRadius = ReadWord(bus, YRadiusTable + frameIndex * 2);
                if (hitbox.YPosition != expectedY || hitbox.YRadius != expectedRadius ||
                    frameIndex == 0 && hitbox.XRadius != 8)
                {
                    throw new InvalidDataException(
                        $"Hibashi shape {frameIndex} mismatch: position/radii=" +
                        $"${hitbox.YPosition:X4}/${hitbox.XRadius}/{hitbox.YRadius}, " +
                        $"expected=${expectedY:X4}/" +
                        $"{(frameIndex == 0 ? 8 : hitbox.XRadius)}/{expectedRadius}.");
                }
                nextActivityFrame++;
            }

            if (sawActive && state.Function == HibashiEnemyFunction.Inactive &&
                nextActivityFrame == ActivityFrameCount)
            {
                break;
            }
        }

        if (!sawActive || !heardEruption || nextActivityFrame != ActivityFrameCount ||
            state.Function != HibashiEnemyFunction.Inactive ||
            state.FinishedActivityFlag != 1 || state.InactiveTimer != 0x0050 ||
            graphics.YPosition != state.SpawnYPosition ||
            !graphics.Properties.HasAny(EnemyProperties.Invisible) ||
            hitbox.XRadius != 0 || hitbox.YRadius != 0 ||
            !hitbox.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) ||
            maps.Count < 2)
        {
            throw new InvalidDataException(
                $"Hibashi eruption did not finish cleanly after {elapsedFrames} frames: " +
                $"active/sound/shapes={sawActive}/{heardEruption}/{nextActivityFrame}, " +
                $"function=$A6:{(ushort)state.Function:X4}, finished/timer=" +
                $"{state.FinishedActivityFlag}/${state.InactiveTimer}, radii=" +
                $"{hitbox.XRadius}/{hitbox.YRadius}, properties=" +
                $"${graphics.Properties:X4}/${hitbox.Properties:X4}, maps={maps.Count}.");
        }

        // Native DEC/BPL leaves the actor inactive for exactly parameter1+1 frames. Starting
        // from reset value $50, the first 80 decrements end at zero; the following decrement
        // wraps to $FFFF and starts a new eruption.
        for (int delayFrame = 0; delayFrame < 0x50; delayFrame++)
        {
            Step(loaded, assets);
            if (state.Function != HibashiEnemyFunction.Inactive)
            {
                throw new InvalidDataException(
                    $"Hibashi restarted early on inactive frame {delayFrame + 1}.");
            }
        }
        if (state.InactiveTimer != 0)
            throw new InvalidDataException($"Hibashi inactive timer ended at ${state.InactiveTimer:X4}.");
        Step(loaded, assets);
        if (state.Function != HibashiEnemyFunction.Active ||
            hitbox.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
        {
            throw new InvalidDataException(
                $"Hibashi did not restart after timer underflow: function=" +
                $"$A6:{(ushort)state.Function:X4}, properties=${hitbox.Properties:X4}.");
        }

        return (elapsedFrames, maps.Count);
    }

    private static void VerifyContactAttack(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedHibashi loaded = LoadPair(bus, room, assets, populationPointer);
        WaitForFirstShape(loaded, assets);
        loaded.Samus.XPosition = loaded.Hitbox.XPosition;
        loaded.Samus.YPosition = loaded.Hitbox.YPosition;
        ushort healthBefore = loaded.Samus.Health;
        bool contacted = loaded.Enemies.ResolveOrdinarySamusContact(
            loaded.Samus,
            controllerInput: 0,
            assets.LevelData);
        if (!contacted || loaded.Samus.Health != healthBefore - 30 ||
            !loaded.Samus.KnockbackActive || loaded.Samus.InvincibilityTimer != 0x0060)
        {
            throw new InvalidDataException(
                $"Hibashi contact attack mismatch: contact={contacted}, health=" +
                $"{healthBefore}->{loaded.Samus.Health}, knockback=" +
                $"{loaded.Samus.KnockbackActive}, invincibility=" +
                $"{loaded.Samus.InvincibilityTimer}.");
        }
    }

    private static void VerifyShotNoOp(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedHibashi loaded = LoadPair(bus, room, assets, populationPointer);
        WaitForFirstShape(loaded, assets);
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        SamusProjectileSlot projectile = projectiles.Slots[0];
        ArmProjectile(projectile, loaded.Hitbox);
        ushort healthBefore = loaded.Hitbox.Health;
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            sharedProjectiles,
            loaded.Samus);
        if (hits != 1 || loaded.Hitbox.Health != healthBefore ||
            !projectile.IsActive || (projectile.Type & 0x0f00) == 0x0700)
        {
            throw new InvalidDataException(
                $"Hibashi RTL shot callback mismatch: hits={hits}, health=" +
                $"{healthBefore}->{loaded.Hitbox.Health}, active={projectile.IsActive}, " +
                $"type=${projectile.Type:X4}.");
        }
    }

    private static void WaitForFirstShape(
        LoadedHibashi loaded,
        CartridgeRoomAssets assets)
    {
        for (int frame = 0; frame < 16; frame++)
        {
            Step(loaded, assets);
            if (loaded.Enemies.LastHibashiActivityFrameIndex == 0)
                return;
        }
        throw new InvalidDataException("Hibashi never executed its first shape command.");
    }

    private static LoadedHibashi LoadPair(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        var pairBus = new PopulationPrefixAddressSpace(
            bus,
            populationPointer,
            retainedRecordCount: 2,
            deathQuota: 0);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x0197,
            YPosition = 0x02b8,
        };
        samus.RefreshCollisionRadii(pairBus);
        samus.InitializeAnimation(pairBus);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            pairBus,
            populationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        return new LoadedHibashi(enemies, samus, enemies.Slots[0], enemies.Slots[1]);
    }

    private static void Step(LoadedHibashi loaded, CartridgeRoomAssets assets)
    {
        ushort cameraX = unchecked((ushort)Math.Max(0, loaded.Graphics.XPosition - 128));
        ushort cameraY = unchecked((ushort)Math.Max(0, loaded.Graphics.YPosition - 112));
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData);
    }

    private static ushort FindFirstPopulationRecord(
        ISnesAddressSpace bus,
        ushort populationPointer,
        ushort definitionPointer)
    {
        ushort cursor = populationPointer;
        for (int record = 0; record < RoomEnemySystem.MaximumEnemyCount; record++)
        {
            ushort candidate = ReadWord(bus, 0xa10000 | cursor);
            if (candidate == definitionPointer)
                return cursor;
            if (candidate == 0xffff)
                break;
            cursor = unchecked((ushort)(cursor + 16));
        }
        throw new InvalidDataException(
            $"Population $A1:{populationPointer:X4} contains no Hibashi pair.");
    }

    private static HibashiEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot slot) =>
        enemies.HibashiStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Hibashi slot {slot.SlotIndex} has no typed state.");

    private static void ArmProjectile(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = 20;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private sealed record LoadedHibashi(
        RoomEnemySystem Enemies,
        SamusState Samus,
        RoomEnemySlot Graphics,
        RoomEnemySlot Hitbox);
}
