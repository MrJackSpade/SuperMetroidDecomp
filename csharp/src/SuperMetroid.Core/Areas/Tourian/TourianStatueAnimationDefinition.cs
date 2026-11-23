using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// One Tourian boss statue's background animation: the named pictures it shows, the typed animation the statue runs and
/// the values that animation's cues act on.
/// </summary>
public sealed partial class TourianStatueAnimationDefinition
{
    /// <summary>
    /// The installed name of each statue's background animation (#2511), written out so renaming a statue's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly IdentityTable<TourianStatue, string> AnimationKeys = new(new Dictionary<TourianStatue, string>
        {
            [TourianStatue.Phantoon] = "Phantoon statue",
            [TourianStatue.Ridley] = "Ridley statue",
            [TourianStatue.Kraid] = "Kraid statue",
            [TourianStatue.Draygon] = "Draygon statue",
        });

    /// <summary>
    /// The installed name of each statue picture's frame (#2511), written out so renaming a picture's identity never changes
    /// a name on disk.
    /// </summary>
    private static readonly IdentityTable<TourianStatueBlockPicture, string> PictureKeys =
        new(new Dictionary<TourianStatueBlockPicture, string>
        {
            [TourianStatueBlockPicture.EyesBright] = "EyesBright",
            [TourianStatueBlockPicture.EyesDim] = "EyesDim",
            [TourianStatueBlockPicture.EyesDark] = "EyesDark",
            [TourianStatueBlockPicture.Released] = "Released",
        });

    /// <summary>Only the statue catalog creates definitions.</summary>
    internal TourianStatueAnimationDefinition()
    {
    }

    /// <summary>The statue this animation animates, as the shared statue animation state names it.</summary>
    public required TourianStatue Statue { get; init; }

    /// <summary>The background animation whose frames are the statue's pictures.</summary>
    public ArtworkAnimation Animation => new(AnimationKeys[Statue]);

    /// <summary>The event set once this statue has turned grey, and tested to skip straight to the grey picture.</summary>
    public required StoryEvent GreyEvent { get; init; }

    /// <summary>The updates the first idle frame lasts after the statue spawns.</summary>
    public required int FirstFrameDuration { get; init; }

    /// <summary>The area whose defeated bosses decide whether the statue releases.</summary>
    public required AreaId BossArea { get; init; }

    /// <summary>The bosses of <see cref="BossArea"/> whose defeat, any one of them, starts the release.</summary>
    public required DefeatedBosses DefeatedBosses { get; init; }

    /// <summary>The statue with its eyes blacked out, which it shows from the start of its release (#1512).</summary>
    public required PictureShade ClearedShade { get; init; }

    /// <summary>The statue's eye, where its release's eye glow and soul appear, and the glow it lights.</summary>
    public required TourianStatueEye Eye { get; init; }

    /// <summary>The colour effect spawned during release, the statue's grey-out.</summary>
    public required ColourEffectId ColourEffect { get; init; }

    /// <summary>
    /// The entrance statue's sprite statue this statue is, whose own draws show its grey-out (#2288, #2481); null for the
    /// Kraid and Draygon statues, drawn in the room's background, whose grey-out recolours the room's pictures.
    /// </summary>
    public required TourianSpriteStatue? GreyedSprite { get; init; }

    /// <summary>The statue greyed, which a statue released on an earlier visit shows (#1512).</summary>
    public required PictureShade GreyShade { get; init; }

    /// <summary>The frame of the statue's animation that shows <paramref name="picture"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The picture has no installed name.</exception>
    public ArtworkFrame Picture(TourianStatueBlockPicture picture) => new(Animation, PictureKeys[picture]);
}
