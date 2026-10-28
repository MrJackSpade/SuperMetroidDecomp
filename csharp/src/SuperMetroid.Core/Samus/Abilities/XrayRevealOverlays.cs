using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Ports uncollected-item and room-special reveal writes after the terrain pass.</summary>
public static class XrayRevealOverlays
{
    /// <summary>Applies $84:831A in descending PLM slot order, then room-authored record order.</summary>
    public static void Apply(RoomLevelData level, Span<BackgroundTile> tilemap,
        IReadOnlyList<CollectiblePlmSnapshot> items, SharedGameState system,
        XrayRoomOverlayDefinition? roomOverlay, Position16<RoomPixels> layer1Position,
        XrayRevealVisualCatalog? visuals = null)
    {
        Ensure.LengthEqual(tilemap, XrayTilemapLayout.BufferWords);
        if (visuals is null)
            throw new InvalidOperationException("X-ray reveal requires installed overlay visuals.");
        XrayOverlayVisualCatalog overlays = visuals.Overlays ?? throw new InvalidOperationException(
            "X-ray reveal requires installed item and room-overlay definitions.");
        for (int i = items.Count - 1; i >= 0; i--)
        {
            CollectiblePlmSnapshot item = items[i];
            if (unchecked((short)item.RoomArgument) < 0 || system.HasCollectedItemBit(item.RoomArgument)) continue;
            // The four tanks draw from the dynamic slots that follow the loaded item graphics, in tank order.
            int graphics = item.Kind switch
            {
                InWorldCollectibleKind.EnergyTank => XrayOverlayVisualCatalog.DynamicGraphicsSlots,
                InWorldCollectibleKind.MissileTank => XrayOverlayVisualCatalog.DynamicGraphicsSlots + 1,
                InWorldCollectibleKind.SuperMissileTank => XrayOverlayVisualCatalog.DynamicGraphicsSlots + 2,
                InWorldCollectibleKind.PowerBombTank => XrayOverlayVisualCatalog.DynamicGraphicsSlots + 3,
                InWorldCollectibleKind.Bombs or InWorldCollectibleKind.ChargeBeam or InWorldCollectibleKind.IceBeam or
                    InWorldCollectibleKind.HiJumpBoots or InWorldCollectibleKind.SpeedBooster or
                    InWorldCollectibleKind.WaveBeam or InWorldCollectibleKind.SpazerBeam or
                    InWorldCollectibleKind.SpringBall or InWorldCollectibleKind.VariaSuit or
                    InWorldCollectibleKind.GravitySuit or InWorldCollectibleKind.XrayScope or
                    InWorldCollectibleKind.PlasmaBeam or InWorldCollectibleKind.GrappleBeam or
                    InWorldCollectibleKind.SpaceJump or InWorldCollectibleKind.ScrewAttack or
                    InWorldCollectibleKind.MorphBall or InWorldCollectibleKind.ReserveTank => item.GraphicsSlot,
                _ => throw new InvalidDataException($"Item {item.Header} has undefined collectible kind {item.Kind}."),
            };
            if ((uint)graphics >= XrayOverlayVisualCatalog.DynamicGraphicsSlots * 2)
                throw new InvalidDataException($"Item {item.Header} has invalid X-ray graphics slot {graphics}.");
            var metatile = new RoomLevelWord(overlays.ItemMetatile(graphics));
            Write(level, tilemap, metatile, level.Size.CellAtRowMajorIndex(item.BlockIndex), layer1Position);
        }
        if (roomOverlay is null) return;
        foreach (XrayRoomOverlayVisual tile in overlays.RoomTiles(roomOverlay))
            Write(level, tilemap, tile.Metatile, tile.Block, layer1Position);
    }

    /// <summary>Copies a visible metatile's four tiles, swapping rows for native vertical flip without changing each tile's mirroring.</summary>
    private static void Write(RoomLevelData level, Span<BackgroundTile> tilemap, RoomLevelWord metatileWord, Position16<RoomBlocks> block,
        Position16<RoomPixels> layer1Position)
    {
        // The overlay's metatile within the visible window, relative to the camera's block.
        Offset16<RoomBlocks> visible = block - layer1Position.ToRoomBlock();
        int x = visible.X, y = visible.Y;
        // $91:D04C admits only the first 16x16 metatiles, not the fine-scroll column.
        if ((uint)x >= XrayTilemapLayout.MetatileColumns - 1 || (uint)y >= XrayTilemapLayout.MetatileRows) return;
        int metatile = metatileWord.VisualBlockIndex;
        int source = metatile * RoomLevelPlane.DefinitionTiles;
        ReadOnlySpan<BackgroundTile> definitions = level.BlockDefinitions.Span;
        if (source + RoomLevelPlane.DefinitionTiles > definitions.Length)
            throw new InvalidDataException($"X-ray overlay metatile ${metatile:X4} is outside room definitions.");
        int destination = y * XrayTilemapLayout.MetatileRowStride + x * 2;
        // $91:D0A6 swaps the two tile rows for vertical flip. It neither toggles each
        // tile's mirroring nor implements horizontal flip; preserve that native behavior.
        bool flip = (metatileWord.VisualFlipFlags & LevelBlockFlipFlags.Vertical) != LevelBlockFlipFlags.None;
        for (int row = 0; row < 2; row++)
        for (int col = 0; col < 2; col++)
        {
            tilemap[destination + row * 32 + col] = definitions[source + (flip ? 1 - row : row) * 2 + col];
        }
    }

}
