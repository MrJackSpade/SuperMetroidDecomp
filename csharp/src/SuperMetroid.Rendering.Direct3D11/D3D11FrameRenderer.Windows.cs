using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    /// <summary>Packs one enabled SNES BG1–BG4 or OBJ window target's selection, logic, and edge registers into shader constants.</summary>
    /// <param name="data">Destination shader-constant words updated for an enabled target.</param>
    /// <param name="windows">Literal PPU window selections, logic operations, and horizontal edges.</param>
    /// <param name="enabled">Main-screen layer mask that determines whether this target is admitted.</param>
    /// <param name="target">Background or OBJ target whose register fields are packed.</param>
    /// <exception cref="ArgumentOutOfRangeException">The target is outside BG1 through OBJ.</exception>
    private static void SetWindowConstants(Span<uint> data, SnesWindowRegisters windows,
        SnesMainScreenLayers enabled, SnesWindowTarget target)
    {
        int index = (int)target;
        if (index > (int)SnesWindowTarget.Obj)
            throw new ArgumentOutOfRangeException(nameof(target));
        if ((enabled & (SnesMainScreenLayers)(1 << index)) == 0) return;
        byte selection = target switch
        {
            SnesWindowTarget.Bg1 or SnesWindowTarget.Bg2 => windows.Window12Selection,
            SnesWindowTarget.Bg3 or SnesWindowTarget.Bg4 => windows.Window34Selection,
            _ => windows.ObjectColorSelection,
        };
        data[D3D11ShaderLayout.WindowSelectionWord] = (uint)(selection >> ((index & 1) * 4)) & 15;
        data[D3D11ShaderLayout.WindowLogicWord] = (uint)(target == SnesWindowTarget.Obj
            ? windows.ObjectColorLogic : windows.BackgroundLogic >> (index * 2)) & 3;
        data[D3D11ShaderLayout.WindowEdgesWord] = (uint)(windows.FirstLeft | windows.FirstRight << 8 |
            windows.SecondLeft << 16 | windows.SecondRight << 24);
        data[D3D11ShaderLayout.WindowEnabledWord] = 1;
    }
}
