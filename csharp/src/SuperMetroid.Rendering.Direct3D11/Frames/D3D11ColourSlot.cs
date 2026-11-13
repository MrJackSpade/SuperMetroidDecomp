using SuperMetroid.Core.Assets;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>How a packed picture is coloured (#1408, #1512).</summary>
internal enum D3D11PictureColouring
{
    /// <summary>In the shading the picture names itself, as a frontend screen's pictures and floors do.</summary>
    OwnShading,
    /// <summary>In the shading of the gameplay room's pictures.</summary>
    Room,
    /// <summary>In the shading of the pictures written over the room's layers.</summary>
    WrittenOverRoom,
    /// <summary>In the shading of the room a door leaves.</summary>
    DoorSourceRoom,
    /// <summary>In the shading of the HUD's pictures.</summary>
    Hud,
    /// <summary>In the shading of the room effect's effect overlay pictures.</summary>
    FxOverlay,
    /// <summary>In the shading of the message box's pictures.</summary>
    MessageBox,
    /// <summary>In the shading of the boss body on the background.</summary>
    BossBody,
}

/// <summary>An image the scene's pictures are cut from and how they are coloured: one colour table each frame (#1408).</summary>
/// <param name="Sheet">The image.</param>
/// <param name="Colouring">The gameplay picture group whose shading its pictures show while they name none of their own.</param>
/// <param name="Shading">
/// The shades and colour effects its pictures show (#1512), or null for <paramref name="Colouring"/>'s group's; a picture
/// coloured in <see cref="D3D11PictureColouring.OwnShading"/> must name one.
/// </param>
internal readonly record struct D3D11ColourSlot(SpriteImage Sheet, D3D11PictureColouring Colouring, PictureShading? Shading = null)
{
    /// <summary>
    /// The group of gameplay pictures <paramref name="colouring"/> colours from its frame's shading (#1512), or null for a
    /// picture that names its own shading.
    /// </summary>
    /// <exception cref="InvalidDataException">The colouring is undefined.</exception>
    internal static GameplayPictures? GameplayGroupOf(D3D11PictureColouring colouring) => colouring switch
    {
        D3D11PictureColouring.OwnShading => null,
        D3D11PictureColouring.Room => GameplayPictures.Room,
        D3D11PictureColouring.WrittenOverRoom => GameplayPictures.WrittenOverRoom,
        D3D11PictureColouring.DoorSourceRoom => GameplayPictures.DoorSourceRoom,
        D3D11PictureColouring.Hud => GameplayPictures.Hud,
        D3D11PictureColouring.FxOverlay => GameplayPictures.FxOverlay,
        D3D11PictureColouring.MessageBox => GameplayPictures.MessageBox,
        D3D11PictureColouring.BossBody => GameplayPictures.BossBody,
        _ => throw new InvalidDataException($"Unknown picture colouring {colouring}."),
    };
}
