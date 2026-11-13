namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The display layer a written picture sheet's pictures show on (#1392, #1466, #1601, #1731), which the written picture
/// manifest names each sheet's layer by.
/// </summary>
public enum WrittenPictureLayer
{
    /// <summary>The room's foreground and background: pictures an encounter writes over them, such as a warning's typed text.</summary>
    WrittenOverRoom,

    /// <summary>The effect overlay in front of the room: the room effect's liquid or atmosphere.</summary>
    FxOverlay,
}
