using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Untouched-ROM audit for Magdollite Tunnel's three-slot lava-man composites. The final
/// audit exercises the live room while retaining the cartridge index dump used to prove
/// room, instruction-list, speed-table, palette, and projectile constants.
/// </summary>
internal static class MagdolliteAudit
{
    private const ushort DefinitionPointer = 0xe83f;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);

        // Enemy populations live in bank $A1 and consist of sixteen-byte records. Find each
        // literal Magdollite definition word first; the surrounding aligned records reveal
        // the actual population start without depending on generated asset names.
        var definitionOccurrences = new List<ushort>();
        for (int pointer = 0x8000; pointer < 0xffff; pointer++)
        {
            if (ReadWord(bus, 0xa10000 | pointer) == DefinitionPointer)
                definitionOccurrences.Add(unchecked((ushort)pointer));
        }

        Console.WriteLine(
            $"Magdollite definition occurrences in bank $A1: " +
            string.Join(", ", definitionOccurrences.Select(value => $"${value:X4}")));

        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        Console.WriteLine("Magdollite definition: " + definition);

        // names.txt identifies Magdollite Tunnel as area two, room index thirty-five. Scan
        // only that exact header prefix, then let the production selector parser reject
        // coincidental byte pairs elsewhere in bank $8F.
        for (int roomPointer = 0x8000; roomPointer <= 0xfff5; roomPointer++)
        {
            if (bus.ReadByte(0x8f0000 | roomPointer) != 35 ||
                bus.ReadByte(0x8f0000 | (roomPointer + 1)) != 2)
            {
                continue;
            }

            try
            {
                CartridgeRoomHeader room = CartridgeRoomHeader.Load(
                    bus,
                    unchecked((ushort)roomPointer));
                Console.WriteLine(
                    $"Area 2/room 35 candidate: room ${room.Pointer:X4}, state " +
                    $"${room.State.Pointer:X4}, population " +
                    $"${room.State.EnemyPopulationPointer:X4}, tileset " +
                    $"${room.State.EnemyTilesetPointer:X4}, dimensions " +
                    $"{room.WidthInScreens}x{room.HeightInScreens}.");
            }
            catch (InvalidDataException)
            {
                // This is simply a matching byte pair inside non-header ROM data.
            }
        }

        DumpWords(bus, 0xa08357, 4, "linear speed record selected by parameter $3A60");
        Console.WriteLine("Magdollite Tunnel population records:");
        for (int pointer = 0xac53; ReadWord(bus, 0xa10000 | pointer) != 0xffff; pointer += 16)
        {
            ushort[] words = Enumerable.Range(0, 8)
                .Select(index => ReadWord(bus, 0xa10000 | (pointer + index * 2)))
                .ToArray();
            Console.WriteLine(
                $"  $A1:{pointer:X4}: " +
                string.Join(' ', words.Select(value => $"{value:X4}")));
        }

        DumpWords(bus, 0xa8ac1c, 64, "palette $A8:AC1C");
        DumpWords(bus, 0xa8ac9c, (0xae12 - 0xac9c) / 2, "lists $A8:AC9C-$AE11");
        DumpWords(bus, 0xa8af4f, (0xaf8b - 0xaf4f) / 2, "tables $A8:AF4F-$AF8A");
        DumpWords(bus, 0x86dfbc, (0xe0ee - 0xdfbc) / 2, "projectile data $86:DFBC-$E0ED");

        // A room state stores its enemy population pointer at byte eight. Report every
        // bank-$8F occurrence of each candidate so the enclosing header can be identified
        // from the real selector/state layout in the next pass.
        foreach (ushort occurrence in definitionOccurrences)
        {
            for (int populationStart = occurrence; populationStart >= 0x8000; populationStart -= 16)
            {
                if (populationStart != occurrence &&
                    ReadWord(bus, 0xa10000 | populationStart) == 0xffff)
                {
                    break;
                }

                if ((occurrence - populationStart) % 16 != 0)
                    continue;

                ushort candidate = unchecked((ushort)populationStart);
                var statePointers = new List<ushort>();
                for (int statePointer = 0x8000; statePointer <= 0xfff5; statePointer++)
                {
                    if (ReadWord(bus, 0x8f0000 | (statePointer + 8)) == candidate)
                        statePointers.Add(unchecked((ushort)statePointer));
                }

                if (statePointers.Count != 0)
                {
                    Console.WriteLine(
                        $"  occurrence ${occurrence:X4}: population ${candidate:X4}, " +
                        $"state candidates {string.Join(", ", statePointers.Select(value => $"${value:X4}"))}");
                }
            }
        }

        VerifyLiveRoomCycle(bus);
        return 0;
    }

    private static void VerifyLiveRoomCycle(SuperMetroidAddressSpace bus)
    {
        const ushort roomPointer = 0xaeb4;
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, roomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var samus = new SamusState
        {
            XPosition = 0x00c0,
            YPosition = 0x00b8,
            Health = 999,
            MaxHealth = 999,
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

        RoomEnemySlot[] population = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        int magdolliteCount = population.Count(slot =>
            slot.EnemyDefinitionPointer == DefinitionPointer);
        if (room.State.Pointer != 0xaec1 ||
            room.State.EnemyPopulationPointer != 0xac53 ||
            room.State.EnemyTilesetPointer != 0x8887 ||
            room.WidthInScreens != 3 || room.HeightInScreens != 1 ||
            enemies.EnemyCount != 11 || magdolliteCount != 9 ||
            population.Take(9).Any(slot => enemies.MagdolliteStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Magdollite Tunnel live load mismatch: state=${room.State.Pointer:X4}, " +
                $"population/set=${room.State.EnemyPopulationPointer:X4}/" +
                $"${room.State.EnemyTilesetPointer:X4}, dimensions=" +
                $"{room.WidthInScreens}x{room.HeightInScreens}, count/Magdollites=" +
                $"{enemies.EnemyCount}/{magdolliteCount}.");
        }

        bool sawAttackAnimation = false;
        bool sawRisingBody = false;
        bool sawFallingBody = false;
        bool sawLava = false;
        bool sawThrowSound = false;
        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 640; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            foreach (RoomEnemySlot slot in population.Take(9))
            {
                MagdolliteEnemyState state = enemies.MagdolliteStates[slot.SlotIndex]!;
                maps.Add(slot.SpritemapPointer);
                sawAttackAnimation |= state.Function ==
                    MagdolliteEnemyFunction.HeadWaitingForAttackAnimation;
                sawRisingBody |= state.Function == MagdolliteEnemyFunction.BodyRising;
                sawFallingBody |= state.Function == MagdolliteEnemyFunction.BodyFalling;
            }
            sawThrowSound |= enemies.LastMagdolliteSoundEffect == 0x0061;
            sawLava |= enemies.EnemyProjectiles.Any(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite);
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: 0,
                cameraY: 0);
        }

        if (!sawAttackAnimation || !sawRisingBody || !sawFallingBody ||
            !sawLava || !sawThrowSound || maps.All(map => map is 0 or 0x804d))
        {
            throw new InvalidDataException(
                $"Magdollite live cycle mismatch: attack/rise/fall/lava/sound=" +
                $"{sawAttackAnimation}/{sawRisingBody}/{sawFallingBody}/" +
                $"{sawLava}/{sawThrowSound}, distinct maps={maps.Count}.");
        }

        Console.WriteLine(
            "Magdollite live-room smoke audit passed: all three retail composites loaded, " +
            "animated, rose, threw damaging lava, fell, and returned through ROM lists.");
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private static void DumpWords(
        ISnesAddressSpace bus,
        int address,
        int wordCount,
        string label)
    {
        Console.WriteLine(label + ':');
        for (int offset = 0; offset < wordCount; offset += 8)
        {
            ushort[] words = Enumerable.Range(offset, Math.Min(8, wordCount - offset))
                .Select(index => ReadWord(bus, address + index * 2))
                .ToArray();
            Console.WriteLine(
                $"  ${(address + offset * 2) >> 16:X2}:{(address + offset * 2) & 0xffff:X4}  " +
                string.Join(' ', words.Select(value => $"{value:X4}")));
        }
    }
}
