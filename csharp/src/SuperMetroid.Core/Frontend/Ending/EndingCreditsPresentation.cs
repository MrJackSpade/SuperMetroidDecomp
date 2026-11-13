using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending scene's presentation (#1514): its turning floor, pictures, sprite layers and glows as an ordered layered
/// scene, drawn from the scene state it belongs to. The ending's stars lie behind the planet, the gunship and the
/// backdrops they shine through; its other sprites lie in front of the scene's pictures. The per-tile and
/// per-sprite-part priority tiers that once interleaved them, and the single pixel that blended where a sprite overlapped
/// a glow, are presentation quirks the port does not keep.
/// </summary>
internal static class EndingCreditsPresentation
{
    /// <summary>Resolves the ending display at its existing draw boundary, without rasterization.</summary>
    public static LayeredScene CaptureRenderSnapshot(EndingCreditsState ending)
    {
        if (ending.UsesExplosionFinaleDisplay) return ending.CaptureExplosionFinale();
        var layers = new List<DisplayLayer>();
        EndingObjectSheet objectSheet = CurrentEscapeObjectSheet(ending);
        FramePaint paint = Paint(ending);
        if (ending.InstalledArtwork.CurrentEndingLogo is not null)
        {
            bool crossfading = ending.InstalledArtwork.CurrentEndingLogo.CrossfadeStarted;
            if (crossfading)
                layers.Add(new PicturedPlaneLayer(ending.ScreenPicture(SpriteSheetId.EndingPostCreditsPictures, EndingPictureNames.Logo, EndingScenery.LogoPicture),
                    Offset16<PlanePixels>.Zero));
            // While the logo's picture fades in, its sprites glow over it. The logo is drawn from the shooting scene's
            // sheet, which holds it.
            layers.Add(Sprites(ending, EndingObjectSheet.RewardFalling, blend: crossfading ? SpriteBlend.Glows : SpriteBlend.Drawn));
            layers.Add(PostCreditsLayer(ending));
            return new(paint, layers.ToArray(), ending.Stage.Brightness);
        }
        if (ending.CreditsRoll.CurrentPostShot is not null)
        {
            // The reward's shot scene lies behind the turning planet, which darkens where the sprites lie behind it.
            layers.Add(Sprites(ending, CurrentRewardObjectSheet(ending)));
            if (ending.Floor() is { } floor) layers.Add(new TurningFloorLayer(PostShotTransform(ending), floor, true));
            return new(paint, layers.ToArray(), ending.Stage.Brightness);
        }
        if (ending.Stage.Phase == EndingCreditsPhase.Credits)
        {
            // Credits intentionally do not run the sprite draw routine.
            layers.Add(new PicturedPlaneLayer(ending.CreditsPlane(), Offset16<PlanePixels>.Of(0, ending.Credits.ScrolledPixels)));
            return new(paint, layers.ToArray(), ending.Stage.Brightness);
        }
        bool waiting = ending.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop;
        RgbColor? skyWhite = FlyawaySkyWhite(ending);
        if (EndingCreditsPhaseGroups.PostCredits.Contains(ending.Stage.Phase))
        {
            objectSheet = CurrentRewardObjectSheet(ending);
            // The shooting stars cross the sky behind the waiting backdrop.
            if (waiting)
                layers.Add(Sprites(ending, objectSheet, EndingSpriteLayer.Stars));
            if (PostCreditsBackgroundEnabled(ending))
                layers.Add(PostCreditsLayer(ending));
            if (waiting)
            {
                // The wavy Samus glows over the waiting backdrop where it is drawn.
                int age = EndingSequenceDefinitions.Timing.WaitingBackdropFrames - ending.Stage.PhaseTimer;
                layers.Add(new AddedPicturedPlaneLayer(ending.WavySamusTexture(), PostCreditsLayer(ending),
                    VerticalScroll: EndingWavySamus.VerticalScrollAtAge(age))
                {
                    HorizontalWave = EndingWavySamus.WaveAtAge(age),
                });
            }
        }
        else if (ending.EscapeBackgroundEnabled)
        {
            // The flyaway's white brightens the sky and the sprites, not the gunship's floor: it is added before any
            // other layer, so it reaches only the backdrop, and the sprite layers carry it as their tint.
            if (ending.Flyaway.FlyawayFixedColor is { } backdropWhite) layers.Add(backdropWhite);
            // During the flyaway the exploding planet's afterglow and the starfields lie behind the departing gunship's
            // floor, which every one of them passes behind.
            if (UsesFlyawayFloorPriority(ending) && EndingObjectsEnabled(ending)) layers.Add(Sprites(ending, objectSheet, tint: skyWhite));
            if (ending.Floor() is { } floor)
                {
                    layers.Add(new TurningFloorLayer(FloorView.Turned(ending.Stage.FloorAngle, ending.Stage.FloorZoom,
                        CurrentFloorCenter(ending),
                        ending.Stage.FloorPosition.Whole.AsSignedOffset(), FloorRepeatsBeyondEdges(ending) ? FloorOverflowMode.Wrap : FloorOverflowMode.Transparent),
                        floor, AddsForeground: ShowsAtmosphere(ending)));
                }
        }
        // Over the atmosphere the clouds are drawn by the turning floor's own glow, which composes the frame's sprites
        // itself; every other scene draws its sprites in its sprite layers.
        if (ShowsAtmosphere(ending))
            paint = FloorGlowPaint(ending, objectSheet);
        else if (EndingObjectsEnabled(ending) && !UsesFlyawayFloorPriority(ending))
        {
            if (waiting)
                layers.Add(Sprites(ending, objectSheet, EndingSpriteLayer.Actors));
            else layers.Add(Sprites(ending, objectSheet, blend: RewardSpritesGlow(ending) ? SpriteBlend.Glows : SpriteBlend.Drawn));
        }
        if (ending.Stage.Phase == EndingCreditsPhase.PostCreditsWhiteFlash)
            layers.Add(WhiteWash.Layer(ending.CreditsRoll.WhiteFlashColor));
        return new(paint, layers.ToArray(), ending.Stage.Brightness);
    }

    /// <summary>How the post-shot reward scene's turning shot picture appears: turned and zoomed about its centre.</summary>
    private static FloorView PostShotTransform(EndingCreditsState ending) => FloorView.Turned(ending.PostShot.Angle,
        ending.PostShot.Zoom,
        new Offset16<FloorPixels>(EndingPostShotDefinitions.CenterX, EndingPostShotDefinitions.CenterY),
        new Offset16<FloorPixels>(EndingPostShotDefinitions.OffsetX, EndingPostShotDefinitions.OffsetY));

    /// <summary>Selects the cloud sheet before the explosion and the explosion sheet afterward.</summary>
    internal static EndingObjectSheet CurrentEscapeObjectSheet(EndingCreditsState ending) => EndingCreditsPhaseGroups.BeforeZebesExplosion.Contains(ending.Stage.Phase)
        ? EndingObjectSheet.Clouds : EndingObjectSheet.Explosion;


    /// <summary>Whether the phase shows the escape's atmospheric scenes, whose clouds the turning floor's own glow composes.</summary>
    private static bool ShowsAtmosphere(EndingCreditsState ending) => EndingCreditsPhaseGroups.BeforeZebesExplosion.Contains(ending.Stage.Phase);

    /// <summary>
    /// Whether the floor repeats beyond its edges (#1674): from the escape's setup through the planet's explosion and the
    /// music waits after it, until the gunship's planet escape shows nothing beyond its floor's edges.
    /// </summary>
    internal static bool FloorRepeatsBeyondEdges(EndingCreditsState ending) => EndingCreditsPhaseGroups.BeforePlanetEscape.Contains(ending.Stage.Phase);

    /// <summary>Whether the current explosion music wait renders a white screen.</summary>
    internal static bool ExplosionWhiteout(EndingCreditsState ending) => ending.Stage.Phase is EndingCreditsPhase.WaitForPlanetEscapeMusic
        or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue;
    /// <summary>Whether the planet escape shows the gunship flying away, the planet's afterglow and the starfields behind it.</summary>
    private static bool UsesFlyawayFloorPriority(EndingCreditsState ending) => EndingCreditsPhaseGroups.PlanetEscape.Contains(ending.Stage.Phase);
    /// <summary>Whether the current ending phase draws object actors at all.</summary>
    private static bool EndingObjectsEnabled(EndingCreditsState ending) => !ExplosionWhiteout(ending) && ending.Stage.Phase is not
        (EndingCreditsPhase.PostCreditsCopyright or EndingCreditsPhase.PostCreditsWaitingSamus or EndingCreditsPhase.PostCreditsBlank);

    /// <summary>The floor point the floor turns about, shared by atmospheric scenes and changed for flyaway.</summary>
    private static Offset16<FloorPixels> CurrentFloorCenter(EndingCreditsState ending) => EndingCreditsPhaseGroups.BeforePlanetEscape.Contains(ending.Stage.Phase)
        ? EndingMotionDefinitions.AtmosphericFloorCenter
        : EndingMotionDefinitions.FloorCenter;

    /// <summary>Whether the phase shows the post-credits background; the armored reward shows sprites alone.</summary>
    private static bool PostCreditsBackgroundEnabled(EndingCreditsState ending) => ending.Stage.Phase != EndingCreditsPhase.PostCreditsBlank
        && ending.Stage.Phase != EndingCreditsPhase.PostCreditsFadeIn
        && ending.Stage.Phase != EndingCreditsPhase.PostCreditsGesture
        && ending.Stage.Phase != EndingCreditsPhase.PostCreditsJump
        && !(ending.Stage.Phase == EndingCreditsPhase.PostCreditsReward && ending.Stage.EndingReward == EndingReward.Armored);
    /// <summary>Whether to show the waiting scene's background.</summary>
    private static bool UsesWaitingBackground(EndingCreditsState ending) => !EndingCreditsPhaseGroups.FromWaitingSamus.Contains(ending.Stage.Phase)
        || ending.Stage.Phase == EndingCreditsPhase.PostCreditsReward;
    /// <summary>Whether the scene's sprites glow over the background and the sky: during the explosion's crossfade, the shooting stars and an unarmoured reward.</summary>
    private static bool RewardSpritesGlow(EndingCreditsState ending) => ending.Explosion.ExplosionCrossfadeActive
        || ending.Stage.Phase == EndingCreditsPhase.PostCreditsShootingStars
        || (ending.Stage.Phase == EndingCreditsPhase.PostCreditsReward && ending.Stage.EndingReward != EndingReward.Armored);

    /// <summary>The post-credits background: the waiting backdrop, or the post-credits text, each one unscrolled screen.</summary>
    private static PicturedPlaneLayer PostCreditsLayer(EndingCreditsState ending) =>
        new(UsesWaitingBackground(ending) ? ending.WaitingPlane() : ending.PostCreditsTextPlane(), Offset16<PlanePixels>.Zero);

    /// <summary>What the ending paints with: its backdrop; its sprites are drawn by its sprite layers (#1514).</summary>
    internal static FramePaint Paint(EndingCreditsState ending) => new(ending.screen.Colours.Backdrop(ending.InstalledArtwork.SpaceBackdrops));

    /// <summary>
    /// The backdrop with this frame's sprite entries drawn from <paramref name="objectSheet"/>, which the atmosphere's
    /// turning floor composes in its own glow (#1514).
    /// </summary>
    private static FramePaint FloorGlowPaint(EndingCreditsState ending, EndingObjectSheet objectSheet) =>
        new(ending.screen.Colours.Backdrop(ending.InstalledArtwork.SpaceBackdrops), PrepareSprites(ending, objectSheet).Draws);

    /// <summary>
    /// The sprite layer of this frame's sprites drawn from <paramref name="objectSheet"/> (#1514): those of
    /// <paramref name="only"/>'s layer, or all of them when it is null, shown as <paramref name="blend"/> says and
    /// tinted by <paramref name="tint"/>.
    /// </summary>
    internal static SpritesLayer Sprites(EndingCreditsState ending, EndingObjectSheet objectSheet, EndingSpriteLayer? only = null,
        SpriteBlend blend = SpriteBlend.Drawn, RgbColor? tint = null) =>
        new(PrepareSprites(ending, objectSheet, only).Draws, blend, tint);

    /// <summary>The colour the flyaway's white sky tints the sprites with, or null while it shows none.</summary>
    private static RgbColor? FlyawaySkyWhite(EndingCreditsState ending) => ending.Flyaway.FlyawayFixedColor?.Colour;

    /// <summary>The sprite layer <paramref name="role"/>'s sprite shows on (#1514): the starfields lie behind, every other sprite in front.</summary>
    /// <exception cref="InvalidOperationException">The role is undefined.</exception>
    private static EndingSpriteLayer LayerOf(EndingSpriteRole role) => role switch
    {
        EndingSpriteRole.ExplosionStars or EndingSpriteRole.ExplosionStarsRight or EndingSpriteRole.ExplosionStarsLeft => EndingSpriteLayer.Stars,
        EndingSpriteRole.CloudRightA or EndingSpriteRole.CloudLeftA or EndingSpriteRole.CloudRightB or EndingSpriteRole.CloudLeftB
            or EndingSpriteRole.CloudTopA or EndingSpriteRole.CloudTopB or EndingSpriteRole.CloudBottomA or EndingSpriteRole.CloudBottomB
            or EndingSpriteRole.ExplodingZebes or EndingSpriteRole.ExplosionLava or EndingSpriteRole.ExplosionGlow
            or EndingSpriteRole.ExplosionSilhouette or EndingSpriteRole.ExplosionAfterglow or EndingSpriteRole.OperationWasText
            or EndingSpriteRole.CompletedSuccessfullyText or EndingSpriteRole.ClearTimeText or EndingSpriteRole.ClearTimeDigit
            or EndingSpriteRole.RewardSamus or EndingSpriteRole.AnimalEscape => EndingSpriteLayer.Actors,
        _ => throw new InvalidOperationException($"Undefined ending sprite role {role}."),
    };

    /// <summary>
    /// Places the scene's sprite entries at the simulation display boundary without advancing sprite state, each drawn
    /// from the sheet of the characters <paramref name="objectSheet"/>'s page holds now (#1392), in the ending's colours
    /// under its colouring (#1512): those of <paramref name="only"/>'s sprite layer, or all of them when it is null (#1514).
    /// </summary>
    internal static ScreenSprites PrepareSprites(EndingCreditsState ending, EndingObjectSheet objectSheet, EndingSpriteLayer? only = null)
    {
        var objects = new ScreenSprites();
        objects.BeginFrame();
        if (ending.InstalledArtwork.CurrentEndingLogo is not null)
        {
            if (Shows(EndingSpriteLayer.Actors))
                ending.InstalledArtwork.CurrentEndingLogo.Draw(objects, Family(CinematicSpriteFamily.EndingLogo));
        }
        else if (ending.CurrentRewardGesture is not null)
        {
            if (Shows(EndingSpriteLayer.Actors))
                ending.CurrentRewardGesture.Draw(objects, Family(CinematicSpriteFamily.EndingReward));
        }
        else if (ending.CreditsRoll.CurrentRewardJump is not null)
        {
            if (Shows(EndingSpriteLayer.Actors))
                ending.CreditsRoll.CurrentRewardJump.Draw(objects, Family(CinematicSpriteFamily.EndingReward));
        }
        else
        {
            foreach (EndingSprite wrapper in ending.sprites)
            {
                if (!Shows(LayerOf(wrapper.Role)))
                    continue;
                CinematicSpriteFamily family = wrapper.Role.Family() switch
                {
                    EndingSpriteFamily.Cloud => CinematicSpriteFamily.EndingCloud,
                    EndingSpriteFamily.Explosion => CinematicSpriteFamily.EndingExplosion,
                    EndingSpriteFamily.CompletionText => CinematicSpriteFamily.EndingCompletionText,
                    EndingSpriteFamily.Reward => CinematicSpriteFamily.EndingReward,
                    EndingSpriteFamily.AnimalEscape => CinematicSpriteFamily.EndingAnimalEscape,
                    var undefined => throw new InvalidOperationException($"Undefined ending sprite family {undefined}."),
                };
                wrapper.Sprite.Draw(objects, installedArt: Family(family));
            }
        }
        if (EndingCreditsPhaseGroups.PostCredits.Contains(ending.Stage.Phase) && Shows(EndingSpriteLayer.Stars))
            ending.CreditsRoll.ShootingStars?.Draw(objects, Sheet(), ending.screen.Colours.Drawn(EndingShades.Drawn(EndingShootingStarDefinitions.Colouring)));
        objects.FinishFrame();
        return objects;

        // Whether the sprites of `layer` are placed.
        bool Shows(EndingSpriteLayer layer) => only is null || only == layer;

        // A family's frames, drawn in the ending's colours under the colouring each actor names; the result text's typed
        // texts are drawn as the glyphs typed so far (#1513).
        IIntroCinematicSpritePresentation Family(CinematicSpriteFamily family) => family == EndingTypedTexts.Family
            ? new TypedTextSpriteSheet(Sheet(), family, ending.screen.Colours.Drawn, EndingTypedTexts.Texts)
            : new CinematicSpriteSheet(Sheet(), family, ending.screen.Colours.Drawn);

        // A frame that draws nothing needs no sheet.
        SpriteSheet Sheet() => ending.InstalledArtwork.SpriteSheet(EndingSpriteSheets.For(objectSheet, ending.Stage.EndingReward));
    }

    /// <summary>The falling reward sheet once the jumping Samus has switched to it, otherwise the reward sheet.</summary>
    internal static EndingObjectSheet CurrentRewardObjectSheet(EndingCreditsState ending) => ending.CreditsRoll.CurrentRewardJump?.FallingSheetSelected == true
        ? EndingObjectSheet.RewardFalling : EndingObjectSheet.Reward;
}
