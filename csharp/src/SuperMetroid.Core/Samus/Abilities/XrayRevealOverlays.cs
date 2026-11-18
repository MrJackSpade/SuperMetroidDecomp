using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Runs uncollected-item and room-special reveal writes after the terrain pass.</summary>
public static class XrayRevealOverlays
{
    /// <summary>
    /// Applies the item overlays in descending room object order, then room-authored record order. A tank shows its
    /// installed reveal block; every other item shows its kind's own picture's first frame (#1538), whatever was set up
    /// before it.
    /// </summary>
    /// <param name="blocks">The revealed blocks, as <see cref="XrayRevealMap.Build"/> builds them.</param>
    /// <param name="items">The room's collectibles.</param>
    /// <param name="system">The collected-item record.</param>
    /// <param name="roomOverlay">The room state's special overlay, or null.</param>
    /// <param name="foregroundPosition">Camera position, which places the overlays on screen.</param>
    /// <param name="visuals">Installed reveal art.</param>
    public static void Apply(XrayRevealGrid blocks,
        IReadOnlyList<CollectibleObjectSnapshot> items, SharedGameState system,
        XrayRoomOverlayDefinition? roomOverlay, Position16<RoomPixels> foregroundPosition,
        InstalledXrayReveals visuals)
    {
        Ensure.NotNull(blocks);
        InstalledXrayOverlays overlays = Ensure.NotNull(visuals).Overlays;
        for (int i = items.Count - 1; i >= 0; i--)
        {
            CollectibleObjectSnapshot item = items[i];
            if (system.HasCollectedItem(item.Location)) continue;
            RevealedBlock revealed = CollectibleDefinitions.Items[item.Kind].Picture == CollectiblePictureSource.OwnPicture
                ? new RevealedBlock(RoomCellPicture.Of(new CollectiblePicture(item.Kind, CollectibleLook.Primary)), RevealedBlockArrangement.AsDefined)
                : Arranged(overlays.TankBlock(item.Kind));
            Write(blocks, revealed, item.Block, foregroundPosition);
        }
        if (roomOverlay is null) return;
        foreach (XrayRoomOverlayVisual visual in overlays.RoomBlocks(roomOverlay))
            Write(blocks, Arranged(visual.Appearance), visual.Block, foregroundPosition);
    }

    /// <summary>A visible overlay's whole block, mirrored as its appearance mirrors it (#2047).</summary>
    private static RevealedBlock Arranged(RoomBlockAppearance appearance) =>
        new(RoomCellPicture.Of(appearance), XrayRevealMap.Mirrored(appearance.Mirroring));

    /// <summary>Places an overlay's block where it lies, when that is inside the revealed area.</summary>
    private static void Write(XrayRevealGrid blocks, RevealedBlock revealed, Position16<RoomBlocks> block,
        Position16<RoomPixels> foregroundPosition)
    {
        // The overlay's block within the visible window, relative to the camera's block.
        Offset16<RoomBlocks> visible = block - foregroundPosition.ToRoomBlock();
        // Only an overlay inside the revealed area shows: one before the camera's block lies before the area's first.
        if (visible.X < 0 || visible.Y < 0) return;
        blocks.Show(Position16<RoomBlocks>.Of(visible.X, visible.Y), revealed);
    }
}
