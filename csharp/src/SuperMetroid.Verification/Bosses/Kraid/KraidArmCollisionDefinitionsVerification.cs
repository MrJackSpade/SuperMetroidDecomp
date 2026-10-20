using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmCallbackMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmComponentHitboxesVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmPhysicalFrameMappingsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid's compiled arm frame and collision data against bank-$A7 cartridge records.</summary>
internal static class KraidArmCollisionDefinitionsVerification
{
    /// <summary>Runs callback, frame-selection, and hitbox-list comparisons for the selected Kraid arm records.</summary>
    internal static void VerifyKraidArmCollisionDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidArmComponentHitboxes), () => VerifyKraidArmComponentHitboxes(rom));
        Suite(nameof(VerifyKraidArmTouchCallbacks), () => VerifyKraidArmTouchCallbacks(rom));
        Suite(nameof(VerifyKraidArmShotCallbacks), () => VerifyKraidArmShotCallbacks(rom));
        Suite(nameof(VerifyKraidArmPhysicalFramePointers), () => VerifyKraidArmPhysicalFramePointers(rom));
        HashSet<ushort> hitboxPointers = VerifyKraidArmPhysicalLayoutSelection(rom);

        AssertEqual(16, hitboxPointers.Count,
            "Kraid arm selected frames reference all 16 physical hitbox lists");
        Suite(nameof(VerifyKraidArmHitboxListSelection), () => VerifyKraidArmHitboxListSelection(rom));
        Console.WriteLine(
            "Kraid arm physical definitions: 22 frame maps, 16 hitbox lists, " +
            "and 24 rectangles/callback pairs match the retail ROM.");
    }

    /// <summary>Compares all sixteen selected arm lists and twenty-four rectangles while rejecting other pointer identities.</summary>
    internal static void VerifyKraidArmHitboxListSelection(SuperMetroidAddressSpace rom)
    {
        // Native arm-list identities, independently transcribed from bank_A7.asm.
        // Counts, ordering and selected geometry are checked against the ROM;
        // the existing semantic case implementation supplies no expected values.
        ushort[] original = [0x92d1, 0x92eb, 0x92f9, 0x9313, 0x9321, 0x933b,
            0x9349, 0x9371, 0x937f, 0x9399, 0x93f7, 0x9411, 0x941f, 0x9439,
            0x946f, 0x947d];
        var selected = original.ToHashSet();
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            ushort candidate = (ushort)pointer;
            if (!selected.Contains(candidate))
                AssertThrows<InvalidDataException>(() => KraidArmCollisionDefinitions.HitboxesAt(candidate),
                    $"Non-arm-list identity {candidate:X4} is rejected");
        }
        int hitboxCount = 0;
        foreach (ushort pointer in original)
        {
            System.Collections.Immutable.ImmutableArray<KraidCollisionHitbox> compiled =
                KraidArmCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual(ReadKraidArmInstructionWord(rom, pointer), compiled.Length,
                $"Kraid arm hitbox count $A7:{pointer:X4}");
            for (int index = 0; index < compiled.Length; index++)
            {
                KraidCollisionHitbox hitbox = compiled[index];
                ushort record = unchecked((ushort)(pointer + 2 + index * 12));
                AssertEqual(unchecked((short)ReadKraidArmInstructionWord(rom, record)),
                    hitbox.Left, $"Kraid arm hitbox left $A7:{record:X4}");
                AssertEqual(unchecked((short)ReadKraidArmInstructionWord(rom,
                        unchecked((ushort)(record + 2)))),
                    hitbox.Top, $"Kraid arm hitbox top $A7:{record:X4}");
                AssertEqual(unchecked((short)ReadKraidArmInstructionWord(rom,
                        unchecked((ushort)(record + 4)))),
                    hitbox.Right, $"Kraid arm hitbox right $A7:{record:X4}");
                AssertEqual(unchecked((short)ReadKraidArmInstructionWord(rom,
                        unchecked((ushort)(record + 6)))),
                    hitbox.Bottom, $"Kraid arm hitbox bottom $A7:{record:X4}");
                hitboxCount++;
            }
            AssertTrue(compiled.SequenceEqual(Enumerable.Range(0, compiled.Length).Select(i => compiled[i])),
                "Arm slice materialization preserves native rectangle order");
            foreach (int invalid in new[] { int.MinValue, -1, compiled.Length, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => _ = compiled[invalid], "Arm slice rejects invalid ordinal");
        }
        AssertEqual(24, hitboxCount, "Kraid arm compiled physical rectangles");
    }

    /// <summary>Reads a bank-$A7 arm word from the retail cartridge for selector comparisons.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid arm instruction words")]
    internal static ushort ReadKraidArmInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa70000 | address);
}
