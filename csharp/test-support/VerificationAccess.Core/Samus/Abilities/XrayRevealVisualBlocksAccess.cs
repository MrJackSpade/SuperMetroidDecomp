using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Builds the X-ray reveal's visual blocks from the room blocks a check names by number.</summary>
internal static class XrayRevealVisualBlocksAccess
{
    /// <summary>Builds the blocks.</summary>
    extension(XrayRevealVisualBlocks)
    {
        /// <summary>One block's picture.</summary>
        internal static XrayRevealVisualBlocks One(int block) => XrayRevealVisualBlocks.One(RoomCellPicture.OfBlock(block));

        /// <summary>A square of four blocks' pictures.</summary>
        internal static XrayRevealVisualBlocks Square(int topLeft, int topRight, int bottomLeft, int bottomRight) =>
            XrayRevealVisualBlocks.Square(RoomCellPicture.OfBlock(topLeft), RoomCellPicture.OfBlock(topRight),
                RoomCellPicture.OfBlock(bottomLeft), RoomCellPicture.OfBlock(bottomRight));

        /// <summary>Two blocks' pictures side by side.</summary>
        internal static XrayRevealVisualBlocks Wide(int left, int right) =>
            XrayRevealVisualBlocks.Wide(RoomCellPicture.OfBlock(left), RoomCellPicture.OfBlock(right));
    }
}
