using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending scene's presentation (#1514): its turning floor, pictures, sprite layers and glows as an ordered layered
/// scene, drawn from the scene state it belongs to. The ending's stars lie behind the planet, the gunship and the
/// backdrops they shine through; its other sprites lie in front of the scene's pictures. Each picture and sprite is drawn
/// whole, in its layer's order.
/// </summary>
internal static class EndingCreditsPresentation
{
    /// <summary>Resolves the ending display at its existing draw boundary, without rasterization.</summary>
    /// <exception cref="InvalidOperationException">The ending shows a scene the presentation does not draw.</exception>
    public static LayeredScene ComposeScene(EndingCreditsState ending)
    {
        if (ending.UsesExplosionFinaleDisplay) return ending.CaptureExplosionFinale();
        // The credits intentionally do not run the sprite draw step; the logo, its typed texts and the reward's shot draw
        // their own screens too.
        return ending.Scene.CaptureScreen(ending, Paint(ending));
    }

    /// <summary>
    /// The screen the ending's shared layering draws for the cast, the reward's reveal, gesture and jump, and the white
    /// flash: the post-credits backdrops or the escape's turning floor and atmosphere, and the sprites in their layers.
    /// </summary>
    /// <param name="ending">The ending.</param>
    /// <param name="paint">The frame's paint.</param>
    internal static LayeredScene CaptureSharedScreen(EndingCreditsState ending, FramePaint paint)
    {
        var layers = new List<DisplayLayer>();
        bool waiting = ending.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop;
        RgbColor? skyWhite = FlyawaySkyWhite(ending)?.Colour;
        if (EndingCreditsPhaseGroups.PostCredits.Contains(ending.Phase))
        {
            // The shooting stars cross the sky behind the waiting backdrop.
            if (waiting)
                layers.Add(Sprites(ending, EndingSpriteLayer.Stars));
            if (PostCreditsBackgroundEnabled(ending))
                layers.Add(PostCreditsLayer(ending));
            if (waiting)
            {
                // The wavy Samus glows over the waiting backdrop where it is drawn.
                int age = ending.PhaseHold.Age;
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
            if (FlyawaySkyWhite(ending) is { } backdropWhite) layers.Add(backdropWhite);
            // During the flyaway the exploding planet's afterglow and the starfields lie behind the departing gunship's
            // floor, which every one of them passes behind.
            if (UsesFlyawayFloorPriority(ending.Phase) && ShowsSprites(ending)) layers.Add(Sprites(ending, tint: skyWhite));
            if (ending.Floor() is { } floor)
                {
                    EndingTurningFloor view = ending.Stage.Floor;
                    layers.Add(new TurningFloorLayer(FloorView.Turned(view.Angle, view.Zoom,
                        CurrentFloorCenter(ending.Phase),
                        view.Position.Whole, FloorRepeatsBeyondEdges(ending) ? FloorOverflowMode.Wrap : FloorOverflowMode.Transparent),
                        floor, ShowsAtmosphere(ending.Phase) ? FloorColorBlend.AddForeground : FloorColorBlend.None));
                }
        }
        // Over the atmosphere the clouds are drawn by the turning floor's own glow, which composes the frame's sprites
        // itself; every other scene draws its sprites in its sprite layers.
        if (ShowsAtmosphere(ending.Phase))
            paint = FloorGlowPaint(ending);
        else if (ShowsSprites(ending) && !UsesFlyawayFloorPriority(ending.Phase))
        {
            if (waiting)
                layers.Add(Sprites(ending, EndingSpriteLayer.Actors));
            else layers.Add(Sprites(ending, blend: RewardSpritesGlow(ending) ? SpriteBlend.Glows : SpriteBlend.Drawn));
        }
        if (ending.Scene is EndingWhiteFlashScene { Flash: var flash })
            layers.Add(flash.Layer);
        return new(paint, layers.ToArray(), ending.Stage.Brightness);
    }

    /// <summary>
    /// The reward's shot screen: the shot scene lies behind the turning planet, which darkens where the sprites lie behind it.
    /// </summary>
    internal static LayeredScene CapturePostShot(EndingCreditsState ending, EndingPostShot shot, FramePaint paint)
    {
        var layers = new List<DisplayLayer> { Sprites(ending) };
        if (ending.Floor() is { } floor) layers.Add(new TurningFloorLayer(PostShotTransform(shot), floor, FloorColorBlend.SubtractSprites));
        return new(paint, layers.ToArray(), ending.Stage.Brightness);
    }

    /// <summary>
    /// The final logo's screen: while the logo's picture fades in, the picture with the logo's sprites glowing over it; then
    /// the post-credits text in front.
    /// </summary>
    internal static LayeredScene CaptureLogo(EndingCreditsState ending, EndingLogo logo, FramePaint paint)
    {
        var layers = new List<DisplayLayer>();
        bool crossfading = logo.CrossfadeStarted;
        if (crossfading)
            layers.Add(new PicturedPlaneLayer(ending.ScreenPicture(SpriteSheetId.EndingPostCreditsPictures, EndingPictureNames.Logo, EndingScenery.LogoPicture),
                Offset16<PlanePixels>.Zero));
        // While the logo's picture fades in, its sprites glow over it.
        layers.Add(Sprites(ending, blend: crossfading ? SpriteBlend.Glows : SpriteBlend.Drawn));
        layers.Add(PostCreditsLayer(ending));
        return new(paint, layers.ToArray(), ending.Stage.Brightness);
    }

    /// <summary>How the post-shot reward scene's turning shot picture appears: turned and zoomed about its centre.</summary>
    private static FloorView PostShotTransform(EndingPostShot shot) => FloorView.Turned(shot.Angle,
        shot.Zoom,
        new Offset16<FloorPixels>(EndingPostShotDefinitions.CenterX, EndingPostShotDefinitions.CenterY),
        new Offset16<FloorPixels>(EndingPostShotDefinitions.OffsetX, EndingPostShotDefinitions.OffsetY));

    /// <summary>Whether the phase shows the escape's atmospheric scenes, whose clouds the turning floor's own glow composes.</summary>
    private static bool ShowsAtmosphere(EndingCreditsPhase phase) => EndingCreditsPhaseGroups.BeforeZebesExplosion.Contains(phase);

    /// <summary>
    /// Whether the floor repeats beyond its edges (#1674): from the escape's setup through the planet's explosion and the
    /// music waits after it, until the gunship's planet escape shows nothing beyond its floor's edges.
    /// </summary>
    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "The ending is read only through the narrower capability interface SME6332 asks this parameter to take.")]
    private static bool FloorRepeatsBeyondEdges(IEndingActorScene ending) => EndingCreditsPhaseGroups.BeforePlanetEscape.Contains(ending.Phase);

    /// <summary>Whether the current explosion music wait renders a white screen.</summary>
    internal static bool ExplosionWhiteout(IEndingActorScene ending) => (ending.Phase == EndingCreditsPhase.WaitForPlanetEscapeMusic || ending.Phase == EndingCreditsPhase.WaitForPlanetEscapeMusicQueue);
    /// <summary>Whether the planet escape shows the gunship flying away, the planet's afterglow and the starfields behind it.</summary>
    private static bool UsesFlyawayFloorPriority(EndingCreditsPhase phase) => EndingCreditsPhaseGroups.PlanetEscape.Contains(phase);
    /// <summary>Whether the current ending phase draws its actors and stars at all.</summary>
    private static bool ShowsSprites(EndingCreditsState ending) => !ExplosionWhiteout(ending) && ending.Phase != EndingCreditsPhase.PostCreditsCopyright
        && ending.Phase != EndingCreditsPhase.PostCreditsWaitingSamus && ending.Phase != EndingCreditsPhase.PostCreditsBlank;

    /// <summary>The floor point the floor turns about, shared by atmospheric scenes and changed for flyaway.</summary>
    private static Offset16<FloorPixels> CurrentFloorCenter(EndingCreditsPhase phase) => EndingCreditsPhaseGroups.BeforePlanetEscape.Contains(phase)
        ? EndingMotionDefinitions.AtmosphericFloorCenter
        : EndingMotionDefinitions.FloorCenter;

    /// <summary>Whether the phase shows the post-credits background; the armored reward shows sprites alone.</summary>
    private static bool PostCreditsBackgroundEnabled(EndingCreditsState ending) => ending.Phase != EndingCreditsPhase.PostCreditsBlank
        && ending.Phase != EndingCreditsPhase.PostCreditsFadeIn
        && ending.Phase != EndingCreditsPhase.PostCreditsGesture
        && ending.Phase != EndingCreditsPhase.PostCreditsJump
        && !(ending.Phase == EndingCreditsPhase.PostCreditsReward && ending.Stage.EndingReward == EndingReward.Armored);
    /// <summary>Whether to show the waiting scene's background.</summary>
    private static bool UsesWaitingBackground(EndingCreditsPhase phase) => !EndingCreditsPhaseGroups.FromWaitingSamus.Contains(phase)
        || phase == EndingCreditsPhase.PostCreditsReward;
    /// <summary>
    /// Whether the scene's sprites glow over the background and the sky: during the explosion's crossfade, the shooting
    /// stars and an unarmoured reward.
    /// </summary>
    private static bool RewardSpritesGlow(EndingCreditsState ending) => (ending.Phase == EndingCreditsPhase.FadeInZebesExplosion || ending.Phase == EndingCreditsPhase.ZebesExplosionCrossfade || ending.Phase == EndingCreditsPhase.PostCreditsShootingStars)
        || (ending.Phase == EndingCreditsPhase.PostCreditsReward && ending.Stage.EndingReward != EndingReward.Armored);

    /// <summary>The post-credits background: the waiting backdrop, or the post-credits text, each one unscrolled screen.</summary>
    private static PicturedPlaneLayer PostCreditsLayer(EndingCreditsState ending) =>
        new(UsesWaitingBackground(ending.Phase) ? ending.WaitingPlane() : ending.PostCreditsTextPlane(), Offset16<PlanePixels>.Zero);

    /// <summary>What the ending paints with: its backdrop; its sprites are drawn by its sprite layers (#1514).</summary>
    internal static FramePaint Paint(EndingCreditsState ending) => new(ending.screen.Colours.Backdrop(ending.InstalledArtwork.SpaceBackdrops));

    /// <summary>
    /// The backdrop with this frame's sprite entries, which the atmosphere's turning floor composes in its own glow (#1514).
    /// </summary>
    private static FramePaint FloorGlowPaint(EndingCreditsState ending) =>
        new(ending.screen.Colours.Backdrop(ending.InstalledArtwork.SpaceBackdrops), PrepareSprites(ending).Draws);

    /// <summary>
    /// The sprite layer of this frame's sprites (#1514): those of <paramref name="only"/>'s layer, or all of them when it
    /// is null, shown as <paramref name="blend"/> says and tinted by <paramref name="tint"/>.
    /// </summary>
    internal static SpritesLayer Sprites(EndingCreditsState ending, EndingSpriteLayer? only = null,
        SpriteBlend blend = SpriteBlend.Drawn, RgbColor? tint = null) =>
        new(PrepareSprites(ending, only).Draws, blend, tint);

    /// <summary>The flyaway's white over the sky while the planet escape shows it, or null while it shows none.</summary>
    private static ScreenTintLayer? FlyawaySkyWhite(EndingCreditsState ending) =>
        EndingCreditsPhaseGroups.PlanetEscape.Contains(ending.Phase) ? ending.Flyaway.WhiteWashShown : null;

    /// <summary>
    /// Places the scene's sprite entries at the simulation display boundary without advancing sprite state, each actor's
    /// frames drawn from its own sheet (#1392, #1626), in the ending's colours under its colouring (#1512): those of
    /// <paramref name="only"/>'s sprite layer, or all of them when it is null (#1514).
    /// </summary>
    /// <exception cref="InvalidOperationException">The ending shows a scene the presentation does not draw.</exception>
    private static ScreenSprites PrepareSprites(EndingCreditsState ending, EndingSpriteLayer? only = null)
    {
        var sprites = new ScreenSprites();
        sprites.BeginFrame();
        // A scene's own actors show on the actors' layer; the cast's each on its own role's layer.
        ending.Scene.DrawActors(ending, sprites, Shows);
        if (EndingCreditsPhaseGroups.PostCredits.Contains(ending.Phase) && Shows(EndingSpriteLayer.Stars))
            ending.CreditsRoll.ShootingStars.Draw(sprites, Sheet(ending, EndingSpriteSheets.Reward(ending.Stage.EndingReward)),
                ending.screen.Colours.Drawn(EndingShades.Drawn(EndingShootingStarDefinitions.Colouring)));
        sprites.FinishFrame();
        return sprites;

        // Whether the sprites of the layer are placed.
        bool Shows(EndingSpriteLayer layer) => only is null || only == layer;
    }

    /// <summary>Places the ending's cast, each actor on its own role's layer when <paramref name="shows"/> that layer.</summary>
    internal static void DrawCast(EndingCreditsState ending, ScreenSprites sprites, Func<EndingSpriteLayer, bool> shows)
    {
        var art = new EndingSpriteArt(family => ActorArt(ending, family),
            Sheet(ending, EndingSpriteSheets.Of(EndingTextAnimations.Family, ending.Stage.EndingReward)), ending.screen.Colours.Drawn);
        foreach (EndingSprite wrapper in ending.sprites)
        {
            if (shows(wrapper.Definition.Layer))
                wrapper.Draw(sprites, art);
        }
    }

    /// <summary>Places the jumping Samus's frames, and the falling Samus's once she falls back.</summary>
    internal static void DrawJump(EndingCreditsState ending, ScreenSprites sprites, EndingRewardJump jump) =>
        jump.Draw(sprites, ActorArt(ending, CinematicSpriteFamily.EndingReward),
            new CinematicSpriteSheet(Sheet(ending, EndingSpriteSheets.FallingSamusAndLogo), CinematicSpriteFamily.EndingReward, ending.screen.Colours.Drawn));

    /// <summary>A family's frames from the sheet they lie on, drawn in the ending's colours under the colouring each actor names.</summary>
    internal static IIntroCinematicSpritePresentation ActorArt(EndingCreditsState ending, CinematicSpriteFamily family) =>
        new CinematicSpriteSheet(Sheet(ending, EndingSpriteSheets.Of(family, ending.Stage.EndingReward)), family, ending.screen.Colours.Drawn);

    /// <summary>One of the installed sprite sheets the ending draws from.</summary>
    private static SpriteSheet Sheet(EndingCreditsState ending, SpriteSheetId sheet) => ending.InstalledArtwork.SpriteSheet(sheet);
}
