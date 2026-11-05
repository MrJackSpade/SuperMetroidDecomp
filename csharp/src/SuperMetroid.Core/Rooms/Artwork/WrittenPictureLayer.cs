namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>The backgrounds a written picture sheet pictures for (#1392, #1466).</summary>
public enum WrittenPictureLayer
{
    /// <summary>The foreground and the background: sixteen-colour pictures written over a room's backgrounds, such as typed text.</summary>
    WrittenOverRoom,

    /// <summary>The effect overlay: four-colour pictures, the HUD's images and a room effect's page.</summary>
    FxOverlayAndHud,
}
