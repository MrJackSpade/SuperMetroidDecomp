using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Where the ending's colours lie among the screen's colours (#1512): which colours each load, fade, flash and palette-effect
/// group writes, and which sprite group each actor colouring draws in. A screen colour is its place among the screen's
/// background colours, then its sprite colours: the floors' inks, and the background pictures' inks below the sprite colours;
/// a sprite colour less the background colours is the sprite sheets' ink for an own-palette part.
/// </summary>
internal static class EndingScreenInks
{
    /// <summary>The colours of one colour group.</summary>
    internal const int GroupColours = 16;

    /// <summary>The screen's background colours, which its sprite colours follow.</summary>
    internal const int SpriteFirst = 8 * GroupColours;

    /// <summary>The screen's colours.</summary>
    internal const int ScreenColourCount = 2 * SpriteFirst;

    /// <summary>The screen colours each load writes.</summary>
    private static readonly Dictionary<EndingColourLoad, int[]> Loads = new()
    {
        [EndingColourLoad.Escape] = Range(0, ScreenColourCount),
        [EndingColourLoad.ExplosionSprites] = Range(SpriteFirst, SpriteFirst),
        [EndingColourLoad.ExplosionBackgrounds] = Range(0, SpriteFirst),
        [EndingColourLoad.FinalGunship] = Background(5),
        [EndingColourLoad.CreditsBackgrounds] = Range(0, SpriteFirst),
        // The credits' end restores every colour but the first four, which keep the credits' colours.
        [EndingColourLoad.PostCredits] = Range(4, ScreenColourCount - 4),
        [EndingColourLoad.RestoredShot] = Sprite(4),
        [EndingColourLoad.LogoInitial] = Sprite(7),
    };

    /// <summary>The screen colours each faded group scales.</summary>
    private static readonly Dictionary<EndingFade, int[]> Fades = new()
    {
        [EndingFade.ExplosionBackground] = Background(7),
        [EndingFade.ExplosionLava] = Sprite(5),
        [EndingFade.ExplodingPlanet] = Sprite(7),
        [EndingFade.WaitingBackdrop] = Background(2),
        [EndingFade.SuitlessSamus] = Sprite(5),
        [EndingFade.SuitedSamus] = Sprite(6),
        [EndingFade.Shot] = Sprite(4),
        [EndingFade.ShootingSamus] = Sprite(7),
    };

    /// <summary>The screen colours each flash covers.</summary>
    private static readonly Dictionary<EndingFlash, int[]> Flashes = new()
    {
        [EndingFlash.PlanetGlow] = [SpriteFirst + 7 * GroupColours + 14],
        [EndingFlash.SilhouetteBackdrop] = [0],
        [EndingFlash.ExplosionWhiteout] = [0, .. Background(1), SpriteFirst],
        [EndingFlash.WhiteoutCleared] = [0, GroupColours, SpriteFirst],
        [EndingFlash.LogoBackground] = Background(1),
    };

    /// <summary>The logo crossfade's background colours, then its sprite colours.</summary>
    internal static int[] LogoCrossfade { get; } = [.. Background(1), .. Sprite(7)];

    /// <summary>The sprite group each actor colouring draws in.</summary>
    private static readonly Dictionary<EndingSpriteColouring, int> Sets = new()
    {
        [EndingSpriteColouring.ClearTimeDigits] = 0,
        [EndingSpriteColouring.ClearTimeLabel] = 1,
        [EndingSpriteColouring.CompletionText] = 2,
        [EndingSpriteColouring.Clouds] = 5,
        [EndingSpriteColouring.ExplosionLava] = 5,
        [EndingSpriteColouring.ExplodingPlanet] = 7,
        [EndingSpriteColouring.ExplosionAfterglow] = 6,
        [EndingSpriteColouring.AnimalEscape] = 7,
        [EndingSpriteColouring.Logo] = 7,
        [EndingSpriteColouring.SuitlessSamus] = 5,
        [EndingSpriteColouring.SuitedSamus] = 6,
        [EndingSpriteColouring.FallingSamus] = 6,
        [EndingSpriteColouring.ShootingSamus] = 7,
        [EndingSpriteColouring.ShootingStars] = 4,
    };

    /// <summary>
    /// The screen colour each palette-effect group the ending shows writes its first colour to, and the colour after the
    /// last it may write: the end of its colour group.
    /// </summary>
    private static readonly Dictionary<PaletteFxColours, (int First, int End)> PaletteFxGroups = new()
    {
        [PaletteFxColours.WideZebesExplosionForeground] = (1, GroupColours),
        [PaletteFxColours.WideZebesExplosionBackground] = (GroupColours + 1, 2 * GroupColours),
        [PaletteFxColours.ZebesExplosionFinale] = (1, GroupColours),
        [PaletteFxColours.SpaceDuringZebesExplosion] = (0, GroupColours),
        [PaletteFxColours.ExplodingZebesLava] = (4 * GroupColours, 5 * GroupColours),
        [PaletteFxColours.ExplodingZebesCrust] = (4 * GroupColours + 1, 5 * GroupColours),
        [PaletteFxColours.ExplodingZebesGreyClouds] = (5 * GroupColours + 1, 6 * GroupColours),
        [PaletteFxColours.GunshipEmergingFromExplosion] = (5 * GroupColours, 6 * GroupColours),
        [PaletteFxColours.ZebesPlanetAfterglow] = (SpriteFirst + 6 * GroupColours + 1, SpriteFirst + 7 * GroupColours),
        [PaletteFxColours.ZoomedOutExplodingZebes] = (SpriteFirst + 7 * GroupColours, ScreenColourCount),
        [PaletteFxColours.GunshipGlow] = (ScreenColourCount - 1, ScreenColourCount),
        [PaletteFxColours.PostCreditsSuperMetroidIcon] = (SpriteFirst + 7 * GroupColours, ScreenColourCount),
    };

    /// <summary>The screen colours <paramref name="load"/> writes.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The load is not one the ending makes.</exception>
    internal static int[] Load(EndingColourLoad load) => Loads.TryGetValue(load, out int[]? inks)
        ? inks
        : throw new ArgumentOutOfRangeException(nameof(load), load, "Not an ending colour load.");

    /// <summary>The screen colours <paramref name="colours"/> scales.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The group is not one the ending fades.</exception>
    internal static int[] Fade(EndingFade colours) => Fades.TryGetValue(colours, out int[]? inks)
        ? inks
        : throw new ArgumentOutOfRangeException(nameof(colours), colours, "Not a faded ending group.");

    /// <summary>The screen colours <paramref name="flash"/> covers.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The flash is not one the ending shows.</exception>
    internal static int[] Flash(EndingFlash flash) => Flashes.TryGetValue(flash, out int[]? inks)
        ? inks
        : throw new ArgumentOutOfRangeException(nameof(flash), flash, "Not an ending flash.");

    /// <summary>The sprite group <paramref name="colouring"/> draws in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring is not an ending actor colouring.</exception>
    internal static int SpriteSet(EndingSpriteColouring colouring) => Sets.TryGetValue(colouring, out int set)
        ? set
        : throw new ArgumentOutOfRangeException(nameof(colouring), colouring, "Not an ending actor colouring.");

    /// <summary>Where <paramref name="colours"/> writes its first colour, and the colour after the last it may write.</summary>
    /// <exception cref="InvalidDataException">The ending shows a record of a group no ending colour holds.</exception>
    internal static (int First, int End) PaletteFx(PaletteFxColours colours) => PaletteFxGroups.TryGetValue(colours, out var group)
        ? group
        : throw new InvalidDataException($"The ending shows {colours}, which no ending colour holds.");

    /// <summary>The screen colours of background group <paramref name="group"/>.</summary>
    private static int[] Background(int group) => Range(group * GroupColours, GroupColours);

    /// <summary>The screen colours of sprite group <paramref name="group"/>.</summary>
    private static int[] Sprite(int group) => Range(SpriteFirst + group * GroupColours, GroupColours);

    /// <summary><paramref name="count"/> screen colours from <paramref name="first"/>.</summary>
    private static int[] Range(int first, int count) => [.. Enumerable.Range(first, count)];
}
