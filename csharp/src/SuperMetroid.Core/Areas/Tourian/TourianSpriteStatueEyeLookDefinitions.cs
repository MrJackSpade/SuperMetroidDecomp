using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The looks of the sprite statues' eyes (#2625): each statue's eye animation copies its eye pictures over the characters its
/// sprite is drawn from, as it copies the background statues' over the room's tiles, so the statue's sprite shows the eye
/// picture its animation shows. Each statue is an encounter whose looks are its eye pictures.
/// </summary>
internal static partial class TourianSpriteStatueEyeLookDefinitions
{
    /// <summary>The encounter whose looks each sprite statue's eyes show.</summary>
    internal static IReadOnlyDictionary<TourianSpriteStatue, EncounterArtwork> Encounters { get; } = new Dictionary<TourianSpriteStatue, EncounterArtwork>
    {
        [TourianSpriteStatue.Ridley] = EncounterArtwork.RidleyStatue,
        [TourianSpriteStatue.Phantoon] = EncounterArtwork.PhantoonStatue,
    };

    /// <summary>The installed name of the look each eye picture is, written out so renaming a picture never renames a look.</summary>
    internal static IReadOnlyDictionary<TourianStatueBlockPicture, string> LookNames { get; } = new Dictionary<TourianStatueBlockPicture, string>
    {
        [TourianStatueBlockPicture.EyesBright] = "EyesBright",
        [TourianStatueBlockPicture.EyesDim] = "EyesDim",
        [TourianStatueBlockPicture.EyesDark] = "EyesDark",
        [TourianStatueBlockPicture.Released] = "Released",
    };
}
