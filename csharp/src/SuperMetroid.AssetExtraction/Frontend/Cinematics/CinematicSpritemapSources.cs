using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Frontend.Cinematics;

/// <summary>Where each cinematic sprite pose's spritemap is stored (<see cref="CinematicSpritemapSourceDefinitions"/>).</summary>
internal static class CinematicSpritemapSources
{
    /// <summary>The bank-$8C pointer to the spritemap pose <paramref name="pose"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is the blank pose, which shows no spritemap, or is undefined.</exception>
    internal static ushort Of(CinematicSpritePose pose) =>
        (uint)pose.Index < (uint)CinematicSpritemapSourceDefinitions.Spritemaps.Length &&
        CinematicSpritemapSourceDefinitions.Spritemaps[pose.Index] is SnesAddress spritemap
            ? spritemap.Offset
            : throw new ArgumentOutOfRangeException(nameof(pose), pose, "The pose shows no cinematic spritemap.");
}
