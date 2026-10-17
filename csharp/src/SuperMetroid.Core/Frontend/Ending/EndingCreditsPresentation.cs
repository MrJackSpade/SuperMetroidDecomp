using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

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
        byte objectSelection = ObjectSelection(CurrentEscapeObjectSheet);
        OamBuffer oam;
        if (_ending.endingLogo is not null)
        {
            if (_ending.endingLogo.CrossfadeStarted)
                layers.Add(new Bg4BppRenderLayer(EndingLogoDefinitions.Tilemap, EndingLogoDefinitions.Characters,
                    new Position16<BgPlanePixels>(0, 0), new Extent16<TilemapCells>(32, 32), null));
            layers.Add(new ObjRenderLayer(_ending.endingLogo.CrossfadeStarted));
            layers.Add(PostCreditsLayer());
            return new(PpuMemorySnapshot.Capture(_ending.vram, _ending.cgram,
                PrepareSprites()), layers.ToArray(), 3, checked((byte)_ending.brightness));
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
                ObjectSelection(CurrentRewardObjectSheet), checked((byte)_ending.brightness));
        }
        if (_ending.Phase == EndingCreditsPhase.Credits)
        {
            layers.Add(new Bg4BppRenderLayer(EndingCreditsRomData.Rendering.CreditsTilemapWord,
                EndingCreditsRomData.Rendering.CreditsCharacterWord, _ending.Credits.ScrollPosition, new Extent16<TilemapCells>(32, 32), null));
            // Credits intentionally do not run the sprite draw routine.
            oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        }
        else
        {
            if (_ending.Phase >= EndingCreditsPhase.PostCreditsBlank)
            {
                objectSelection = ObjectSelection(CurrentRewardObjectSheet);
                if (_ending.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                    layers.Add(new ObjPriorityRenderLayer(0));
                if (PostCreditsBackgroundEnabled)
                    layers.Add(PostCreditsLayer());
                if (_ending.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                    layers.Add(new BgSubscreenAddRenderLayer(EndingWavySamusDefinitions.TilemapWord,
                        EndingWavySamusDefinitions.CharacterWord,
                        PostCreditsLayer())
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
                layers.Add(EndingCreditsState.WhiteAddition(_ending.whiteFlashColor));
        }
        return new(PpuMemorySnapshot.Capture(_ending.vram, _ending.cgram, oam), layers.ToArray(), objectSelection, checked((byte)_ending.brightness));
    }

    /// <summary>Mode 7 matrix and center for the post-shot reward scene.</summary>
    private Mode7RenderRegisters PostShotRegisters
    {
        get
        {
            short a = Scale(ReadSine(_ending.PostShot.Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex), _ending.PostShot.Scale);
            short b = Scale(ReadSine(_ending.PostShot.Angle.TableIndex), _ending.PostShot.Scale);
            return new(a, b, unchecked((short)-b), a,
                new Offset16<Mode7Pixels>(EndingPostShotDefinitions.CenterX, EndingPostShotDefinitions.CenterY),
                new Offset16<Mode7Pixels>(EndingPostShotDefinitions.OffsetX, EndingPostShotDefinitions.OffsetY));
        }
    }

    /// <summary>Selects the cloud sheet before the explosion and the explosion sheet afterward.</summary>
    internal EndingObjectSheet CurrentEscapeObjectSheet => _ending.Phase < EndingCreditsPhase.FadeInZebesExplosion
        ? EndingObjectSheet.Clouds : EndingObjectSheet.Explosion;

    /// <summary>The OBJ selection the render snapshot needs to draw actors from <paramref name="sheet"/>.</summary>
    internal static byte ObjectSelection(EndingObjectSheet sheet) => sheet switch
    {
        EndingObjectSheet.Clouds => EndingCreditsRomData.Rendering.CloudObjectSelection,
        EndingObjectSheet.Explosion => EndingCreditsRomData.Rendering.EscapeObjectSelection,
        EndingObjectSheet.Reward => EndingCreditsRomData.Rendering.RewardObjectSelection,
        EndingObjectSheet.RewardFalling => EndingCreditsRomData.Rendering.FallingRewardObjectSelection,
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined ending object sheet."),
    };

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

    /// <summary>The post-credits background plane: the waiting backdrop, or the uploaded text map at its current scroll and height.</summary>
    private Bg4BppRenderLayer PostCreditsLayer()
    {
        var size = new Extent16<TilemapCells>(32, checked((ushort)_ending.postCreditsMapHeight));
        return UsesWaitingBackground
            ? new(EndingCreditsRomData.Rendering.WaitingTilemapWord, EndingCreditsRomData.Rendering.WaitingCharacterWord,
                _ending.postCreditsScroll, size, null)
            : new(EndingVram.TextTilemapWord(_ending.postCreditsTextTilemap), EndingCreditsRomData.Rendering.PostCreditsCharacterWord,
                _ending.postCreditsScroll, size, null);
    }

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

    /// <summary>The falling reward sheet once the jumping Samus has switched to it, otherwise the reward sheet.</summary>
    private EndingObjectSheet CurrentRewardObjectSheet => _ending.rewardJump?.FallingSheetSelected == true
        ? EndingObjectSheet.RewardFalling : EndingObjectSheet.Reward;

    /// <summary>Multiplies a signed Mode 7 component by the scene's positive 8.8 zoom scalar.</summary>
    private static short Scale(short component, int scalar) =>
        unchecked((short)SnesEightEight.Multiply(component, scalar));
}
