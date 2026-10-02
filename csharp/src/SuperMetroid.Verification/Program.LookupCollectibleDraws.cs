using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCollectibleDrawGeometry(SuperMetroidAddressSpace rom) => VerifyCollectibleDrawField(rom, 0);
    private static void VerifyCollectibleDrawCollision(SuperMetroidAddressSpace rom) => VerifyCollectibleDrawField(rom, 1);
    private static void VerifyCollectibleDrawVisuals(SuperMetroidAddressSpace rom) => VerifyCollectibleDrawField(rom, 2);
    private static void VerifyCollectibleDrawIdentity(SuperMetroidAddressSpace rom) => VerifyCollectibleDrawField(rom, 3);

    private static void VerifyCollectibleDrawField(SuperMetroidAddressSpace rom, int field)
    {
        // Original exported pointer/name contract; physical values come from the ROM.
        (ushort Pointer, string Id)[] expected = [(0xa2b5,"empty"),(0xa2c7,"chozo-orb-0"),(0xa2cd,"chozo-orb-1"),(0xa2d3,"chozo-orb-2"),(0xa2d9,"chozo-orb-burst"),(0xa2df,"energy-tank-0"),(0xa2e5,"energy-tank-1"),(0xa2eb,"missile-tank-0"),(0xa2f1,"missile-tank-1"),(0xa2f7,"super-missile-tank-0"),(0xa2fd,"super-missile-tank-1"),(0xa303,"power-bomb-tank-0"),(0xa309,"power-bomb-tank-1"),(0xa30f,"dynamic-slot-0-frame-0"),(0xa315,"dynamic-slot-0-frame-1"),(0xa31b,"dynamic-slot-1-frame-0"),(0xa321,"dynamic-slot-1-frame-1"),(0xa327,"dynamic-slot-2-frame-0"),(0xa32d,"dynamic-slot-2-frame-1"),(0xa333,"dynamic-slot-3-frame-0"),(0xa339,"dynamic-slot-3-frame-1"),(0xa3dd,"shot-reveal-0"),(0xa3e3,"shot-reveal-1"),(0xa3e9,"shot-reveal-2")];
        var exported = RoomPlmCollectibleDrawDefinitions.All.ToArray();
        AssertEqual(24, exported.Length, "Collectible original export count");
        if (field == 0)
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                bool owned = expected.Any(entry => entry.Pointer == raw);
                AssertEqual(owned, RoomPlmCollectibleDrawDefinitions.TryGet((ushort)raw, out var value), "Collectible complete pointer domain");
                AssertEqual(owned, RoomPlmCollectibleDrawDefinitions.TryGetWord((ushort)raw, out ushort word), "Collectible runtime word domain");
                if (!owned)
                {
                    AssertEqual(default(RoomPlmCollectibleDrawFrame), value, "Collectible rejected output");
                    AssertEqual((ushort)0, word, "Collectible rejected runtime output");
                }
            }
        for (int index = 0; index < expected.Length; index++)
        {
            var entry = expected[index];
            AssertTrue(RoomPlmCollectibleDrawDefinitions.TryGet(entry.Pointer, out var direct), "Collectible direct identity");
            AssertTrue(RoomPlmCollectibleDrawDefinitions.TryGetById(entry.Id, out var named), "Collectible reverse identity");
            foreach (var value in new[] {direct,named,exported[index]})
            {
                AssertEqual(entry.Pointer, value.Pointer, "Collectible original pointer and export order");
                if (field == 0)
                {
                    AssertEqual((ushort)1, ReadCollectibleVisualWord(rom, entry.Pointer), "Collectible original horizontal one-cell geometry");
                    AssertEqual((ushort)0, ReadCollectibleVisualWord(rom, (ushort)(entry.Pointer + 4)), "Collectible original terminator");
                }
                else if (field == 3) AssertEqual(entry.Id, value.Id, "Collectible original published name");
                else
                {
                    int mask = field == 1 ? 0xf000 : 0xfff;
                    AssertTrue(RoomPlmCollectibleDrawDefinitions.TryGetWord(entry.Pointer, out ushort runtimeWord), "Collectible runtime word ownership");
                    AssertEqual(ReadCollectibleVisualWord(rom, (ushort)(entry.Pointer + 2)) & mask, runtimeWord & mask, "Collectible original runtime field");
                    AssertEqual(ReadCollectibleVisualWord(rom, (ushort)(entry.Pointer + 2)) & mask,
                        value.LevelWord & mask, "Collectible original physical field");
                }
            }
            if (field == 3)
                foreach (string bad in new[] {entry.Id.ToUpperInvariant(), " " + entry.Id, entry.Id + " "})
                {
                    AssertTrue(!RoomPlmCollectibleDrawDefinitions.TryGetById(bad, out var missing), "Collectible ordinal identity");
                    AssertEqual(default(RoomPlmCollectibleDrawFrame), missing, "Collectible rejected identity output");
                }
        }
        if (field == 3)
            foreach (string? bad in new string?[] {null,"","energy-tank-2","dynamic-slot-4-frame-0","chozo-orb-3","shot-reveal-3"})
                AssertTrue(!RoomPlmCollectibleDrawDefinitions.TryGetById(bad!, out _), "Collectible unsupported ID");
    }
}
