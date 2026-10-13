using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>The ending scene's presentation: Mode 7, background and sprite composition into a render snapshot or the legacy raster, drawn from the scene state it belongs to.</summary>
internal sealed class EndingCreditsPresentation
{
    /// <summary>Scene state whose phase, PPU memory, and actors determine the rendered image.</summary>
    private readonly EndingCreditsState _ending;

    /// <summary>Binds presentation to the ending scene without copying its changing state.</summary>
    internal EndingCreditsPresentation(EndingCreditsState owner)
    {
        _ending = owner;
    }
    /// <summary>Resolves the ending display at its existing draw boundary, without rasterization.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        if (_ending.UsesExplosionFinaleDisplay) return _ending.CaptureExplosionFinale();
        var layers = new List<RenderLayer>();
        byte objectSelection = CurrentEscapeObjectSelection;
        OamBuffer oam;
        if (_ending.endingLogo is not null)
        {
            if (_ending.endingLogo.CrossfadeStarted)
                layers.Add(new Bg4BppRenderLayer(EndingLogoDefinitions.Tilemap, EndingLogoDefinitions.Characters,
                    new Position16<BgPlanePixels>(0, 0), new Extent16<TilemapCells>(32, 32), null));
            layers.Add(new ObjRenderLayer(_ending.endingLogo.CrossfadeStarted));
            layers.Add(new Bg4BppRenderLayer(CurrentPostCreditsTilemapWord, CurrentPostCreditsCharacterWord,
                new Position16<BgPlanePixels>(0, _ending.postCreditsVerticalScroll), new Extent16<TilemapCells>(32, checked((ushort)_ending.postCreditsMapHeight)), null));
            return new(PpuMemorySnapshot.Capture(_ending.vram, _ending.cgram,
                PrepareSprites()), layers.ToArray(), 3, _ending.brightness);
        }
        if (_ending.postShot is not null)
        {
            // Mode 7 places OBJ priority zero below BG1; the other three priorities
            // are above it. BG1 subtracts the winning OBJ subscreen, without halving.
            layers.Add(new ObjPriorityRenderLayer(0));
            layers.Add(new Mode7RenderLayer(PostShotRegisters, true));
            layers.Add(new ObjPriorityRenderLayer(1));
            layers.Add(new ObjPriorityRenderLayer(2));
            layers.Add(new ObjPriorityRenderLayer(3));
            return new(PpuMemorySnapshot.Capture(_ending.vram, _ending.cgram, PrepareSprites()), layers.ToArray(),
                CurrentRewardObjectSelection, _ending.brightness);
        }
        if (_ending.Phase == EndingCreditsPhase.Credits)
        {
            layers.Add(new Bg4BppRenderLayer(EndingCreditsRomData.Rendering.CreditsTilemapWord,
                EndingCreditsRomData.Rendering.CreditsCharacterWord, new Position16<BgPlanePixels>(0, _ending.Credits.VerticalScroll), new Extent16<TilemapCells>(32, 32), null));
            // Credits intentionally do not run the sprite draw routine.
            oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        }
        else
        {
            if (_ending.Phase >= EndingCreditsPhase.PostCreditsBlank)
            {
                objectSelection = CurrentRewardObjectSelection;
                if (_ending.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                    layers.Add(new ObjPriorityRenderLayer(0));
                if (PostCreditsBackgroundEnabled)
                    layers.Add(new Bg4BppRenderLayer(CurrentPostCreditsTilemapWord,
                        CurrentPostCreditsCharacterWord, new Position16<BgPlanePixels>(0, _ending.postCreditsVerticalScroll), new Extent16<TilemapCells>(32, checked((ushort)_ending.postCreditsMapHeight)), null));
                if (_ending.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                    layers.Add(new BgSubscreenAddRenderLayer(EndingWavySamusDefinitions.TilemapWord,
                        EndingWavySamusDefinitions.CharacterWord,
                        new Bg4BppRenderLayer(CurrentPostCreditsTilemapWord, CurrentPostCreditsCharacterWord,
                            new Position16<BgPlanePixels>(0, _ending.postCreditsVerticalScroll), new Extent16<TilemapCells>(32, checked((ushort)_ending.postCreditsMapHeight)), null))
                        .WithScrolls(EndingWavySamusDefinitions.ScrollsAtAge(
                            EndingCreditsRomData.Timing.WaitingBackdropFrames - _ending.phaseTimer)));
            }
            else if (_ending.EscapeBackgroundEnabled)
            {
                // Fixed addition belongs to backdrop and eligible OBJ, not BG1.
                if (_ending.FlyawayFixedColor is { } backdropWhite) layers.Add(backdropWhite);
                if (UsesFlyawayMode7Priority) layers.Add(new ObjPriorityRenderLayer(0, _ending.FlyawayFixedColor));
                short a = Scale(ReadSine(_ending.mode7Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex), _ending.mode7Zoom);
                short b = Scale(ReadSine(_ending.mode7Angle.TableIndex), _ending.mode7Zoom);
                layers.Add(new Mode7RenderLayer(new(a, b, unchecked((short)-b), a,
                    CurrentMode7Center,
                    _ending.mode7.Whole.AsSignedRegister(), WrapOutsideMap: AtmosphericMapWraps),
                    AddBg1Subscreen: AtmosphericMapWraps));
            }
            oam = PrepareSprites();
            if (EndingObjectsEnabled && !AtmosphericMapWraps)
            {
                if (UsesFlyawayMode7Priority)
                {
                    layers.Add(new ObjPriorityRenderLayer(1, _ending.FlyawayFixedColor));
                    layers.Add(new ObjPriorityRenderLayer(2, _ending.FlyawayFixedColor));
                    layers.Add(new ObjPriorityRenderLayer(3, _ending.FlyawayFixedColor));
                }
                else if (_ending.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                {
                    // Star priority zero sits behind BG2. Other cinematic OBJ remain
                    // above it; the BG3 wave applies only to the background plane.
                    layers.Add(new ObjPriorityRenderLayer(1));
                    layers.Add(new ObjPriorityRenderLayer(2));
                    layers.Add(new ObjPriorityRenderLayer(3));
                }
                else layers.Add(new ObjRenderLayer(RewardSubscreenAddition, _ending.FlyawayFixedColor));
            }
            if (_ending.Phase == EndingCreditsPhase.PostCreditsWhiteFlash)
                layers.Add(new FixedColorAddRenderLayer(_ending.whiteFlashColor, _ending.whiteFlashColor, _ending.whiteFlashColor));
        }
        return new(PpuMemorySnapshot.Capture(_ending.vram, _ending.cgram, oam), layers.ToArray(), objectSelection, _ending.brightness);
    }

    /// <summary>Mode 7 matrix and center for the post-shot reward scene.</summary>
    private Mode7RenderRegisters PostShotRegisters
    {
        get
        {
            short a = Scale(ReadSine(unchecked((byte)(_ending.PostShot.Angle + SnesAngle.QuarterTurn.TableIndex))), (ushort)_ending.PostShot.Scale);
            short b = Scale(ReadSine(_ending.PostShot.Angle), (ushort)_ending.PostShot.Scale);
            return new(a, b, unchecked((short)-b), a,
                new Offset16<Mode7Pixels>(EndingPostShotDefinitions.CenterX, EndingPostShotDefinitions.CenterY),
                new Offset16<Mode7Pixels>(EndingPostShotDefinitions.OffsetX, EndingPostShotDefinitions.OffsetY));
        }
    }
    /// <summary>Temporary OBJ pixels reused across renders and cleared by the span renderer.</summary>
    [NonSerialized] private Rgba32[]? objectLayerScratch;
    /// <summary>Reusable final raster; callers retain it only until the next scene render.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;

    /// <summary>Projects the current cartridge-backed PPU image into the desktop raster.</summary>
    public Rgba32[] Render()
    {
        if (AtmosphericMapWraps || UsesFlyawayMode7Priority || _ending.UsesExplosionFinaleDisplay || _ending.postShot is not null || _ending.endingLogo is not null || _ending.Phase >= EndingCreditsPhase.PostCreditsBlank)
            return SoftwareLayeredSnapshotRenderer.Render(CaptureRenderSnapshot());
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(_ending.cgram, 256 * 224, frameBuffer ??= new Rgba32[256 * 224]);

        if (_ending.Phase == EndingCreditsPhase.Credits)
        {
            // Function 126 installs font 3 at word $4000 and the circular credits map at
            // BG1SC word $4800. The half-pixel accumulator itself is held by the credits
            // object; the PPU consumes only its whole vertical-scroll word.
            SnesBgTilemapRenderer.Composite4BppViewport(pixels, _ending.vram, _ending.cgram, new BgVramAddresses { TilemapBaseWord = EndingCreditsRomData.Rendering.CreditsTilemapWord, CharacterBaseWord = EndingCreditsRomData.Rendering.CreditsCharacterWord }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, _ending.Credits.VerticalScroll), Size = SnesPpuLayout.ScreenSize, TilemapSize = new Extent16<TilemapCells>(32, 32) });
        }
        else if (_ending.Phase >= EndingCreditsPhase.PostCreditsBlank)
        {
            RenderPostCreditsBackground(pixels);
            if (EndingObjectsEnabled) RenderSprites(pixels, obsel: CurrentRewardObjectSelection);
        }
        else
        {
            if (_ending.EscapeBackgroundEnabled) RenderMode7Background(pixels);
            if (EndingObjectsEnabled) RenderSprites(pixels, obsel: CurrentEscapeObjectSelection);
        }

        MasterBrightnessFilter.Apply(pixels, _ending.brightness);
        return pixels;
    }

    /// <summary>Composes the atmospheric Mode 7 viewport from the scene's native zoom, angle, and scroll.</summary>
    private void RenderMode7Background(Span<Rgba32> pixels)
    {
        // Ending setup uses the standard bank-$8B matrix helper. X/Y are the same signed
        // scroll words and zoom/angle are the same 8.8 scalar and sine-table index used by
        // the Ceres cinematic, so no host camera transform is introduced here.
        short cosine = ReadSine(_ending.mode7Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
        short sine = ReadSine(_ending.mode7Angle.TableIndex);
        short matrixA = Scale(cosine, _ending.mode7Zoom);
        short matrixB = Scale(sine, _ending.mode7Zoom);
        SnesMode7Renderer.CompositeViewport(pixels, _ending.vram, _ending.cgram, new Mode7RenderRegisters { MatrixA = matrixA, MatrixB = matrixB, MatrixC = unchecked((short)-matrixB), MatrixD = matrixA, Center = CurrentMode7Center, Scroll = _ending.mode7.Whole.AsSignedRegister(), WrapOutsideMap = AtmosphericMapWraps });
    }

    /// <summary>Selects cloud OBJ tiles before the explosion and escape OBJ tiles afterward.</summary>
    internal byte CurrentEscapeObjectSelection => _ending.Phase < EndingCreditsPhase.FadeInZebesExplosion
        ? EndingCreditsRomData.Rendering.CloudObjectSelection : EndingCreditsRomData.Rendering.EscapeObjectSelection;

    /// <summary>Whether atmospheric scenes use the Mode 7 wrap policy from SetupPpu_5_Mode7.</summary>
    private bool AtmosphericMapWraps => _ending.Phase < EndingCreditsPhase.FadeInZebesExplosion;

    /// <summary>Whether the current explosion music wait renders a white screen.</summary>
    internal bool ExplosionWhiteout => _ending.Phase is EndingCreditsPhase.WaitForPlanetEscapeMusic
        or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue;
    /// <summary>Whether flyaway Mode 7 places priority-zero OBJ behind the ship on BG1.</summary>
    private bool UsesFlyawayMode7Priority => _ending.Phase is >= EndingCreditsPhase.PlanetEscapeFast and < EndingCreditsPhase.Credits;
    /// <summary>Whether the current ending phase draws object actors at all.</summary>
    private bool EndingObjectsEnabled => !ExplosionWhiteout && _ending.Phase is not
        (EndingCreditsPhase.PostCreditsCopyright or EndingCreditsPhase.PostCreditsWaitingSamus or EndingCreditsPhase.PostCreditsBlank);

    /// <summary>Mode 7 rotation center, shared by atmospheric scenes and changed for flyaway.</summary>
    private Offset16<Mode7Pixels> CurrentMode7Center => _ending.Phase < EndingCreditsPhase.PlanetEscapeFast
        ? new(EndingCreditsRomData.Rendering.AtmosphericMode7Center, EndingCreditsRomData.Rendering.AtmosphericMode7Center)
        : new(EndingCreditsRomData.Rendering.Mode7CenterX, EndingCreditsRomData.Rendering.Mode7CenterY);

    /// <summary>Composes the active BG tilemap and font during post-credit scenes.</summary>
    private void RenderPostCreditsBackground(Span<Rgba32> pixels)
    {
        if (!PostCreditsBackgroundEnabled)
            return;

        // The opening waiting scene uses BG2 ($4C00/$5000). Result text is uploaded to
        // BG1 ($4800/$4000), so its map and font must change together at the handoff.
        SnesBgTilemapRenderer.Composite4BppViewport(pixels, _ending.vram, _ending.cgram, new BgVramAddresses { TilemapBaseWord = CurrentPostCreditsTilemapWord, CharacterBaseWord = CurrentPostCreditsCharacterWord }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, _ending.postCreditsVerticalScroll), Size = SnesPpuLayout.ScreenSize, TilemapSize = new Extent16<TilemapCells>(32, checked((ushort)_ending.postCreditsMapHeight)) });
    }

    /// <summary>Renders ending OBJ and applies the reward scene's subscreen addition when selected.</summary>
    private void RenderSprites(Span<Rgba32> pixels, byte obsel)
    {
        OamBuffer oam = PrepareSprites();
        Rgba32[] objects = objectLayerScratch ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
        SnesObjRenderer.Render(objects, oam, _ending.vram, _ending.cgram, obsel);
        if (RewardSubscreenAddition) SnesLayerCompositor.AddSubscreen(pixels, objects);
        else SnesLayerCompositor.Composite(pixels, objects);
    }

    /// <summary>Whether the phase displays post-credit BG; the armored reward uses OBJ alone.</summary>
    private bool PostCreditsBackgroundEnabled => _ending.Phase != EndingCreditsPhase.PostCreditsBlank
        && _ending.Phase != EndingCreditsPhase.PostCreditsFadeIn
        && _ending.Phase != EndingCreditsPhase.PostCreditsGesture
        && _ending.Phase != EndingCreditsPhase.PostCreditsJump
        && !(_ending.Phase == EndingCreditsPhase.PostCreditsReward && _ending.EndingReward == EndingReward.Armored);
    /// <summary>Whether to use the waiting scene's BG2 map and character base.</summary>
    private bool UsesWaitingBackground => _ending.Phase is < EndingCreditsPhase.PostCreditsWaitingSamus
        or EndingCreditsPhase.PostCreditsReward;
    /// <summary>Whether native E1D2/E2DD subscreen settings add OBJ to BG2 and backdrop.</summary>
    private bool RewardSubscreenAddition => _ending.ExplosionCrossfadeActive
        || _ending.Phase == EndingCreditsPhase.PostCreditsShootingStars
        || (_ending.Phase == EndingCreditsPhase.PostCreditsReward && _ending.EndingReward != EndingReward.Armored);

    /// <summary>VRAM map base for the waiting backdrop or uploaded result panel.</summary>
    private ushort CurrentPostCreditsTilemapWord => UsesWaitingBackground
        ? EndingCreditsRomData.Rendering.WaitingTilemapWord : _ending.postCreditsUploadWord;
    /// <summary>VRAM character base matching the selected post-credit BG map.</summary>
    private ushort CurrentPostCreditsCharacterWord => UsesWaitingBackground
        ? EndingCreditsRomData.Rendering.WaitingCharacterWord : EndingCreditsRomData.Rendering.PostCreditsCharacterWord;

    /// <summary>Builds scene OAM at the simulation display boundary without advancing sprite state.</summary>
    internal OamBuffer PrepareSprites()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        if (_ending.endingLogo is not null) _ending.endingLogo.Draw(_ending.objectArtwork?.LogoSprites, oam);
        else if (_ending.rewardGesture is not null) _ending.rewardGesture.Draw(_ending.objectArtwork?.RewardSprites, oam);
        else if (_ending.rewardJump is not null) _ending.rewardJump.Draw(_ending.objectArtwork?.RewardSprites, oam);
        else
        {
            foreach (EndingSprite wrapper in _ending.sprites.OrderByDescending(actor => actor.NativeSlot))
            {
                IIntroCinematicSpritePresentation? spriteArt = wrapper.Role.Family() switch
                {
                    EndingSpriteFamily.Cloud => _ending.objectArtwork?.CloudSprites,
                    EndingSpriteFamily.Explosion => _ending.objectArtwork?.ExplosionSprites,
                    EndingSpriteFamily.CompletionText => _ending.objectArtwork?.CompletionTextSprites,
                    EndingSpriteFamily.Reward => _ending.objectArtwork?.RewardSprites,
                    EndingSpriteFamily.AnimalEscape => EndingAnimalEscapeDefinitions.Presentation,
                    var family => throw new InvalidOperationException($"Undefined ending sprite family {family}."),
                };
                wrapper.Sprite.Draw(oam, installedArt: spriteArt);
            }
        }
        if (_ending.Phase >= EndingCreditsPhase.PostCreditsBlank) _ending.shootingStars?.Draw(oam);
        oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Reads a signed sine component from the installed enemy trigonometry table.</summary>
    internal static short ReadSine(byte index) => EnemyTrigonometryTables.SignedSine(index);

    /// <summary>Uses the reward jump's OBJ selection when active, otherwise the default reward bank.</summary>
    private byte CurrentRewardObjectSelection => _ending.rewardJump?.ObjectSelection
        ?? EndingCreditsRomData.Rendering.RewardObjectSelection;

    /// <summary>Multiplies a signed Mode 7 component by the scene's 8.8 zoom scalar.</summary>
    private static short Scale(short component, ushort scalar) =>
        unchecked((short)(SnesEightEight.Multiply(component, (short)scalar)));
}
