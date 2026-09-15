/// <summary>Cartridge and fixture identities for the room-local suit-interruption audit.</summary>
internal static class FlashSuitCollectibleAuditData
{
    /// <summary>Room header <c>$8F:A6E2</c>, the cartridge-authored Varia Suit room.</summary>
    public const ushort VariaSuitRoomHeader = 0xa6e2;

    /// <summary>Fixed X coordinate that shares the placed Power Bomb and ordinary-bomb origin.</summary>
    public const ushort SamusX = 120;

    /// <summary>Grounded Y coordinate in the loaded Varia room's item chamber.</summary>
    public const ushort SamusY = 136;

    /// <summary>Camera Y used by the room-local fixture while the item chamber is visible.</summary>
    public const ushort CameraY = 8;

    /// <summary>
    /// First pre-pickup ordinary-bomb fuse that survives the two cartridge-ordered
    /// contact/message handoff frames and reaches the proven timer-eight overlap.
    /// </summary>
    public const int FirstSuccessfulPrePickupFuse = 10;

    /// <summary>
    /// Power Bomb afterglow offset that converts a requested pre-pickup fuse into the
    /// corresponding real ordinary-bomb placement frame; established by strict observation.
    /// </summary>
    public const int PowerBombPlacementFrameOffset = 93;
}
