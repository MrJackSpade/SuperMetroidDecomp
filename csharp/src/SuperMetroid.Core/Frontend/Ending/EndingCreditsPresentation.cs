using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending scene's presentation: Mode 7, background and sprite composition into a render snapshot or the legacy raster, drawn from the scene state it belongs to.</summary>
internal static class EndingCreditsPresentation
{
    /// <summary>Resolves the ending display at its existing draw boundary, without rasterization.</summary>
    public static LayeredRenderSnapshot CaptureRenderSnapshot(EndingCreditsState ending)
    {
        if (ending.UsesExplosionFinaleDisplay) return ending.CaptureExplosionFinale();
        byte[] image = ending.Image();
        var layers = new List<RenderLayer>();
        EndingObjectSheet objectSheet = CurrentEscapeObjectSheet(ending);
        PpuMemorySnapshot memory;
        if (ending.InstalledArtwork.CurrentEndingLogo is not null)
        {
            if (ending.InstalledArtwork.CurrentEndingLogo.CrossfadeStarted)
                layers.Add(new Bg4BppRenderLayer(ending.Cells(image, EndingLogoDefinitions.Tilemap, new Extent16<TilemapCells>(32, 32),
                    EndingPaletteId.PostCredits, EndingLogoDefinitions.Characters, BgBitDepth.FourBpp), new Position16<BgPlanePixels>(0, 0), null));
            layers.Add(new ObjRenderLayer(ending.InstalledArtwork.CurrentEndingLogo.CrossfadeStarted));
            layers.Add(PostCreditsLayer(ending, image));
            // The logo assembles over the shooting scene's page.
            return new(CaptureMemory(ending, image, EndingObjectSheet.RewardFalling), layers.ToArray(), checked((byte)ending.Stage.Brightness));
        }
        if (ending.CreditsRoll.CurrentPostShot is not null)
        {
            // Mode 7 places OBJ priority zero below BG1; the other three priorities
            // are above it. BG1 subtracts the winning OBJ subscreen, without halving.
            layers.Add(new ObjPriorityRenderLayer(0));
            layers.Add(new Mode7RenderLayer(PostShotRegisters(ending), true, Tiles: ending.Mode7Tiles()));
            layers.Add(new ObjPriorityRenderLayer(1));
            layers.Add(new ObjPriorityRenderLayer(2));
            layers.Add(new ObjPriorityRenderLayer(3));
            return new(CaptureMemory(ending, image, CurrentRewardObjectSheet(ending)), layers.ToArray(), checked((byte)ending.Stage.Brightness));
        }
        if (ending.Stage.Phase == EndingCreditsPhase.Credits)
        {
            layers.Add(new Bg4BppRenderLayer(ending.Cells(image, EndingCreditsRomData.Rendering.CreditsTilemapWord, new Extent16<TilemapCells>(32, 32),
                EndingPaletteId.Credits, EndingCreditsRomData.Rendering.CreditsCharacterWord, BgBitDepth.FourBpp), ending.Credits.ScrollPosition, null));
            // Credits intentionally do not run the sprite draw routine.
            memory = new PpuMemorySnapshot(image, ending.screen.Palette.Colors, []);
        }
        else
        {
            if (ending.Stage.Phase >= EndingCreditsPhase.PostCreditsBlank)
            {
                objectSheet = CurrentRewardObjectSheet(ending);
                if (ending.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                    layers.Add(new ObjPriorityRenderLayer(0));
                if (PostCreditsBackgroundEnabled(ending))
                    layers.Add(PostCreditsLayer(ending, image));
                if (ending.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                    layers.Add(new BgSubscreenAddRenderLayer(ending.Cells(image, EndingWavySamusDefinitions.TilemapWord, Bg2BppViewportRenderLayer.MapSize,
                            EndingPaletteId.PostCredits, EndingWavySamusDefinitions.CharacterWord, BgBitDepth.TwoBpp),
                        PostCreditsLayer(ending, image))
                        .WithScrolls(EndingWavySamusDefinitions.ScrollsAtAge(
                            EndingCreditsRomData.Timing.WaitingBackdropFrames - ending.Stage.PhaseTimer)));
            }
            else if (ending.EscapeBackgroundEnabled)
            {
                // Fixed addition belongs to backdrop and eligible OBJ, not BG1.
                if (ending.Flyaway.FlyawayFixedColor is { } backdropWhite) layers.Add(backdropWhite);
                if (UsesFlyawayMode7Priority(ending)) layers.Add(new ObjPriorityRenderLayer(0, ending.Flyaway.FlyawayFixedColor));
                short a = Scale(ReadSine(ending.Stage.Mode7Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex), ending.Stage.Mode7Zoom);
                short b = Scale(ReadSine(ending.Stage.Mode7Angle.TableIndex), ending.Stage.Mode7Zoom);
                layers.Add(new Mode7RenderLayer(new(a, b, unchecked((short)-b), a,
                    CurrentMode7Center(ending),
                    ending.Stage.Mode7.Whole.AsSignedRegister(), WrapOutsideMap: AtmosphericMapWraps(ending)),
                    AddBg1Subscreen: AtmosphericMapWraps(ending), Tiles: ending.Mode7Tiles()));
            }
            memory = CaptureMemory(ending, image, objectSheet);
            if (EndingObjectsEnabled(ending) && !AtmosphericMapWraps(ending))
            {
                if (UsesFlyawayMode7Priority(ending))
                {
                    layers.Add(new ObjPriorityRenderLayer(1, ending.Flyaway.FlyawayFixedColor));
                    layers.Add(new ObjPriorityRenderLayer(2, ending.Flyaway.FlyawayFixedColor));
                    layers.Add(new ObjPriorityRenderLayer(3, ending.Flyaway.FlyawayFixedColor));
                }
                else if (ending.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                {
                    // Star priority zero sits behind BG2. Other cinematic OBJ remain
                    // above it; the BG3 wave applies only to the background plane.
                    layers.Add(new ObjPriorityRenderLayer(1));
                    layers.Add(new ObjPriorityRenderLayer(2));
                    layers.Add(new ObjPriorityRenderLayer(3));
                }
                else layers.Add(new ObjRenderLayer(RewardSubscreenAddition(ending), ending.Flyaway.FlyawayFixedColor));
            }
            if (ending.Stage.Phase == EndingCreditsPhase.PostCreditsWhiteFlash)
                layers.Add(EndingFlyaway.WhiteAddition(ending.CreditsRoll.WhiteFlashColor));
        }
        return new(memory, layers.ToArray(), checked((byte)ending.Stage.Brightness));
    }

    /// <summary>Mode 7 matrix and center for the post-shot reward scene.</summary>
    private static Mode7RenderRegisters PostShotRegisters(EndingCreditsState ending)
    {
            short a = Scale(ReadSine(ending.PostShot.Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex), ending.PostShot.Scale);
            short b = Scale(ReadSine(ending.PostShot.Angle.TableIndex), ending.PostShot.Scale);
            return new(a, b, unchecked((short)-b), a,
                new Offset16<Mode7Pixels>(EndingPostShotDefinitions.CenterX, EndingPostShotDefinitions.CenterY),
                new Offset16<Mode7Pixels>(EndingPostShotDefinitions.OffsetX, EndingPostShotDefinitions.OffsetY));
        }

    /// <summary>Selects the cloud sheet before the explosion and the explosion sheet afterward.</summary>
    internal static EndingObjectSheet CurrentEscapeObjectSheet(EndingCreditsState ending) => ending.Stage.Phase < EndingCreditsPhase.FadeInZebesExplosion
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
    private static bool AtmosphericMapWraps(EndingCreditsState ending) => ending.Stage.Phase < EndingCreditsPhase.FadeInZebesExplosion;

    /// <summary>Whether the current explosion music wait renders a white screen.</summary>
    internal static bool ExplosionWhiteout(EndingCreditsState ending) => ending.Stage.Phase is EndingCreditsPhase.WaitForPlanetEscapeMusic
        or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue;
    /// <summary>Whether flyaway Mode 7 places priority-zero OBJ behind the ship on BG1.</summary>
    private static bool UsesFlyawayMode7Priority(EndingCreditsState ending) => ending.Stage.Phase is >= EndingCreditsPhase.PlanetEscapeFast and < EndingCreditsPhase.Credits;
    /// <summary>Whether the current ending phase draws object actors at all.</summary>
    private static bool EndingObjectsEnabled(EndingCreditsState ending) => !ExplosionWhiteout(ending) && ending.Stage.Phase is not
        (EndingCreditsPhase.PostCreditsCopyright or EndingCreditsPhase.PostCreditsWaitingSamus or EndingCreditsPhase.PostCreditsBlank);

    /// <summary>Mode 7 rotation center, shared by atmospheric scenes and changed for flyaway.</summary>
    private static Offset16<Mode7Pixels> CurrentMode7Center(EndingCreditsState ending) => ending.Stage.Phase < EndingCreditsPhase.PlanetEscapeFast
        ? new(EndingCreditsRomData.Rendering.AtmosphericMode7Center, EndingCreditsRomData.Rendering.AtmosphericMode7Center)
        : new(EndingCreditsRomData.Rendering.Mode7CenterX, EndingCreditsRomData.Rendering.Mode7CenterY);

    /// <summary>Whether the phase displays post-credit BG; the armored reward uses OBJ alone.</summary>
    private static bool PostCreditsBackgroundEnabled(EndingCreditsState ending) => ending.Stage.Phase != EndingCreditsPhase.PostCreditsBlank
        && ending.Stage.Phase != EndingCreditsPhase.PostCreditsFadeIn
        && ending.Stage.Phase != EndingCreditsPhase.PostCreditsGesture
        && ending.Stage.Phase != EndingCreditsPhase.PostCreditsJump
        && !(ending.Stage.Phase == EndingCreditsPhase.PostCreditsReward && ending.Stage.EndingReward == EndingReward.Armored);
    /// <summary>Whether to use the waiting scene's BG2 map and character base.</summary>
    private static bool UsesWaitingBackground(EndingCreditsState ending) => ending.Stage.Phase is < EndingCreditsPhase.PostCreditsWaitingSamus
        or EndingCreditsPhase.PostCreditsReward;
    /// <summary>Whether native E1D2/E2DD subscreen settings add OBJ to BG2 and backdrop.</summary>
    private static bool RewardSubscreenAddition(EndingCreditsState ending) => ending.Explosion.ExplosionCrossfadeActive
        || ending.Stage.Phase == EndingCreditsPhase.PostCreditsShootingStars
        || (ending.Stage.Phase == EndingCreditsPhase.PostCreditsReward && ending.Stage.EndingReward != EndingReward.Armored);

    /// <summary>The post-credits background plane: the waiting backdrop, or the uploaded text map at its current scroll and height.</summary>
    private static Bg4BppRenderLayer PostCreditsLayer(EndingCreditsState ending, byte[] image)
    {
        var size = new Extent16<TilemapCells>(32, checked((ushort)ending.CreditsRoll.PostCreditsMapHeight));
        return UsesWaitingBackground(ending)
            ? new(ending.Cells(image, EndingCreditsRomData.Rendering.WaitingTilemapWord, size,
                EndingPaletteId.PostCredits, EndingCreditsRomData.Rendering.WaitingCharacterWord, BgBitDepth.FourBpp), ending.Stage.PostCreditsScroll, null)
            : new(ending.Cells(image, EndingVram.TextTilemapWord(ending.CreditsRoll.PostCreditsTextTilemap), size,
                EndingPaletteId.PostCredits, EndingCreditsRomData.Rendering.PostCreditsCharacterWord, BgBitDepth.FourBpp), ending.Stage.PostCreditsScroll, null);
    }

    /// <summary>The scene's memory image <paramref name="image"/> with this frame's sprite entries over the OBJ page of <paramref name="objectSheet"/>.</summary>
    internal static PpuMemorySnapshot CaptureMemory(EndingCreditsState ending, byte[] image, EndingObjectSheet objectSheet) =>
        new(image, ending.screen.Palette.Colors, PrepareSprites(ending, objectSheet).Draws);

    /// <summary>
    /// Places the scene's sprite entries at the simulation display boundary without advancing sprite state, each drawn
    /// from the sheet of the characters <paramref name="objectSheet"/>'s page holds now (#1392).
    /// </summary>
    internal static ScreenSprites PrepareSprites(EndingCreditsState ending, EndingObjectSheet objectSheet)
    {
        var objects = new ScreenSprites();
        objects.BeginFrame();
        if (ending.InstalledArtwork.CurrentEndingLogo is not null)
            ending.InstalledArtwork.CurrentEndingLogo.Draw(objects, new CinematicSpriteSheet(Sheet(), CinematicSpriteFamily.EndingLogo));
        else if (ending.CurrentRewardGesture is not null)
            ending.CurrentRewardGesture.Draw(objects, new CinematicSpriteSheet(Sheet(), CinematicSpriteFamily.EndingReward));
        else if (ending.CreditsRoll.CurrentRewardJump is not null)
            ending.CreditsRoll.CurrentRewardJump.Draw(objects, new CinematicSpriteSheet(Sheet(), CinematicSpriteFamily.EndingReward));
        else
        {
            foreach (EndingSprite wrapper in ending.sprites.OrderByDescending(actor => actor.NativeSlot))
            {
                CinematicSpriteFamily family = wrapper.Role.Family() switch
                {
                    EndingSpriteFamily.Cloud => CinematicSpriteFamily.EndingCloud,
                    EndingSpriteFamily.Explosion => CinematicSpriteFamily.EndingExplosion,
                    EndingSpriteFamily.CompletionText => CinematicSpriteFamily.EndingCompletionText,
                    EndingSpriteFamily.Reward => CinematicSpriteFamily.EndingReward,
                    EndingSpriteFamily.AnimalEscape => CinematicSpriteFamily.EndingAnimalEscape,
                    var undefined => throw new InvalidOperationException($"Undefined ending sprite family {undefined}."),
                };
                wrapper.Sprite.Draw(objects, installedArt: new CinematicSpriteSheet(Sheet(), family));
            }
        }
        if (ending.Stage.Phase >= EndingCreditsPhase.PostCreditsBlank) ending.CreditsRoll.ShootingStars?.Draw(objects, Sheet());
        objects.FinishFrame();
        return objects;

        // A frame that draws nothing needs no sheet.
        SpriteSheet Sheet() => ending.InstalledArtwork.SpriteSheet(EndingSpriteSheets.For(objectSheet, ending.Stage.EndingReward,
            ending.screen.LogoTileTransfers));
    }

    /// <summary>Reads a signed sine component from the installed enemy trigonometry table.</summary>
    internal static short ReadSine(byte index) => EnemyTrigonometryTables.SignedSine(new SnesByteAngle(index));

    /// <summary>The falling reward sheet once the jumping Samus has switched to it, otherwise the reward sheet.</summary>
    private static EndingObjectSheet CurrentRewardObjectSheet(EndingCreditsState ending) => ending.CreditsRoll.CurrentRewardJump?.FallingSheetSelected == true
        ? EndingObjectSheet.RewardFalling : EndingObjectSheet.Reward;

    /// <summary>Multiplies a signed Mode 7 component by the scene's positive 8.8 zoom scalar.</summary>
    private static short Scale(short component, int scalar) =>
        unchecked((short)SnesEightEight.Multiply(component, scalar));
}
