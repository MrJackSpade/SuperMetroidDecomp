using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// One Tourian boss statue's background animation: the named pictures it shows, the typed animation the statue runs and
/// the values that animation's cues act on.
/// </summary>
public sealed class TourianStatueAnimationDefinition
{

    /// <summary>Only the statue catalog creates definitions.</summary>
    internal TourianStatueAnimationDefinition()
    {
    }

    /// <summary>The statue this animation animates, as the shared statue animation state names it.</summary>
    public required TourianGreyStatues Statue { get; init; }

    /// <summary>The background animation whose frames are the statue's pictures.</summary>
    public ArtworkAnimation Animation => new($"{Statue} statue");

    /// <summary>The event set once this statue has turned grey, and tested to skip straight to the grey picture.</summary>
    public required StoryEvent GreyEvent { get; init; }

    /// <summary>The updates the first idle frame lasts after the statue spawns.</summary>
    public required int FirstFrameDuration { get; init; }

    /// <summary>The area whose defeated bosses decide whether the statue releases.</summary>
    public required AreaId BossArea { get; init; }

    /// <summary>The bosses of <see cref="BossArea"/> whose defeat, any one of them, starts the release.</summary>
    public required DefeatedBosses DefeatedBosses { get; init; }

    /// <summary>The statue with its three eye colours blacked out, which it shows from the start of its release (#1512).</summary>
    public required PictureShade ClearedShade { get; init; }

    /// <summary>The statue's eye, where its release's eye glow and soul appear, and the glow it lights.</summary>
    public required TourianStatueEye Eye { get; init; }

    /// <summary>The palette-FX object spawned during release.</summary>
    public required PaletteFxObject PaletteFx { get; init; }

    /// <summary>The statue greyed, which it shows once its eight colours take the common grey colours (#1512).</summary>
    public required PictureShade GreyShade { get; init; }

    /// <summary>The frame of the statue's animation that shows <paramref name="picture"/>.</summary>
    public ArtworkFrame Picture(TourianStatueTilePicture picture) => new(Animation, $"{picture}");
}
