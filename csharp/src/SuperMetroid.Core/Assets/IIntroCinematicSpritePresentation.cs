using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Visual-only frame lookup for cartridge cinematic sprite actors. The actor still owns
/// its list, timing, position and clipping branch; the presentation supplies only OAM parts.
/// </summary>
public interface IIntroCinematicSpritePresentation
{
    /// <summary>Appends the selected cinematic composition in authored OAM order without advancing the actor's instruction list, timers, or motion.</summary>
    /// <param name="pointer">Bank-relative cinematic spritemap identity resolved by the selected presentation owner, not an instruction-list pointer.</param>
    /// <param name="oam">This frame's destination OAM buffer.</param>
    /// <param name="x">Native wrapped 16-bit screen-origin X coordinate in pixels.</param>
    /// <param name="y">Native wrapped 16-bit screen-origin Y coordinate in pixels.</param>
    /// <param name="paletteBits">Packed OBJ palette bits inherited by parts that do not specify their own palette.</param>
    /// <param name="originIsOnScreen">Selects the caller's ordinary origin-clipping path when true, or native off-screen Y-wrap path when false.</param>
    void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen);
}
