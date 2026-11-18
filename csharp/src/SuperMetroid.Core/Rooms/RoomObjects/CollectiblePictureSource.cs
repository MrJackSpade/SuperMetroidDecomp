namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where a permanent item's picture comes from where it lies (#1452, #1538, #2481). Its values are arbitrary.</summary>
internal enum CollectiblePictureSource
{
    /// <summary>One of the four tanks, whose pictures are blocks every room loads.</summary>
    SharedTankBlocks,

    /// <summary>An item that shows a picture of its own, drawn into the block pair it takes as it is placed.</summary>
    OwnPicture,
}
