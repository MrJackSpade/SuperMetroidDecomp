using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using System.Buffers.Binary;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.IntegrationVerification.Gameplay;

/// <summary>Compares the displayed tube tilemap words with the authored room allocation.</summary>
internal static class MaridiaPipeTerrainAudit
{
    /// <summary>Compares visible BG1/BG2 VRAM words with expanded authored tube blocks and reports the first mismatch.</summary>
    public static void Observe(SuperMetroidRuntime runtime, DisplayPacket snapshot, int frame, bool requireAuthoredTerrain = false)
    {
        if (runtime.Room.ActiveRoom?.Definition != RoomHeaderDefinitions.Toilet || snapshot.Layers is not { } layers) return;
        var gameplay = layers.LayersOf<GameplaySceneLayer>().Single();
        var r = gameplay.View;
        var scroll = runtime.BackgroundScroll;
        var level = runtime.Room.LoadedLevelData;
        // The frame no longer carries VRAM (#1403); the audit reads the tilemaps gameplay's VRAM holds now.
        byte[] vram = runtime.ComposedVram();
        // The room's blocks as the cartridge composes them; extraction pictures them from the same compositions (#1431).
        SelectedRoom room = runtime.Room.ActiveRoom ?? throw new InvalidOperationException("The tube audit observed no room.");
        BackgroundTile[] blockTiles = InstalledFixtures.StockBlockTiles(RoomTilesets.Get(room.State.GraphicsSet), ceres: false);
        foreach (bool background in new[] { false, true })
        {
            if (background && !(scroll.Layer2Scroll.Horizontal.Streams && scroll.Layer2Scroll.Vertical.Streams)) continue;
            // The audit compares wrapped register words, so take each offset as its native word.
            int offsetX = unchecked((ushort)(background ? scroll.Bg2Offset.X : scroll.Bg1Offset.X));
            int offsetY = unchecked((ushort)(background ? scroll.Bg2Offset.Y : scroll.Bg1Offset.Y));
            int registerX = background ? r.BackgroundScroll.X : r.ForegroundScroll.X;
            int registerY = background ? r.BackgroundScroll.Y : r.ForegroundScroll.Y;
            int tilemap = background ? GameplayVramLayout.Bg2TilemapWord : GameplayVramLayout.Bg1TilemapWord;
            ReadOnlySpan<ushort> source = background ? level.BackgroundEntries : level.ForegroundEntries.Span;
            int differences = 0;
            var rows = new SortedSet<int>();
            string first = "";
            for (int screenY = 32; screenY < 224; screenY += 8)
            for (int screenX = 0; screenX < 256; screenX += 8)
            {
                int mapX = (registerX + screenX) & 511;
                int mapY = unchecked((byte)(registerY + screenY + 1));
                int worldX = unchecked((ushort)(registerX - offsetX + screenX));
                int worldY = unchecked((ushort)(registerY - offsetY + screenY + 1));
                int blockX = worldX / 16, blockY = worldY / 16;
                if (blockX >= level.Size.X || blockY >= level.Size.Y) continue;
                var tiles = LevelBlockTilemapExpander.Expand(source[blockY * level.Size.X + blockX], blockTiles);
                ushort expected = BackgroundTileWords.ToWord((worldY & 8) == 0
                    ? (worldX & 8) == 0 ? tiles.TopLeft : tiles.TopRight
                    : (worldX & 8) == 0 ? tiles.BottomLeft : tiles.BottomRight);
                int address = tilemap + (mapX >= 256 ? 1024 : 0) + (mapY / 8) * 32 + ((mapX / 8) & 31);
                ushort actual = BinaryPrimitives.ReadUInt16LittleEndian(vram.AsSpan(address * 2));
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
