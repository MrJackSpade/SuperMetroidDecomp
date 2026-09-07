using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

public sealed partial class IntroCinematicState
{
    /// <summary>Prepares the current display once and owns all inputs needed by a detached renderer.</summary>
    internal LayeredRenderSnapshot CaptureTranslatedRenderSnapshot()
    {
        if (ceresFlight is not null && Phase == IntroCinematicPhase.CeresFlight)
            return ceresFlight.CaptureRenderSnapshot();

        OamBuffer oam;
        Bg4BppRenderLayer room;
        if (Phase is IntroCinematicPhase.MotherBrainCrossfade or
            IntroCinematicPhase.MotherBrainFlashback or IntroCinematicPhase.PageTwoCrossfade)
        {
            oam = PrepareMotherBrainOam();
            room = Plane(IntroCinematicRomData.Layers.SceneBg1TilemapWord, 0,
                flashbackMotherBrain?.BackgroundVerticalScroll ?? GameplayFlashbackBg1VerticalScroll);
        }
        else if (Phase is IntroCinematicPhase.BabyDiscoveryCrossfade or
            IntroCinematicPhase.BabyDiscovery or IntroCinematicPhase.PageThreeCrossfade)
        {
            oam = PrepareBabyDiscoveryOam();
            room = Plane(IntroCinematicRomData.Layers.SceneBg2TilemapWord, 0, GameplayFlashbackBg1VerticalScroll);
        }
        else if (Phase is IntroCinematicPhase.BabyMetroidDeliveryCrossfade or
            IntroCinematicPhase.BabyMetroidDelivery or IntroCinematicPhase.PageFourCrossfade or
            IntroCinematicPhase.BabyMetroidExaminationCrossfade or
            IntroCinematicPhase.BabyMetroidExamination or IntroCinematicPhase.PageFiveCrossfade)
        {
            oam = PrepareScientistOam();
            room = Plane(scientistCutscene!.TilemapBaseWord, scientistCutscene.BackgroundX, scientistCutscene.BackgroundY);
        }
        else if (objects is null)
        {
            oam = new OamBuffer();
            oam.BeginFrame();
            oam.FinalizeFrame();
            // The first card is opaque and unscrolled; later text is transparent
            // and cropped one tile down. Do not conflate those two PPU setups.
            return new(PpuMemorySnapshot.Capture(vram, cgram, oam),
                new RenderLayer[] { new Bg2BppViewportRenderLayer(
                    IntroCinematicRomData.Layers.NarrationTilemapWord,
                    IntroCinematicRomData.Layers.FontCharacterBaseWord, 0, false, null) },
                MenuRenderDefinitions.ObjectSelection, (byte)brightness);
        }
        else
        {
            oam = PrepareIllustratedPageOam();
            room = Plane(IntroCinematicRomData.Layers.PortraitTilemapWord, 0, GameplayFlashbackBg1VerticalScroll);
        }

        var text = new Bg2BppViewportRenderLayer(IntroCinematicRomData.Layers.NarrationTilemapWord,
            IntroCinematicRomData.Layers.FontCharacterBaseWord, GameplayFlashbackBg1VerticalScroll, true, false);
        // Mode 1 with high-priority BG3: preserve both the OBJ winner and the
        // insertion points of text/room priority planes during palette crossfades.
        return new(PpuMemorySnapshot.Capture(vram, cgram, oam), new RenderLayer[]
        {
            new ObjPriorityRenderLayer(0), text, new ObjPriorityRenderLayer(1), room,
            new ObjPriorityRenderLayer(2), room with { Priority = true },
            new ObjPriorityRenderLayer(3), text with { Priority = true },
        }, MenuRenderDefinitions.ObjectSelection, (byte)brightness);
    }

    private static Bg4BppRenderLayer Plane(ushort tilemap, ushort x, ushort y) =>
        new(tilemap, 0, x, y, 32, 32, false);
}
