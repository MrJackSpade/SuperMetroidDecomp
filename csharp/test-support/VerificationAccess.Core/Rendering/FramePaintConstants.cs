using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Captures of <see cref="FramePaint"/> from native colour memory that only verification makes (#1512).</summary>
internal static class FramePaintConstants
{
    /// <summary>
    /// Captures the displayed frame's whole-frame sprite draws painted from <paramref name="cgram"/>, a native 256-colour
    /// image, as <see cref="FramePaintAccess"/>'s native frames paint them.
    /// </summary>
    public static FramePaint Capture(IPaletteColors cgram, SpriteFrameBuffer sprites)
    {
        Ensure.NotNull(cgram);
        Ensure.NotNull(sprites);
        return FramePaint.FromNative(cgram.Colors, sprites.Draws);
    }

    /// <summary>
    /// Captures a frontend screen's memories, converting its OAM storage into the draws it stands for, painted from its
    /// native colours.
    /// </summary>
    /// <param name="vram">The screen's VRAM.</param>
    /// <param name="cgram">The screen's CGRAM, a native 256-colour image.</param>
    /// <param name="oam">The screen's finalized OAM storage.</param>
    /// <param name="objectSelection">The screen's OBSEL value.</param>
    public static FramePaint CaptureFrontend(SnesVram vram, IPaletteColors cgram, OamBuffer oam, int objectSelection)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        return FramePaint.FromNative(cgram.Colors, ObjCharacterImageConstants.Draws(oam, vram, objectSelection));
    }
}
