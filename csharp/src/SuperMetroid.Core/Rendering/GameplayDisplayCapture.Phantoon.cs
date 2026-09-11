using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Rendering;

public static partial class GameplayDisplayCapture
{
    private static RenderLayer CapturePhantoonBlending(SuperMetroidRuntime runtime, LayeredRenderSnapshot basis)
    {
        var ordinary = (OrdinaryGameplayRenderLayer)basis.Layers[0];
        var mode = runtime.Enemies.Phantoon!.Blending.DisplayedConfiguration;
        if (mode is not (LayerBlendingConfiguration.PhantoonHidden or LayerBlendingConfiguration.PhantoonSemiTransparent))
            return ordinary;
        var gameplay = new OrdinaryGameplayRenderLayer(ordinary.Registers with
        {
            MainScreenLayers = ordinary.Registers.MainScreenLayers & ~SnesMainScreenLayers.Bg2,
        }, ordinary.HorizontalScrolls, ordinary.VerticalScrolls);
        if (mode == LayerBlendingConfiguration.PhantoonHidden) return gameplay;
        // $88:80D9: TM=$15, TS=$02, CGADSUB=$35. Gameplay HUD is the separate
        // IRQ band; below it BG2 adds to eligible main sources, never occluding BG1.
        return new XrayGameplayRenderLayer(gameplay,
            Enumerable.Repeat(new XrayWindowLine(255, 0), SnesPpuLayout.ScreenHeightPixels).ToArray(), false,
            SnesColorMathControl.Bg1 | SnesColorMathControl.Bg3 | SnesColorMathControl.Obj | SnesColorMathControl.Backdrop,
            true, 0, 0, 0, subscreenUsesBg2: true);
    }
}
