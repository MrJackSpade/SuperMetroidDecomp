using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification's native view of the permanent-item artwork: the bank-$89 address of each block.</summary>
internal static class RoomObjectDynamicCollectibleGraphicsNative
{
    /// <summary>The item graphics in the order bank $89 stores their 256-byte blocks, from $89:8000 (ItemPLMGFX_Bombs on).</summary>
    private static readonly RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic[] StoredOrder =
    [
        RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.Bombs, RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.GravitySuit,
        RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.SpringBall, RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.VariaSuit,
        RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.HiJumpBoots, RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.ScrewAttack,
        RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.SpaceJump, RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.MorphBall,
        RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.GrappleBeam, RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.XrayScope,
        RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.SpeedBooster, RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.ChargeBeam,
        RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.IceBeam, RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.WaveBeam,
        RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.PlasmaBeam, RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.Spazer,
        RoomObjectDynamicCollectibleGraphicsDefinitions.ItemGraphic.ReserveTank,
    ];

    /// <summary>Item artwork by bank-$89 address.</summary>
    extension(RoomObjectDynamicCollectibleGraphicsDefinitions)
    {
        /// <summary>$89:8000 onward: the bank-$89 address of <paramref name="kind"/>'s 256-byte artwork block.</summary>
        internal static ushort GraphicsPointer(InWorldCollectibleKind kind) =>
            checked((ushort)(0x8000 + Array.IndexOf(StoredOrder, DynamicCollectibleGraphics.GraphicFor(kind)) * 0x100));
    }

    /// <summary>A graphic's bank-$89 address.</summary>
    extension(RoomObjectDynamicCollectibleGraphic graphic)
    {
        /// <summary>The bank-$89 address of the graphic's artwork block.</summary>
        internal ushort GraphicsPointer => RoomObjectDynamicCollectibleGraphicsDefinitions.GraphicsPointer(graphic.Kind);
    }
}
