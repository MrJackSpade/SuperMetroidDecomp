using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Cinematics;

/// <summary>Cartridge values and helpers of <see cref="CinematicSpritemapSourceDefinitions"/> that only verification reads.</summary>
internal static class CinematicSpritemapSourceDefinitionsConstants
{
    /// <summary>
    /// The further spritemaps the cartridge's lists select for a pose, each drawing the pose's own picture: the escape
    /// pod's long trail is stored three times, over three characters of identical pixels, which its flicker steps through.
    /// </summary>
    internal static readonly IReadOnlyDictionary<CinematicSpritePose, SnesAddress[]> SamePictureSpritemaps =
        new Dictionary<CinematicSpritePose, SnesAddress[]>
    {
        [CinematicSpritePose.EscapePodLongTrail] = [new(CinematicSpritemapSourceDefinitions.Bank, 0xbc4f), new(CinematicSpritemapSourceDefinitions.Bank, 0xbc56)],
    };
}
