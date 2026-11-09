using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Takes the backdrops of the space behind the exploding planet (#1583) from the installed palette-effect colours: the
/// space's records are the shared whiteout ramp's, and each record's first colour is the backdrop it shows. The game reads
/// these backdrops and no other palette-effect colour.
/// </summary>
internal static class EndingSpaceBackdropColours
{
    /// <summary>The backdrop each record of the space's colours shows, read from <paramref name="colours"/>.</summary>
    /// <param name="colours">The installed palette-effect colours.</param>
    /// <exception cref="InvalidDataException">A record's first colour is not installed.</exception>
    internal static EndingSpaceBackdrops From(IPaletteFxColorSource colours)
    {
        Ensure.NotNull(colours);
        PaletteFxColorFrames frames = ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorFrames;
        var backdrops = new Dictionary<PaletteFxFrame, RgbColor>(frames.FrameCount);
        for (int frame = 0; frame < frames.FrameCount; frame++)
        {
            PaletteFxColorKey first = frames.Color(frame, 0);
            backdrops.Add(first.Frame, colours.TryReadColor(first, out RgbColor backdrop)
                ? backdrop
                : throw new InvalidDataException($"Palette-FX record {first.Frame.Record}'s first colour has no installed definition."));
        }
        return new EndingSpaceBackdrops(backdrops);
    }
}
