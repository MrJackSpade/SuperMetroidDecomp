using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    /// <summary>Resolves the ending display at its existing draw boundary, without rasterization.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        var layers = new List<RenderLayer>();
        byte objectSelection = EndingCreditsRomData.Rendering.EscapeObjectSelection;
        OamBuffer oam;
        if (Phase == EndingCreditsPhase.Credits)
        {
            layers.Add(new Bg4BppRenderLayer(EndingCreditsRomData.Rendering.CreditsTilemapWord,
                EndingCreditsRomData.Rendering.CreditsCharacterWord, 0, credits!.VerticalScroll, 32, 32, null));
            // Credits intentionally do not run the sprite draw routine.
            oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        }
        else
        {
            if (Phase >= EndingCreditsPhase.PostCreditsBlank)
            {
                objectSelection = EndingCreditsRomData.Rendering.RewardObjectSelection;
                if (Phase != EndingCreditsPhase.PostCreditsBlank)
                    layers.Add(new Bg4BppRenderLayer(EndingCreditsRomData.Rendering.PostCreditsTilemapWord,
                        EndingCreditsRomData.Rendering.PostCreditsCharacterWord, 0, postCreditsVerticalScroll, 32, 32, null));
            }
            else
            {
                short a = Scale(ReadSine(mode7Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex), mode7Zoom);
                short b = Scale(ReadSine(mode7Angle.TableIndex), mode7Zoom);
                layers.Add(new Mode7RenderLayer(new(a, b, unchecked((short)-b), a,
                    EndingCreditsRomData.Rendering.Mode7CenterX, EndingCreditsRomData.Rendering.Mode7CenterY,
                    unchecked((short)mode7X), unchecked((short)mode7Y))));
            }
            oam = PrepareSprites();
            layers.Add(new ObjRenderLayer());
        }
        return new(PpuMemorySnapshot.Capture(vram, cgram, oam), layers.ToArray(), objectSelection, brightness);
    }
}
