using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    // OBJ layer reused across renders; the span overload clears it first. Never saved state.
    /// <summary>Reusable OBJ-layer raster cleared before each sprite composition.</summary>
    [NonSerialized] private Rgba32[]? objectLayerScratch;
    // Final frame, reused by every render: a returned frame is valid until this scene renders again.
    /// <summary>Reusable 256-by-224 output buffer returned by the direct software rendering path.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;

    /// <summary>Projects the current cartridge-backed PPU image into the desktop raster.</summary>
    public Rgba32[] Render()
    {
        if (AtmosphericMapWraps || UsesFlyawayMode7Priority || UsesExplosionFinaleDisplay || postShot is not null || endingLogo is not null || Phase >= EndingCreditsPhase.PostCreditsBlank)
            return SoftwareLayeredSnapshotRenderer.Render(CaptureRenderSnapshot());
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224, frameBuffer ??= new Rgba32[256 * 224]);

        if (Phase == EndingCreditsPhase.Credits)
        {
            // Function 126 installs font 3 at word $4000 and the circular credits map at
            // BG1SC word $4800. The half-pixel accumulator itself is held by the credits
            // object; the PPU consumes only its whole vertical-scroll word.
            SnesBgTilemapRenderer.Composite4BppViewport(
                pixels,
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

    /// <summary>Composites the current Mode 7 scene using the active matrix, center, scroll offsets, and wrap policy.</summary>
    private void RenderMode7Background(Span<Rgba32> pixels)
    {
        // Ending setup uses the standard bank-$8B matrix helper. X/Y are the same signed
        // scroll words and zoom/angle are the same 8.8 scalar and sine-table index used by
        // the Ceres cinematic, so no host camera transform is introduced here.
        short cosine = ReadSine(mode7Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
        short sine = ReadSine(mode7Angle.TableIndex);
        short matrixA = Scale(cosine, mode7Zoom);
        short matrixB = Scale(sine, mode7Zoom);
        SnesMode7Renderer.CompositeViewport(
            pixels,
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
    }

    /// <summary>Selects cloud OBJ settings before the explosion fade and escape-scene settings afterward.</summary>
    private byte CurrentEscapeObjectSelection => Phase < EndingCreditsPhase.FadeInZebesExplosion
        ? EndingCreditsRomData.Rendering.CloudObjectSelection : EndingCreditsRomData.Rendering.EscapeObjectSelection;

    // SetupPpu_5_Mode7 leaves M7SEL=0 through both atmospheric scenes.
    // Later scene setup owns its own overflow policy; do not change those implicitly.
    /// <summary>Reports whether the active pre-explosion atmospheric scene wraps outside the Mode 7 map.</summary>
    private bool AtmosphericMapWraps => Phase < EndingCreditsPhase.FadeInZebesExplosion;

    /// <summary>Identifies the two planet-escape wait phases that render the explosion whiteout.</summary>
    private bool ExplosionWhiteout => Phase is EndingCreditsPhase.WaitForPlanetEscapeMusic
        or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue;
    // Func120 selects Mode 7: priority-zero OBJ is behind BG1, the ship itself.
    /// <summary>Uses the flyaway scene's OBJ priority so the ship is drawn behind the Mode 7 background.</summary>
    private bool UsesFlyawayMode7Priority => Phase >= EndingCreditsPhase.PlanetEscapeFast && Phase < EndingCreditsPhase.Credits;

    /// <summary>Disables ending actors during the whiteout and post-credits scenes that contain no objects.</summary>
    private bool EndingObjectsEnabled => !ExplosionWhiteout && Phase is not
        (EndingCreditsPhase.PostCreditsCopyright or EndingCreditsPhase.PostCreditsWaitingSamus or EndingCreditsPhase.PostCreditsBlank);

    /// <summary>Chooses the authored horizontal Mode 7 center for the atmospheric or planet-escape phase.</summary>
    private short CurrentMode7CenterX => Phase < EndingCreditsPhase.PlanetEscapeFast
        ? EndingCreditsRomData.Rendering.AtmosphericMode7Center : EndingCreditsRomData.Rendering.Mode7CenterX;
    /// <summary>Chooses the authored vertical Mode 7 center for the atmospheric or planet-escape phase.</summary>
    private short CurrentMode7CenterY => Phase < EndingCreditsPhase.PlanetEscapeFast
        ? EndingCreditsRomData.Rendering.AtmosphericMode7Center : EndingCreditsRomData.Rendering.Mode7CenterY;

    /// <summary>Composites the active post-credits tilemap and font unless the current phase blanks BG1.</summary>
    private void RenderPostCreditsBackground(Span<Rgba32> pixels)
    {
        if (!PostCreditsBackgroundEnabled)
            return;

        // The opening waiting scene uses BG2 ($4C00/$5000). Result text is uploaded to
        // BG1 ($4800/$4000), so its map and font must change together at the handoff.
        SnesBgTilemapRenderer.Composite4BppViewport(
            pixels,
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
    }

    /// <summary>Draws prepared OBJ pixels and applies either normal compositing or the reward subscreen addition.</summary>
    private void RenderSprites(Span<Rgba32> pixels, byte obsel)
    {
        OamBuffer oam = PrepareSprites();
        Rgba32[] objects = objectLayerScratch ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
        SnesObjRenderer.Render(objects, oam, vram, cgram, obsel);
        if (RewardSubscreenAddition) SnesLayerCompositor.AddSubscreen(pixels, objects);
        else SnesLayerCompositor.Composite(pixels, objects);
    }

    // E1D2 turns BG1 off when reward actors spawn. The armored reward uses OBJ only;
    // faster rewards return to BG2 for the waiting-Samus dissolve.
    /// <summary>Controls BG1 visibility across post-credits transitions and the armored reward scene.</summary>
    private bool PostCreditsBackgroundEnabled => Phase != EndingCreditsPhase.PostCreditsBlank
        && Phase != EndingCreditsPhase.PostCreditsFadeIn
        && Phase != EndingCreditsPhase.PostCreditsGesture
        && Phase != EndingCreditsPhase.PostCreditsJump
        && !(Phase == EndingCreditsPhase.PostCreditsReward && EndingReward == EndingReward.Armored);
    /// <summary>Selects the waiting-room background during the opening wait and returning-reward phases.</summary>
    private bool UsesWaitingBackground => Phase < EndingCreditsPhase.PostCreditsWaitingSamus
        || Phase == EndingCreditsPhase.PostCreditsReward;
    // E1D2/E2DD: TM=BG2, TS=OBJ, CGADSUB=$22 adds OBJ to BG2 and backdrop.
    /// <summary>Enables the native OBJ-to-subscreen addition during the explosion crossfade and non-armored reward scenes.</summary>
    private bool RewardSubscreenAddition => ExplosionCrossfadeActive
        || Phase == EndingCreditsPhase.PostCreditsShootingStars
        || (Phase == EndingCreditsPhase.PostCreditsReward && EndingReward != EndingReward.Armored);

    /// <summary>Tilemap VRAM word for the waiting scene or the currently uploaded results screen.</summary>
    private ushort CurrentPostCreditsTilemapWord => UsesWaitingBackground
        ? EndingCreditsRomData.Rendering.WaitingTilemapWord : postCreditsUploadWord;
    /// <summary>Character VRAM word paired with the selected post-credits tilemap.</summary>
    private ushort CurrentPostCreditsCharacterWord => UsesWaitingBackground
        ? EndingCreditsRomData.Rendering.WaitingCharacterWord : EndingCreditsRomData.Rendering.PostCreditsCharacterWord;

    // Keep sprite drawing on the simulation-side display boundary. Neither the
    // immutable packet consumer nor repeated rendering may advance sprite state.
    /// <summary>Builds the frame's OAM from the active logo, reward animation, scene actors, and shooting stars.</summary>
    private OamBuffer PrepareSprites()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        if (endingLogo is not null) endingLogo.Draw(objectArtwork?.LogoSprites, oam);
        else if (rewardGesture is not null) rewardGesture.Draw(objectArtwork?.RewardSprites, oam);
        else if (rewardJump is not null) rewardJump.Draw(objectArtwork?.RewardSprites, oam);
        else
        {
            foreach (EndingSprite wrapper in sprites.OrderByDescending(actor => actor.NativeSlot))
            {
                IIntroCinematicSpritePresentation? spriteArt = wrapper.Role switch
                {
                    <= EndingSpriteRole.CloudBottomB => objectArtwork?.CloudSprites,
                    >= EndingSpriteRole.ExplodingZebes and
                        <= EndingSpriteRole.ExplosionAfterglow => objectArtwork?.ExplosionSprites,
                    >= EndingSpriteRole.OperationWasText and
                        <= EndingSpriteRole.ClearTimeDigit => objectArtwork?.CompletionTextSprites,
                    EndingSpriteRole.RewardSamus => objectArtwork?.RewardSprites,
                    EndingSpriteRole.AnimalEscape => EndingAnimalEscapeDefinitions.Presentation,
                    _ => null,
                };
                wrapper.Sprite.Draw(bus, oam, installedArt: spriteArt);
            }
        }
        if (Phase >= EndingCreditsPhase.PostCreditsBlank) shootingStars?.Draw(oam);
        oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Reads a signed sine-table component used by the Mode 7 transform.</summary>
    private static short ReadSine(byte index) => EnemyTrigonometryTables.SignedSine(index);

    /// <summary>Uses reward-jump OBJ settings while that actor is active, otherwise the default reward settings.</summary>
    private byte CurrentRewardObjectSelection => rewardJump?.ObjectSelection
        ?? EndingCreditsRomData.Rendering.RewardObjectSelection;

    /// <summary>Multiplies a signed Mode 7 matrix component by its 8.8 scale and returns the wrapped high word.</summary>
    private static short Scale(short component, ushort scalar) =>
        unchecked((short)((component * unchecked((short)scalar)) >> 8));
}
