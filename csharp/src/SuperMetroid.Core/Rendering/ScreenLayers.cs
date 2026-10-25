namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The layers a screen shows (#1407): any combination of the foreground, the background, the overlay and the sprites.
/// The gameplay screen shows the foreground, the background and the sprites; a door transition shows the sprites, with
/// or without the foreground; the waterfall rooms show the waterfall and the liquid only through blending.
/// </summary>
/// <remarks>
/// The members combine freely. The Direct3D 11 backend uploads a combination as the bits below, which its shaders
/// test layer by layer.
/// </remarks>
[Flags]
public enum ScreenLayers : byte
{
    /// <summary>No layer and no sprites.</summary>
    None = 0,
    /// <summary>The room's foreground.</summary>
    Foreground = 0x01,
    /// <summary>The room's background, or a boss body drawn as a background.</summary>
    Background = 0x02,
    /// <summary>The overlay: the HUD, and a room's liquid, fog or haze effect.</summary>
    Overlay = 0x04,
    /// <summary>The sprites: Samus, enemies, projectiles and the other drawn objects.</summary>
    Sprites = 0x10,
}
