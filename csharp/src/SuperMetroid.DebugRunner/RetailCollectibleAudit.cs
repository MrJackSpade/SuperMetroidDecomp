using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-wide proof that every named room PLM population containing a permanent item
/// enters the production loader and maps to one unique physical save bit.
/// </summary>
internal static class RetailCollectibleAudit
{
    private const int ExpectedPhysicalItemCount = 100;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        string symbolPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "upstream-sm",
            "assets",
            "names.txt");
        if (!File.Exists(symbolPath))
        {
            throw new FileNotFoundException(
                "Retail collectible audit requires upstream-sm/assets/names.txt.",
                symbolPath);
        }

        ushort[] populations = File.ReadLines(symbolPath)
            .Where(line => line.StartsWith("0x8f", StringComparison.OrdinalIgnoreCase) &&
                line.Contains(" kRoomPlms_", StringComparison.Ordinal))
            .Select(ParseBank8fPointer)
            .Distinct()
            .Order()
            .ToArray();
        var records = new List<RetailCollectibleRecord>();

        foreach (ushort population in populations)
        {
            int cursor = population;
            bool terminated = false;
            for (int recordIndex = 0; recordIndex < 256; recordIndex++)
            {
                ushort header = ReadWord(bus, 0x8f0000 | cursor);
                if (header == 0)
                {
                    terminated = true;
                    break;
                }

                byte blockX = bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 2)));
                byte blockY = bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 3)));
                ushort argument = ReadWord(
                    bus,
                    0x8f0000 | unchecked((ushort)(cursor + 4)));
                if (RoomPlmSystem.TryIdentifyPermanentCollectible(
                    header,
                    out InWorldCollectibleKind kind,
                    out CollectiblePresentation presentation))
                {
                    if ((argument & 0x8000) != 0 || argument >= Bank80SystemState.ItemBitByteCount * 8)
                    {
                        throw new InvalidDataException(
                            $"Item {kind}/{presentation} in $8F:{population:X4} has invalid " +
                            $"physical bit argument ${argument:X4}.");
                    }
                    records.Add(new RetailCollectibleRecord(
                        population,
                        header,
                        blockX,
                        blockY,
                        argument,
                        kind,
                        presentation));
                }
                cursor = unchecked((ushort)(cursor + 6));
            }
            if (!terminated)
            {
                throw new InvalidDataException(
                    $"Named room-PLM population $8F:{population:X4} has no zero terminator.");
            }
        }

        if (records.Count != ExpectedPhysicalItemCount)
        {
            throw new InvalidDataException(
                $"Retail item inventory contains {records.Count} physical records, expected " +
                $"{ExpectedPhysicalItemCount}.");
        }
        int uniqueArguments = records.Select(record => record.Argument).Distinct().Count();
        if (uniqueArguments != records.Count)
        {
            string duplicates = string.Join(", ", records
                .GroupBy(record => record.Argument)
                .Where(group => group.Count() > 1)
                .Select(group => $"${group.Key:X4} x{group.Count()}"));
            throw new InvalidDataException(
                $"Retail physical-item arguments are not unique: {duplicates}.");
        }

        // Decode every permanent-item message through the same bank-$85 owner used by
        // gameplay. This catches config-table, next-pointer sizing, small/large draw,
        // border, and configured-button errors against the actual cartridge rather than
        // relying only on the synthetic lifecycle regression.
        byte[] permanentItemMessageIds = Enumerable.Range(1, 19)
            .Select(index => unchecked((byte)index))
            .Concat([(byte)25, (byte)26])
            .ToArray();
        foreach (byte messageId in permanentItemMessageIds)
        {
            var message = new GameplayMessageBoxState();
            message.Begin(bus, messageId);
            if (message.TilemapRowCount is not (3 or 6))
            {
                throw new InvalidDataException(
                    $"Retail permanent-item message {messageId} built " +
                    $"{message.TilemapRowCount} rows instead of three or six.");
            }
        }

        RetailEnemyDropAuditResult enemyDrops = AuditRetailEnemyDrops(bus, symbolPath);

        foreach (InWorldCollectibleKind kind in Enum.GetValues<InWorldCollectibleKind>())
        {
            RetailCollectibleRecord[] kindRecords = records
                .Where(record => record.Kind == kind)
                .ToArray();
            Console.WriteLine(
                $"{kind,-18} total={kindRecords.Length,2} " +
                string.Join(' ', Enum.GetValues<CollectiblePresentation>().Select(
                    presentation =>
                        $"{presentation}={kindRecords.Count(record => record.Presentation == presentation),2}")));
        }
        Console.WriteLine(
            $"Retail collectible inventory: {records.Count} physical items, " +
            $"{uniqueArguments} unique SRAM bits, {records.Select(record => record.Population).Distinct().Count()} " +
            $"item-bearing populations, {permanentItemMessageIds.Length} item messages; " +
            "every cartridge item header and persistence bit inventoried. " +
            "Whole-population execution is covered by the retail PLM audit.");
        Console.WriteLine(
            $"Retail enemy drops: {enemyDrops.PopulationCount} named populations, " +
            $"{enemyDrops.EnemyHeaderCount} enemy headers, {enemyDrops.DropTableCount} " +
            $"six-byte chance tables, {enemyDrops.SelectionCount} deterministic RNG selections, " +
            $"{enemyDrops.DeathVariantCount} stepped death variants; " +
            "pickup/death definitions and list tables agree.");
        return 0;
    }

    /// <summary>
    /// Audits the other in-world collectible family: temporary refills emitted by enemies.
    /// Population/header discovery comes from the retail symbol map, while every chance byte,
    /// projectile definition, list pointer, and random selection is read from the supplied ROM.
    /// </summary>
    private static RetailEnemyDropAuditResult AuditRetailEnemyDrops(
        SuperMetroidAddressSpace bus,
        string symbolPath)
    {
        ushort[] populations = File.ReadLines(symbolPath)
            .Where(line => line.StartsWith("0xa1", StringComparison.OrdinalIgnoreCase) &&
                line.Contains(" kEnemyPopulation_", StringComparison.Ordinal))
            .Select(ParseBankA1Pointer)
            .Distinct()
            .Order()
            .ToArray();
        var enemyHeaders = new HashSet<ushort>();
        ushort? emptyPopulation = null;

        foreach (ushort population in populations)
        {
            int cursor = population;
            bool terminated = false;
            // Inspect one position beyond the 32 physical slots so a completely full legal
            // population can prove its terminator without accepting a thirty-third actor.
            for (int recordIndex = 0; recordIndex <= RoomEnemySystem.MaximumEnemyCount;
                 recordIndex++)
            {
                ushort header = ReadWord(bus, 0xa10000 | cursor);
                if (header == 0xffff)
                {
                    terminated = true;
                    if (recordIndex == 0)
                        emptyPopulation ??= population;
                    break;
                }
                if (recordIndex == RoomEnemySystem.MaximumEnemyCount)
                {
                    throw new InvalidDataException(
                        $"Named enemy population $A1:{population:X4} contains more than " +
                        $"{RoomEnemySystem.MaximumEnemyCount} actors.");
                }
                if (header == 0)
                {
                    throw new InvalidDataException(
                        $"Named enemy population $A1:{population:X4} contains a null " +
                        $"header in record {recordIndex}.");
                }
                enemyHeaders.Add(header);
                cursor = unchecked((ushort)(cursor + 16));
            }
            if (!terminated)
            {
                throw new InvalidDataException(
                    $"Named enemy population $A1:{population:X4} exceeds the native " +
                    $"{RoomEnemySystem.MaximumEnemyCount}-slot pool without a terminator.");
            }
        }

        if (emptyPopulation is null)
        {
            throw new InvalidDataException(
                "Retail symbol map contains no named empty enemy population for drop audit context.");
        }

        var dropTables = new HashSet<ushort>();
        foreach (ushort header in enemyHeaders)
        {
            ushort pointer = RoomEnemySystem.ReadDefinition(bus, header)
                .ItemDropChancesPointer;
            if (pointer != 0)
                dropTables.Add(pointer);
        }

        // Every retail six-byte probability record consumes the complete nonzero random-byte
        // domain. A sum other than 255 either identifies a bad pointer or silently changes the
        // native probability budget, so fail with the exact bank-$B4 address.
        foreach (ushort pointer in dropTables)
        {
            int sum = 0;
            for (int index = 0; index < 6; index++)
                sum += bus.ReadByte(0xb40000 | unchecked((ushort)(pointer + index)));
            if (sum != 0xff)
            {
                throw new InvalidDataException(
                    $"Enemy drop table $B4:{pointer:X4} sums to {sum}, expected 255.");
            }
        }

        // Lock the shared bank-$86 structures to the translated identities. These are data
        // assertions rather than guessed visual aliases: EF04 is indexed by eproj_E, and
        // EFD5 is indexed by the clamped generic death-animation variant.
        if (ReadWord(bus, 0x86f337 + 6) != 0x1010 ||
            ReadWord(bus, 0x86f337 + 8) != 0x3000)
        {
            throw new InvalidDataException(
                "Retail pickup projectile $F337 does not have radii $10/$10 and properties $3000.");
        }
        (EnemyPickupKind Kind, ushort List)[] expectedPickupLists =
        [
            (EnemyPickupKind.SmallEnergy, 0xed8d),
            (EnemyPickupKind.BigEnergy, 0xeda3),
            (EnemyPickupKind.PowerBomb, 0xedeb),
            (EnemyPickupKind.Missile, 0xedb9),
            (EnemyPickupKind.SuperMissile, 0xeddd),
        ];
        foreach ((EnemyPickupKind kind, ushort expectedList) in expectedPickupLists)
        {
            ushort actualList = ReadWord(bus, 0x86ef04 + (ushort)kind * 2);
            if (actualList != expectedList)
            {
                throw new InvalidDataException(
                    $"Retail {kind} pickup list is $86:{actualList:X4}, expected " +
                    $"$86:{expectedList:X4}.");
            }
        }
        for (int variant = 0; variant < 5; variant++)
        {
            if (ReadWord(bus, 0x86efd5 + variant * 2) == 0)
            {
                throw new InvalidDataException(
                    $"Retail generic death-animation variant {variant} has a null list.");
            }
        }

        int steppedDeathVariants = StepEveryRetailEnemyDeathVariant(
            bus,
            emptyPopulation.Value,
            enemyHeaders.First(header =>
            {
                ushort pointer = RoomEnemySystem.ReadDefinition(bus, header)
                    .ItemDropChancesPointer;
                return pointer != 0 && bus.ReadByte(0xb40000 | pointer) != 0;
            }));

        // Exercise $86:F106 against every real table and every possible nonzero RNG byte.
        // Missing-all and full-resource profiles cover each eligibility gate; critical energy
        // independently covers the stateful <30 bias. No projectile allocation is involved,
        // so the loop audits probability semantics without exhausting the eighteen-slot pool.
        ushort random = 1;
        var missingResources = new SamusState
        {
            Health = 50,
            MaxHealth = 99,
            MaxReserveEnergy = 100,
            MaxMissiles = 5,
            MaxSuperMissiles = 5,
            MaxPowerBombs = 5,
        };
        var fullResources = new SamusState
        {
            Health = 99,
            MaxHealth = 99,
            ReserveEnergy = 100,
            MaxReserveEnergy = 100,
            Missiles = 5,
            MaxMissiles = 5,
            SuperMissiles = 5,
            MaxSuperMissiles = 5,
            PowerBombs = 5,
            MaxPowerBombs = 5,
        };
        var criticalEnergy = new SamusState
        {
            Health = 1,
            MaxHealth = 99,
            MaxReserveEnergy = 100,
            MaxMissiles = 5,
            MaxSuperMissiles = 5,
            MaxPowerBombs = 5,
        };
        SamusState[] profiles = [missingResources, fullResources, criticalEnergy];
        int selectionCount = 0;
        foreach (SamusState profile in profiles)
        {
            var enemies = new RoomEnemySystem();
            enemies.Load(
                bus,
                emptyPopulation.Value,
                tilesetPointer: 0,
                new SnesVram(),
                new SnesCgram(),
                nextRandom: () => random,
                samus: profile);
            foreach (ushort pointer in dropTables)
            {
                var candidate = new RoomEnemyProjectileSlot(slotIndex: 1)
                {
                    ItemDropChancesPointerOverride = pointer,
                };
                for (random = 1; random <= 0xff; random++)
                {
                    EnemyPickupKind selected = enemies.SelectRandomEnemyDrop(candidate);
                    if (!Enum.IsDefined(selected))
                    {
                        throw new InvalidDataException(
                            $"Drop table $B4:{pointer:X4} returned invalid pickup " +
                            $"${(ushort)selected:X4} for RNG {random}.");
                    }
                    selectionCount++;
                }
            }
        }

        return new RetailEnemyDropAuditResult(
            populations.Length,
            enemyHeaders.Count,
            dropTables.Count,
            selectionCount,
            steppedDeathVariants);
    }

    /// <summary>
    /// Runs each EFD5 list through the production bank-$86 interpreter until EEAF installs
    /// either a live pickup or the shared no-drop tail. This is the end-to-end guard against
    /// untranslated explosion decorators hiding between a correct death spawn and its item.
    /// </summary>
    private static int StepEveryRetailEnemyDeathVariant(
        SuperMetroidAddressSpace bus,
        ushort emptyPopulation,
        ushort enemyHeader)
    {
        var level = new RoomLevelData(
            1,
            1,
            new ushort[1],
            new byte[1],
            new ushort[1],
            new byte[8]);

        for (ushort variant = 0; variant < 5; variant++)
        {
            var samus = new SamusState
            {
                XPosition = 1000,
                YPosition = 1000,
                Health = 50,
                MaxHealth = 99,
                MaxReserveEnergy = 100,
                MaxMissiles = 5,
                MaxSuperMissiles = 5,
                MaxPowerBombs = 5,
            };
            var enemies = new RoomEnemySystem();
            enemies.Load(
                bus,
                emptyPopulation,
                tilesetPointer: 0,
                new SnesVram(),
                new SnesCgram(),
                nextRandom: () => 1,
                level: level,
                samus: samus);
            RoomEnemySlot dying = enemies.Slots[0];
            dying.EnemyDefinitionPointer = enemyHeader;
            dying.XPosition = 100;
            dying.YPosition = 100;
            enemies.StartGenericEnemyDeath(dying, variant);

            RoomEnemyProjectileSlot death = enemies.EnemyProjectiles[17];
            bool converted = false;
            for (int frame = 0; frame < 512; frame++)
            {
                enemies.StepEnemyProjectiles(level, samus);
                if (death.PreInstruction == 0xefe0 && death.Variable1 == 400)
                {
                    converted = true;
                    break;
                }
                if (!death.IsActive)
                    break;
            }
            if (!converted)
            {
                throw new InvalidDataException(
                    $"Retail death-animation variant {variant} did not reach EEAF within " +
                    "512 enemy-projectile frames.");
            }
        }
        return 5;
    }

    private static ushort ParseBank8fPointer(string line)
    {
        int separator = line.IndexOf(' ');
        if (separator < 0 ||
            !int.TryParse(
                line.AsSpan(2, separator - 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out int address) ||
            (address & 0xff0000) != 0x8f0000)
        {
            throw new InvalidDataException($"Malformed bank-$8F symbol-map line: {line}");
        }
        return unchecked((ushort)address);
    }

    private static ushort ParseBankA1Pointer(string line)
    {
        int separator = line.IndexOf(' ');
        if (separator < 0 ||
            !int.TryParse(
                line.AsSpan(2, separator - 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out int address) ||
            (address & 0xff0000) != 0xa10000)
        {
            throw new InvalidDataException($"Malformed bank-$A1 symbol-map line: {line}");
        }
        return unchecked((ushort)address);
    }

    private static ushort ReadWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct RetailCollectibleRecord(
        ushort Population,
        ushort Header,
        byte BlockX,
        byte BlockY,
        ushort Argument,
        InWorldCollectibleKind Kind,
        CollectiblePresentation Presentation);

    private readonly record struct RetailEnemyDropAuditResult(
        int PopulationCount,
        int EnemyHeaderCount,
        int DropTableCount,
        int SelectionCount,
        int DeathVariantCount);
}
