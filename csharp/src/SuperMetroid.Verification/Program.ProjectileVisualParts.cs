using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void VerifyProjectileVisualParts()
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
            parts.AddProjectileSpritePart(new((ushort)rawX), yOffset, new(attribute), new Position16<OamPixels>(origin, origin));
            DrawImportedProjectileSpritemap(bus, packed, 0xf700, new(origin, origin));
            ushort expectedX = unchecked((ushort)(origin + rawX));
            int offset = index * 4;
            AssertEqual((byte)expectedX, parts.LowTable[offset], "Projectile X wraps without clipping");
            AssertEqual(unchecked((byte)(origin + yOffset)), parts.LowTable[offset + 1], "Projectile Y wraps rather than parking");
            AssertEqual((byte)attribute, parts.LowTable[offset + 2], "Projectile attributes preserve low byte");
            AssertEqual((byte)(attribute >> 8), parts.LowTable[offset + 3], "Projectile attributes preserve palette/priority/flip bits");
            int expectedPair = ((expectedX >> 8) & 1) | ((rawX & 0x8000) != 0 ? 2 : 0);
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
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for a one-part bank-$93 projectile spritemap")]
    private static void SeedSinglePartProjectileSpritemap(TestAddressSpace bus, ushort rawX, byte yOffset, ushort attribute)
    {
        WriteTestWord(bus, 0x93f700, 1);
        WriteTestWord(bus, 0x93f702, rawX);
        bus.WriteBytes(0x93f704, [yOffset]);
        WriteTestWord(bus, 0x93f705, attribute);
    }
}
