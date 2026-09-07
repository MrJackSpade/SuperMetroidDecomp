using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyXrayTilemap()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var vram = new SnesVram();
        vram.ExecuteWordTransfer(Enumerable.Range(0, XrayTilemapLayout.BufferWords)
            .Select(i => (ushort)(0x4000 + i)).ToArray(), SnesPpuLayout.GameplayBg1TilemapWord, 1);
        var definitions = new byte[1024 * 8];
        for (int i = 0; i < definitions.Length / 2; i++)
        {
            definitions[i * 2] = (byte)i;
            definitions[i * 2 + 1] = (byte)(i >> 8);
        }
        var level = new RoomLevelData(32, 32, Enumerable.Repeat((ushort)0x8000, 1024).ToArray(),
            new byte[1024], new ushort[1024], definitions);
        ushort[] Build(byte area = (byte)AreaId.Brinstar) => XrayRevealTilemap.Build(bus, level, vram, 248, 248, 16, 16, area);
        void Set(int index, RoomCollisionType type, byte bts)
        {
            level.SetPlmForegroundEntry(index, (ushort)((int)type << 12));
            level.SetPlmBehavior(index, bts);
        }
        ushort[] basis = Build();
        AssertEqual(0x4000 + 30 * 32 + 30, basis[0], "base copy starts at the aligned physical BG1 metatile");
        AssertEqual(0x4000 + 1024 + 30 * 32, basis[2], "base copy crosses the BG1 horizontal screen boundary");
        AssertEqual(0x4000 + 30, basis[64], "base copy wraps the BG1 tilemap vertically");
        Set(33, RoomCollisionType.ShootableBlock, 3);
        Set(34, RoomCollisionType.HorizontalExtension, 0xff);
        Set(65, RoomCollisionType.VerticalExtension, 0xff);
        Set(66, RoomCollisionType.HorizontalExtension, 0xff);
        var square = Build();
        AssertEqual(0x99 * 4, square[0], "square top left");
        AssertEqual(0x9a * 4, square[2], "square top right survives extension lookup");
        AssertEqual(0xb9 * 4, square[64], "square lower left");
        AssertEqual(0xba * 4 + 3, square[99], "square lower right keeps its fourth 8px tile");
        Set(32, RoomCollisionType.ShootableBlock, 3);
        Set(33, RoomCollisionType.HorizontalExtension, 0xff);
        var left = Build();
        AssertEqual(0x9a * 4, left[0], "offscreen predecessor contributes its right half at the viewport edge");
        AssertEqual(0xba * 4, left[64], "offscreen predecessor contributes its lower right half");
        Set(48, RoomCollisionType.ShootableBlock, 1);
        Set(49, RoomCollisionType.HorizontalExtension, 0xff);
        var edge = Build();
        AssertEqual(0x96 * 4, edge[30], "last first-screen column receives wide reveal's left half");
        AssertEqual(basis[1024], edge[1024], "last first-screen column does not write into the second screen");
        Set(40, RoomCollisionType.SpecialBlock, 0x82);
        AssertEqual(0xb6 * 4, Build()[14], "Brinstar-only reveal is applied in Brinstar");
        AssertEqual(basis[14], Build((byte)AreaId.Crateria)[14], "Brinstar-only reveal preserves copied art elsewhere");
        Console.WriteLine("  X-ray tilemap: BG1 screen wrapping, four-piece reveals, left predecessor, right-edge suppression and area gate agree.");
    }
}
