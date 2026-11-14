using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The backdrops of the space behind the exploding planet (#1583), by frame of the space's colour effect. The import takes
/// them from the palette-effect colours, which are its import data (#1637): the space's records are the shared whiteout
/// ramp's, and each record's first colour is the backdrop it shows. The installed ending backdrops hold them in frame order,
/// and the game reads these backdrops and no other palette-effect colour.
/// </summary>
internal static class EndingSpaceBackdropColours
{
    /// <summary>The frames of the space's colour effect, each showing one backdrop.</summary>
    internal static int FrameCount => ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorFrames.FrameCount;

    /// <summary>The backdrop each record of the space's colours shows, read from <paramref name="colours"/>.</summary>
    /// <param name="colours">The import's palette-effect colours.</param>
    /// <exception cref="InvalidDataException">A record's first colour is not defined.</exception>
    internal static EndingSpaceBackdrops From(IPaletteFxColorSource colours)
    {
        Ensure.NotNull(colours);
        PaletteFxColorFrames frames = ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorFrames;
        var backdrops = new RgbColor[frames.FrameCount];
        for (int frame = 0; frame < frames.FrameCount; frame++)
        {
            PaletteFxColorKey first = frames.Color(frame, 0);
            backdrops[frame] = colours.TryReadColor(first, out RgbColor backdrop)
                ? backdrop
                : throw new InvalidDataException($"Palette-FX record {first.Record.Record}'s first colour has no definition.");
        }
        return FromFrameOrder(backdrops);
    }

    /// <summary>The backdrops of <paramref name="backdrops"/>, first frame first.</summary>
    /// <param name="backdrops">The space's backdrops, each frame's.</param>
    internal static RgbColor[] InFrameOrder(EndingSpaceBackdrops backdrops)
    {
        Ensure.NotNull(backdrops);
        PaletteFxColorFrames frames = ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorFrames;
        var ordered = new RgbColor[frames.FrameCount];
        for (int frame = 0; frame < ordered.Length; frame++)
            ordered[frame] = backdrops.Of(frames.Frame(frame).Frame);
        return ordered;
    }

    /// <summary>The space's backdrops from one colour for each frame of its colour effect, first frame first.</summary>
    /// <param name="backdrops">Each frame's backdrop, first frame first.</param>
    /// <exception cref="InvalidDataException">The backdrops are not one for each frame.</exception>
    internal static EndingSpaceBackdrops FromFrameOrder(ReadOnlySpan<RgbColor> backdrops)
    {
        PaletteFxColorFrames frames = ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorFrames;
        if (backdrops.Length != frames.FrameCount)
            throw new InvalidDataException($"The space behind the exploding planet requires a backdrop for each of its {frames.FrameCount} frames.");
        var byFrame = new Dictionary<ColourEffectFrame, RgbColor>(frames.FrameCount);
        for (int frame = 0; frame < frames.FrameCount; frame++)
            byFrame.Add(frames.Frame(frame).Frame, backdrops[frame]);
        return new EndingSpaceBackdrops(byFrame);
    }
}
