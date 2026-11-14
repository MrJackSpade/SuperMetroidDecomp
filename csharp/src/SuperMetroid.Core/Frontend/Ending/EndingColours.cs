using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the running ending scene's colours show (#1512, #1544, #1597), derived from the scene's current colour state: the
/// scene's own colours, the loads within it that restore some of them, each fade at its current level, the flashes the
/// scene shows, the logo crossfade's step, and the record each of the ending's running or settled palette effects shows.
/// A scene's colours are its own: a scene starts in its whole screen of colours, and nothing an earlier scene showed shows
/// in it. Each background picture and floor the ending draws is shaded in those authored for it, and an actor's pictures
/// in those authored for it under its colouring (#1544). Which colours each covers is the import's.
/// </summary>
/// <remarks>
/// The shades lie in a fixed order, the order the ending's scenes bring them in: the scene's colours, the loads that
/// restore some of them, the fades, the flashes, the logo crossfade and the palette effects. A load that restores colours
/// ends the effects whose colours it restores (<see cref="EndingColourOwnerDefinitions.Restores"/>), so no shade depends
/// on which was shown last.
/// </remarks>
[RoomScopeBoundary("The ending's colours own the ending's colour effects for the cinematic's lifetime; no gameplay room is loaded during it.")]
internal sealed class EndingColours
{
    /// <summary>The running scene's whole screen of colours, or none before the first scene.</summary>
    private EndingColourLoad? scene;

    /// <summary>The backdrop the running scene shows of its own, black before the first scene.</summary>
    private RgbColor sceneBackdrop = RgbColor.Black;

    /// <summary>The loads within the running scene that restore some of its colours.</summary>
    private readonly HashSet<EndingColourLoad> restored = [];

    /// <summary>The palette-effect colours whose settled look a load within the running scene replaced.</summary>
    private readonly HashSet<ColourEffectColours> restoredColourEffects = [];

    /// <summary>Each fade the running scene shows, at its current level.</summary>
    private readonly Dictionary<EndingFade, int> fades = [];

    /// <summary>Each flash the running scene shows, with the backdrop colour it shows behind every layer, if any.</summary>
    private readonly Dictionary<EndingFlash, RgbColor?> flashes = [];

    /// <summary>The logo crossfade's current step, or none before the crossfade.</summary>
    private int? logoCrossfadeStep;

    /// <summary>The ending's palette effects, which each scene boundary clears.</summary>
    internal ColourEffectSystem ColourEffects { get; private set; } = new(new ColourEffectSessionState());

    /// <summary>
    /// Shows the installed colours <paramref name="load"/>. A scene's whole screen of colours starts that scene, with
    /// <paramref name="backdrop"/> as its backdrop: nothing shown before it shows any more. A load within a scene restores
    /// some of its colours and ends the effects in them.
    /// </summary>
    /// <exception cref="ArgumentException">A scene's load names no backdrop, or a load within a scene names one.</exception>
    /// <exception cref="InvalidOperationException">A load within a scene restores colours a running palette effect still animates.</exception>
    internal void Load(EndingColourLoad load, RgbColor? backdrop)
    {
        if (StartsScene(load))
        {
            scene = load;
            sceneBackdrop = backdrop ?? throw new ArgumentException($"The ending's {load} scene names no backdrop.", nameof(backdrop));
            restored.Clear();
            restoredColourEffects.Clear();
            fades.Clear();
            flashes.Clear();
            logoCrossfadeStep = null;
            return;
        }
        if (backdrop is not null)
            throw new ArgumentException($"The ending's {load} load restores colours within its scene and shows no backdrop of its own.", nameof(backdrop));
        EndingRestoredEffects restores = EndingColourOwnerDefinitions.Restores.TryGetValue(load, out EndingRestoredEffects? listed)
            ? listed
            : throw new InvalidDataException($"The ending's {load} load lists nothing it restores.");
        foreach (EndingFade fade in restores.Fades)
            fades.Remove(fade);
        foreach (ColourEffectColours colours in restores.ColourEffects)
        {
            if (ColourEffects.Animates(colours))
                throw new InvalidOperationException($"The ending's {load} load restores {colours}, which a running palette effect still animates.");
            restoredColourEffects.Add(colours);
        }
        restored.Add(load);
    }

    /// <summary>Shows <paramref name="colours"/> at fade <paramref name="level"/> of <see cref="EndingShades.FadeLevels"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The level is outside the fade.</exception>
    internal void Fade(EndingFade colours, int level) =>
        fades[colours] = Ensure.BetweenInclusive(0, EndingShades.FadeLevels, level);

    /// <summary>Shows <paramref name="flash"/>, and <paramref name="backdrop"/> behind every layer when the flash covers it.</summary>
    internal void Flash(EndingFlash flash, RgbColor? backdrop) => flashes[flash] = backdrop;

    /// <summary>Shows the logo crossfade's <paramref name="step"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step is outside the crossfade.</exception>
    internal void LogoCrossfade(int step) =>
        logoCrossfadeStep = Ensure.BetweenInclusive(0, EndingShades.LogoCrossfadeSteps - 1, step);

    /// <summary>
    /// Clears the palette effects at a scene boundary (#1544): every effect ends, so the records they showed stop showing.
    /// </summary>
    internal void ResetColourEffects() => ColourEffects = new ColourEffectSystem(new ColourEffectSessionState());

    /// <summary>
    /// The solid colour shown behind every layer, where nothing is drawn (#1597): the scene's own backdrop, the white the
    /// explosion's silhouette flashes it to, the space the explosion's burst shows behind the planet, the explosion's
    /// whiteout and the flyaway's clearing of it to black, each over those the scene brings in before it.
    /// </summary>
    /// <param name="space">The installed backdrops of the space behind the exploding planet.</param>
    /// <exception cref="InvalidOperationException">The space behind the exploding planet shows while its backdrops are not installed.</exception>
    internal RgbColor Backdrop(EndingSpaceBackdrops? space)
    {
        RgbColor backdrop = sceneBackdrop;
        if (FlashBackdrop(EndingFlash.SilhouetteBackdrop) is { } silhouette)
            backdrop = silhouette;
        if (ColourEffects.FrameOf(EndingColourEffectDefinitions.SpaceColours) is { } frame)
        {
            backdrop = (space ?? throw new InvalidOperationException(
                "The ending's explosion requires the installed backdrops of the space behind the exploding planet.")).Of(frame);
        }
        if (FlashBackdrop(EndingFlash.ExplosionWhiteout) is { } whiteout)
            backdrop = whiteout;
        if (FlashBackdrop(EndingFlash.WhiteoutCleared) is { } cleared)
            backdrop = cleared;
        return backdrop;
    }

    /// <summary>Every shade the running scene shows now, in the order the scenes bring them in, whoever each is authored for.</summary>
    internal IReadOnlyList<PictureShade> Shades
    {
        get
        {
            var shades = new List<PictureShade>();
            if (scene is { } own)
                shades.Add(EndingShades.Loaded(own));
            foreach (EndingColourLoad load in Enum.GetValues<EndingColourLoad>())
            {
                if (restored.Contains(load))
                    shades.Add(EndingShades.Loaded(load));
            }
            foreach (EndingFade fade in Enum.GetValues<EndingFade>())
            {
                if (fades.TryGetValue(fade, out int level))
                    shades.Add(EndingShades.Faded(fade, level));
            }
            foreach (EndingFlash flash in Enum.GetValues<EndingFlash>())
            {
                if (flashes.ContainsKey(flash))
                    shades.Add(EndingShades.Flashed(flash));
            }
            if (logoCrossfadeStep is { } step)
                shades.Add(EndingShades.LogoCrossfade(step));
            foreach ((ColourEffectColours colours, ColourEffectFrame frame) in ColourEffects.Shown)
            {
                if (!restoredColourEffects.Contains(colours))
                    shades.Add(ColourEffectShades.Of(colours, frame));
            }
            return shades;
        }
    }

    /// <summary>
    /// The shading <paramref name="scenery"/>, a background picture or floor, shows (#1544): each shade shown that is
    /// authored for it (<see cref="EndingColourOwners"/>), in order. A shade authored for an actor or for other scenery is
    /// not shown on it, even where the two share a colour group.
    /// </summary>
    /// <exception cref="InvalidDataException">A shade shown is of no colour effect the ending shows.</exception>
    internal PictureShading SceneryShading(EndingScenery scenery)
    {
        IReadOnlyList<PictureShade> shown = Shades;
        ImmutableArray<PictureShade>.Builder shades = ImmutableArray.CreateBuilder<PictureShade>(shown.Count);
        foreach (PictureShade screen in shown)
        {
            if (EndingColourOwners.Shows(scenery, screen))
                shades.Add(screen);
        }
        return new PictureShading(shades.ToImmutable(), []);
    }

    /// <summary>
    /// The shading an actor's frame shows when it draws in the colouring whose drawn shade is <paramref name="drawn"/>
    /// (#1544): that shade, then each shade shown that is authored for that colouring's actors
    /// (<see cref="EndingColourOwners"/>), under the colouring, in order. A shade authored for another actor is not shown on
    /// the frame, even where the two once shared a colour group.
    /// </summary>
    /// <exception cref="InvalidDataException">The shade is no ending actor colouring's drawn shade, or a shade shown is of no colour effect the ending shows.</exception>
    internal PictureShading Drawn(PictureShade drawn)
    {
        EndingSpriteColouring colouring = EndingColourOwners.ColouringOf(drawn);
        IReadOnlyList<PictureShade> shown = Shades;
        ImmutableArray<PictureShade>.Builder shades = ImmutableArray.CreateBuilder<PictureShade>(1 + shown.Count);
        shades.Add(drawn);
        foreach (PictureShade screen in shown)
        {
            if (EndingColourOwners.Shows(colouring, screen))
                shades.Add(EndingShades.Under(drawn, screen));
        }
        return new PictureShading(shades.ToImmutable(), []);
    }

    /// <summary>Whether <paramref name="load"/> is a scene's whole screen of colours, which starts that scene.</summary>
    private static bool StartsScene(EndingColourLoad load) =>
        load is EndingColourLoad.Escape or EndingColourLoad.Explosion or EndingColourLoad.Credits or EndingColourLoad.PostCredits;

    /// <summary>The backdrop colour <paramref name="flash"/> shows while the scene shows it, or none.</summary>
    private RgbColor? FlashBackdrop(EndingFlash flash) => flashes.TryGetValue(flash, out RgbColor? backdrop) ? backdrop : null;
}
