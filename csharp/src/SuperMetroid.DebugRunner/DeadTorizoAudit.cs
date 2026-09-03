using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>End-to-end ROM-backed audit for Dead Torizo in the pre-Mother-Brain room.</summary>
internal static class DeadTorizoAudit
{
    private const ushort RoomPointer = 0xdc65;
    private const ushort StatePointer = 0xdc77;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer) with
        {
            State = CartridgeRoomState.Load(bus, StatePointer),
        };
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        LoadedDeadTorizo loaded = Load(bus, room, assets);
        RoomEnemySlot corpse = loaded.Corpse;
        DeadTorizoEnemyState state = loaded.State;

        if (corpse.Definition.InitializationAiPointer != 0xd308 ||
            corpse.Definition.MainAiPointer != 0xd368 ||
            corpse.Definition.TouchAiPointer != 0xd433 ||
            corpse.Definition.ShotAiPointer != 0xd433 ||
            corpse.Definition.PowerBombReactionPointer != 0xd42a)
        {
            throw new InvalidDataException(
                $"Dead Torizo header mismatch: init/main=${corpse.Definition.InitializationAiPointer:X4}/" +
                $"${corpse.Definition.MainAiPointer:X4}, touch/shot=" +
                $"${corpse.Definition.TouchAiPointer:X4}/${corpse.Definition.ShotAiPointer:X4}, " +
                $"power=${corpse.Definition.PowerBombReactionPointer:X4}.");
        }

        if (corpse.CurrentInstruction != 0xd6dc || corpse.InstructionTimer != 1 ||
            corpse.PaletteIndex != 0x0200 || corpse.VariableA != 0xd3ad ||
            corpse.VariableB != 0 || corpse.VariableC != 8 ||
            state.CopyFunction != 0xe38b || state.MoveFunction != 0xe272 ||
            state.FinishFunction != 0xd5bd || state.EntryCount != 96 ||
            state.YLimit != 95 || state.LateMoveEntryIndex != 94 ||
            state.SandLineCounter != 15)
        {
            throw new InvalidDataException(
                $"Dead Torizo initialization mismatch: list=${corpse.CurrentInstruction:X4}, " +
                $"function=${corpse.VariableA:X4}, velocity={corpse.VariableB:X4}/" +
                $"{corpse.VariableC:X4}, callbacks=${state.CopyFunction:X4}/" +
                $"${state.MoveFunction:X4}/${state.FinishFunction:X4}, entries=" +
                $"{state.EntryCount}/{state.YLimit}/{state.LateMoveEntryIndex}, " +
                $"sand={state.SandLineCounter}.");
        }

        CorpseRottingTableEntry first = CorpseRottingTableProcessor.ReadEntry(
            bus,
            0x7e0000 | state.TablePointer,
            state.EntryCount,
            0);
        CorpseRottingTableEntry last = CorpseRottingTableProcessor.ReadEntry(
            bus,
            0x7e0000 | state.TablePointer,
            state.EntryCount,
            state.EntryCount - 1);
        if (first is not { YOffset: 95, Timer: 0 } ||
            last is not { YOffset: 0, Timer: 190 })
        {
            throw new InvalidDataException(
                $"Dead Torizo rot table endpoints mismatch: first={first}, last={last}.");
        }

        // The initial extraction leaves the deliberately uncopied padding zero while a
        // known copied region must match its untouched bank-$B7 source byte-for-byte.
        for (int byteIndex = 0; byteIndex < 0x00c0; byteIndex++)
        {
            byte expected = bus.ReadByte(0xb7a920 + byteIndex);
            byte actual = bus.ReadByte(0x7e2060 + byteIndex);
            if (actual != expected)
            {
                throw new InvalidDataException(
                    $"Dead Torizo initial graphics diverged at byte ${byteIndex:X3}: " +
                    $"${actual:X2} != ${expected:X2}.");
            }
        }

        Step(bus, assets.LevelData, loaded, frame: 0);
        if (loaded.Enemies.LastDeadTorizoVramTransfers.Count == 0)
            throw new InvalidDataException("Dead Torizo queued no first-frame VRAM records.");

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, cameraX: 0x0100, cameraY: 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Dead Torizo graphics hook emitted no OBJ pieces.");

        ushort healthBeforeShot = corpse.Health;
        var shots = new SamusProjectileSystem();
        ArmProjectile(shots.Slots[0], corpse);
        int shotHits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            loaded.SharedProjectiles,
            loaded.Samus);
        if (shotHits != 1 || corpse.VariableA != 0xd3e6 ||
            corpse.Health != healthBeforeShot ||
            (shots.Slots[0].Direction & 0x0010) == 0 ||
            shots.Slots[0].PackedType.Family == SamusProjectileFamily.BeamExplosion)
        {
            throw new InvalidDataException(
                $"Dead Torizo shot callback mismatch: hits={shotHits}, " +
                $"function=${corpse.VariableA:X4}, health={healthBeforeShot}->{corpse.Health}, " +
                $"projectile=${shots.Slots[0].Type:X4}/${shots.Slots[0].Direction:X4}.");
        }

        bool sawOddTransfers = false;
        bool sawEvenTransfers = false;
        for (int frame = 1; frame < 2000 && corpse.VariableA != 0xd3c7; frame++)
        {
            Step(bus, assets.LevelData, loaded, frame);
            sawOddTransfers |= (state.VramTransferPhase & 1) != 0 &&
                loaded.Enemies.LastDeadTorizoVramTransfers.Count != 0;
            sawEvenTransfers |= (state.VramTransferPhase & 1) == 0 &&
                loaded.Enemies.LastDeadTorizoVramTransfers.Count != 0;
        }

        if (corpse.VariableA != 0xd3c7 ||
            state.FinishedEntryCount != state.EntryCount ||
            state.DustSpawnCount != state.EntryCount ||
            state.LastFinishedEntryIndex != state.EntryCount - 1 ||
            state.SandLineCopyCount != 15 || state.SandLineCounter != 0 ||
            !sawOddTransfers || !sawEvenTransfers)
        {
            throw new InvalidDataException(
                $"Dead Torizo decomposition incomplete: function=${corpse.VariableA:X4}, " +
                $"finished/dust={state.FinishedEntryCount}/{state.DustSpawnCount}, " +
                $"last={state.LastFinishedEntryIndex}, sand={state.SandLineCopyCount}/" +
                $"{state.SandLineCounter}, alternating={sawOddTransfers}/{sawEvenTransfers}.");
        }

        // This retail header's vulnerability byte rejects power bombs before `$D42A` can
        // dispatch. Keep that outer bank-$A0 admission observable too: merely having a
        // private reaction pointer must not synthesize a callback or property mutation.
        ushort propertiesBeforePowerBomb = corpse.Properties;
        int powerBombReactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            corpse.XPosition,
            corpse.YPosition,
            explosionRadius: 0xff,
            loaded.Samus);
        if (powerBombReactions != 0 || corpse.VariableA != 0xd3c7 ||
            corpse.Properties != propertiesBeforePowerBomb)
        {
            throw new InvalidDataException(
                $"Dead Torizo power-bomb callback mismatch: reactions={powerBombReactions}, " +
                $"function=${corpse.VariableA:X4}, properties=${corpse.Properties:X4}.");
        }

        Console.WriteLine(
            "Dead Torizo audit passed: retail room/state $8F:DC65/DC77 loaded the exact " +
            "header and rot configuration; initial graphics extraction, shared 96-entry " +
            "scheduler, ordinary draw hook, private shot and power-bomb admission, alternating " +
            "VRAM lists, velocity, fifteen sand rows, 96 dust completions, and terminal " +
            "state matched bank $A9.");
        return 0;
    }

    private static LoadedDeadTorizo Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x0180,
            YPosition = 0x00b0,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            isAreaBossDefeated: () => false,
            hasEvent: _ => false,
            setEvent: _ => { },
            clearEvent: _ => { },
            isAreaMiniBossDefeated: () => false,
            setAreaMiniBossDefeated: () => { },
            isAreaTorizoDefeated: () => false,
            setAreaTorizoDefeated: () => { },
            isRoomPlmPresent: _ => false,
            setSamusControlsEnabled: _ => { },
            setRoomScrollByte: (_, _) => { },
            setAreaBossDefeated: () => { },
            incrementMotherBrainGlassRoomArgument: () => { },
            readRoomScrollByte: _ => 0,
            setMotherBrainLayerBlendingDefaultConfig: _ => { },
            setMotherBrainBg2Scroll: (_, _) => { });

        RoomEnemySlot corpse = enemies.Slots.Single(slot =>
            slot.EnemyDefinitionPointer == RoomEnemySystem.DeadTorizoDefinition);
        DeadTorizoEnemyState state = enemies.DeadTorizo ??
            throw new InvalidDataException("Dead Torizo load produced no typed state.");
        return new LoadedDeadTorizo(
            enemies,
            samus,
            corpse,
            state,
            new SamusBombProjectileSystem(),
            vram,
            new VramWriteQueue());
    }

    private static void Step(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        LoadedDeadTorizo loaded,
        int frame)
    {
        // NMI drains the records authored by the preceding main-loop frame before EnemyMain
        // appends this frame's alternating list.
        loaded.VramWrites.DrainTo(loaded.Vram, bus);
        loaded.Enemies.StepFrame(
            cameraX: 0x0100,
            cameraY: 0,
            timeIsFrozen: false,
            loaded.Samus,
            level: level,
            nmiFrameCounter8: unchecked((byte)frame),
            sharedProjectiles: loaded.SharedProjectiles,
            vramWriteQueue: loaded.VramWrites);
    }

    private static void ArmProjectile(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = (ushort)SamusProjectileFamily.Missile;
        projectile.Damage = 100;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 8;
        projectile.YRadius = 8;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private sealed record LoadedDeadTorizo(
        RoomEnemySystem Enemies,
        SamusState Samus,
        RoomEnemySlot Corpse,
        DeadTorizoEnemyState State,
        SamusBombProjectileSystem SharedProjectiles,
        SnesVram Vram,
        VramWriteQueue VramWrites);
}
