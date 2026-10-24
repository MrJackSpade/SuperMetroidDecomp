namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The layers a screen shows (#1407): any combination of the four background layers and the sprites. The gameplay
/// screen shows the foreground, the background and the sprites; a door transition shows the sprites, with or without
/// the foreground; the waterfall rooms show the waterfall and the liquid only through blending.
/// </summary>
/// <remarks>
/// The members combine freely. The Direct3D 11 backend uploads a combination as the bits below, which its shaders
/// test layer by layer.
/// </remarks>
[Flags]
public enum ScreenLayers : byte
{
    /// <summary>No background layer and no sprites.</summary>
    None = 0,
    /// <summary>Background layer 1: the room's foreground.</summary>
    Bg1 = 0x01,
    /// <summary>Background layer 2: the room's background, or a boss body drawn as a background.</summary>
    Bg2 = 0x02,
    /// <summary>Background layer 3: the HUD, and a room's liquid, fog or haze effect.</summary>
    Bg3 = 0x04,
    /// <summary>Background layer 4.</summary>
    Bg4 = 0x08,
    /// <summary>The sprites: Samus, enemies, projectiles and the other drawn objects.</summary>
    Sprites = 0x10,
}
