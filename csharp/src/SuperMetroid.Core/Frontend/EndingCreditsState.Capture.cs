using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    /// <summary>Resolves the ending display at its existing draw boundary, without rasterization.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        if (UsesExplosionFinaleDisplay) return CaptureExplosionFinale();
        var layers = new List<RenderLayer>();
        byte objectSelection = CurrentEscapeObjectSelection;
        OamBuffer oam;
        if (endingLogo is not null)
        {
            if (endingLogo.CrossfadeStarted)
                layers.Add(new Bg4BppRenderLayer(EndingLogoDefinitions.Tilemap, EndingLogoDefinitions.Characters,
                    0, 0, 32, 32, null));
            if (!endingLogo.Completed) layers.Add(new ObjRenderLayer(endingLogo.CrossfadeStarted));
            layers.Add(new Bg4BppRenderLayer(CurrentPostCreditsTilemapWord, CurrentPostCreditsCharacterWord,
                0, postCreditsVerticalScroll, 32, postCreditsMapHeight, null));
            return new(PpuMemorySnapshot.Capture(vram, cgram, endingLogo.Draw()), layers.ToArray(), 3, brightness);
        }
        if (postShot is not null)
        {
            // Mode 7 places OBJ priority zero below BG1; the other three priorities
            // are above it. BG1 subtracts the winning OBJ subscreen, without halving.
            layers.Add(new ObjPriorityRenderLayer(0));
            layers.Add(new Mode7RenderLayer(PostShotRegisters, true));
            layers.Add(new ObjPriorityRenderLayer(1));
            layers.Add(new ObjPriorityRenderLayer(2));
            layers.Add(new ObjPriorityRenderLayer(3));
            return new(PpuMemorySnapshot.Capture(vram, cgram, PrepareSprites()), layers.ToArray(),
                CurrentRewardObjectSelection, brightness);
        }
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
                objectSelection = CurrentRewardObjectSelection;
                if (PostCreditsBackgroundEnabled)
                    layers.Add(new Bg4BppRenderLayer(CurrentPostCreditsTilemapWord,
                        CurrentPostCreditsCharacterWord, 0, postCreditsVerticalScroll, 32, postCreditsMapHeight, null));
            }
            else if (EscapeBackgroundEnabled)
            {
                // Fixed addition belongs to backdrop and eligible OBJ, not BG1.
                if (FlyawayFixedColor is { } backdropWhite) layers.Add(backdropWhite);
                short a = Scale(ReadSine(mode7Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex), mode7Zoom);
                short b = Scale(ReadSine(mode7Angle.TableIndex), mode7Zoom);
                layers.Add(new Mode7RenderLayer(new(a, b, unchecked((short)-b), a,
                    CurrentMode7CenterX, CurrentMode7CenterY,
                    unchecked((short)mode7X), unchecked((short)mode7Y))));
            }
            oam = PrepareSprites();
            if (EndingObjectsEnabled) layers.Add(new ObjRenderLayer(RewardSubscreenAddition, FlyawayFixedColor));
            if (Phase == EndingCreditsPhase.PostCreditsWhiteFlash)
                layers.Add(new FixedColorAddRenderLayer(whiteFlashColor, whiteFlashColor, whiteFlashColor));
        }
        return new(PpuMemorySnapshot.Capture(vram, cgram, oam), layers.ToArray(), objectSelection, brightness);
    }

    private Mode7RenderRegisters PostShotRegisters
    {
        get
        {
            short a = Scale(ReadSine(unchecked((byte)(postShot!.Angle + SnesAngle.QuarterTurn.TableIndex))), (ushort)postShot.Scale);
            short b = Scale(ReadSine(postShot.Angle), (ushort)postShot.Scale);
            return new(a, b, unchecked((short)-b), a,
                EndingPostShotDefinitions.CenterX, EndingPostShotDefinitions.CenterY,
                EndingPostShotDefinitions.OffsetX, EndingPostShotDefinitions.OffsetY);
        }
    }
}
