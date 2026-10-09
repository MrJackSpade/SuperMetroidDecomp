using System.Buffers.Binary;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares the rain tilemap's calculated palette selector bits with ROM and installed page data.</summary>
    /// <param name="rom">Address space containing the native rain tilemap page.</param>
    /// <param name="stock">Installed rain and fog layer-three tilemaps under verification.</param>
    private static void VerifyRainTilePalette(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifyAtmosphereTileField), () => VerifyAtmosphereTileField(rom, stock, RoomFxType.Rain, 0x1c00));

    /// <summary>Compares the fog tilemap's calculated palette selector bits with ROM and installed page data.</summary>
    /// <param name="rom">Address space containing the native fog tilemap page.</param>
    /// <param name="stock">Installed rain and fog layer-three tilemaps under verification.</param>
    private static void VerifyFogTilePalette(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifyAtmosphereTileField), () => VerifyAtmosphereTileField(rom, stock, RoomFxType.Fog, 0x1c00));

    /// <summary>Checks the fog tilemap's priority bit against the corresponding native and installed fields.</summary>
    /// <param name="rom">Address space containing the native fog tilemap page.</param>
    /// <param name="stock">Installed rain and fog layer-three tilemaps under verification.</param>
    private static void VerifyFogTilePriority(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifyAtmosphereTileField), () => VerifyAtmosphereTileField(rom, stock, RoomFxType.Fog, 0x2000));

    /// <summary>Compares one masked calculated field across the native page, calculated field logic, and installed presentation bytes.</summary>
    /// <param name="rom">Address space used to read the native page pointer and tile words.</param>
    /// <param name="stock">Installed layer-three tilemap catalog supplying the authored page bytes.</param>
    /// <param name="type">Rain or fog effect whose native page and field rules are being compared.</param>
    /// <param name="mask">Tile-word bits selected for this check, such as palette selection or priority.</param>
    private static void VerifyAtmosphereTileField(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock,
        RoomFxType type, int mask)
    {
        int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + (ushort)type);
        ReadOnlySpan<byte> installed = stock.Resolve(type).Span;
        AssertEqual(0x840, installed.Length, "Complete original atmosphere page");
        for (int index = 0; index < 1056; index++)
        {
            int expected = ReadVerificationWord(rom, source + 2 * index) & mask;
            AssertEqual(expected, RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, index) & mask,
                "Original calculated atmosphere field");
            AssertEqual(expected, BinaryPrimitives.ReadUInt16LittleEndian(installed[(2 * index)..]) & mask,
                "Installed atmosphere field view");
        }
    }

    /// <summary>Checks that only rain and fog use calculated fields and that edits preserve all other tilemap data.</summary>
    /// <param name="rom">Address space containing the original rain and fog pages used as edit fixtures.</param>
    /// <param name="stock">Installed catalog inspected to ensure calculated bits are not cached as authored edits.</param>
    private static void VerifyAtmosphereTilemapDomainAndEdits(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            var type = (RoomFxType)value;
            if (type is RoomFxType.Rain or RoomFxType.Fog) continue;
            AssertThrows<ArgumentOutOfRangeException>(() => RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, 0),
                "Only rain/fog have these field rules");
        }
        foreach (RoomFxType type in new[] { RoomFxType.Rain, RoomFxType.Fog })
        {
            int mask = type == RoomFxType.Rain ? 0x1c00 : 0x3c00;
            AssertEqual(mask, (int)RoomFxAtmosphereTilemapDefinitions.CalculatedMask(type), "Exact converted field mask");
            object page = typeof(RoomFxLayer3TilemapCatalog).GetField(type.ToString().ToLowerInvariant(),
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
            FieldInfo edits = typeof(RoomFxAtmosphereTilemap).GetField("customFields", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var retained = (ushort[])typeof(RoomFxAtmosphereTilemap).GetField("preservedBits",
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page)!;
            AssertTrue(edits.GetValue(page) is null && retained.All(word => (word & mask) == 0),
                "Calculated stock fields absent from both stores");
            _ = stock.Resolve(type);
            AssertTrue(edits.GetValue(page) is null && retained.All(word => (word & mask) == 0),
                "Transfer never caches stock fields");
            int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + (ushort)type);
            var original = new byte[0x840];
            for (int offset = 0; offset < original.Length; offset++) original[offset] = rom.ReadByte(source + offset);
            for (int index = 0; index < 1056; index++)
            {
                byte[] changed = original.ToArray();
                changed[index * 2 + 1] ^= (byte)(type == RoomFxType.Fog && (index & 1) != 0 ? 32 : 4);
                var custom = new RoomFxAtmosphereTilemap(type, changed);
                byte[] transfer = custom.CreateTransfer();
                AssertTrue(changed.AsSpan().SequenceEqual(transfer), "A custom field edit at every cell survives");
                Array.Clear(changed);
                AssertTrue(transfer.AsSpan().SequenceEqual(custom.CreateTransfer()), "Custom input ownership is independent");
            }
            foreach (int invalid in new[] { int.MinValue, -1, 1056, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, invalid),
                    "Exact33-row cell bounds");
            foreach (int length in new[] { 0, 1, 0x83f, 0x841 })
                AssertThrows<ArgumentException>(() => new RoomFxAtmosphereTilemap(type, new byte[length]),
                    "Incomplete atmosphere pages reject");
        }
    }

    /// <summary>Runs the palette, priority, valid-domain, and edit-preservation checks for atmosphere tilemaps.</summary>
    /// <param name="rom">Address space containing the native rain and fog pages.</param>
    /// <param name="stock">Installed rain and fog page catalog passed to each focused check.</param>
    private static void VerifyAtmosphereTilemapFields(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        Suite(nameof(VerifyRainTilePalette), () => VerifyRainTilePalette(rom, stock));
        Suite(nameof(VerifyFogTilePalette), () => VerifyFogTilePalette(rom, stock));
        Suite(nameof(VerifyFogTilePriority), () => VerifyFogTilePriority(rom, stock));
        Suite(nameof(VerifyAtmosphereTilemapDomainAndEdits), () => VerifyAtmosphereTilemapDomainAndEdits(rom, stock));
    }
}
