using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidMouthMappingsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Kraid mouth hitbox geometry and lower-bank bus behavior against retail cartridge and live address-space reads.</summary>
internal static class KraidMouthHitboxesVerification
{
    /// <summary>Checks head-program pointers, rectangle boundaries, all lower-half starts, and rejection of unrelated cartridge pointers.</summary>
    internal static void VerifyKraidMouthHitboxes(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidMouthShapeCases), () => VerifyKraidMouthShapeCases(rom));
        ushort Word(int a) => SnesLittleEndian.Word(KraidMouthHitboxesBusByte(rom, a), KraidMouthHitboxesBusByte(rom, a + 1));
        for (int cursor = 0x96d2; cursor < 0x9788;)
        {
            ushort command = Word(0xa70000 | cursor);
            if ((command & 0x8000) != 0) { cursor += 2; continue; }
            foreach (int offset in new[] { 4, 6 })
            {
                ushort pointer = Word(0xa70000 | (cursor + offset));
                if (pointer != ushort.MaxValue) _ = KraidMouthHitboxDefinitionsConstants.Resolve(pointer);
            }
            cursor += 8;
        }
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var guarded = NewKraidMouthLowHalfGuard(rom);
        Func<RoomEnemySlot, ushort, SamusProjectileSlot, bool> overlaps = CoreAccess.KraidMouthHitboxOverlapsShot;
        var body = enemies.SlotTable.Slots[0];
        var shot = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None).Slots[0];
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

        foreach (ushort pointer in new ushort[] { 0, 2, 0x1ff8 })
        {
            AssertThrows<InvalidDataException>(() => KraidMouthHitboxDefinitionsConstants.ResolveCollision(pointer),
                $"Kraid low-half mouth pointer ${pointer:X4} has no compiled geometry");
            AssertThrows<InvalidDataException>(() => overlaps(body, pointer, shot),
                $"Kraid mouth collision rejects low-half pointer ${pointer:X4}");
        }

        foreach (ushort pointer in new ushort[] { 0x8000, 0x9787, 0x9789, 0x97c8, 0xffff })
        {
            AssertThrows<InvalidDataException>(
                () => KraidMouthHitboxDefinitionsConstants.ResolveCollision(pointer),
                $"Kraid non-catalog cartridge pointer ${pointer:X4} rejects");
        }

        Console.WriteLine("Kraid mouth geometry: all head-program pointers, 32 native rectangle words and low-half rejection agree.");
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
            SnesDmaSourceKind.Cartridge => rom.ReadCartridgeByte(address),
            SnesDmaSourceKind.Unmapped => throw new InvalidOperationException(
                $"CPU read ${SnesAddressMath.BankNumber(address):X2}:{SnesAddressMath.OffsetInBank(address):X4} is outside the runtime address map."),
            _ => throw new InvalidOperationException(
                $"CPU read ${SnesAddressMath.BankNumber(address):X2}:{SnesAddressMath.OffsetInBank(address):X4} is outside the runtime address map."),
        };

    /// <summary>Allows live lower-half bus reads while rejecting upper bank-A7 cartridge geometry reads.</summary>
    private static MutableCartridgeReadGuard NewKraidMouthLowHalfGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => SnesAddress.FromBusAddress(address) is { Bank: 0xa7, IsUpperLoRomWindow: true },
            address => $"Kraid mouth geometry attempted upper-ROM read {SnesAddress.FromBusAddress(address)}.");
        return guard;
    }

    /// <summary>Runs the Kraid mouth geometry suite using the retail ROM oracle.</summary>
    [VerificationCommand("--kraid-mouth-hitboxes")]
    internal static int KraidMouthHitboxesCommand()
    {
        Suite(nameof(VerifyKraidMouthHitboxes), () => VerifyKraidMouthHitboxes(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
