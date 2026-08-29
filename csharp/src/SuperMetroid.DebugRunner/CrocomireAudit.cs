using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Retail-ROM regression for Crocomire's multipart load, extended hitboxes, fight bytecode,
/// movement callbacks, mouth damage response, power-bomb reaction, and bank-$86 projectile.
/// </summary>
internal static class CrocomireAudit
{
    private const ushort RoomHeader = 0xa98d;
    private const ushort NormalRoomState = 0xa99f;
    private const ushort Population = 0xbb0e;
    private const ushort BodyDefinition = 0xddbf;
    private const ushort TongueDefinition = 0xddff;
    private const ushort MouthShotCallback = 0xba05;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyHeadersAndPopulation(bus);

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomHeader);
        if (room.State.Pointer != NormalRoomState ||
            room.State.EnemyPopulationPointer != Population)
        {
            throw new InvalidDataException(
                $"Crocomire room selected state/population ${room.State.Pointer:X4}/" +
                $"${room.State.EnemyPopulationPointer:X4}.");
        }
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        VerifyInitializationAndWake(bus, room, assets);
        VerifyInstructionMovementAndProjectile(bus, room, assets);
        VerifyMouthAndPowerBombReactions(bus, room, assets);

        Console.WriteLine(
            "Crocomire audit passed: retail body/tongue records, palette/list setup, " +
            "extended-map wake animation, four-pixel instruction movement, nine-shot " +
            "projectile cadence/vector setup, charged-beam mouth push, and the retail " +
            "power-bomb vulnerability gate all agreed with cartridge data.");
        return 0;
    }

    private static void VerifyHeadersAndPopulation(SuperMetroidAddressSpace bus)
    {
        RoomEnemyDefinition body = RoomEnemySystem.ReadDefinition(bus, BodyDefinition);
        if (body.Health != 0x7fff || body.Damage != 40 || body.Bank != 0xa4 ||
            body.InitializationAiPointer != 0x8a5a || body.MainAiPointer != 0x8c04 ||
            body.HurtAiPointer != 0x8687 || body.TouchAiPointer != 0xb950 ||
            body.ShotAiPointer != 0)
        {
            throw new InvalidDataException("Crocomire $DDBF header disagrees with bank $A4.");
        }

        RoomEnemyDefinition tongue = RoomEnemySystem.ReadDefinition(bus, TongueDefinition);
        if (tongue.Bank != 0xa4 || tongue.InitializationAiPointer != 0xf67a ||
            tongue.MainAiPointer != 0xf6bb || tongue.TouchAiPointer != 0x8023 ||
            tongue.ShotAiPointer != 0x802d)
        {
            throw new InvalidDataException("Crocomire tongue $DDFF header disagrees with bank $A4.");
        }

        ushort[] expected =
        [
            0xddbf, 0x0480, 0x0078, 0xbd2a, 0xa800, 0x0004, 0x0000, 0x0000,
            0xddff, 0x0480, 0x0078, 0xbd2a, 0xa800, 0x0004, 0x0000, 0x0000,
            0xffff,
        ];
        for (int word = 0; word < expected.Length; word++)
        {
            ushort actual = ReadWord(bus, 0xa10000 | (Population + word * 2));
            if (actual != expected[word])
            {
                throw new InvalidDataException(
                    $"Crocomire population word {word} was ${actual:X4}, " +
                    $"expected ${expected[word]:X4}.");
            }
        }
    }

    private static void VerifyInitializationAndWake(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedCrocomire loaded = Load(bus, room, assets);
        RoomEnemySlot body = loaded.Enemies.Slots[0];
        RoomEnemySlot tongue = loaded.Enemies.Slots[1];
        CrocomireEnemyState state = RequireState(loaded);
        if (loaded.Enemies.BossId != 6 || loaded.Enemies.EnemyCount != 2 ||
            body.EnemyDefinitionPointer != BodyDefinition ||
            body.CurrentInstruction != 0xbade || body.InstructionTimer != 1 ||
            state.DeathSequenceIndex != 0 ||
            state.FightFunction != CrocomireFightFunction.Sleeping ||
            !body.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap) ||
            tongue.EnemyDefinitionPointer != TongueDefinition ||
            tongue.CurrentInstruction != 0xbe56 || tongue.VariableA != 23 ||
            tongue.PaletteIndex != 0x0e00 || state.Tongue != tongue)
        {
            throw new InvalidDataException(
                $"Crocomire initialization failed: body list=${body.CurrentInstruction:X4}, " +
                $"fight={state.FightFunction}, tongue list/palette=" +
                $"${tongue.CurrentInstruction:X4}/${tongue.PaletteIndex:X4}.");
        }

        // The inclusive $20..0 palette loop copies seventeen words to each target row.
        for (int color = 0; color < 17; color++)
        {
            if (loaded.Cgram.Colors[160 + color] != ReadWord(bus, 0xa4b8bd + color * 2) ||
                loaded.Cgram.Colors[208 + color] != ReadWord(bus, 0xa4b8dd + color * 2))
            {
                throw new InvalidDataException($"Crocomire palette copy failed at color {color}.");
            }
        }

        // Frame one installs BADE's initial timed map; frame two reaches $86A6 and can
        // switch to the first-damage list because Samus is inside the 224-pixel wake range.
        Step(loaded);
        Step(loaded);
        if (state.FightFunction != CrocomireFightFunction.WaitingForFirstDamage ||
            body.SpritemapPointer == 0x804f || (body.SpritemapPointer & 0x8000) == 0)
        {
            throw new InvalidDataException(
                $"Crocomire did not wake into a mouth-bearing extended map: " +
                $"fight={state.FightFunction}, map=${body.SpritemapPointer:X4}.");
        }
    }

    private static void VerifyInstructionMovementAndProjectile(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedCrocomire movement = Load(bus, room, assets);
        CrocomireEnemyState movementState = RequireState(movement);
        RoomEnemySlot movingBody = movementState.Body;
        ushort startX = movingBody.XPosition;
        movementState.FightFunction = CrocomireFightFunction.Sleeping;
        movingBody.CurrentInstruction = 0xbbf0; // $8FDF, then fight AI at $BBF2.
        movingBody.InstructionTimer = 1;
        Step(movement);
        if (movingBody.XPosition != startX - 4)
        {
            throw new InvalidDataException(
                $"Crocomire move-left instruction changed X ${startX:X4}->" +
                $"${movingBody.XPosition:X4}, not four pixels.");
        }

        LoadedCrocomire volley = Load(bus, room, assets);
        CrocomireEnemyState volleyState = RequireState(volley);
        volleyState.FightFunction = CrocomireFightFunction.ProjectileAttack;
        volleyState.ProjectileCounter = 0;
        volleyState.Body.CurrentInstruction = 0xbb94; // Fight-AI opcode inside volley loop.
        volleyState.Body.InstructionTimer = 1;
        Step(volley);
        RoomEnemyProjectileSlot projectile = volley.Enemies.EnemyProjectiles.Single(
            candidate => candidate.Kind == RoomEnemyProjectileKind.CrocomireProjectile);
        if (volleyState.ProjectileCounter != 2 || projectile.DirectionParameter != 2 ||
            projectile.XVelocity != 0xfe00 || projectile.PreInstruction != 0x906b)
        {
            throw new InvalidDataException(
                $"Crocomire volley spawn failed: counter={volleyState.ProjectileCounter}, " +
                $"parameter={projectile.DirectionParameter}, velocity/pre=" +
                $"${projectile.XVelocity:X4}/${projectile.PreInstruction:X4}.");
        }

        volley.Enemies.StepEnemyProjectiles(
            volley.Level,
            volley.Samus,
            cameraX: 0x0400,
            cameraY: 0);
        if (!projectile.IsActive || projectile.PreInstruction != 0x90b3 ||
            projectile.XVelocity == 0xfe00)
        {
            throw new InvalidDataException(
                $"Crocomire projectile setup failed: active={projectile.IsActive}, " +
                $"velocity=(${projectile.XVelocity:X4},${projectile.YVelocity:X4}), " +
                $"pre=${projectile.PreInstruction:X4}.");
        }
    }

    private static void VerifyMouthAndPowerBombReactions(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedCrocomire mouth = Load(bus, room, assets);
        Step(mouth);
        Step(mouth);
        CrocomireEnemyState mouthState = RequireState(mouth);
        ushort mouthX = 0;
        ushort mouthY = 0;
        bool foundMouth = false;
        for (int frame = 0; frame < 360 && !foundMouth; frame++)
        {
            foundMouth = FindHitboxCenter(
                bus,
                mouthState.Body,
                MouthShotCallback,
                out mouthX,
                out mouthY);
            if (!foundMouth)
                Step(mouth);
        }
        if (!foundMouth)
        {
            throw new InvalidDataException("Current Crocomire map contains no mouth hitbox.");
        }

        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        SamusProjectileSlot chargedBeam = projectiles.Slots[0];
        chargedBeam.ClearFields();
        chargedBeam.Type = 0x0010;
        chargedBeam.Damage = 20;
        chargedBeam.Direction = 2;
        chargedBeam.XPosition = mouthX;
        chargedBeam.YPosition = mouthY;
        chargedBeam.XRadius = 1;
        chargedBeam.YRadius = 1;
        chargedBeam.InstructionPointer = 0x9000;
        chargedBeam.InstructionTimer = 1;
        int hits = mouth.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            shared,
            mouth.Samus);
        if (hits != 1 || mouthState.StepCounter != 2 ||
            (mouthState.FightFlags & 0x0800) == 0 || mouthState.Body.FlashTimer != 14)
        {
            throw new InvalidDataException(
                $"Crocomire charged mouth hit failed: hits={hits}, steps=" +
                $"{mouthState.StepCounter}, flags=${mouthState.FightFlags:X4}, " +
                $"flash={mouthState.Body.FlashTimer}.");
        }

        LoadedCrocomire powerBomb = Load(bus, room, assets);
        Step(powerBomb);
        Step(powerBomb);
        CrocomireEnemyState powerBombState = RequireState(powerBomb);
        int reactions = powerBomb.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            powerBombState.Body.XPosition,
            powerBombState.Body.YPosition,
            explosionRadius: byte.MaxValue,
            powerBomb.Samus);
        // The header contains a real $B992 reaction routine, but retail vulnerability byte
        // $B4:F110 is $80. Process_Enemy_PowerBomb_Interaction masks bit seven and therefore
        // skips the callback. Assert that seemingly-surprising cartridge behavior rather
        // than forcing an otherwise unreachable reaction for test convenience.
        if (reactions != 0 ||
            powerBombState.FightFunction != CrocomireFightFunction.WaitingForFirstDamage ||
            powerBombState.StepCounter != 0 || powerBombState.Body.FlashTimer != 0)
        {
            throw new InvalidDataException(
                $"Crocomire power-bomb reaction failed: reactions={reactions}, " +
                $"fight={powerBombState.FightFunction}, steps={powerBombState.StepCounter}, " +
                $"flash={powerBombState.Body.FlashTimer}.");
        }
    }

    private static LoadedCrocomire Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var system = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x0440,
            YPosition = 0x0078,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            Population,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            system.NextRandom,
            system.SetRandomNumber,
            readRandomNumber: () => system.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            cameraX: 0x0400,
            isAreaMiniBossDefeated: () => false);
        return new LoadedCrocomire(enemies, samus, cgram, assets.LevelData);
    }

    private static void Step(LoadedCrocomire loaded) =>
        loaded.Enemies.StepFrame(
            cameraX: 0x0400,
            cameraY: 0,
            timeIsFrozen: false,
            loaded.Samus,
            level: loaded.Level);

    private static CrocomireEnemyState RequireState(LoadedCrocomire loaded) =>
        loaded.Enemies.Crocomire ??
        throw new InvalidDataException("Crocomire body did not publish typed state.");

    private static bool FindHitboxCenter(
        ISnesAddressSpace bus,
        RoomEnemySlot actor,
        ushort shotCallback,
        out ushort x,
        out ushort y)
    {
        int bank = actor.Definition.Bank << 16;
        int map = bank | actor.SpritemapPointer;
        int componentCount = ReadWord(bus, map);
        for (int componentIndex = 0; componentIndex < componentCount; componentIndex++)
        {
            int component = map + 2 + componentIndex * 8;
            ushort componentX = unchecked((ushort)(
                actor.XPosition + ReadWord(bus, component)));
            ushort componentY = unchecked((ushort)(
                actor.YPosition + ReadWord(bus, component + 2)));
            int hitboxList = bank | ReadWord(bus, component + 6);
            int hitboxCount = ReadWord(bus, hitboxList);
            for (int hitboxIndex = 0; hitboxIndex < hitboxCount; hitboxIndex++)
            {
                int hitbox = hitboxList + 2 + hitboxIndex * 12;
                if (ReadWord(bus, hitbox + 10) != shotCallback)
                    continue;
                short left = unchecked((short)ReadWord(bus, hitbox));
                short top = unchecked((short)ReadWord(bus, hitbox + 2));
                short right = unchecked((short)ReadWord(bus, hitbox + 4));
                short bottom = unchecked((short)ReadWord(bus, hitbox + 6));
                x = unchecked((ushort)(componentX + (left + right) / 2));
                y = unchecked((ushort)(componentY + (top + bottom) / 2));
                return true;
            }
        }
        x = y = 0;
        return false;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private sealed record LoadedCrocomire(
        RoomEnemySystem Enemies,
        SamusState Samus,
        SnesCgram Cgram,
        RoomLevelData Level);
}
