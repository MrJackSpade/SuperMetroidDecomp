using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    /// <summary>Projects the current cartridge-backed PPU image into the desktop raster.</summary>
    public Rgba32[] Render()
    {
        if (AtmosphericMapWraps || UsesFlyawayMode7Priority || UsesExplosionFinaleDisplay || postShot is not null || endingLogo is not null || Phase == EndingCreditsPhase.PostCreditsWhiteFlash)
            return SoftwareLayeredSnapshotRenderer.Render(CaptureRenderSnapshot());
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);

        if (Phase == EndingCreditsPhase.Credits)
        {
            // Function 126 installs font 3 at word $4000 and the circular credits map at
            // BG1SC word $4800. The half-pixel accumulator itself is held by the credits
            // object; the PPU consumes only its whole vertical-scroll word.
            Rgba32[] text = SnesBgTilemapRenderer.Render4BppViewport(
                vram,
                cgram,
                tilemapBaseWord: EndingCreditsRomData.Rendering.CreditsTilemapWord,
                characterBaseWord: EndingCreditsRomData.Rendering.CreditsCharacterWord,
                horizontalScroll: 0,
                verticalScroll: credits!.VerticalScroll,
                width: 256,
                height: 224,
                tilemapWidthInTiles: 32,
                tilemapHeightInTiles: 32);
            SnesLayerCompositor.Composite(pixels, text);
        }
        else if (Phase >= EndingCreditsPhase.PostCreditsBlank)
        {
            RenderPostCreditsBackground(pixels);
            if (EndingObjectsEnabled) RenderSprites(pixels, obsel: CurrentRewardObjectSelection);
        }
        else
        {
            if (EscapeBackgroundEnabled) RenderMode7Background(pixels);
            if (EndingObjectsEnabled) RenderSprites(pixels, obsel: CurrentEscapeObjectSelection);
        }

        MasterBrightnessFilter.Apply(pixels, brightness);
        return pixels;
    }

    private void RenderMode7Background(Span<Rgba32> pixels)
    {
        // Ending setup uses the standard bank-$8B matrix helper. X/Y are the same signed
        // scroll words and zoom/angle are the same 8.8 scalar and sine-table index used by
        // the Ceres cinematic, so no host camera transform is introduced here.
        short cosine = ReadSine(mode7Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
        short sine = ReadSine(mode7Angle.TableIndex);
        short matrixA = Scale(cosine, mode7Zoom);
        short matrixB = Scale(sine, mode7Zoom);
        Rgba32[] mode7 = SnesMode7Renderer.RenderViewport(
            vram,
            cgram,
            matrixA,
            matrixB,
            unchecked((short)-matrixB),
            matrixA,
            centerX: CurrentMode7CenterX,
            centerY: CurrentMode7CenterY,
            horizontalOffset: unchecked((short)mode7X),
            verticalOffset: unchecked((short)mode7Y), wrapOutsideMap: AtmosphericMapWraps);
        SnesLayerCompositor.Composite(pixels, mode7);
    }

    private byte CurrentEscapeObjectSelection => Phase < EndingCreditsPhase.FadeInZebesExplosion
        ? EndingCreditsRomData.Rendering.CloudObjectSelection : EndingCreditsRomData.Rendering.EscapeObjectSelection;

    // SetupPpu_5_Mode7 leaves M7SEL=0 through both atmospheric scenes.
    // Later scene setup owns its own overflow policy; do not change those implicitly.
    private bool AtmosphericMapWraps => Phase < EndingCreditsPhase.FadeInZebesExplosion;

    private bool ExplosionWhiteout => Phase is EndingCreditsPhase.WaitForPlanetEscapeMusic
        or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue;
    // Func120 selects Mode 7: priority-zero OBJ is behind BG1, the ship itself.
    private bool UsesFlyawayMode7Priority => Phase >= EndingCreditsPhase.PlanetEscapeFast && Phase < EndingCreditsPhase.Credits;
    private bool EndingObjectsEnabled => !ExplosionWhiteout && Phase != EndingCreditsPhase.PostCreditsCopyright;

    private short CurrentMode7CenterX => Phase < EndingCreditsPhase.PlanetEscapeFast
        ? EndingCreditsRomData.Rendering.AtmosphericMode7Center : EndingCreditsRomData.Rendering.Mode7CenterX;
    private short CurrentMode7CenterY => Phase < EndingCreditsPhase.PlanetEscapeFast
        ? EndingCreditsRomData.Rendering.AtmosphericMode7Center : EndingCreditsRomData.Rendering.Mode7CenterY;

    private void RenderPostCreditsBackground(Span<Rgba32> pixels)
    {
        if (!PostCreditsBackgroundEnabled)
            return;

        // The opening waiting scene uses BG2 ($4C00/$5000). Result text is uploaded to
        // BG1 ($4800/$4000), so its map and font must change together at the handoff.
        Rgba32[] waiting = SnesBgTilemapRenderer.Render4BppViewport(
            vram,
            cgram,
            tilemapBaseWord: CurrentPostCreditsTilemapWord,
            characterBaseWord: CurrentPostCreditsCharacterWord,
            horizontalScroll: 0,
            verticalScroll: postCreditsVerticalScroll,
            width: 256,
            height: 224,
            tilemapWidthInTiles: 32,
            tilemapHeightInTiles: postCreditsMapHeight);
        SnesLayerCompositor.Composite(pixels, waiting);
    }

    private void RenderSprites(Span<Rgba32> pixels, byte obsel)
    {
        OamBuffer oam = PrepareSprites();
        Rgba32[] objects = SnesObjRenderer.Render(oam, vram, cgram, obsel);
        if (RewardSubscreenAddition) SnesLayerCompositor.AddSubscreen(pixels, objects);
        else SnesLayerCompositor.Composite(pixels, objects);
    }

    // E1D2 turns BG1 off when reward actors spawn. The armored reward uses OBJ only;
    // faster rewards return to BG2 for the waiting-Samus dissolve.
    private bool PostCreditsBackgroundEnabled => Phase != EndingCreditsPhase.PostCreditsBlank
        && Phase != EndingCreditsPhase.PostCreditsGesture
        && Phase != EndingCreditsPhase.PostCreditsJump
        && !(Phase == EndingCreditsPhase.PostCreditsReward && EndingReward == EndingReward.Armored);
    private bool UsesWaitingBackground => Phase < EndingCreditsPhase.PostCreditsWaitingSamus
        || Phase == EndingCreditsPhase.PostCreditsReward;
    // E1D2/E2DD: TM=BG2, TS=OBJ, CGADSUB=$22 adds OBJ to BG2 and backdrop.
    private bool RewardSubscreenAddition => ExplosionCrossfadeActive
        || (Phase == EndingCreditsPhase.PostCreditsReward && EndingReward != EndingReward.Armored);

    private ushort CurrentPostCreditsTilemapWord => UsesWaitingBackground
        ? EndingCreditsRomData.Rendering.WaitingTilemapWord : postCreditsUploadWord;
    private ushort CurrentPostCreditsCharacterWord => UsesWaitingBackground
        ? EndingCreditsRomData.Rendering.WaitingCharacterWord : EndingCreditsRomData.Rendering.PostCreditsCharacterWord;

    // Keep sprite drawing on the simulation-side display boundary. Neither the
    // immutable packet consumer nor repeated rendering may advance sprite state.
    private OamBuffer PrepareSprites()
    {
        if (rewardGesture is not null) return rewardGesture.Draw();
        if (rewardJump is not null) return rewardJump.Draw();
        var oam = new OamBuffer();
        oam.BeginFrame();
        foreach (EndingSprite wrapper in sprites.OrderByDescending(actor => actor.NativeSlot))
            wrapper.Sprite.Draw(bus, oam);
        oam.FinalizeFrame();
        return oam;
    }

    private short ReadSine(byte index) => unchecked((short)
        RomDataReader.ReadWordFixedBank(
            bus,
            EndingCreditsRomData.Assets.SignedSineTable + index * sizeof(ushort)));

    private byte CurrentRewardObjectSelection => rewardJump?.ObjectSelection
        ?? EndingCreditsRomData.Rendering.RewardObjectSelection;

    private static short Scale(short component, ushort scalar) =>
        unchecked((short)((component * unchecked((short)scalar)) >> 8));
}
