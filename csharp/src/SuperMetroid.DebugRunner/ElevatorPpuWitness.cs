using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;

/// <summary>Local-only memory input for the independent #516 PPU comparison.</summary>
internal static class ElevatorPpuWitness
{
    public static void InspectTerrain(SuperMetroidRuntime runtime, LayeredRenderSnapshot packet, int x, int y)
    {
        var r = ((OrdinaryGameplayRenderLayer)packet.Layers[0]).Registers;
        var level = runtime.LevelData!;
        int worldX = runtime.DisplayedGameplayPpu.Layer1XPosition + x;
        int worldY = runtime.DisplayedGameplayPpu.Layer1YPosition + y + SnesPpuLayout.FirstVisibleBackgroundScanline;
        int block = level.GetBlockIndex(worldX / 16, worldY / 16);
        var tiles = LevelBlockTilemapExpander.Expand(level.ForegroundEntries.Span[block], level.BlockDefinitions.Span);
        ushort expected = ((worldY & 8) != 0, (worldX & 8) != 0) switch {
            (false, false) => tiles.TopLeft, (false, true) => tiles.TopRight,
            (true, false) => tiles.BottomLeft, _ => tiles.BottomRight };
        int tx = ((r.Bg1X + x) & 511) / 8;
        int ty = ((r.Bg1Y + y + SnesPpuLayout.FirstVisibleBackgroundScanline) & 255) / 8;
        int word = SnesPpuLayout.GameplayBg1TilemapWord +
            (tx / 32) * SnesPpuLayout.TilemapPageWordCount + ty * 32 + (tx & 31);
        ushort actual = BinaryPrimitives.ReadUInt16LittleEndian(packet.Memory.Vram.Slice(word * 2, 2));
        uint tileHash = 2166136261u; // FNV-1a; matches the independent native asset probe.
        int tileByte = r.Bg1CharacterWord * 2 + (actual & 1023) * 32;
        foreach (byte value in packet.Memory.Vram.Slice(tileByte, 32))
            tileHash = unchecked((tileHash ^ value) * 16777619u);
        Console.WriteLine($"Terrain witness: world=({worldX},{worldY}), block={block}, " +
            $"level={level.ForegroundEntries.Span[block]:X4}, expectedTile={expected:X4}, " +
            $"VRAM[{word:X4}]={actual:X4}, tileHash={tileHash:X8}, BG1=({r.Bg1X},{r.Bg1Y}).");
        if (actual != expected)
            throw new InvalidDataException("Elevator witness BG1 ring does not match the corresponding live level block.");
    }

    public static void Write(string path, LayeredRenderSnapshot packet)
    {
        if (packet.Layers.Length != 1 || packet.Layers[0] is not OrdinaryGameplayRenderLayer layer ||
            !layer.HorizontalScrolls.IsEmpty || !layer.VerticalScrolls.IsEmpty)
            throw new InvalidDataException("Elevator PPU witness requires a single unmodified ordinary gameplay layer.");
        var r = layer.Registers;
        if (r.Bg2FirstScanline != 32 || r.Bg2EndScanline != 224 ||
            r.MainScreenWindowMask != SuperMetroid.Core.Hardware.SnesMainScreenLayers.None)
            throw new InvalidDataException("Elevator independent-PPU witness does not model custom layer windows.");
        using var writer = new BinaryWriter(File.Create(path));
        writer.Write(packet.Memory.Vram);
        foreach (ushort color in packet.Memory.Cgram) writer.Write(color);
        writer.Write(packet.Memory.Oam);
        foreach (ushort word in new ushort[] { r.Bg1X, r.Bg1Y, r.Bg2X, r.Bg2Y,
            (ushort)r.Bg2WidthTiles, (ushort)r.Bg2HeightTiles, r.Bg2TilemapWord,
            r.Bg1CharacterWord, r.Bg2CharacterWord, r.HudCharacterWord,
            (ushort)r.MainScreenLayers, packet.ObjectSelection, packet.Brightness })
            writer.Write(word);
    }
}
