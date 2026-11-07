using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification's native view of the permanent-item artwork: the bank-$89 address of each block.</summary>
internal static class RoomObjectDynamicCollectibleGraphicsNative
{
    /// <summary>Item artwork by bank-$89 address.</summary>
    extension(RoomObjectDynamicCollectibleGraphicsDefinitions)
    {
        /// <summary>$89:8000 onward: the bank-$89 address of <paramref name="kind"/>'s 256-byte artwork block.</summary>
        internal static ushort GraphicsPointer(InWorldCollectibleKind kind) =>
            checked((ushort)(0x8000 + DynamicCollectibleGraphics.GraphicIndex(kind) * 0x100));
    }

    /// <summary>A graphic's bank-$89 address.</summary>
    extension(RoomObjectDynamicCollectibleGraphic graphic)
    {
        /// <summary>The bank-$89 address of the graphic's artwork block.</summary>
        internal ushort GraphicsPointer => RoomObjectDynamicCollectibleGraphicsDefinitions.GraphicsPointer(graphic.Kind);
    }
}
