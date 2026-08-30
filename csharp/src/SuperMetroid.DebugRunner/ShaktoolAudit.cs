using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Retail-ROM audit for Shaktool's seven linked enemy records and the three unreachable but
/// fully-authored bank-$86 attack-circle definitions that remain in the shipped cartridge.
/// </summary>
internal static partial class ShaktoolAudit
{
    private const ushort RoomPointer = 0xd8c5;
    private const ushort StatePointer = 0xd8d7;
    private const ushort PopulationPointer = 0xd281;
    private const ushort Definition = 0xf07f;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.State.Pointer != StatePointer ||
            room.State.EnemyPopulationPointer != PopulationPointer)
        {
            throw new InvalidDataException(
                $"Shaktool room mismatch: state=${room.State.Pointer:X4}, " +
                $"population=${room.State.EnemyPopulationPointer:X4}.");
        }

        VerifyPopulationAndDefinition(bus);
        VerifyLiveEncounter(bus, room, assets);
        VerifyFatalNormalBomb(bus, room, assets);
        VerifyUnusedAttackCircleLifecycles(bus, room, assets);
        Console.WriteLine(
            "Shaktool audit passed: seven retail records initialized from ROM tables, " +
            "formed the aliased linked chain, moved/oriented/reversed against room terrain, " +
            "ran authored animation callbacks, rendered OBJ, dealt contact damage, deleted " +
            "as one group on a fatal shot, and exercised all three unused attack-circle " +
            "definitions through exact linkage, delayed motion, animation, interaction, " +
            "terrain disposal, and dependent teardown.");
        return 0;
    }

    private static void VerifyPopulationAndDefinition(ISnesAddressSpace bus)
    {
        for (int record = 0; record < 7; record++)
        {
            ushort[] expected =
            [
                Definition,
                0x00a8,
                0x00b8,
                0,
                0x2000,
                0,
                0,
                unchecked((ushort)(record * 2)),
            ];
            int address = 0xa10000 | (PopulationPointer + record * 16);
            for (int word = 0; word < expected.Length; word++)
            {
                ushort actual = ReadWord(bus, address + word * 2);
                if (actual != expected[word])
                {
                    throw new InvalidDataException(
                        $"Shaktool record {record} word {word} is ${actual:X4}, " +
                        $"expected ${expected[word]:X4}.");
                }
            }
        }

        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, Definition);
        if (definition.Bank != 0xaa || definition.Health != 300 || definition.Damage != 120 ||
            definition.InitializationAiPointer != 0xde43 ||
            definition.MainAiPointer != 0xdca3 ||
            definition.TouchAiPointer != 0xdf2f || definition.ShotAiPointer != 0xdf34)
        {
            throw new InvalidDataException(
                $"Shaktool header mismatch: bank=${definition.Bank:X2}, " +
                $"health/damage={definition.Health}/{definition.Damage}, init/main=" +
                $"${definition.InitializationAiPointer:X4}/${definition.MainAiPointer:X4}, " +
                $"touch/shot=${definition.TouchAiPointer:X4}/${definition.ShotAiPointer:X4}.");
        }

        RoomEnemyProjectileKind[] kinds =
        [
            RoomEnemyProjectileKind.ShaktoolAttackFrontCircle,
            RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle,
            RoomEnemyProjectileKind.ShaktoolAttackBackCircle,
        ];
        ushort[] initializers = [0xbda2, 0xbd9c, 0xbd9c];
        ushort[] preInstructions = [0xbe03, 0x84fb, 0x84fb];
        ushort[] lists = [0xbd68, 0xbd78, 0xbd8c];
        for (int index = 0; index < kinds.Length; index++)
        {
            int address = 0x860000 | (ushort)kinds[index];
            if (ReadWord(bus, address) != initializers[index] ||
                ReadWord(bus, address + 2) != preInstructions[index] ||
                ReadWord(bus, address + 4) != lists[index] ||
                ReadWord(bus, address + 6) != 0x0404)
            {
                throw new InvalidDataException(
                    $"Shaktool circle definition ${(ushort)kinds[index]:X4} is not retail data.");
            }
        }
    }

    private static void VerifyLiveEncounter(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySystem enemies = CreateEncounter(bus, room, assets, out SamusState samus);
        RoomEnemySlot[] group = GetGroup(enemies);
        VerifyInitialState(bus, enemies, group);

        var observedMaps = new HashSet<ushort>();
        var observedHeadPositions = new HashSet<(ushort X, ushort Y)>();
        var observedTailPositions = new HashSet<(ushort X, ushort Y)>();
        var observedMotionFlags = new HashSet<ushort>();
        for (int frame = 0; frame < 1800; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            foreach (RoomEnemySlot segment in group)
                observedMaps.Add(segment.SpritemapPointer);
            observedHeadPositions.Add((group[0].XPosition, group[0].YPosition));
            observedTailPositions.Add((group[6].XPosition, group[6].YPosition));
            observedMotionFlags.Add(group[6].Parameter1);
        }
        if (observedMaps.Count < 10 || observedMaps.Contains(0) ||
            observedHeadPositions.Count < 2 || observedTailPositions.Count < 8 ||
            observedMotionFlags.Count < 2)
        {
            throw new InvalidDataException(
                $"Shaktool natural cycle incomplete: maps={observedMaps.Count}, " +
                $"head positions={observedHeadPositions.Count}, tail positions=" +
                $"{observedTailPositions.Count}, flags={observedMotionFlags.Count}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Shaktool emitted no OBJ pieces after movement.");

        VerifyUnusedAttackLists(enemies, assets.LevelData, samus, group);
        VerifyContactDamage(enemies, samus, group[0]);
        VerifyFatalShot(bus, enemies, samus, group);
    }

    private static RoomEnemySystem CreateEncounter(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        out SamusState samus)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 0x0040,
            YPosition = 0x0040,
            Pose = SamusState.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        var enemies = new RoomEnemySystem();
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
            samus: samus);
        return enemies;
    }

    private static RoomEnemySlot[] GetGroup(RoomEnemySystem enemies)
    {
        RoomEnemySlot[] group = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == Definition)
            .ToArray();
        if (group.Length != 7 || group.Select(slot => slot.Parameter2)
                .SequenceEqual(Enumerable.Range(0, 7).Select(index => (ushort)(index * 2))) == false)
        {
            throw new InvalidDataException(
                $"Shaktool group mismatch: count={group.Length}, parameters=" +
                $"[{string.Join(',', group.Select(slot => slot.Parameter2))}].");
        }
        return group;
    }

    private static void VerifyInitialState(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies,
        RoomEnemySlot[] group)
    {
        ushort[] expectedProperties = [0x2800, 0x2c00, 0x2c00, 0x2c00, 0x2c00, 0x2c00, 0x2800];
        ushort[] expectedAngles = [0, 0xf800, 0xe800, 0xd000, 0xb000, 0x9800, 0x8800];
        ushort[] expectedVelocities = [0, 0x20, 0x60, 0xc0, 0x140, 0x1a0, 0x1e0];
        ShaktoolPreInstruction[] expectedPreInstructions =
        [
            ShaktoolPreInstruction.IdleHead,
            ShaktoolPreInstruction.OrbitPreviousSegment,
            ShaktoolPreInstruction.OrbitPreviousSegment,
            ShaktoolPreInstruction.OrbitAndOrientCenter,
            ShaktoolPreInstruction.OrbitPreviousSegment,
            ShaktoolPreInstruction.OrbitPreviousSegment,
            ShaktoolPreInstruction.DriveTailAndReverseAtWalls,
        ];
        ushort[] expectedLists = [0xda0e, 0xda72, 0xda72, 0xdad4, 0xda72, 0xda72, 0xda0e];
        ushort[] expectedLayers = [2, 4, 4, 2, 4, 4, 2];

        for (int index = 0; index < group.Length; index++)
        {
            RoomEnemySlot segment = group[index];
            ShaktoolSegmentState? state = enemies.ShaktoolSegments[segment.SlotIndex];
            if (state is null || segment.Properties != expectedProperties[index] ||
                state.OwnerNativeIndex != 0 || state.OrbitAngle != expectedAngles[index] ||
                state.AngularVelocity != expectedVelocities[index] ||
                state.PreInstruction != expectedPreInstructions[index] ||
                segment.CurrentInstruction != expectedLists[index] ||
                segment.Layer != expectedLayers[index])
            {
                throw new InvalidDataException(
                    $"Shaktool segment {index} initialization mismatch: properties=" +
                    $"${segment.Properties:X4}, owner=${state?.OwnerNativeIndex:X4}, angle=" +
                    $"${state?.OrbitAngle:X4}, velocity=${state?.AngularVelocity:X4}, pre=" +
                    $"${(ushort?)state?.PreInstruction:X4}, list=${segment.CurrentInstruction:X4}, " +
                    $"layer={segment.Layer}.");
            }

            if (index == 0)
                continue;
            RoomEnemySlot previous = group[index - 1];
            int angle = expectedAngles[index] >> 8;
            (ushort expectedX, ushort expectedXSub) = AddFixed(
                previous.XPosition,
                previous.XSubposition,
                unchecked((short)ReadWord(bus, 0xaae0bd + angle * 2)) << 8);
            (ushort expectedY, ushort expectedYSub) = AddFixed(
                previous.YPosition,
                previous.YSubposition,
                unchecked((short)ReadWord(bus, 0xaae03d + angle * 2)) << 8);
            if (segment.XPosition != expectedX || segment.XSubposition != expectedXSub ||
                segment.YPosition != expectedY || segment.YSubposition != expectedYSub)
            {
                throw new InvalidDataException(
                    $"Shaktool segment {index} did not orbit the preceding physical slot.");
            }
        }
    }

    private static void VerifyUnusedAttackLists(
        RoomEnemySystem enemies,
        RoomLevelData level,
        SamusState samus,
        RoomEnemySlot[] group)
    {
        enemies.StartUnusedShaktoolAttack(group[6]);
        if (group.Any(segment => enemies.ShaktoolSegments[segment.SlotIndex]?.PreInstruction !=
                ShaktoolPreInstruction.IdleAfterAttack))
        {
            throw new InvalidDataException("Unused Shaktool attack did not pause all seven segments.");
        }

        var attackPositions = new HashSet<(ushort X, ushort Y)>();
        for (int frame = 0; frame < 650; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: level);
            foreach (RoomEnemySlot segment in group)
                attackPositions.Add((segment.XPosition, segment.YPosition));
        }
        if (attackPositions.Count < 12 ||
            group[6].CurrentInstruction is not (>= 0xda0e and <= 0xda22) ||
            enemies.ShaktoolSegments[group[6].SlotIndex]?.PreInstruction !=
                ShaktoolPreInstruction.DriveTailAndReverseAtWalls)
        {
            throw new InvalidDataException(
                $"Unused Shaktool attack lists did not move/restore the group: " +
                $"positions={attackPositions.Count}, tail list=${group[6].CurrentInstruction:X4}.");
        }
    }

    private static void VerifyContactDamage(
        RoomEnemySystem enemies,
        SamusState samus,
        RoomEnemySlot endpoint)
    {
        samus.XPosition = endpoint.XPosition;
        samus.YPosition = endpoint.YPosition;
        samus.InvincibilityTimer = 0;
        ushort healthBefore = samus.Health;
        bool hit = enemies.ResolveOrdinarySamusContact(samus, controllerInput: 0);
        if (!hit || healthBefore - samus.Health != 120 || samus.KnockbackTimer == 0)
        {
            throw new InvalidDataException(
                $"Shaktool contact mismatch: hit={hit}, health={healthBefore}->{samus.Health}, " +
                $"knockback={samus.KnockbackTimer}.");
        }
    }

    private static void VerifyFatalShot(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies,
        SamusState samus,
        RoomEnemySlot[] group)
    {
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        SamusProjectileSlot shot = shots.Slots[0];
        shot.ClearFields();
        shot.Type = 0;
        shot.Damage = 1000;
        shot.Direction = (ushort)SamusProjectileDirection.Right;
        shot.XPosition = group[0].XPosition;
        shot.YPosition = group[0].YPosition;
        shot.XRadius = group[0].XRadius;
        shot.YRadius = group[0].YRadius;
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 1;

        int hits = enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus);
        if (hits != 1 || group.Any(segment => segment.Properties != 0x0200) ||
            enemies.EnemiesKilled == 0)
        {
            throw new InvalidDataException(
                $"Shaktool fatal shot mismatch: hits={hits}, killed={enemies.EnemiesKilled}, " +
                $"properties=[{string.Join(',', group.Select(segment => $"${segment.Properties:X4}"))}].");
        }
    }

    /// <summary>
    /// Sends a real exploding family-<c>$0500</c> actor through Shaktool's private
    /// <c>$AA:DF34</c> callback. Its common death tail must delete all seven linked physical
    /// records, exactly like the independently verified beam path above.
    /// </summary>
    private static void VerifyFatalNormalBomb(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySystem enemies = CreateEncounter(bus, room, assets, out SamusState samus);
        RoomEnemySlot[] group = GetGroup(enemies);
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        var bombs = new SamusBombProjectileSystem();
        var shots = new SamusProjectileSystem();
        SamusBombProjectileSlot bomb =
            EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                bombs,
                group[0].XPosition,
                group[0].YPosition,
                damage: 1000);
        ushort vulnerabilityPointer = group[0].Definition.VulnerabilityPointer != 0
            ? group[0].Definition.VulnerabilityPointer
            : (ushort)0xec1c;
        byte vulnerability = bus.ReadByte(
            0xb40000 | unchecked((ushort)(vulnerabilityPointer + 14)));
        int expectedDamage = (bomb.Damage >> 1) * (vulnerability & 0x7f);
        if (expectedDamage < group[0].Health)
        {
            throw new InvalidDataException(
                $"Shaktool's retail normal-bomb vulnerability ${vulnerability:X2} " +
                "cannot prove the linked fatal callback.");
        }

        int hits = enemies.ResolveOrdinaryBombHits(bombs, shots, samus);
        if (hits != 1 || (bomb.Direction & 0x0010) == 0 ||
            group.Any(segment => segment.Properties != 0x0200) ||
            enemies.EnemiesKilled == 0)
        {
            throw new InvalidDataException(
                $"Shaktool fatal normal bomb mismatch: hits={hits}, " +
                $"direction=${bomb.Direction:X4}, killed={enemies.EnemiesKilled}, " +
                $"properties=[{string.Join(',', group.Select(segment => $"${segment.Properties:X4}"))}].");
        }
    }

    private static (ushort Position, ushort Subposition) AddFixed(
        ushort position,
        ushort subposition,
        int displacement)
    {
        int value = unchecked((position << 16) | subposition);
        value = unchecked(value + displacement);
        return (unchecked((ushort)(value >> 16)), unchecked((ushort)value));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
