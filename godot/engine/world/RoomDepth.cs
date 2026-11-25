namespace Engine.World;

/// <summary>
/// Where what a room shows is drawn against the rest of it, from the back to the front: the depths a room's
/// pictures lie at. Things at one depth are drawn in the order they stand in the room. A thing with a place of its
/// own against the room's tiles takes one of these in place of the depth of the room's things.
/// </summary>
public static class RoomDepth
{
    /// <summary>The picture behind the room.</summary>
    public const int Backdrop = -100;

    /// <summary>In front of the backdrop and behind the room's tiles and everything standing in the room.</summary>
    public const int BehindRoom = -1;

    /// <summary>The room's tiles and, after them, everything standing in the room: the depth a thing has unless it says otherwise.</summary>
    public const int Things = 0;

    /// <summary>The part of the room's tiles that lies in front of the things standing in it.</summary>
    public const int TilesInFront = 20;

    /// <summary>In front of every tile of the room, the ones in front of its things too.</summary>
    public const int InFrontOfRoom = 30;

    /// <summary>The room's liquid: over the room and what is in it, under the overlays.</summary>
    public const int Liquid = 90;

    /// <summary>What is laid over the whole room.</summary>
    public const int Overlay = 100;
}
