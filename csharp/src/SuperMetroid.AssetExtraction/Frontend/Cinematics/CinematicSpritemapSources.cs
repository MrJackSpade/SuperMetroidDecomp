using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Frontend.Cinematics;

/// <summary>Where each cinematic sprite pose's spritemap is stored (<see cref="CinematicSpritemapSourceDefinitions"/>).</summary>
internal static class CinematicSpritemapSources
{
    /// <summary>Each pose's place in the cartridge's order of the pose spritemaps.</summary>
    private static readonly Dictionary<CinematicSpritePose, int> Places = PlacesOf(CinematicSpritemapSourceDefinitions.Spritemaps);

    /// <summary>Each pose of <paramref name="sources"/> by its place among them.</summary>
    private static Dictionary<CinematicSpritePose, int> PlacesOf(CinematicSpritemapSource[] sources)
    {
        var places = new Dictionary<CinematicSpritePose, int>();
        for (int place = 0; place < sources.Length; place++)
            places.Add(sources[place].Pose, place);
        return places;
    }

    /// <summary>The bank-$8C pointer to the spritemap pose <paramref name="pose"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is the blank pose, which shows no spritemap, or is undefined.</exception>
    internal static ushort Of(CinematicSpritePose pose) =>
        Places.TryGetValue(pose, out int place) && CinematicSpritemapSourceDefinitions.Spritemaps[place].Spritemap is SnesAddress spritemap
            ? spritemap.Offset
            : throw new ArgumentOutOfRangeException(nameof(pose), pose, "The pose shows no cinematic spritemap.");

    /// <summary>
    /// The place of <paramref name="pose"/> in the cartridge's order of the pose spritemaps, which the import lays out and
    /// identifies a family's frames in.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is undefined.</exception>
    internal static int CartridgeOrder(CinematicSpritePose pose) =>
        Places.TryGetValue(pose, out int place) ? place : throw new ArgumentOutOfRangeException(nameof(pose), pose, "Undefined cinematic pose.");
}
