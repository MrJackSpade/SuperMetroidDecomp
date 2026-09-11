using SuperMetroid.Core.Rendering;

/// <summary>Local-only memory input for the independent #516 PPU comparison.</summary>
internal static class ElevatorPpuWitness
{
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
