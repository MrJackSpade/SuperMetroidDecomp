using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the rumble definitions against Crocomire's native words and compares the production death-rumble sequence while migrated source reads are blocked.</summary>
    /// <param name="rom">Address space containing the cartridge rumble words and palette source data.</param>
    private static void VerifyCrocomireRumbleDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa498ca;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        var expectedWords = new ushort[32];
        var assigned = new bool[32];
        // Walk the native cursor chain: each target's next-target cursor names the following
        // word, skipping the two timing words that only negative targets carry.
        ushort tableOffset = 0;
        foreach (CrocomireRumbleDefinition definition in CrocomireRumbleDefinitions.All)
        {
            AssertEqual(definition, CrocomireRumbleDefinitions.AtOffset(tableOffset),
                $"Crocomire rumble cursor ${tableOffset:X4} resolves its enumerated target");
            int targetIndex = tableOffset >> 1;
            expectedWords[targetIndex] = unchecked((ushort)definition.TargetYOffset);
            assigned[targetIndex] = true;
            bool hasTiming = !definition.IsTerminator && definition.NextTargetOffset == tableOffset + 6;
            tableOffset = definition.IsTerminator
                ? (ushort)(tableOffset + 2)
                : definition.NextTargetOffset;
            if (!hasTiming)
                continue;

            expectedWords[targetIndex + 1] = definition.Cooldown;
            expectedWords[targetIndex + 2] = definition.Delta;
            assigned[targetIndex + 1] = true;
            assigned[targetIndex + 2] = true;
        }

        for (int index = 0; index < expectedWords.Length; index++)
        {
            AssertTrue(assigned[index], $"Crocomire rumble source word {index} ownership");
            AssertEqual(ReadWord(rom, sourceAddress + index * 2), expectedWords[index],
                $"Crocomire rumble source word {index}");
        }

        AssertThrows<InvalidDataException>(
            () => CrocomireRumbleDefinitions.AtOffset(1),
            "Crocomire odd rumble offset");
        AssertThrows<InvalidDataException>(
            () => CrocomireRumbleDefinitions.AtOffset(8),
            "Crocomire timing word cannot become a target");
        AssertThrows<InvalidDataException>(
            () => CrocomireRumbleDefinitions.AtOffset(0x40),
            "Crocomire rumble offset outside table");

        var guarded = new CrocomireRumbleReadGuard(rom);
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        CrocomireColorCatalog colors = CrocomireColorCatalog.Load(
            new MemoryStream(CrocomireColorExtractor.Extract(rom), writable: false));
        var enemies = new RoomEnemySystem
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
                new Dictionary<ushort, RoomCharacterAtlas>(),
                new Dictionary<ushort, EnemyPaletteSheet>(), crocomireColors: colors),
        };
        var cgram = new SnesCgram();
        var death = new CrocomireDeathState
        {
            RumbleYOffset = 0,
            RumbleCooldown = 10,
            RumbleDelta = 1,
        };
        RoomEnemySlot body = enemies.Slots[0];
        var state = new CrocomireEnemyState(body)
        {
            DeathSequenceIndex = CrocomireDeathPhases.RumbleHiddenWall,
            StepCounter = 4,
        };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guarded);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
        typeof(RoomEnemySystem).GetField("_crocomireDeath", flags)!.SetValue(enemies, death);
        MethodInfo runRumble = typeof(RoomEnemySystem).GetMethod(
            "RunCrocomireWallRumble",
            flags)!;

        ushort referenceIndex = state.StepCounter;
        ushort referenceY = death.RumbleYOffset;
        ushort referenceCooldown = death.RumbleCooldown;
        ushort referenceDelta = death.RumbleDelta;
        ushort referenceDeathIndex = state.DeathSequenceIndex;
        int frames = 0;
        while (referenceIndex != 0x0080 && frames < 2048)
        {
            StepCrocomireRumbleReference(
                rom,
                ref referenceIndex,
                ref referenceY,
                ref referenceCooldown,
                ref referenceDelta,
                ref referenceDeathIndex);
            runRumble.Invoke(enemies, [state]);
            frames++;

            AssertEqual(referenceIndex, state.StepCounter,
                $"Crocomire rumble frame {frames} target index");
            AssertEqual(referenceY, death.RumbleYOffset,
                $"Crocomire rumble frame {frames} Y offset");
            AssertEqual(referenceCooldown, death.RumbleCooldown,
                $"Crocomire rumble frame {frames} cooldown");
            AssertEqual(referenceDelta, death.RumbleDelta,
                $"Crocomire rumble frame {frames} delta");
            AssertEqual(referenceDeathIndex, state.DeathSequenceIndex,
                $"Crocomire rumble frame {frames} death index");
        }

        AssertTrue(frames < 2048, "Crocomire rumble production sequence terminates");
        AssertEqual((ushort)0x0080, state.StepCounter,
            "Crocomire rumble production terminator handoff");
        AssertEqual((ushort)0x8080, death.RumbleYOffset,
            "Crocomire rumble production terminator Y marker");
        for (int color = 0; color < CrocomirePaletteRomData.WallSpikesCount; color++)
            AssertEqual(colors.ResolveWallSpikes(color),
                cgram.Colors[CrocomirePaletteRomData.WallSpikesDestination + color],
                $"Crocomire rumble termination applies installed wall-spike color {color}");

        Console.WriteLine(
            $"Crocomire rumble definitions: all 32 native words and {frames} exact production frames pass with the source stream forbidden.");
    }

    /// <summary>Advances one reference step by reading the native rumble table and applying its target, timing, and terminator rules.</summary>
    /// <param name="rom">Address space supplying the native rumble words.</param>
    /// <param name="index">Current byte offset into the rumble table; updated to the next target or terminal marker.</param>
    /// <param name="yOffset">Current rumble vertical offset, updated toward the selected target.</param>
    /// <param name="cooldown">Remaining wait before a negative target advances; updated when timing words are consumed.</param>
    /// <param name="delta">Per-step vertical movement applied while approaching a target.</param>
    /// <param name="deathIndex">Death-sequence index advanced when the table terminator is reached.</param>
    private static void StepCrocomireRumbleReference(
        ISnesAddressSpace rom,
        ref ushort index,
        ref ushort yOffset,
        ref ushort cooldown,
        ref ushort delta,
        ref ushort deathIndex)
    {
        const int sourceAddress = 0xa498ca;
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        ushort target = ReadWord(rom, sourceAddress + index);
        if (target == 0x8080)
        {
            yOffset = 0x8080;
            index = 0x0080;
            deathIndex += 2;
            return;
        }

        if (yOffset == target)
        {
            if (unchecked((short)target) < 0)
            {
                if (cooldown != 0)
                {
                    cooldown--;
                    index -= 2;
                    return;
                }

                index += 2;
                cooldown = ReadWord(rom, sourceAddress + index);
                index += 2;
                delta = ReadWord(rom, sourceAddress + index);
            }

            index += 2;
            return;
        }

        yOffset = unchecked((short)(yOffset - target)) >= 0
            ? unchecked((ushort)(yOffset - delta))
            : unchecked((ushort)(yOffset + delta));
    }

    /// <summary>Address-space wrapper that fails if production rumble or migrated Crocomire palette code reads cartridge data.</summary>
    /// <param name="source">Underlying address space used for reads outside the forbidden ranges and for all writes.</param>
    private sealed class CrocomireRumbleReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge reads through the guard so forbidden source ranges are rejected consistently.</summary>
        /// <param name="address">Full SNES address requested by the caller.</param>
        /// <returns>The wrapped byte when the address is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects accesses to native rumble and migrated palette data, forwarding other reads to the wrapped address space.</summary>
        /// <param name="address">Full SNES address to read.</param>
        /// <returns>The wrapped byte when the address is outside all blocked ranges.</returns>
        public byte ReadByte(int address) =>
            address is >= 0xa498ca and < 0xa4990a ||
                address >= CrocomirePaletteRomData.FightBodySource &&
                address < CrocomirePaletteRomData.WallSpikesSource +
                    CrocomirePaletteRomData.WallSpikesCount * sizeof(ushort)
                ? throw new InvalidOperationException(
                    $"Crocomire rumble attempted migrated data read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">Full SNES address to write.</param>
        /// <param name="value">Byte stored at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
