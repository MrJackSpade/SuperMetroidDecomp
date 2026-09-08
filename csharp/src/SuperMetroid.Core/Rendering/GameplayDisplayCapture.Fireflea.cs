using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Rendering;

public static partial class GameplayDisplayCapture
{
    /// <summary>Normal Fireflea blending ($88:80B0), before and after X-ray owns the window.</summary>
    private static XrayGameplayRenderLayer CaptureFirefleaDarkness(SuperMetroidRuntime runtime, LayeredRenderSnapshot basis)
    {
        if (basis.Layers[0] is not OrdinaryGameplayRenderLayer ordinary)
            throw new InvalidOperationException("Fireflea darkness requires ordinary Mode-1 gameplay.");
        // The source-aware operation is shared with X-ray, but an empty window means
        // every pixel is eligible. Native CGADSUB enables only BG2 and the backdrop:
        // foreground and Samus must not be tinted by a whole-frame darkening overlay.
        var closed = Enumerable.Repeat(new XrayWindowLine(255, 0), SnesPpuLayout.ScreenHeightPixels).ToArray();
        return new(ordinary, closed, revealBlocks: false,
            SnesColorMathControl.Bg2 | SnesColorMathControl.Backdrop | SnesColorMathControl.Subtract,
            addSubscreen: false,
            Fixed(PpuFixedColorMirrors.Red), Fixed(PpuFixedColorMirrors.Green), Fixed(PpuFixedColorMirrors.Blue));

        byte Fixed(int address) => (byte)(runtime.AddressSpace.ReadByte(address) & 31);
    }
}
