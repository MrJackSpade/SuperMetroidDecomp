using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Harness;

/// <summary>Houses typed sparse-bus writers for native Samus pose, physics, palette, and projectile tables.</summary>
internal static class SyntheticSamusCartridgeVerification
{

/// <summary>
/// Typed writers for the Samus cartridge tables that many synthetic sparse-bus fixtures
/// seed. Each writer owns exactly one native table's base, stride, and record layout, so
/// test bodies state which pose, list, or color they install instead of computing CPU
/// addresses. Payload bytes stay raw where they are byte-for-byte fixture evidence.
/// </summary>
internal static class SyntheticSamusCartridge
{
    /// <summary>
    /// Points pose <paramref name="pose"/>'s entry of the <c>$91:9EE2</c> pose-transition
    /// pointer table (one bank-$91 word per pose) at <paramref name="list"/>.
    /// </summary>
    internal static void WritePoseTransitionPointer(TestAddressSpace bus, SamusPoseId pose, ushort list) =>
        WriteTestWord(bus, 0x919ee2 + (int)pose * 2, list);

    /// <summary>
    /// Installs a bank-$91 pose-transition list at <paramref name="list"/>: six-byte
    /// records of (required newly pressed input, required held input, target pose) words,
    /// terminated by <c>$FFFF</c>.
    /// </summary>
    internal static void WritePoseTransitionList(
        TestAddressSpace bus,
        ushort list,
        params ReadOnlySpan<SyntheticSamusCartridgeRecord> records)
    {
        int address = 0x910000 | list;
        foreach ((ushort newInput, ushort heldInput, ushort targetPose) in records)
        {
            WriteTestWord(bus, address, newInput);
            WriteTestWord(bus, address + 2, heldInput);
            WriteTestWord(bus, address + 4, targetPose);
            address += 6;
        }
        WriteTestWord(bus, address, 0xffff);
    }

    /// <summary>
    /// Installs one twelve-byte record of the <c>$90:9F55</c> normal-air horizontal speed
    /// table (acceleration, maximum, and deceleration as whole/subspeed word pairs),
    /// indexed by movement type.
    /// </summary>
    internal static void WriteNormalAirSpeedEntry(
        TestAddressSpace bus,
        SamusMovementType movementType,
        ReadOnlySpan<byte> entry)
    {
        Ensure.LengthEqual(entry, 12);
        bus.WriteBytes(0x909f55 + (int)movementType * 12, entry);
    }

    /// <summary>
    /// Installs the dry-air initial jump speed: whole word <c>$90:9EB9</c> and subspeed
    /// word <c>$90:9EBF</c>.
    /// </summary>
    internal static void WriteDryAirJumpSpeed(TestAddressSpace bus, ushort whole, ushort subspeed)
    {
        WriteTestWord(bus, 0x909eb9, whole);
        WriteTestWord(bus, 0x909ebf, subspeed);
    }

    /// <summary>
    /// Installs the dry-air gravity: subspeed word <c>$90:9EA1</c> and whole word
    /// <c>$90:9EA7</c>.
    /// </summary>
    internal static void WriteDryAirGravity(TestAddressSpace bus, ushort whole, ushort subspeed)
    {
        WriteTestWord(bus, 0x909ea1, subspeed);
        WriteTestWord(bus, 0x909ea7, whole);
    }

    /// <summary>
    /// Installs the Morph Ball bounce speed: whole word <c>$90:9EB5</c> and fractional
    /// word <c>$90:9EB7</c>.
    /// </summary>
    internal static void WriteBallBounceSpeed(TestAddressSpace bus, ushort whole, ushort subspeed)
    {
        WriteTestWord(bus, 0x909eb5, whole);
        WriteTestWord(bus, 0x909eb7, subspeed);
    }

    /// <summary>
    /// Installs the dry-air wall-jump speed (table zero): whole word <c>$90:9ED1</c> and
    /// subspeed word <c>$90:9ED7</c>.
    /// </summary>
    internal static void WriteDryAirWallJumpSpeed(TestAddressSpace bus, ushort whole, ushort subspeed)
    {
        WriteTestWord(bus, 0x909ed1, whole);
        WriteTestWord(bus, 0x909ed7, subspeed);
    }

    /// <summary>
    /// Installs the dry-air bomb-jump magnitude read by <c>$90:9A2C</c>: whole word
    /// <c>$90:9EF5</c> and subspeed word <c>$90:9EFB</c>.
    /// </summary>
    internal static void WriteDryAirBombJumpSpeed(TestAddressSpace bus, ushort whole, ushort subspeed)
    {
        WriteTestWord(bus, 0x909ef5, whole);
        WriteTestWord(bus, 0x909efb, subspeed);
    }

    /// <summary>
    /// Installs the standalone twelve-byte bomb-jump horizontal speed record at
    /// <c>$90:9F25</c>, which <c>$90:8EF4</c> passes directly to <c>$90:9A7E</c>.
    /// </summary>
    internal static void WriteBombJumpSpeedEntry(TestAddressSpace bus, ReadOnlySpan<byte> entry)
    {
        Ensure.LengthEqual(entry, 12);
        bus.WriteBytes(0x909f25, entry);
    }

    /// <summary>
    /// Writes consecutive BGR555 color words into bank $9B starting at bank-local
    /// <paramref name="pointer"/>, the bank that every Samus body/beam palette pointer selects.
    /// </summary>
    internal static void WriteSamusPaletteColors(TestAddressSpace bus, ushort pointer, params ReadOnlySpan<ushort> colors)
    {
        for (int color = 0; color < colors.Length; color++)
        {
            WriteTestWord(
                bus,
                SamusSourceDefinitions.PaletteBank.Value | unchecked((ushort)(pointer + color * 2)),
                colors[color]);
        }
    }

    /// <summary>
    /// Installs a one-object bank-$93 spritemap at <paramref name="pointer"/>: entry count
    /// one, then the native X word, signed Y byte, and tile/attribute word.
    /// </summary>
    internal static void WriteSingleObjectProjectileSpritemap(
        TestAddressSpace bus,
        ushort pointer,
        ushort x,
        byte y,
        ushort tileAttributes)
    {
        int address = 0x930000 | pointer;
        WriteTestWord(bus, address, 1);
        WriteTestWord(bus, address + 2, x);
        WriteTestByte(bus, address + 4, y);
        WriteTestWord(bus, address + 5, tileAttributes);
    }
}
}
