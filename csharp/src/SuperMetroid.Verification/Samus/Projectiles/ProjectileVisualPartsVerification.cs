using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileCompositionsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks authored projectile OAM parts against packed cartridge drawing semantics.</summary>
internal static class ProjectileVisualPartsVerification
{
    /// <summary>Exhausts X/size words across wrapped origins while preserving Y, attributes, and OAM high bits.</summary>
    internal static void VerifyProjectileVisualParts()
    {
        var parts = new OamBuffer();
        var packed = new OamBuffer();
        var bus = new TestAddressSpace();
        int count = 0;
        for (int rawX = 0; rawX <= ushort.MaxValue; rawX++)
        foreach (ushort origin in new ushort[] { 0, 127, 255, 256, 0x7fff, 0xffff })
        {
            // Sweep every encoded X/size word and every attribute word; low X also
            // sweeps all byte Y offsets. Origins exercise both sides of wrap boundaries.
            byte yOffset = (byte)rawX;
            ushort attribute = unchecked((ushort)~rawX);
            int index = count % OamBuffer.SpriteCount;
            SeedSinglePartProjectileSpritemap(bus, (ushort)rawX, yOffset, attribute);
            parts.AddProjectileSpritePart(new((ushort)rawX), yOffset, new(attribute), new Position16<SpritePixels>(origin, origin));
            DrawImportedProjectileSpritemap(bus, packed, 0xf700, new(origin, origin));
            ushort expectedX = unchecked((ushort)(origin + rawX));
            int offset = index * 4;
            AssertEqual((byte)expectedX, parts.LowTable[offset], "Projectile X wraps without clipping");
            AssertEqual(unchecked((byte)(origin + yOffset)), parts.LowTable[offset + 1], "Projectile Y wraps rather than parking");
            AssertEqual((byte)attribute, parts.LowTable[offset + 2], "Projectile attributes preserve low byte");
            AssertEqual(LittleEndianBytes.HighByte(attribute), parts.LowTable[offset + 3], "Projectile attributes preserve palette/priority/flip bits");
            int expectedPair = ((LittleEndianBytes.HighByte(expectedX)) & 1) | ((rawX & 0x8000) != 0 ? 2 : 0);
            AssertEqual(expectedPair, (parts.HighTable[index / 4] >> ((index % 4) * 2)) & 3, "Projectile high OAM preserves size and X bit eight");
            AssertEqual(((index + 1) * 4) & 0x1ff, parts.NextByteOffset, "Projectile OAM write pointer wraps at 128 parts");
            AssertTrue(parts.ImageEquals(packed), "Authored part and packed-ROM draw paths publish identical OAM");
            count++;
        }
        Console.WriteLine($"Projectile visual parts: {count} emissions preserve native wrapping, attributes and packed draw parity.");
    }

    /// <summary>
    /// Seeds the one-part packed projectile spritemap at <c>$93:F700</c>: a part count of one,
    /// then the X/size word, Y offset byte and attribute word of that part.
    /// </summary>
    private static void SeedSinglePartProjectileSpritemap(TestAddressSpace bus, ushort rawX, byte yOffset, ushort attribute)
    {
        WriteTestWord(bus, 0x93f700, 1);
        WriteTestWord(bus, 0x93f702, rawX);
        bus.WriteBytes(0x93f704, [yOffset]);
        WriteTestWord(bus, 0x93f705, attribute);
    }

    /// <summary>Runs single-part packing checks with the retail projectile composition comparison.</summary>
    [VerificationCommand("--projectile-sprite-compositions")]
    internal static int ProjectileSpriteCompositionsCommand()
    {
        string projectileRom = RepositoryRomPath;
        var source = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath(projectileRom));
        Suite(nameof(VerifyProjectileVisualParts), () => VerifyProjectileVisualParts());
        Suite(nameof(VerifyProjectileCompositions), () => VerifyProjectileCompositions(source, compositionOnly: true,
            sourceRom: projectileRom));
        return 0;
    }
}
