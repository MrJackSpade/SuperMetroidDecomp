using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private bool explosionFinaleDisplay;
    private bool explosionBurstDisplay;
    private bool UsesExplosionFinaleDisplay => (explosionFinaleDisplay || explosionBurstDisplay) && !ExplosionWhiteout
        && Phase < EndingCreditsPhase.PlanetEscapeFast;

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
                EndingExplosionDisplayDefinitions.Characters, FourBpp: true, IncludeObjects: true),
        ];
        return new(PpuMemorySnapshot.Capture(vram, cgram, PrepareSprites()), layers,
            CurrentEscapeObjectSelection, brightness);

        static Bg4BppRenderLayer Plane(ushort map, bool high) => new(map,
            EndingExplosionDisplayDefinitions.Characters, 0, 0, 32, 32, high);
    }

    private LayeredRenderSnapshot CaptureExplosionBurst()
    {
        // F2B7: BG1 and OBJ on main, BG2 on sub, math only for BG1 and
        // eligible OBJ palettes. Keep the backdrop and OBJ palettes 0–3 unmodified.
        var main = new Bg4BppRenderLayer(EndingExplosionDisplayDefinitions.InitialMap,
            EndingExplosionDisplayDefinitions.Characters, 0, 0, 32, 32, null);
        RenderLayer[] layers =
        [
            new ObjPriorityRenderLayer(0), new ObjPriorityRenderLayer(1),
            main with { Priority = false }, new ObjPriorityRenderLayer(2),
            main with { Priority = true }, new ObjPriorityRenderLayer(3),
            new BgSubscreenAddRenderLayer(EndingExplosionDisplayDefinitions.BurstSubMap,
                EndingExplosionDisplayDefinitions.Characters, main, FourBpp: true, MainObjects: true),
        ];
        return new(PpuMemorySnapshot.Capture(vram, cgram, PrepareSprites()), layers,
            CurrentEscapeObjectSelection, brightness);
    }
}
