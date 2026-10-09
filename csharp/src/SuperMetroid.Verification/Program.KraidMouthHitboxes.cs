using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks native Kraid mouth rectangles against collision boundaries and verifies their low-half address mapping.</summary>
    /// <param name="rom">Cartridge address space containing the authored head instructions and hitbox words.</param>
    private static void VerifyKraidMouthHitboxes(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidMouthShapeCases), () => VerifyKraidMouthShapeCases(rom));
        Suite(nameof(VerifyKraidLowHalfBoundaryMapping), () => VerifyKraidLowHalfBoundaryMapping(rom));
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
        ushort PointerWord(ushort pointer) => (ushort)(
            rom.ReadByte(0xa70000 | pointer) |
            rom.ReadByte(0xa70000 | unchecked((ushort)(pointer + 1))) << 8);
        for (int cursor = 0x96d2; cursor < 0x9788;)
        {
            ushort command = Word(0xa70000 | cursor);
            if ((command & 0x8000) != 0) { cursor += 2; continue; }
            foreach (int offset in new[] { 4, 6 })
            {
                ushort pointer = Word(0xa70000 | (cursor + offset));
                if (pointer != ushort.MaxValue) _ = KraidMouthHitboxes.Resolve(pointer);
            }
            cursor += 8;
        }
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guarded = new KraidMouthLowHalfReadGuard(rom);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guarded);
        Func<RoomEnemySlot, ushort, SamusProjectileSlot, bool> overlaps = enemies.KraidMouthHitboxOverlapsShot;
        var body = enemies.Slots[0];
        var shot = new SamusProjectileSystem().Slots[0];
        body.XPosition = 256;
        body.YPosition = 256;
        shot.XRadius = 4;
        shot.YRadius = 4;
        for (int entry = 0; entry < 8; entry++)
        {
            ushort pointer = (ushort)(0x9788 + entry * 8);
            int address = 0xa70000 | pointer;
            short left = (short)Word(address), top = (short)Word(address + 2), right = (short)Word(address + 4), bottom = (short)Word(address + 6);

            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            for (int edge = -1; edge <= 1; edge++)
            {
                shot.YPosition = (ushort)raw;
                shot.XPosition = (ushort)(body.XPosition + left - shot.XRadius + edge);
                bool expected = unchecked((short)(raw - shot.YRadius - 1 - body.YPosition - bottom)) < 0 &&
                    unchecked((short)(raw + shot.YRadius - body.YPosition - top)) >= 0 && edge >= 0;
                AssertEqual(expected, overlaps(body, pointer, shot), "Actual mouth collision preserves vertical bounds and inclusive left edge");
            }
        }

        for (int rawPointer = 0; rawPointer < 0x8000; rawPointer++)
        {
            ushort pointer = (ushort)rawPointer;
            (short Left, short Top, short Bottom) expected = default;
            (short Left, short Top, short Bottom) actual = default;
            Exception? expectedFailure = null;
            Exception? actualFailure = null;
            try
            {
                expected = (
                    unchecked((short)PointerWord(pointer)),
                    unchecked((short)PointerWord(unchecked((ushort)(pointer + 2)))),
                    unchecked((short)PointerWord(unchecked((ushort)(pointer + 6)))));
            }
            catch (Exception failure)
            {
                expectedFailure = failure;
            }

            try
            {
                actual = KraidMouthHitboxes.ResolveCollision(guarded, pointer);
            }
            catch (Exception failure)
            {
                actualFailure = failure;
            }

            AssertEqual(expectedFailure?.GetType(), actualFailure?.GetType(),
                $"Kraid low-half failure type ${pointer:X4}");
            AssertEqual(expectedFailure?.Message, actualFailure?.Message,
                $"Kraid low-half first-failing access ${pointer:X4}");
            if (expectedFailure is null)
            {
                AssertEqual(expected, actual,
                    $"Kraid live low-half mouth geometry pointer ${pointer:X4}");
            }
        }

        shot.XPosition = 260;
        shot.YPosition = 256;
        foreach (ushort pointer in new ushort[] { 0, 2, 0x1ff8 })
        {
            short left = unchecked((short)PointerWord(pointer));
            short top = unchecked((short)PointerWord(unchecked((ushort)(pointer + 2))));
            short bottom = unchecked((short)PointerWord(unchecked((ushort)(pointer + 6))));
            bool expected = unchecked((short)(shot.YPosition - shot.YRadius - 1 - body.YPosition - bottom)) < 0 &&
                unchecked((short)(shot.YPosition + shot.YRadius - body.YPosition - top)) >= 0 &&
                unchecked((short)(shot.XPosition + shot.XRadius - body.XPosition - left)) >= 0;
            AssertEqual(expected, overlaps(body, pointer, shot), "Non-catalog pointers preserve address-space reads");
        }

        foreach (ushort pointer in new ushort[] { 0x8000, 0x9787, 0x9789, 0x97c8, 0xffff })
        {
            AssertThrows<InvalidDataException>(
                () => KraidMouthHitboxes.ResolveCollision(guarded, pointer),
                $"Kraid non-catalog cartridge pointer ${pointer:X4} rejects");
        }

        Console.WriteLine("Kraid mouth geometry: all head-program pointers, 32 native rectangle words, seven low-half boundary bytes, 32768 live address-space starts and 1572864 authored collision probes match; unrelated cartridge pointers reject.");
    }

    /// <summary>Wraps an address space to fail if Kraid mouth collision reads enter the upper LoROM window.</summary>
    /// <param name="source">Address space used for permitted reads and writes.</param>
    private sealed class KraidMouthLowHalfReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Routes work-RAM reads through the guard's address validation.</summary>
        /// <param name="address">Bus address to read.</param>
        /// <returns>The byte at the address when the read is allowed.</returns>
        public byte ReadWorkRamByte(int address) => ReadByte(address);
        /// <summary>Routes save-RAM reads through the guard's address validation.</summary>
        /// <param name="address">Bus address to read.</param>
        /// <returns>The byte at the address when the read is allowed.</returns>
        public byte ReadSaveRamByte(int address) => ReadByte(address);

        /// <summary>Routes cartridge reads through the guard so forbidden upper-ROM accesses are detected.</summary>
        /// <param name="address">Bus address to read.</param>
        /// <returns>The byte at the address when the read is allowed.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects upper-LoROM reads from bank $A7 and delegates other addresses to the wrapped bus.</summary>
        /// <param name="address">Bus address requested by the code under verification.</param>
        /// <returns>The wrapped bus value for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The read targets the forbidden upper-LoROM window of bank $A7.</exception>
        public byte ReadByte(int address)
        {
            SnesAddress sourceAddress = SnesAddress.FromBusAddress(address);
            if (sourceAddress.Bank == 0xa7 && sourceAddress.IsUpperLoRomWindow)
            {
                throw new InvalidOperationException(
                    $"Kraid mouth geometry attempted upper-ROM read {sourceAddress}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Delegates a write to the wrapped address space.</summary>
        /// <param name="address">Bus address to write.</param>
        /// <param name="value">Byte stored at the address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    /// <summary>Restricted low-half LoROM bus used to supply deterministic Kraid hitbox boundary bytes.</summary>
    private sealed class KraidMouthBoundaryReadBus : ISnesAddressSpace, IImportCartridgeSource,
        ISnesMutableMemory, ISnesCpuPeripheralSource
    {
        /// <summary>Routes work-RAM reads through the low-half address check.</summary>
        /// <param name="address">Bus address to read.</param>
        /// <returns>The deterministic byte associated with the permitted cartridge offset.</returns>
        public byte ReadWorkRamByte(int address) => ReadByte(address);
        /// <summary>Routes save-RAM reads through the low-half address check.</summary>
        /// <param name="address">Bus address to read.</param>
        /// <returns>The deterministic byte associated with the permitted cartridge offset.</returns>
        public byte ReadSaveRamByte(int address) => ReadByte(address);
        /// <summary>Routes peripheral reads through the low-half address check.</summary>
        /// <param name="address">Bus address to read.</param>
        /// <returns>The deterministic byte associated with the permitted cartridge offset.</returns>
        public byte ReadPeripheralByte(int address) => ReadByte(address);

        /// <summary>Produces the deterministic fixture byte for a cartridge pointer.</summary>
        /// <param name="pointer">Lower-LoROM offset used to distinguish boundary addresses.</param>
        /// <returns>The low byte of the pointer XORed with the fixture pattern.</returns>
        public static byte Value(ushort pointer) => unchecked((byte)(pointer ^ 0x5a));

        /// <summary>Routes cartridge reads through the low-half address check.</summary>
        /// <param name="address">Bus address to read.</param>
        /// <returns>The deterministic byte associated with the permitted cartridge offset.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Accepts only lower-LoROM addresses in bank $A7 and returns a deterministic value for their offsets.</summary>
        /// <param name="address">Bus address requested by the collision reader.</param>
        /// <returns>The fixture value for the mapped cartridge offset.</returns>
        /// <exception cref="InvalidOperationException">The address is outside bank $A7's lower-LoROM window.</exception>
        public static byte ReadByte(int address)
        {
            SnesAddress sourceAddress = SnesAddress.FromBusAddress(address);
            if (sourceAddress.Bank != 0xa7 || sourceAddress.IsUpperLoRomWindow)
            {
                throw new InvalidOperationException(
                    $"Kraid boundary fixture rejected unexpected read {sourceAddress}.");
            }
            return Value(sourceAddress.Offset);
        }

        /// <summary>Rejects writes because this boundary fixture models a read-only cartridge region.</summary>
        /// <param name="address">Bus address the caller attempted to modify.</param>
        /// <param name="value">Byte the caller attempted to store.</param>
        /// <exception cref="InvalidOperationException">The fixture is read-only.</exception>
        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException("Kraid mouth geometry is read-only.");
    }
}
