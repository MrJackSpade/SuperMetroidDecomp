using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static KraidMouthMappingsVerification;
using static SuiteTimingVerification;

/// <summary>Represents kraid mouth hitboxes verification.</summary>
internal static class KraidMouthHitboxesVerification
{
    /// <summary>Verifies kraid mouth hitboxes.</summary>
    internal static void VerifyKraidMouthHitboxes(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidMouthShapeCases), () => VerifyKraidMouthShapeCases(rom));
        Suite(nameof(VerifyKraidLowHalfBoundaryMapping), () => VerifyKraidLowHalfBoundaryMapping(rom));
        ushort Word(int a) => (ushort)(SnesLittleEndian.Word(KraidMouthHitboxesBusByte(rom, a), KraidMouthHitboxesBusByte(rom, a + 1)));
        ushort PointerWord(ushort pointer) => (ushort)(
            SnesLittleEndian.Word(KraidMouthHitboxesBusByte(rom, 0xa70000 | pointer), KraidMouthHitboxesBusByte(rom, 0xa70000 | unchecked((ushort)(pointer + 1)))));
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
        var guarded = NewKraidMouthLowHalfGuard(rom);
        enemies.BindFixtureBus(guarded);
        Func<RoomEnemySlot, ushort, SamusProjectileSlot, bool> overlaps = enemies.KraidMouthHitboxOverlapsShot;
        var body = enemies.SlotTable.Slots[0];
        var shot = new SamusProjectileSystem().Slots[0];
        body.Position = body.Position.WithWhole(new(256, 256));
        shot.Radius = new(4, 4);
        for (int entry = 0; entry < 8; entry++)
        {
            ushort pointer = (ushort)(0x9788 + entry * 8);
            int address = 0xa70000 | pointer;
            short left = (short)Word(address), top = (short)Word(address + 2), right = (short)Word(address + 4), bottom = (short)Word(address + 6);

            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            for (int edge = -1; edge <= 1; edge++)
            {
                shot.Position = shot.Position.WithWhole(new((ushort)(body.Position.X.Whole + left - shot.Radius.X + edge), (ushort)raw));
                bool expected = unchecked((short)(raw - shot.Radius.Y - 1 - body.Position.Y.Whole - bottom)) < 0 &&
                    unchecked((short)(raw + shot.Radius.Y - body.Position.Y.Whole - top)) >= 0 && edge >= 0;
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

        shot.Position = shot.Position.WithWhole(new(260, 256));
        foreach (ushort pointer in new ushort[] { 0, 2, 0x1ff8 })
        {
            short left = unchecked((short)PointerWord(pointer));
            short top = unchecked((short)PointerWord(unchecked((ushort)(pointer + 2))));
            short bottom = unchecked((short)PointerWord(unchecked((ushort)(pointer + 6))));
            bool expected = unchecked((short)(shot.Position.Y.Whole - shot.Radius.Y - 1 - body.Position.Y.Whole - bottom)) < 0 &&
                unchecked((short)(shot.Position.Y.Whole + shot.Radius.Y - body.Position.Y.Whole - top)) >= 0 &&
                unchecked((short)(shot.Position.X.Whole + shot.Radius.X - body.Position.X.Whole - left)) >= 0;
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

    /// <summary>
    /// Independent CPU-bus oracle for Kraid mouth geometry: retail cartridge bytes, and for
    /// pointers below $8000 the live low-WRAM mirror or the unmapped-window rejection that
    /// bank $A7's lower half selects.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM and live low-half CPU-bus oracle for Kraid mouth hitbox words")]
    private static byte KraidMouthHitboxesBusByte(SuperMetroidAddressSpace rom, int address) =>
        SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.WorkRam => rom.ReadWorkRamByte(address),
            SnesDmaSourceKind.SaveRam => rom.ReadSaveRamByte(address),
            SnesDmaSourceKind.Cartridge => rom.ReadCartridgeByte(address),
            _ => throw new InvalidOperationException(
                $"CPU read ${SnesAddressMath.BankNumber(address):X2}:{SnesAddressMath.OffsetInBank(address):X4} is outside the runtime address map."),
        };

    /// <summary>Creates kraid mouth low half guard.</summary>
    private static MutableCartridgeReadGuard NewKraidMouthLowHalfGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => SnesAddress.FromBusAddress(address) is { Bank: 0xa7, IsUpperLoRomWindow: true },
            address => $"Kraid mouth geometry attempted upper-ROM read {SnesAddress.FromBusAddress(address)}.");
        return guard;
    }

    /// <summary>Represents kraid mouth boundary read bus.</summary>
    [CpuBusBoundary("exact bus test: synthetic bank-$A7 low-window WRAM/SRAM/peripheral responder witnessing which bus source each Kraid mouth and head boundary-crossing operand byte selects")]
    internal sealed class KraidMouthBoundaryReadBus : ISnesAddressSpace, IImportCartridgeSource,
        ISnesMutableMemory, ISnesCpuPeripheralSource
    {
        /// <summary>Reads work ram byte.</summary>
        public byte ReadWorkRamByte(int address) => ReadByte(address);
        /// <summary>Reads save ram byte.</summary>
        public byte ReadSaveRamByte(int address) => ReadByte(address);
        /// <summary>Reads peripheral byte.</summary>
        public byte ReadPeripheralByte(int address) => ReadByte(address);

        /// <summary>Value.</summary>
        public static byte Value(ushort pointer) => unchecked((byte)(pointer ^ 0x5a));

        /// <summary>Reads cartridge byte.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Reads byte.</summary>
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

        /// <summary>Writes byte.</summary>
        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException("Kraid mouth geometry is read-only.");
    }

    /// <summary>Kraid mouth hitboxes command.</summary>
    [VerificationCommand("--kraid-mouth-hitboxes")]
    internal static int KraidMouthHitboxesCommand()
    {
        Suite(nameof(VerifyKraidMouthHitboxes), () => VerifyKraidMouthHitboxes(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
