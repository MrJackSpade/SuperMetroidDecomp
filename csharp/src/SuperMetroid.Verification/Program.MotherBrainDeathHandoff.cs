using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    static void VerifyMotherBrainDeathHandoff()
    {
        var bus = new TestAddressSpace();
        var vram = new SnesVram();
        const int textAddress = 0xa68000;
        // Delay two, destination $4800, A, space, exclamation, terminator.
        bus.WriteBytes(textAddress, [1, 0, 2, 0, 13, 0, 0, 0x48, (byte)'A', (byte)' ', (byte)'!', 0, 0]);
        var text = new EscapeTypewriterState(textAddress, EscapeTypewriterRomData.ZebesTileBase);
        AssertTrue(!text.Step(bus, vram), "typewriter first glyph is not completion");
        AssertEqual(EscapeTypewriterRomData.ZebesTileBase, vram.ReadWord(0x4800), "text glyph reaches actual VRAM");
        AssertTrue(!text.ClickRequested, "first glyph does not click");
        text.Step(bus, vram);
        AssertEqual((ushort)0x4802, text.Destination, "space advances cursor without a tile");
        text.Step(bus, vram);
        text.Step(bus, vram);
        AssertEqual(1, text.GlyphsWritten, "delay applies before next visible glyph");
        text.Step(bus, vram);
        AssertEqual((ushort)(EscapeTypewriterRomData.ZebesTileBase + 26), vram.ReadWord(0x4802), "exclamation uses native remap");
        AssertTrue(text.ClickRequested, "second visible glyph clicks despite intervening space");
        text.Step(bus, vram); text.Step(bus, vram);
        AssertTrue(text.Step(bus, vram), "typewriter completes only at terminator after delay");

        ushort[] foreground = Enumerable.Repeat((ushort)0x8123, 256).ToArray();
        var level = new RoomLevelData(16, 16, foreground, new byte[256], new ushort[256], new byte[8192]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnMotherBrainMutation(level, 0, 6, RoomPlmHeaders.MotherBrainsRoomEscapeDoor), "escape PLM allocates");
        AssertEqual((ushort)0x9123, level.GetCollisionBlock(0, 6).LevelWord, "door setup preserves visual bits");
        AssertEqual((byte)1, level.GetCollisionBlock(0, 6).Behavior, "door setup selects second door header");
        for (int row = 7; row < 10; row++)
        {
            AssertEqual((ushort)0xd123, level.GetCollisionBlock(0, row).LevelWord, "extension preserves visual bits");
            AssertEqual(byte.MaxValue, level.GetCollisionBlock(0, row).Behavior, "extension links one block upward");
        }
        AssertEqual((ushort)0x8123, level.GetCollisionBlock(0, 10).LevelWord, "door setup does not overrun four blocks");
        Console.WriteLine("  Mother Brain escape: typewriter glyph/delay/click semantics and native door setup agree.");
    }
}
