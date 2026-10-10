using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Compares the displayed tube tilemap words with the authored room allocation.</summary>
internal static class MaridiaPipeTerrainAudit
{
    /// <summary>Compares visible tube-room BG1/BG2 tilemap words with the room's authored level data.</summary>
    /// <param name="runtime">Gameplay runtime providing the active room, scroll offsets, and level allocation.</param>
    /// <param name="snapshot">Captured frame whose VRAM tilemap words are checked.</param>
    /// <param name="frame">Sequence frame number used in diagnostics and periodic reporting.</param>
    /// <param name="requireAuthoredTerrain">Whether any displayed-versus-authored mismatch should fail the audit.</param>
    /// <exception cref="InvalidDataException">Strict checking is enabled and a visible terrain word differs from authored data.</exception>
    public static void Observe(SuperMetroidRuntime runtime, RenderFrameSnapshot snapshot, int frame, bool requireAuthoredTerrain = false)
    {
        if (runtime.ActiveRoom?.Pointer != MaridiaPipeFixtureDefinitions.TubeRoom || snapshot.Layers is not { } layers) return;
        var gameplay = layers.Layers.ToArray().OfType<OrdinaryGameplayRenderLayer>().Single();
        var r = gameplay.Registers;
        var scroll = runtime.BackgroundScroll;
        var level = runtime.LevelData!;
        foreach (bool background in new[] { false, true })
        {
            if (background && ((scroll.Layer2ScrollX | scroll.Layer2ScrollY) & 1) != 0) continue;
            int offsetX = background ? scroll.Bg2XOffset : scroll.Bg1XOffset;
            int offsetY = background ? scroll.Bg2YOffset : scroll.Bg1YOffset;
            int registerX = background ? r.Bg2X : r.Bg1X;
            int registerY = background ? r.Bg2Y : r.Bg1Y;
            int tilemap = background ? r.Bg2TilemapWord : SnesPpuLayout.GameplayBg1TilemapWord;
            var source = background ? level.BackgroundEntries.Span : level.ForegroundEntries.Span;
            int differences = 0;
            var rows = new SortedSet<int>();
            string first = "";
            for (int screenY = 32; screenY < 224; screenY += 8)
            for (int screenX = 0; screenX < 256; screenX += 8)
            {
                int mapX = (registerX + screenX) & 511;
                int mapY = (registerY + screenY + 1) & 255;
                int worldX = unchecked((ushort)(registerX - offsetX + screenX));
                int worldY = unchecked((ushort)(registerY - offsetY + screenY + 1));
                int blockX = worldX / 16, blockY = worldY / 16;
                if (blockX >= level.WidthInBlocks || blockY >= level.HeightInBlocks) continue;
                var tiles = LevelBlockTilemapExpander.Expand(source[blockY * level.WidthInBlocks + blockX], level.BlockDefinitions.Span);
                ushort expected = (worldY & 8) == 0
                    ? (worldX & 8) == 0 ? tiles.TopLeft : tiles.TopRight
                    : (worldX & 8) == 0 ? tiles.BottomLeft : tiles.BottomRight;
                int address = tilemap + (mapX >= 256 ? 1024 : 0) + (mapY / 8) * 32 + ((mapX / 8) & 31);
                ushort actual = BinaryPrimitives.ReadUInt16LittleEndian(layers.Memory.Vram[(address * 2)..]);
                if (expected == actual) continue;
                differences++;
                rows.Add(blockY);
                if (first.Length == 0) first = $"screen={screenX}/{screenY} world={worldX}/{worldY} vram={address:X4} expected={expected:X4} actual={actual:X4}";
            }
            if (differences != 0 || frame % 30 == 0)
                Console.WriteLine($"terrain frame={frame} bg={(background ? 2 : 1)} differences={differences} rows={string.Join(',', rows)} scroll={registerX}/{registerY} offset={offsetX}/{offsetY} {first}");
            if (requireAuthoredTerrain && differences != 0)
                throw new InvalidDataException($"#391: displayed BG{(background ? 2 : 1)} terrain differs from authored tube rows at frame {frame}: {first}");
        }
    }
}
