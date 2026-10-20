using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="PpuMemorySnapshot"/> that only verification reads.</summary>
internal static class PpuMemorySnapshotConstants
{
    /// <summary>Captures currently published memories and the displayed frame's whole-frame sprite draws.</summary>
    public static PpuMemorySnapshot Capture(SnesVram vram, IPaletteColors cgram, SpriteFrameBuffer sprites)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Ensure.NotNull(sprites);
        return new(vram.CaptureImage(), cgram.Colors, sprites.Draws);
    }
    /// <summary>Captures a frontend screen's memories, converting its OAM storage into the draw it stands for.</summary>
    /// <param name="vram">The screen's VRAM.</param>
    /// <param name="cgram">The screen's CGRAM.</param>
    /// <param name="oam">The screen's finalized OAM storage.</param>
    /// <param name="objectSelection">The screen's OBSEL value.</param>
    public static PpuMemorySnapshot CaptureFrontend(SnesVram vram, IPaletteColors cgram, OamBuffer oam, int objectSelection)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        return new(vram.CaptureImage(), cgram.Colors, ObjCharacterImageConstants.Draws(oam, vram, objectSelection));
    }
}
