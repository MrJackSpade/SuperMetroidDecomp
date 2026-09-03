using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed lifecycle and complete ordinary-combat regression for Ripper $D47F. This is
/// deliberately separate from GRipper/Ripper II: regular Ripper uses common shot AI and a
/// different vulnerability record, even though all three share the linear speed table.
/// Room $A253 contains eight unchanged Rippers alternating both direction parameters.
/// </summary>
internal static class RipperAudit
{
    private const ushort RipperRoomPointer = 0xa253;
    private const ushort RipperStatePointer = 0xa260;
    private const ushort RipperPopulationPointer = 0x9452;
    private const ushort DefinitionPointer = 0xd47f;
    private const int LinearSpeedTable = 0xa28187;

    private static readonly WeaponCase[] ProjectileCases =
    [
        new("power beam", 0x0000, 20),
        new("ice beam", 0x0002, 20),
        new("missile", 0x0100, 20),
        new("super missile", 0x0200, 300),
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RipperRoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        ushort populationPointer = RipperPopulationPointer;

        VerifyInitializationAndBothDirections(bus, room, assets, populationPointer);
        VerifyWallReversalAndAnimation(bus, room, assets, populationPointer);
        VerifyContactDamage(bus, room, assets, populationPointer);
        foreach (WeaponCase weapon in ProjectileCases)
            VerifyProjectileReaction(bus, room, assets, populationPointer, weapon);
        VerifyPowerBombReaction(bus, room, assets, populationPointer);

        Console.WriteLine(
            "Ripper audit passed: both retail direction records loaded exact signed 16.16 " +
            "speed-table pairs, moved and reversed against authored room walls with ROM " +
            "animation; contact dealt five damage, ice froze for 400 frames, and power " +
            "beam/missile/super/power-bomb health, immunity, and death outcomes matched " +
            "the cartridge vulnerability table.");
        return 0;
    }

    private static void VerifyInitializationAndBothDirections(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedRippers loaded = LoadPair(bus, room, assets, populationPointer);
        if (room.State.Pointer != RipperStatePointer ||
            room.State.EnemyPopulationPointer != RipperPopulationPointer ||
            loaded.Enemies.EnemyCount != 2)
        {
            throw new InvalidDataException(
                $"Ripper pair selected state/pop ${room.State.Pointer:X4}/" +
                $"${room.State.EnemyPopulationPointer:X4} with " +
                $"{loaded.Enemies.EnemyCount} actors.");
        }

        RoomEnemySlot left = loaded.Enemies.Slots[0];
        RoomEnemySlot right = loaded.Enemies.Slots[1];
        RoomEnemyDefinition definition = left.Definition;
        if (left.EnemyDefinitionPointer != DefinitionPointer ||
            right.EnemyDefinitionPointer != DefinitionPointer ||
            left.Parameter1 != 0x0010 || right.Parameter1 != 0x0010 ||
            left.Parameter2 != 0 || right.Parameter2 != 1 ||
            left.Properties != 0x2000 || right.Properties != 0x2000 ||
            left.CurrentInstruction != 0xe48b || right.CurrentInstruction != 0xe477 ||
            left.VariableE != 0x0080 || right.VariableE != 0x0080 ||
            left.VariableD != ReadWord(bus, LinearSpeedTable + 0x0084) ||
            left.VariableC != ReadWord(bus, LinearSpeedTable + 0x0086) ||
            right.VariableD != ReadWord(bus, LinearSpeedTable + 0x0080) ||
            right.VariableC != ReadWord(bus, LinearSpeedTable + 0x0082) ||
            unchecked((short)left.VariableD) >= 0 || unchecked((short)right.VariableD) < 0 ||
            definition.Bank != 0xa2 || definition.InitializationAiPointer != 0xe49f ||
            definition.MainAiPointer != 0xe4da || definition.TouchAiPointer != 0x8023 ||
            definition.ShotAiPointer != 0x802d || definition.Health != 200 ||
            definition.Damage != 5)
        {
            throw new InvalidDataException(
                $"Ripper pair initialization mismatch: definitions=" +
                $"${left.EnemyDefinitionPointer:X4}/${right.EnemyDefinitionPointer:X4}, " +
                $"params=${left.Parameter1:X4}/${left.Parameter2:X4} and " +
                $"${right.Parameter1:X4}/${right.Parameter2:X4}, lists=" +
                $"${left.CurrentInstruction:X4}/${right.CurrentInstruction:X4}, " +
                $"velocity=${left.VariableD:X4}:{left.VariableC:X4}/" +
                $"${right.VariableD:X4}:{right.VariableC:X4}, header=" +
                $"${definition.Bank:X2}:{definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}.");
        }
    }

    private static void VerifyWallReversalAndAnimation(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        for (int actorIndex = 0; actorIndex < 2; actorIndex++)
        {
            LoadedRippers loaded = LoadPair(bus, room, assets, populationPointer);
            RoomEnemySlot actor = loaded.Enemies.Slots[actorIndex];
            bool startedPositive = unchecked((short)actor.VariableD) >= 0;
            ushort startX = actor.XPosition;
            bool movedInInitialDirection = false;
            bool reversed = false;
            var maps = new HashSet<ushort>();

            for (int frame = 0; frame < 1024 && !reversed; frame++)
            {
                StepCentered(loaded, room, assets, actor);
                if (actor.SpritemapPointer is not (0 or 0x804d))
                    maps.Add(actor.SpritemapPointer);
                short displacement = unchecked((short)(actor.XPosition - startX));
                movedInInitialDirection |= startedPositive ? displacement > 0 : displacement < 0;
                reversed = (unchecked((short)actor.VariableD) >= 0) != startedPositive;
            }

            bool nowPositive = unchecked((short)actor.VariableD) >= 0;
            ushort expectedList = nowPositive ? (ushort)0xe477 : (ushort)0xe48b;
            bool instructionInExpectedList = nowPositive
                ? actor.CurrentInstruction is >= 0xe477 and < 0xe48b
                : actor.CurrentInstruction is >= 0xe48b and < 0xe49f;
            if (!movedInInitialDirection || !reversed || maps.Count < 2 ||
                !instructionInExpectedList)
            {
                throw new InvalidDataException(
                    $"Ripper direction {actorIndex} wall cycle mismatch: moved/reversed=" +
                    $"{movedInInitialDirection}/{reversed}, X=${startX:X4}->" +
                    $"${actor.XPosition:X4}, velocity=${actor.VariableD:X4}:" +
                    $"{actor.VariableC:X4}, list=${actor.CurrentInstruction:X4}/" +
                    $"${expectedList:X4}, maps={maps.Count}.");
            }
        }
    }

    private static void VerifyContactDamage(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedRippers loaded = LoadPair(bus, room, assets, populationPointer);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        StepCentered(loaded, room, assets, actor);
        loaded.Samus.XPosition = actor.XPosition;
        loaded.Samus.YPosition = actor.YPosition;
        ushort healthBefore = loaded.Samus.Health;
        bool contacted = loaded.Enemies.ResolveOrdinarySamusContact(
            loaded.Samus,
            controllerInput: 0,
            assets.LevelData);
        if (!contacted || loaded.Samus.Health != healthBefore - 5 ||
            !loaded.Samus.KnockbackActive || loaded.Samus.InvincibilityTimer != 0x0060)
        {
            throw new InvalidDataException(
                $"Ripper contact mismatch: contact={contacted}, health=" +
                $"{healthBefore}->{loaded.Samus.Health}, knockback=" +
                $"{loaded.Samus.KnockbackActive}, invincibility=" +
                $"{loaded.Samus.InvincibilityTimer}.");
        }
    }

    private static void VerifyProjectileReaction(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer,
        WeaponCase weapon)
    {
        LoadedRippers loaded = LoadPair(bus, room, assets, populationPointer);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        StepCentered(loaded, room, assets, actor);
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], actor, weapon.Type, weapon.Damage);
        byte vulnerability = ReadVulnerability(bus, actor.Definition, weapon.Type);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            sharedProjectiles,
            loaded.Samus);

        ushort expectedHealth = ExpectedProjectileHealth(
            actor.Definition.Health,
            vulnerability,
            weapon.Damage);
        bool expectedFrozen = vulnerability == 0xff;
        bool expectedDeleted = expectedHealth == 0 && !expectedFrozen;
        if (hits != 1 || actor.Health != expectedHealth ||
            (actor.FrozenTimer != 0) != expectedFrozen ||
            expectedFrozen && (actor.FrozenTimer != 400 || actor.InvincibilityTimer != 10) ||
            actor.Properties.HasAny(EnemyProperties.Deleted) != expectedDeleted ||
            loaded.Enemies.EnemiesKilled != (expectedDeleted ? 1 : 0))
        {
            throw new InvalidDataException(
                $"Ripper {weapon.Name} mismatch: vulnerability=${vulnerability:X2}, " +
                $"hits={hits}, health={actor.Health}/{expectedHealth}, frozen=" +
                $"{actor.FrozenTimer}, deleted=" +
                $"{actor.Properties.HasAny(EnemyProperties.Deleted)}/{expectedDeleted}, " +
                $"kills={loaded.Enemies.EnemiesKilled}.");
        }

        if (expectedFrozen)
        {
            ushort frozenX = actor.XPosition;
            ushort frozenY = actor.YPosition;
            StepCentered(loaded, room, assets, actor);
            if (actor.XPosition != frozenX || actor.YPosition != frozenY ||
                actor.FrozenTimer != 399)
            {
                throw new InvalidDataException(
                    $"Frozen Ripper moved or used the wrong timer cadence: " +
                    $"(${frozenX:X4},${frozenY:X4})->" +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), " +
                    $"timer={actor.FrozenTimer}.");
            }
        }
    }

    private static void VerifyPowerBombReaction(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedRippers loaded = LoadPair(bus, room, assets, populationPointer);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        byte vulnerability = ReadVulnerability(bus, actor.Definition, 0x0300);
        int reactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            actor.XPosition,
            actor.YPosition,
            explosionRadius: 32,
            loaded.Samus);

        bool admitted = (vulnerability & 0x7f) != 0;
        int damage = vulnerability == 0xff ? 0 : 100 * (vulnerability & 0x7f);
        ushort expectedHealth = damage >= actor.Definition.Health
            ? (ushort)0
            : unchecked((ushort)(actor.Definition.Health - damage));
        bool expectedDeleted = admitted && expectedHealth == 0;
        if (reactions != (admitted ? 1 : 0) || actor.Health != expectedHealth ||
            actor.Properties.HasAny(EnemyProperties.Deleted) != expectedDeleted ||
            loaded.Enemies.EnemiesKilled != (expectedDeleted ? 1 : 0))
        {
            throw new InvalidDataException(
                $"Ripper power-bomb mismatch: vulnerability=${vulnerability:X2}, " +
                $"reactions={reactions}/{(admitted ? 1 : 0)}, health=" +
                $"{actor.Health}/{expectedHealth}, deleted=" +
                $"{actor.Properties.HasAny(EnemyProperties.Deleted)}/{expectedDeleted}, " +
                $"kills={loaded.Enemies.EnemiesKilled}.");
        }
    }

    private static LoadedRippers LoadPair(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        RoomEnemyPopulationRecord[] records = ReadMatchingPopulationRecords(
            bus,
            populationPointer,
            DefinitionPointer);
        if (records.Length < 2)
        {
            throw new InvalidDataException(
                $"Selected audit population contains {records.Length} ordinary Rippers; " +
                "the first two are required for opposite directions.");
        }
        var pairBus = new PopulationSelectionAddressSpace(
            bus,
            records.Take(2).ToArray(),
            deathQuota: 0);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x1000,
            YPosition = 0x1000,
        };
        samus.RefreshCollisionRadii(pairBus);
        samus.InitializeAnimation(pairBus);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            pairBus,
            PopulationSelectionAddressSpace.PopulationPointer,
            PopulationSelectionAddressSpace.TilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        return new LoadedRippers(enemies, samus);
    }

    private static void StepCentered(
        LoadedRippers loaded,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        RoomEnemySlot actor)
    {
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData);
    }

    private static (ushort X, ushort Y) CenterCamera(
        CartridgeRoomHeader room,
        RoomEnemySlot actor)
    {
        int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
        int maximumY = Math.Max(0, room.HeightInScreens * 256 - 224);
        return (
            unchecked((ushort)Math.Clamp(actor.XPosition - 128, 0, maximumX)),
            unchecked((ushort)Math.Clamp(actor.YPosition - 112, 0, maximumY)));
    }

    private static ushort ExpectedProjectileHealth(
        ushort health,
        byte vulnerability,
        ushort projectileDamage)
    {
        if (vulnerability == 0xff)
            return health;
        int damage = (projectileDamage >> 1) * (vulnerability & 0x7f);
        return damage >= health ? (ushort)0 : unchecked((ushort)(health - damage));
    }

    private static byte ReadVulnerability(
        SuperMetroidAddressSpace bus,
        RoomEnemyDefinition definition,
        ushort projectileType)
    {
        ushort pointer = definition.VulnerabilityPointer != 0
            ? definition.VulnerabilityPointer
            : (ushort)0xec1c;
        int family = projectileType & 0x0f00;
        int byteOffset = family switch
        {
            0x0000 => projectileType & 0x000f,
            0x0100 => 12,
            0x0200 => 13,
            0x0300 => 15,
            _ => throw new InvalidDataException(
                $"Ripper audit has no vulnerability offset for ${projectileType:X4}."),
        };
        return bus.ReadByte(0xb40000 | unchecked((ushort)(pointer + byteOffset)));
    }

    private static RoomEnemyPopulationRecord[] ReadMatchingPopulationRecords(
        ISnesAddressSpace bus,
        ushort populationPointer,
        ushort definitionPointer)
    {
        var records = new List<RoomEnemyPopulationRecord>();
        ushort cursor = populationPointer;
        for (int record = 0; record < RoomEnemySystem.MaximumEnemyCount; record++)
        {
            ushort candidate = ReadWord(bus, 0xa10000 | cursor);
            if (candidate == 0xffff)
                return records.ToArray();
            if (candidate == definitionPointer)
            {
                int address = 0xa10000 | cursor;
                records.Add(new RoomEnemyPopulationRecord(
                    candidate,
                    ReadWord(bus, address + 2),
                    ReadWord(bus, address + 4),
                    ReadWord(bus, address + 6),
                    ReadWord(bus, address + 8),
                    ReadWord(bus, address + 10),
                    ReadWord(bus, address + 12),
                    ReadWord(bus, address + 14)));
            }
            cursor = unchecked((ushort)(cursor + 16));
        }
        throw new InvalidDataException(
            $"Population $A1:{populationPointer:X4} has no terminator within 32 records.");
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort type,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedRippers(
        RoomEnemySystem Enemies,
        SamusState Samus);

    private readonly record struct WeaponCase(string Name, ushort Type, ushort Damage);
}
