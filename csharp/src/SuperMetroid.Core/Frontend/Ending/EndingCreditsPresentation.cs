using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending scene's presentation: Mode 7, background and sprite composition into a render snapshot or the legacy raster, drawn from the scene state it belongs to.</summary>
internal static class EndingCreditsPresentation
{
    /// <summary>Resolves the ending display at its existing draw boundary, without rasterization.</summary>
    public static LayeredRenderSnapshot CaptureRenderSnapshot(EndingCreditsState ending)
    {
        if (ending.UsesExplosionFinaleDisplay) return ending.CaptureExplosionFinale();
        var layers = new List<RenderLayer>();
        byte objectSelection = ObjectSelection(CurrentEscapeObjectSheet(ending));
        PpuMemorySnapshot memory;
        if (ending.CurrentEndingLogo is not null)
        {
            if (ending.CurrentEndingLogo.CrossfadeStarted)
                layers.Add(new Bg4BppRenderLayer(EndingLogoDefinitions.Tilemap, EndingLogoDefinitions.Characters,
                    new Position16<BgPlanePixels>(0, 0), new Extent16<TilemapCells>(32, 32), null));
            layers.Add(new ObjRenderLayer(ending.CurrentEndingLogo.CrossfadeStarted));
            layers.Add(PostCreditsLayer(ending));
            return new(CaptureMemory(ending, 3), layers.ToArray(), 3, checked((byte)ending.Brightness));
        }
        if (ending.CurrentPostShot is not null)
        {
            // Mode 7 places OBJ priority zero below BG1; the other three priorities
            // are above it. BG1 subtracts the winning OBJ subscreen, without halving.
            layers.Add(new ObjPriorityRenderLayer(0));
            layers.Add(new Mode7RenderLayer(PostShotRegisters(ending), true));
            layers.Add(new ObjPriorityRenderLayer(1));
            layers.Add(new ObjPriorityRenderLayer(2));
            layers.Add(new ObjPriorityRenderLayer(3));
            return new(CaptureMemory(ending, ObjectSelection(CurrentRewardObjectSheet(ending))), layers.ToArray(),
                ObjectSelection(CurrentRewardObjectSheet(ending)), checked((byte)ending.Brightness));
        }
        if (ending.Phase == EndingCreditsPhase.Credits)
        {
            layers.Add(new Bg4BppRenderLayer(EndingCreditsRomData.Rendering.CreditsTilemapWord,
                EndingCreditsRomData.Rendering.CreditsCharacterWord, ending.Credits.ScrollPosition, new Extent16<TilemapCells>(32, 32), null));
            // Credits intentionally do not run the sprite draw routine.
            memory = new PpuMemorySnapshot(ending.Image(), ending.screen.Palette.Colors, []);
        }
        else
        {
            if (ending.Phase >= EndingCreditsPhase.PostCreditsBlank)
            {
                objectSelection = ObjectSelection(CurrentRewardObjectSheet(ending));
                if (ending.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                    layers.Add(new ObjPriorityRenderLayer(0));
                if (PostCreditsBackgroundEnabled(ending))
                    layers.Add(PostCreditsLayer(ending));
                if (ending.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                    layers.Add(new BgSubscreenAddRenderLayer(EndingWavySamusDefinitions.TilemapWord,
                        EndingWavySamusDefinitions.CharacterWord,
                        PostCreditsLayer(ending))
                        .WithScrolls(EndingWavySamusDefinitions.ScrollsAtAge(
                            EndingCreditsRomData.Timing.WaitingBackdropFrames - ending.PhaseTimer)));
            }
            else if (ending.EscapeBackgroundEnabled)
            {
                // Fixed addition belongs to backdrop and eligible OBJ, not BG1.
                if (ending.FlyawayFixedColor is { } backdropWhite) layers.Add(backdropWhite);
                if (UsesFlyawayMode7Priority(ending)) layers.Add(new ObjPriorityRenderLayer(0, ending.FlyawayFixedColor));
                short a = Scale(ReadSine(ending.Mode7Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex), ending.Mode7Zoom);
                short b = Scale(ReadSine(ending.Mode7Angle.TableIndex), ending.Mode7Zoom);
                layers.Add(new Mode7RenderLayer(new(a, b, unchecked((short)-b), a,
                    CurrentMode7Center(ending),
                    ending.Mode7.Whole.AsSignedRegister(), WrapOutsideMap: AtmosphericMapWraps(ending)),
                    AddBg1Subscreen: AtmosphericMapWraps(ending)));
            }
            memory = CaptureMemory(ending, objectSelection);
            if (EndingObjectsEnabled(ending) && !AtmosphericMapWraps(ending))
            {
                if (UsesFlyawayMode7Priority(ending))
                {
                    layers.Add(new ObjPriorityRenderLayer(1, ending.FlyawayFixedColor));
                    layers.Add(new ObjPriorityRenderLayer(2, ending.FlyawayFixedColor));
                    layers.Add(new ObjPriorityRenderLayer(3, ending.FlyawayFixedColor));
                }
                else if (ending.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                {
                    // Star priority zero sits behind BG2. Other cinematic OBJ remain
                    // above it; the BG3 wave applies only to the background plane.
                    layers.Add(new ObjPriorityRenderLayer(1));
                    layers.Add(new ObjPriorityRenderLayer(2));
                    layers.Add(new ObjPriorityRenderLayer(3));
                }
                else layers.Add(new ObjRenderLayer(RewardSubscreenAddition(ending), ending.FlyawayFixedColor));
            }
            if (ending.Phase == EndingCreditsPhase.PostCreditsWhiteFlash)
                layers.Add(EndingCreditsState.WhiteAddition(ending.WhiteFlashColor));
        }
        return new(memory, layers.ToArray(), objectSelection, checked((byte)ending.Brightness));
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
    internal static EndingObjectSheet CurrentEscapeObjectSheet(EndingCreditsState ending) => ending.Phase < EndingCreditsPhase.FadeInZebesExplosion
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
    private static bool AtmosphericMapWraps(EndingCreditsState ending) => ending.Phase < EndingCreditsPhase.FadeInZebesExplosion;

    /// <summary>Whether the current explosion music wait renders a white screen.</summary>
    internal static bool ExplosionWhiteout(EndingCreditsState ending) => ending.Phase is EndingCreditsPhase.WaitForPlanetEscapeMusic
        or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue;
    /// <summary>Whether flyaway Mode 7 places priority-zero OBJ behind the ship on BG1.</summary>
    private static bool UsesFlyawayMode7Priority(EndingCreditsState ending) => ending.Phase is >= EndingCreditsPhase.PlanetEscapeFast and < EndingCreditsPhase.Credits;
    /// <summary>Whether the current ending phase draws object actors at all.</summary>
    private static bool EndingObjectsEnabled(EndingCreditsState ending) => !ExplosionWhiteout(ending) && ending.Phase is not
        (EndingCreditsPhase.PostCreditsCopyright or EndingCreditsPhase.PostCreditsWaitingSamus or EndingCreditsPhase.PostCreditsBlank);

    /// <summary>Mode 7 rotation center, shared by atmospheric scenes and changed for flyaway.</summary>
    private static Offset16<Mode7Pixels> CurrentMode7Center(EndingCreditsState ending) => ending.Phase < EndingCreditsPhase.PlanetEscapeFast
        ? new(EndingCreditsRomData.Rendering.AtmosphericMode7Center, EndingCreditsRomData.Rendering.AtmosphericMode7Center)
        : new(EndingCreditsRomData.Rendering.Mode7CenterX, EndingCreditsRomData.Rendering.Mode7CenterY);

    /// <summary>Whether the phase displays post-credit BG; the armored reward uses OBJ alone.</summary>
    private static bool PostCreditsBackgroundEnabled(EndingCreditsState ending) => ending.Phase != EndingCreditsPhase.PostCreditsBlank
        && ending.Phase != EndingCreditsPhase.PostCreditsFadeIn
        && ending.Phase != EndingCreditsPhase.PostCreditsGesture
        && ending.Phase != EndingCreditsPhase.PostCreditsJump
        && !(ending.Phase == EndingCreditsPhase.PostCreditsReward && ending.EndingReward == EndingReward.Armored);
    /// <summary>Whether to use the waiting scene's BG2 map and character base.</summary>
    private static bool UsesWaitingBackground(EndingCreditsState ending) => ending.Phase is < EndingCreditsPhase.PostCreditsWaitingSamus
        or EndingCreditsPhase.PostCreditsReward;
    /// <summary>Whether native E1D2/E2DD subscreen settings add OBJ to BG2 and backdrop.</summary>
    private static bool RewardSubscreenAddition(EndingCreditsState ending) => ending.ExplosionCrossfadeActive
        || ending.Phase == EndingCreditsPhase.PostCreditsShootingStars
        || (ending.Phase == EndingCreditsPhase.PostCreditsReward && ending.EndingReward != EndingReward.Armored);

    /// <summary>The post-credits background plane: the waiting backdrop, or the uploaded text map at its current scroll and height.</summary>
    private static Bg4BppRenderLayer PostCreditsLayer(EndingCreditsState ending)
    {
        var size = new Extent16<TilemapCells>(32, checked((ushort)ending.PostCreditsMapHeight));
        return UsesWaitingBackground(ending)
            ? new(EndingCreditsRomData.Rendering.WaitingTilemapWord, EndingCreditsRomData.Rendering.WaitingCharacterWord,
                ending.PostCreditsScroll, size, null)
            : new(EndingVram.TextTilemapWord(ending.PostCreditsTextTilemap), EndingCreditsRomData.Rendering.PostCreditsCharacterWord,
                ending.PostCreditsScroll, size, null);
    }

    /// <summary>The scene's memories with this frame's sprite entries over the OBJ page <paramref name="objectSelection"/> selects.</summary>
    internal static PpuMemorySnapshot CaptureMemory(EndingCreditsState ending, byte objectSelection) =>
        new(ending.Image(), ending.screen.Palette.Colors, PrepareSprites(ending, objectSelection).Draws);

    /// <summary>Places the scene's sprite entries at the simulation display boundary without advancing sprite state.</summary>
    internal static ScreenSprites PrepareSprites(EndingCreditsState ending, byte objectSelection)
    {
        var objects = new ScreenSprites(ObjCharacterImage.Page(ending.Image(), objectSelection), ObjectSelectionCodec.Sizes(objectSelection));
        objects.BeginFrame();
        if (ending.CurrentEndingLogo is not null) ending.CurrentEndingLogo.Draw(objects, ending.ObjectArtwork?.LogoSprites);
        else if (ending.CurrentRewardGesture is not null) ending.CurrentRewardGesture.Draw(objects, ending.ObjectArtwork?.RewardSprites);
        else if (ending.CurrentRewardJump is not null) ending.CurrentRewardJump.Draw(objects, ending.ObjectArtwork?.RewardSprites);
        else
        {
            foreach (EndingSprite wrapper in ending.sprites.OrderByDescending(actor => actor.NativeSlot))
            {
                IIntroCinematicSpritePresentation? spriteArt = wrapper.Role.Family() switch
                {
                    EndingSpriteFamily.Cloud => ending.ObjectArtwork?.CloudSprites,
                    EndingSpriteFamily.Explosion => ending.ObjectArtwork?.ExplosionSprites,
                    EndingSpriteFamily.CompletionText => ending.ObjectArtwork?.CompletionTextSprites,
                    EndingSpriteFamily.Reward => ending.ObjectArtwork?.RewardSprites,
                    EndingSpriteFamily.AnimalEscape => EndingAnimalEscapeDefinitions.Presentation,
                    var family => throw new InvalidOperationException($"Undefined ending sprite family {family}."),
                };
                wrapper.Sprite.Draw(objects, installedArt: spriteArt);
            }
        }
        if (ending.Phase >= EndingCreditsPhase.PostCreditsBlank) ending.ShootingStars?.Draw(objects);
        objects.FinishFrame();
        return objects;
    }

    /// <summary>Reads a signed sine component from the installed enemy trigonometry table.</summary>
    internal static short ReadSine(byte index) => EnemyTrigonometryTables.SignedSine(new SnesByteAngle(index));

    /// <summary>The falling reward sheet once the jumping Samus has switched to it, otherwise the reward sheet.</summary>
    private static EndingObjectSheet CurrentRewardObjectSheet(EndingCreditsState ending) => ending.CurrentRewardJump?.FallingSheetSelected == true
        ? EndingObjectSheet.RewardFalling : EndingObjectSheet.Reward;

    /// <summary>Multiplies a signed Mode 7 component by the scene's positive 8.8 zoom scalar.</summary>
    private static short Scale(short component, int scalar) =>
        unchecked((short)SnesEightEight.Multiply(component, scalar));
}
