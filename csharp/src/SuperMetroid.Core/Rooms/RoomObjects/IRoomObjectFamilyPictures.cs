namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One room-object catalog's installed pictures (#1557, #2034), each under the draw that shows it (#2571). World composes
/// the room's drawing from every family's catalog when it builds the room's scope, so a draw finds its picture by what it
/// draws and no family resolves its catalog by type. A catalog lists every draw of its families that shows a picture,
/// including the draws that show another draw's picture as the family's own rule says.
/// </summary>
internal interface IRoomObjectFamilyPictures
{
    /// <summary>Each picture of the catalog's families, under the draw that shows it.</summary>
    IEnumerable<KeyValuePair<RoomObjectDraw, RoomObjectFrameImage>> Pictures { get; }
}
