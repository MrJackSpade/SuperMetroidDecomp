using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares every authored Crystal Flash body-palette duration with ROM and verifies the full production palette cycle uses the compiled timings.</summary>
    /// <param name="rom">Address space containing the native duration table and other data needed to begin the production fixture.</param>
    private static void VerifyCrystalFlashPaletteTimingDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (ushort record = 0;
             record < CrystalFlashPaletteTimingDefinitions.RecordCount;
             record++)
        {
            int address = CrystalFlashPaletteTimingDefinitions.NativeFirstTimerAddress +
                record * CrystalFlashPaletteTimingDefinitions.RecordByteCount;
            ushort nativeDuration = unchecked((ushort)(
                rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
            ushort byteOffset = unchecked((ushort)(
                record * CrystalFlashPaletteTimingDefinitions.RecordByteCount));
            AssertEqual(nativeDuration,
                CrystalFlashPaletteTimingDefinitions.DurationForByteOffset(byteOffset),
                $"Crystal Flash body-palette duration {record}");
        }

        AssertThrows<InvalidDataException>(
            () => CrystalFlashPaletteTimingDefinitions.DurationForByteOffset(1),
            "Crystal Flash timing rejects an unaligned restored offset");
        AssertThrows<InvalidDataException>(
            () => CrystalFlashPaletteTimingDefinitions.DurationForByteOffset(
                CrystalFlashPaletteTimingDefinitions.RecordCount *
                    CrystalFlashPaletteTimingDefinitions.RecordByteCount),
            "Crystal Flash timing rejects an offset beyond the authored cycle");

        var samus = new SamusState
        {
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 128,
            YPosition = 128,
            Health = 1,
            MaxHealth = 99,
            Missiles = 10,
            SuperMissiles = 10,
            PowerBombs = 10,
        };
        samus.RefreshCollisionRadii(rom);
        samus.HorizontalSpeed.SpecialPaletteTimer = 0;
        AssertTrue(
            samus.CrystalFlash.TryBegin(
                rom, samus, controllerInput: 0, skipInputCheck: true),
            "Crystal Flash timing production fixture begins");

        var guard = new CrystalFlashPaletteTimingReadGuard(rom);
        var cgram = new SnesCgram();
        var colors = RetailPresentationFixture().CrystalFlashColors;
        int callCount = CrystalFlashPaletteTimingDefinitions.RecordCount *
            CrystalFlashPaletteTimingDefinitions.AuthoredDuration;
        for (int call = 0; call < callCount; call++)
        {
            AssertTrue(samus.CrystalFlash.UpdatePalette(guard, cgram, samus, colors: colors),
                $"Crystal Flash timing call {call} retains palette ownership");

            ushort expectedTimer = unchecked((ushort)(
                CrystalFlashPaletteTimingDefinitions.AuthoredDuration -
                call % CrystalFlashPaletteTimingDefinitions.AuthoredDuration));
            int loadedRecord = call / CrystalFlashPaletteTimingDefinitions.AuthoredDuration;
            ushort expectedOffset = unchecked((ushort)(
                (loadedRecord + 1) % CrystalFlashPaletteTimingDefinitions.RecordCount *
                CrystalFlashPaletteTimingDefinitions.RecordByteCount));
            AssertEqual(expectedTimer, samus.CrystalFlash.CrystalPaletteTimer,
                $"production Crystal Flash body timer call {call}");
            AssertEqual(expectedOffset, samus.CrystalFlash.CommonPaletteTimer,
                $"production Crystal Flash body record call {call}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Crystal Flash performs no body-duration ROM reads");
        Console.WriteLine(
            "Crystal Flash palette timing: all ten native durations and the complete " +
            "100-call production cycle pass with timer-word reads forbidden.");
    }

    /// <summary>Wraps cartridge access and rejects reads of the body-palette timer words during the production cycle.</summary>
    /// <param name="source">Underlying address space forwarded for accesses outside the guarded timer words.</param>
    private sealed class CrystalFlashPaletteTimingReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the number of attempted reads from the guarded body-duration words.</summary>
        public int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes an importer read through the timer-word guard.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The byte at <paramref name="address"/> when the address is not a guarded timer word.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of duration bytes while forwarding all other reads to the wrapped address space.</summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The requested byte when its address is outside the guarded duration words.</returns>
        /// <exception cref="InvalidOperationException">The address selects one of the native body-duration words.</exception>
        public byte ReadByte(int address)
        {
            int relative = address -
                CrystalFlashPaletteTimingDefinitions.NativeFirstTimerAddress;
            if (relative >= 0 &&
                relative < CrystalFlashPaletteTimingDefinitions.RecordCount *
                    CrystalFlashPaletteTimingDefinitions.RecordByteCount &&
                relative % CrystalFlashPaletteTimingDefinitions.RecordByteCount < 2)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Crystal Flash palette handler attempted timer read ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged to the wrapped cartridge address space.</summary>
        /// <param name="address">Cartridge address to write.</param>
        /// <param name="value">Byte to store at <paramref name="address"/>.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
