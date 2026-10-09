using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Compares every compiled Samus death-explosion timer with its retail byte and checks the sequence boundary.</summary>
    private static void VerifySamusDeathExplosionTimingDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (ushort index = 0;
             index < SamusDeathExplosionTimingDefinitions.RecordCount;
             index++)
        {
            int address = SamusDeathExplosionTimingDefinitions.NativeFirstTimerAddress +
                index * SamusDeathExplosionTimingDefinitions.RecordByteCount;
            AssertEqual(
                rom.ReadByte(address),
                SamusDeathExplosionTimingDefinitions.DurationForIndex(index),
                $"Samus death-explosion duration {index}");
        }

        AssertThrows<InvalidDataException>(
            () => SamusDeathExplosionTimingDefinitions.DurationForIndex(
                SamusDeathExplosionTimingDefinitions.RecordCount),
            "Samus death-explosion timing rejects a post-sequence index");

        Console.WriteLine(
            "Samus death-explosion timing: all nine native durations match the " +
            "compiled sequence and invalid restored indexes fail loudly.");
    }

    /// <summary>Guards the native death-explosion timer bytes so production playback must use the compiled timing sequence.</summary>
    /// <param name="source">The underlying address space used for reads outside the protected timer-byte locations and for writes.</param>
    private sealed class SamusDeathExplosionTimingReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the number of attempts to reread a compiled death-explosion timer byte.</summary>
        public int ForbiddenReadAttempts { get; private set; }

        /// <summary>Rejects protected timer-byte reads and forwards other address-space reads.</summary>
        /// <param name="address">The SNES address requested by the gameplay code.</param>
        /// <returns>The source byte for an address outside the compiled timer-byte locations.</returns>
        /// <exception cref="InvalidOperationException">The requested address is a native death-explosion timer byte.</exception>
        public byte ReadByte(int address)
        {
            RejectTimerRead(address);
            return source.ReadByte(address);
        }

        /// <summary>Applies the timer guard before forwarding an allowed cartridge-import read to the underlying import source.</summary>
        /// <param name="address">The SNES address requested during cartridge import.</param>
        /// <returns>The imported byte when the address is not a protected timer location.</returns>
        /// <exception cref="InvalidOperationException">The requested address is a native death-explosion timer byte.</exception>
        public byte ReadCartridgeByte(int address)
        {
            RejectTimerRead(address);
            return CartridgeImportSource.Require(source).ReadCartridgeByte(address);
        }

        /// <summary>Counts and rejects an access to one of the native timer bytes represented by the compiled duration records.</summary>
        /// <param name="address">The SNES address to test.</param>
        /// <exception cref="InvalidOperationException">The address is the timer byte of a compiled death-explosion record.</exception>
        private void RejectTimerRead(int address)
        {
            int relative = address -
                SamusDeathExplosionTimingDefinitions.NativeFirstTimerAddress;
            if (relative >= 0 &&
                relative < SamusDeathExplosionTimingDefinitions.RecordCount *
                    SamusDeathExplosionTimingDefinitions.RecordByteCount &&
                relative % SamusDeathExplosionTimingDefinitions.RecordByteCount == 0)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Samus death sequence attempted timer read ${address:X6}.");
            }
        }

        /// <summary>Forwards writes to the wrapped address space without applying the read guard.</summary>
        /// <param name="address">The SNES address to write.</param>
        /// <param name="value">The byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
