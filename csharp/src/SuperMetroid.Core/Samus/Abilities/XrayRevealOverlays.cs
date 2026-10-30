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
    /// <param name="level">The room, whose size places the overlays' blocks.</param>
    /// <param name="blocks">The reveal's shown cells, as <see cref="XrayRevealMap.Build"/> lays them out.</param>
    /// <param name="items">The room's collectibles.</param>
    /// <param name="system">The collected-item record.</param>
    /// <param name="roomOverlay">The room state's special overlay, or null.</param>
    /// <param name="layer1Position">Layer one's position, which places the overlays on screen.</param>
    /// <param name="visuals">Installed reveal art.</param>
    public static void Apply(RoomLevelData level, Span<RevealedBlock?> blocks,
        IReadOnlyList<CollectiblePlmSnapshot> items, SharedGameState system,
        XrayRoomOverlayDefinition? roomOverlay, Position16<RoomPixels> layer1Position,
        InstalledXrayReveals? visuals = null)
    {
        Ensure.NotNull(level);
        Ensure.LengthEqual(blocks, XrayRevealLayout.Cells);
        if (visuals is null)
            throw new InvalidOperationException("X-ray reveal requires installed overlay visuals.");
        InstalledXrayOverlays overlays = visuals.Overlays ?? throw new InvalidOperationException(
            "X-ray reveal requires installed item and room-overlay definitions.");
        for (int i = items.Count - 1; i >= 0; i--)
        {
            CollectiblePlmSnapshot item = items[i];
            if (item.ItemBit is not { } itemBit || system.HasCollectedItemBit(itemBit)) continue;
            // The four tanks draw from the dynamic slots that follow the loaded item graphics, in tank order.
            int graphics = item.Kind switch
            {
                InWorldCollectibleKind.EnergyTank => InstalledXrayOverlays.DynamicGraphicsSlots,
                InWorldCollectibleKind.MissileTank => InstalledXrayOverlays.DynamicGraphicsSlots + 1,
                InWorldCollectibleKind.SuperMissileTank => InstalledXrayOverlays.DynamicGraphicsSlots + 2,
                InWorldCollectibleKind.PowerBombTank => InstalledXrayOverlays.DynamicGraphicsSlots + 3,
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
            if ((uint)graphics >= InstalledXrayOverlays.DynamicGraphicsSlots * 2)
                throw new InvalidDataException($"Item {item.Header} has invalid X-ray graphics slot {graphics}.");
            var metatile = new RoomLevelWord(overlays.ItemMetatile(graphics));
            Write(blocks, metatile, level.Size.CellAtRowMajorIndex(item.BlockIndex), layer1Position);
        }
        if (roomOverlay is null) return;
        foreach (XrayRoomOverlayVisual tile in overlays.RoomTiles(roomOverlay))
            Write(blocks, tile.Metatile, tile.Block, layer1Position);
    }

    /// <summary>
    /// Places a visible metatile's whole block, its rows swapped for native vertical flip without mirroring its picture.
    /// </summary>
    private static void Write(Span<RevealedBlock?> blocks, RoomLevelWord metatileWord, Position16<RoomBlocks> block,
        Position16<RoomPixels> layer1Position)
    {
        // The overlay's metatile within the visible window, relative to the camera's block.
        Offset16<RoomBlocks> visible = block - layer1Position.ToRoomBlock();
        int x = visible.X, y = visible.Y;
        // $91:D04C admits only the first 16x16 metatiles, not the fine-scroll column.
        if ((uint)x >= XrayRevealLayout.MetatileColumns - 1 || (uint)y >= XrayRevealLayout.MetatileRows) return;
        // $91:D0A6 swaps the two tile rows for vertical flip. It neither toggles each
        // tile's mirroring nor implements horizontal flip; preserve that native behavior.
        bool flip = (metatileWord.VisualFlipFlags & BlockMirroring.Vertical) != BlockMirroring.None;
        blocks[XrayRevealCells.CellOf(x, y)] = new RevealedBlock(metatileWord.VisualBlockIndex,
            flip ? RevealedBlockArrangement.RowsSwapped : RevealedBlockArrangement.AsDefined);
    }
}
