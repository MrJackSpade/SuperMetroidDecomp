using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending scene's presentation: Mode 7, background and sprite composition into a render snapshot or the legacy raster, drawn from the scene state it belongs to.</summary>
internal static class EndingCreditsPresentation
{
    /// <summary>Resolves the ending display at its existing draw boundary, without rasterization.</summary>
    public static LayeredScene CaptureRenderSnapshot(EndingCreditsState ending)
    {
        if (ending.UsesExplosionFinaleDisplay) return ending.CaptureExplosionFinale();
        var layers = new List<DisplayLayer>();
        EndingObjectSheet objectSheet = CurrentEscapeObjectSheet(ending);
        FramePaint memory;
        if (ending.InstalledArtwork.CurrentEndingLogo is not null)
        {
            if (ending.InstalledArtwork.CurrentEndingLogo.CrossfadeStarted)
                layers.Add(new ScreenTilesLayer(ending.Cells(EndingPlane.Logo, new Extent16<TilemapCells>(32, 32),
                    EndingTileLayer.Logo, TileInkDepth.SixteenColours), new Position16<TilePlanePixels>(0, 0), null));
            layers.Add(new SpritesLayer(ending.InstalledArtwork.CurrentEndingLogo.CrossfadeStarted));
            layers.Add(PostCreditsLayer(ending));
            // The logo assembles over the shooting scene's page.
            return new(CaptureMemory(ending, EndingObjectSheet.RewardFalling), layers.ToArray(), checked((byte)ending.Stage.Brightness));
        }
        if (ending.CreditsRoll.CurrentPostShot is not null)
        {
            // Mode 7 places OBJ priority zero below BG1; the other three priorities
            // are above it. BG1 subtracts the winning OBJ subscreen, without halving.
            layers.Add(new SpriteDepthLayer(0));
            layers.Add(new TurningFloorLayer(PostShotTransform(ending), AffineFloor.Of(ending.FloorCells(), ending.FloorTiles()), true));
            layers.Add(new SpriteDepthLayer(1));
            layers.Add(new SpriteDepthLayer(2));
            layers.Add(new SpriteDepthLayer(3));
            return new(CaptureMemory(ending, CurrentRewardObjectSheet(ending)), layers.ToArray(), checked((byte)ending.Stage.Brightness));
        }
        if (ending.Stage.Phase == EndingCreditsPhase.Credits)
        {
            layers.Add(new ScreenTilesLayer(ending.Cells(EndingPlane.Credits, new Extent16<TilemapCells>(32, 32),
                EndingTileLayer.Credits, TileInkDepth.SixteenColours), ending.Credits.ScrollPosition, null));
            // Credits intentionally do not run the sprite draw routine.
            memory = new FramePaint(ending.screen.Palette.Colors, []);
        }
        else
        {
            if (EndingCreditsPhaseGroups.PostCredits.Contains(ending.Stage.Phase))
            {
                objectSheet = CurrentRewardObjectSheet(ending);
                if (ending.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                    layers.Add(new SpriteDepthLayer(0));
                if (PostCreditsBackgroundEnabled(ending))
                    layers.Add(PostCreditsLayer(ending));
                if (ending.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                    layers.Add(new AddedScreenTilesLayer(ending.Cells(EndingPlane.WavySamus, ScreenTilesViewportLayer.MapSize,
                            EndingTileLayer.WavySamus, TileInkDepth.FourColours),
                        PostCreditsLayer(ending))
                        .WithScrolls(EndingWavySamus.ScrollsAtAge(
                            EndingSequenceDefinitions.Timing.WaitingBackdropFrames - ending.Stage.PhaseTimer)));
            }
            else if (ending.EscapeBackgroundEnabled)
            {
                // Fixed addition belongs to backdrop and eligible OBJ, not BG1.
                if (ending.Flyaway.FlyawayFixedColor is { } backdropWhite) layers.Add(backdropWhite);
                if (UsesFlyawayFloorPriority(ending)) layers.Add(new SpriteDepthLayer(0, ending.Flyaway.FlyawayFixedColor));
                int a = Scale(ReadSine(ending.Stage.FloorAngle.AddRaw(RotationAngle.QuarterTurn.RawValue).ToByteAngle()), ending.Stage.FloorZoom);
                int b = Scale(ReadSine(ending.Stage.FloorAngle.ToByteAngle()), ending.Stage.FloorZoom);
                layers.Add(new TurningFloorLayer(new((short)a, (short)b, unchecked((short)-b), (short)a,
                    CurrentFloorCenter(ending),
                    ending.Stage.FloorPosition.Whole.AsSignedWord(), AtmosphericMapWraps(ending) ? FloorOverflowMode.Wrap : FloorOverflowMode.Transparent),
                    AffineFloor.Of(ending.FloorCells(), ending.FloorTiles()), AddsForeground: AtmosphericMapWraps(ending)));
            }
            memory = CaptureMemory(ending, objectSheet);
            if (EndingObjectsEnabled(ending) && !AtmosphericMapWraps(ending))
            {
                if (UsesFlyawayFloorPriority(ending))
                {
                    layers.Add(new SpriteDepthLayer(1, ending.Flyaway.FlyawayFixedColor));
                    layers.Add(new SpriteDepthLayer(2, ending.Flyaway.FlyawayFixedColor));
                    layers.Add(new SpriteDepthLayer(3, ending.Flyaway.FlyawayFixedColor));
                }
                else if (ending.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop)
                {
                    // Star priority zero sits behind BG2. Other cinematic OBJ remain
                    // above it; the BG3 wave applies only to the background plane.
                    layers.Add(new SpriteDepthLayer(1));
                    layers.Add(new SpriteDepthLayer(2));
                    layers.Add(new SpriteDepthLayer(3));
                }
                else layers.Add(new SpritesLayer(RewardSubscreenAddition(ending), ending.Flyaway.FlyawayFixedColor));
            }
            if (ending.Stage.Phase == EndingCreditsPhase.PostCreditsWhiteFlash)
                layers.Add(EndingFlyaway.WhiteAddition(ending.CreditsRoll.WhiteFlashColor));
        }
        return new(memory, layers.ToArray(), checked((byte)ending.Stage.Brightness));
    }

    /// <summary>Mode 7 matrix and center for the post-shot reward scene.</summary>
    private static AffineTransform PostShotTransform(EndingCreditsState ending)
    {
            int a = Scale(ReadSine(ending.PostShot.Angle.AddRaw(RotationAngle.QuarterTurn.RawValue).ToByteAngle()), ending.PostShot.Scale);
            int b = Scale(ReadSine(ending.PostShot.Angle.ToByteAngle()), ending.PostShot.Scale);
            return new((short)a, (short)b, unchecked((short)-b), (short)a,
                new Offset16<FloorPixels>(EndingPostShotDefinitions.CenterX, EndingPostShotDefinitions.CenterY),
                new Offset16<FloorPixels>(EndingPostShotDefinitions.OffsetX, EndingPostShotDefinitions.OffsetY));
        }

    /// <summary>Selects the cloud sheet before the explosion and the explosion sheet afterward.</summary>
    internal static EndingObjectSheet CurrentEscapeObjectSheet(EndingCreditsState ending) => EndingCreditsPhaseGroups.BeforeZebesExplosion.Contains(ending.Stage.Phase)
        ? EndingObjectSheet.Clouds : EndingObjectSheet.Explosion;

    /// <summary>The OBJ selection the render snapshot needs to draw actors from <paramref name="sheet"/>.</summary>
    internal static byte ObjectSelection(EndingObjectSheet sheet) => sheet switch
    {
        EndingObjectSheet.Clouds => EndingScreenDefinitions.CloudObjectSelection,
        EndingObjectSheet.Explosion => EndingScreenDefinitions.EscapeObjectSelection,
        EndingObjectSheet.Reward => EndingScreenDefinitions.RewardObjectSelection,
        EndingObjectSheet.RewardFalling => EndingScreenDefinitions.FallingRewardObjectSelection,
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined ending object sheet."),
    };

    /// <summary>Whether atmospheric scenes use the Mode 7 wrap policy from SetupPpu_5_Mode7.</summary>
    private static bool AtmosphericMapWraps(EndingCreditsState ending) => EndingCreditsPhaseGroups.BeforeZebesExplosion.Contains(ending.Stage.Phase);

    /// <summary>Whether the current explosion music wait renders a white screen.</summary>
    internal static bool ExplosionWhiteout(EndingCreditsState ending) => ending.Stage.Phase is EndingCreditsPhase.WaitForPlanetEscapeMusic
        or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue;
    /// <summary>Whether flyaway Mode 7 places priority-zero OBJ behind the ship on BG1.</summary>
    private static bool UsesFlyawayFloorPriority(EndingCreditsState ending) => EndingCreditsPhaseGroups.PlanetEscape.Contains(ending.Stage.Phase);
    /// <summary>Whether the current ending phase draws object actors at all.</summary>
    private static bool EndingObjectsEnabled(EndingCreditsState ending) => !ExplosionWhiteout(ending) && ending.Stage.Phase is not
        (EndingCreditsPhase.PostCreditsCopyright or EndingCreditsPhase.PostCreditsWaitingSamus or EndingCreditsPhase.PostCreditsBlank);

    /// <summary>Mode 7 rotation center, shared by atmospheric scenes and changed for flyaway.</summary>
    private static Offset16<FloorPixels> CurrentFloorCenter(EndingCreditsState ending) => EndingCreditsPhaseGroups.BeforePlanetEscape.Contains(ending.Stage.Phase)
        ? EndingMotionDefinitions.AtmosphericFloorCenter
        : EndingMotionDefinitions.FloorCenter;

    /// <summary>Whether the phase displays post-credit BG; the armored reward uses OBJ alone.</summary>
    private static bool PostCreditsBackgroundEnabled(EndingCreditsState ending) => ending.Stage.Phase != EndingCreditsPhase.PostCreditsBlank
        && ending.Stage.Phase != EndingCreditsPhase.PostCreditsFadeIn
        && ending.Stage.Phase != EndingCreditsPhase.PostCreditsGesture
        && ending.Stage.Phase != EndingCreditsPhase.PostCreditsJump
        && !(ending.Stage.Phase == EndingCreditsPhase.PostCreditsReward && ending.Stage.EndingReward == EndingReward.Armored);
    /// <summary>Whether to use the waiting scene's BG2 map and character base.</summary>
    private static bool UsesWaitingBackground(EndingCreditsState ending) => !EndingCreditsPhaseGroups.FromWaitingSamus.Contains(ending.Stage.Phase)
        || ending.Stage.Phase == EndingCreditsPhase.PostCreditsReward;
    /// <summary>Whether native E1D2/E2DD subscreen settings add OBJ to BG2 and backdrop.</summary>
    private static bool RewardSubscreenAddition(EndingCreditsState ending) => ending.Explosion.ExplosionCrossfadeActive
        || ending.Stage.Phase == EndingCreditsPhase.PostCreditsShootingStars
        || (ending.Stage.Phase == EndingCreditsPhase.PostCreditsReward && ending.Stage.EndingReward != EndingReward.Armored);

    /// <summary>The post-credits background plane: the waiting backdrop, or the uploaded text map at its current scroll and height.</summary>
    private static ScreenTilesLayer PostCreditsLayer(EndingCreditsState ending)
    {
        var size = new Extent16<TilemapCells>(32, checked((ushort)ending.CreditsRoll.PostCreditsMapHeight));
        return UsesWaitingBackground(ending)
            ? new(ending.Cells(EndingPlane.Text, size, EndingTileLayer.Waiting, TileInkDepth.SixteenColours), ending.Stage.PostCreditsScroll, null)
            : new(ending.Cells(ending.CreditsRoll.PostCreditsTextTilemap == EndingTextTilemap.ResultPanel ? EndingPlane.Credits : EndingPlane.Text, size,
                EndingTileLayer.PostCreditsText, TileInkDepth.SixteenColours), ending.Stage.PostCreditsScroll, null);
    }

    /// <summary>The scene's colours with this frame's sprite entries over the OBJ page of <paramref name="objectSheet"/>.</summary>
    internal static FramePaint CaptureMemory(EndingCreditsState ending, EndingObjectSheet objectSheet) =>
        new(ending.screen.Palette.Colors, PrepareSprites(ending, objectSheet).Draws);

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
            foreach (EndingSprite wrapper in ending.sprites.OrderByDescending(actor => actor.TraversalSlot))
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
        if (EndingCreditsPhaseGroups.PostCredits.Contains(ending.Stage.Phase)) ending.CreditsRoll.ShootingStars?.Draw(objects, Sheet());
        objects.FinishFrame();
        return objects;

        // A frame that draws nothing needs no sheet.
        SpriteSheet Sheet() => ending.InstalledArtwork.SpriteSheet(EndingSpriteSheets.For(objectSheet, ending.Stage.EndingReward,
            ending.screen.LogoChunksShown));
    }

    /// <summary>Reads a signed sine component from the installed enemy trigonometry table.</summary>
    internal static short ReadSine(ByteAngle angle) => Trigonometry.SignedSine(angle);

    /// <summary>The falling reward sheet once the jumping Samus has switched to it, otherwise the reward sheet.</summary>
    private static EndingObjectSheet CurrentRewardObjectSheet(EndingCreditsState ending) => ending.CreditsRoll.CurrentRewardJump?.FallingSheetSelected == true
        ? EndingObjectSheet.RewardFalling : EndingObjectSheet.Reward;

    /// <summary>Multiplies a signed Mode 7 component by the scene's positive 8.8 zoom scalar.</summary>
    private static short Scale(int component, int scalar) =>
        unchecked((short)EightEight.Multiply(component, scalar));
}
