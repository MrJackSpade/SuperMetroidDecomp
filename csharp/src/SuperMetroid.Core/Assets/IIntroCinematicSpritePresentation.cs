using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Visual-only frame lookup for cartridge cinematic sprite actors. The actor still owns
/// its list, timing, position and clipping branch; the presentation supplies only OAM parts.
/// </summary>
public interface IIntroCinematicSpritePresentation
{
    void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen);
}
