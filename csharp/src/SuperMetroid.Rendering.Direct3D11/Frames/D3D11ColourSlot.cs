using SuperMetroid.Core.Assets;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>How a packed picture is coloured (#1408).</summary>
internal enum D3D11PictureColouring
{
    /// <summary>In the colours the frame's background art shows.</summary>
    Frame,
    /// <summary>In the message box's text colours over the frame's.</summary>
    MessageText,
}

/// <summary>An image the scene's pictures are cut from and how they are coloured: one colour table each frame (#1408).</summary>
/// <param name="Sheet">The image.</param>
/// <param name="Colouring">How its pictures are coloured.</param>
internal readonly record struct D3D11ColourSlot(SpriteImage Sheet, D3D11PictureColouring Colouring);
