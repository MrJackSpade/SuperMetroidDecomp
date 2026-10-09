using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the Botwoon health-palette thresholds and verifies production phase progression without allowing runtime threshold-table reads.</summary>
    /// <param name="rom">Address space supplying the cartridge's native threshold data and Botwoon assets.</param>
    private static void VerifyBotwoonHealthPaletteDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort ReadWord(ISnesAddressSpace bus, int address) => unchecked((ushort)(
            bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

        for (ushort phase = 0;
             phase < BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset;
             phase += 2)
        {
            ushort threshold = ReadWord(
                rom,
                BotwoonHealthPaletteDefinitions.NativeThresholdAddress + phase);
            for (int health = 0; health <= ushort.MaxValue; health++)
            {
                bool expected = unchecked((short)((ushort)health - threshold)) < 0;
                AssertEqual(expected,
                    BotwoonHealthPaletteDefinitions.ShouldAdvance(phase, (ushort)health),
                    $"Botwoon phase ${phase:X2} health {health} threshold decision");
            }
        }

        foreach (ushort invalid in new ushort[] { 1, 3, 15, 16, 18, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(
                () => BotwoonHealthPaletteDefinitions.ShouldAdvance(invalid, 0),
                $"Botwoon rejects restored palette phase ${invalid:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new BotwoonHealthThresholdReadGuard(rom);
        using var colors = new MemoryStream(BotwoonColorExtractor.Extract(rom), writable: false);
        var enemies = new RoomEnemySystem
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
                new Dictionary<ushort, RoomCharacterAtlas>(),
                new Dictionary<ushort, EnemyPaletteSheet>(),
                botwoonColors: BotwoonColorCatalog.Load(colors)),
        };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        var update = typeof(RoomEnemySystem).GetMethod(
            "UpdateBotwoonHealthPalette",
            flags)!.CreateDelegate<Action<RoomEnemySlot, BotwoonEnemyState>>(enemies);
        RoomEnemySlot head = enemies.Slots[0];
        var state = new BotwoonEnemyState(head)
        {
            // Sprite palette seven starts at CGRAM color 240 / byte offset $01E0.
            PaletteDestinationByteOffset = 0x01e0,
        };

        for (ushort phase = 0;
             phase < BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset;
             phase += 2)
        {
            ushort threshold = ReadWord(
                rom,
                BotwoonHealthPaletteDefinitions.NativeThresholdAddress + phase);
            state.PalettePhaseByteOffset = phase;
            head.Health = threshold;
            update(head, state);
            AssertEqual(phase, state.PalettePhaseByteOffset,
                $"production Botwoon phase ${phase:X2} holds at threshold");

            state.PalettePhaseByteOffset = phase;
            head.Health = unchecked((ushort)(threshold - 1));
            update(head, state);
            AssertEqual(unchecked((ushort)(phase + 2)), state.PalettePhaseByteOffset,
                $"production Botwoon phase ${phase:X2} advances below threshold");
        }

        state.PalettePhaseByteOffset =
            BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset;
        update(head, state);
        AssertEqual(BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset,
            state.PalettePhaseByteOffset,
            "production Botwoon completed palette phase remains terminal");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Botwoon palette progression performs no threshold reads");

        foreach (ushort invalid in new ushort[] { 1, 18 })
        {
            state.PalettePhaseByteOffset = invalid;
            AssertThrows<InvalidDataException>(
                () => update(head, state),
                $"production Botwoon rejects restored palette phase ${invalid:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "malformed Botwoon palette phase performs no threshold reads");

        Console.WriteLine(
            "Botwoon health palette definitions: eight native thresholds and all " +
            "524,288 signed health/phase decisions match; real hold, one-band advance, " +
            "terminal, and malformed paths run with the threshold table forbidden.");
    }

    /// <summary>Address-space wrapper that rejects reads of Botwoon's native health-threshold table while forwarding unrelated access.</summary>
    /// <param name="source">Underlying address space used for allowed reads and all writes.</param>
    private sealed class BotwoonHealthThresholdReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the number of attempted reads blocked because they targeted the native threshold table.</summary>
        public int ForbiddenReadAttempts { get; private set; }

        /// <summary>Reads a cartridge byte through the same threshold-table guard as ordinary address-space reads.</summary>
        /// <param name="address">Full SNES address requested by the caller.</param>
        /// <returns>The byte at <paramref name="address"/> when the access is allowed.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects threshold-table reads and forwards other byte reads to the wrapped address space.</summary>
        /// <param name="address">Full SNES address to read.</param>
        /// <returns>The byte supplied by the wrapped address space when the address is outside the forbidden table.</returns>
        public byte ReadByte(int address)
        {
            if (address >= BotwoonHealthPaletteDefinitions.NativeThresholdAddress &&
                address < BotwoonHealthPaletteDefinitions.NativeThresholdAddress + 16)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Botwoon palette progression attempted threshold read ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">Full SNES address to write.</param>
        /// <param name="value">Byte stored at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
