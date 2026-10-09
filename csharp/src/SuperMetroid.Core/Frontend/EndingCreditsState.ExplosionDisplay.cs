using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    /// <summary>Marks the finale composition with prioritized backgrounds and subscreen color addition as active.</summary>
    private bool explosionFinaleDisplay;

    /// <summary>Marks the earlier explosion-burst composition with eligible object color addition as active.</summary>
    private bool explosionBurstDisplay;

    /// <summary>Indicates that either explosion composition is selected before whiteout and planet-escape rendering.</summary>
    private bool UsesExplosionFinaleDisplay => (explosionFinaleDisplay || explosionBurstDisplay) && !ExplosionWhiteout
        && Phase < EndingCreditsPhase.PlanetEscapeFast;

    /// <summary>Captures the finale's prioritized backgrounds and the winning subscreen pixel with object color math.</summary>
    private LayeredRenderSnapshot CaptureExplosionFinale()
    {
        if (!explosionFinaleDisplay) return CaptureExplosionBurst();
        // F2FA: TM=BG1|BG2, TS=BG2|OBJ, CGADSUB=$33. Resolve the two
        // main backgrounds by priority, then add the single winning subscreen pixel.
        // OBJ is not independently drawn on main, which would punch holes in the glow.
        RenderLayer[] layers =
        [
            Plane(EndingExplosionDisplayDefinitions.FinaleSubMap, false),
            Plane(EndingExplosionDisplayDefinitions.FinaleMainMap, false),
            Plane(EndingExplosionDisplayDefinitions.FinaleSubMap, true),
            Plane(EndingExplosionDisplayDefinitions.FinaleMainMap, true),
            new BgSubscreenAddRenderLayer(EndingExplosionDisplayDefinitions.FinaleSubMap,
                EndingExplosionDisplayDefinitions.Characters, FourBpp: true, IncludeObjects: true,
                VerticalScroll: EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow),
        ];
        return new(PpuMemorySnapshot.Capture(vram, cgram, PrepareSprites()), layers,
            CurrentEscapeObjectSelection, brightness);

        static Bg4BppRenderLayer Plane(ushort map, bool high) => new(map,
            EndingExplosionDisplayDefinitions.Characters, 0, EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow, 32, 32, high);
    }

    /// <summary>Captures the initial burst map, prioritized object layers, and eligible subscreen color math.</summary>
    private LayeredRenderSnapshot CaptureExplosionBurst()
    {
        // F2B7: BG1 and OBJ on main, BG2 on sub, math only for BG1 and
        // eligible OBJ palettes. Keep the backdrop and OBJ palettes 0–3 unmodified.
        var main = new Bg4BppRenderLayer(EndingExplosionDisplayDefinitions.InitialMap,
            EndingExplosionDisplayDefinitions.Characters, 0, EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow, 32, 32, null);
        RenderLayer[] layers =
        [
            new ObjPriorityRenderLayer(0), new ObjPriorityRenderLayer(1),
            main with { Priority = false }, new ObjPriorityRenderLayer(2),
            main with { Priority = true }, new ObjPriorityRenderLayer(3),
            new BgSubscreenAddRenderLayer(EndingExplosionDisplayDefinitions.BurstSubMap,
                EndingExplosionDisplayDefinitions.Characters, main, FourBpp: true, MainObjects: true,
                VerticalScroll: EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow),
        ];
        return new(PpuMemorySnapshot.Capture(vram, cgram, PrepareSprites()), layers,
            CurrentEscapeObjectSelection, brightness);
    }
}
