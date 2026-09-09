using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private bool explosionFinaleDisplay;
    private bool UsesExplosionFinaleDisplay => explosionFinaleDisplay && !ExplosionWhiteout
        && Phase < EndingCreditsPhase.PlanetEscapeFast;

    private LayeredRenderSnapshot CaptureExplosionFinale()
    {
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
}
