using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidMouthHitboxes(SuperMetroidAddressSpace rom)
    {
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
        var overlaps = typeof(RoomEnemySystem).GetMethod("KraidMouthHitboxOverlapsShot", flags)!
            .CreateDelegate<Func<RoomEnemySlot, ushort, SamusProjectileSlot, bool>>(enemies);
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
            AssertEqual((left, top, right, bottom), KraidMouthHitboxes.Resolve(pointer), "All native mouth geometry words");
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            for (int edge = -1; edge <= 1; edge++)
            {
                shot.YPosition = (ushort)raw;
                shot.XPosition = (ushort)(body.XPosition + left - shot.XRadius + edge);
                bool expected = raw - shot.YRadius - 1 < body.YPosition + bottom &&
                    raw + shot.YRadius >= body.YPosition + top && edge >= 0;
                AssertEqual(expected, overlaps(body, pointer, shot), "Actual mouth collision preserves vertical bounds and inclusive left edge");
            }
        }

        for (int index = 0; index < KraidMouthHitboxes.LowHalfBoundaryBytes.Length; index++)
        {
            AssertEqual(
                rom.ReadByte(0xa78000 + index),
                KraidMouthHitboxes.LowHalfBoundaryBytes[index],
                $"Kraid mouth low-half boundary byte {index}");
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

        var boundaryBus = new KraidMouthBoundaryReadBus();
        byte BoundaryByte(ushort pointer) => pointer < 0x8000
            ? KraidMouthBoundaryReadBus.Value(pointer)
            : KraidMouthHitboxes.LowHalfBoundaryBytes[pointer - 0x8000];
        ushort BoundaryWord(ushort pointer) => (ushort)(
            BoundaryByte(pointer) |
            BoundaryByte(unchecked((ushort)(pointer + 1))) << 8);
        for (ushort pointer = 0x7ff9; pointer <= 0x7fff; pointer++)
        {
            var expected = (
                Left: unchecked((short)BoundaryWord(pointer)),
                Top: unchecked((short)BoundaryWord(unchecked((ushort)(pointer + 2)))),
                Bottom: unchecked((short)BoundaryWord(unchecked((ushort)(pointer + 6)))));
            AssertEqual(expected, KraidMouthHitboxes.ResolveCollision(boundaryBus, pointer),
                $"Kraid low-half crossing pointer ${pointer:X4}");
        }

        shot.XPosition = 260;
        shot.YPosition = 256;
        foreach (ushort pointer in new ushort[] { 0, 2, 0x1ff8 })
        {
            short left = unchecked((short)PointerWord(pointer));
            short top = unchecked((short)PointerWord(unchecked((ushort)(pointer + 2))));
            short bottom = unchecked((short)PointerWord(unchecked((ushort)(pointer + 6))));
            bool expected = shot.YPosition - shot.YRadius - 1 < body.YPosition + bottom &&
                shot.YPosition + shot.YRadius >= body.YPosition + top && shot.XPosition + shot.XRadius >= body.XPosition + left;
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

    private sealed class KraidMouthLowHalfReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
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

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class KraidMouthBoundaryReadBus : ISnesAddressSpace
    {
        public static byte Value(ushort pointer) => unchecked((byte)(pointer ^ 0x5a));

        public byte ReadByte(int address)
        {
            SnesAddress sourceAddress = SnesAddress.FromBusAddress(address);
            if (sourceAddress.Bank != 0xa7 || sourceAddress.IsUpperLoRomWindow)
            {
                throw new InvalidOperationException(
                    $"Kraid boundary fixture rejected unexpected read {sourceAddress}.");
            }
            return Value(sourceAddress.Offset);
        }

        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException("Kraid mouth geometry is read-only.");
    }
}
