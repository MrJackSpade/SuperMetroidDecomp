using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// How a dissolve erases a boss body picture step by step (#1652, #2478), as the import found it: for each pixel of the
/// picture, in reading order, the band of the dissolve that shows it and the steps after which it is erased, or none for a
/// pixel no step erases. The dissolve shows in bands, each band showing the steps taken when it was last shown, so at any
/// moment the picture hides each pixel whose band has shown the steps that erase it. Crocomire's melt images dissolve so.
/// </summary>
public sealed partial class PictureDissolve
{
    /// <summary>The dissolve of a picture of <paramref name="size"/>: <paramref name="pixels"/>, in reading order.</summary>
    /// <exception cref="ArgumentException">
    /// The pixels are not one for each pixel of the picture, or one names a band below zero or is erased after no steps.
    /// </exception>
    public PictureDissolve(Extent16<ImagePixels> size, ImmutableArray<DissolvingPixel?> pixels)
    {
        Ensure.LengthEqual(pixels.AsSpan(), size.X * size.Y);
        int bands = 0;
        foreach (DissolvingPixel? pixel in pixels)
        {
            if (pixel is not { } dissolving)
                continue;
            if (dissolving.Band < 0 || dissolving.ErasedAfterSteps < 1)
                throw new ArgumentException($"A dissolving pixel shows in band {dissolving.Band} and is erased after {dissolving.ErasedAfterSteps} steps; bands count from zero and a pixel is erased after a step or more.", nameof(pixels));
            bands = Math.Max(bands, dissolving.Band + 1);
        }
        Size = size;
        Pixels = pixels;
        BandCount = bands;
    }

    /// <summary>The width and height of the picture.</summary>
    public Extent16<ImagePixels> Size { get; }

    /// <summary>How the dissolve erases each pixel of the picture, in reading order; null for a pixel no step erases.</summary>
    public ImmutableArray<DissolvingPixel?> Pixels { get; }

    /// <summary>The bands the dissolve shows in: one past the highest band a pixel names.</summary>
    public int BandCount { get; }

    /// <summary>
    /// The pixels the picture hides once each band has shown the steps <paramref name="bandSteps"/> gives it, by band: each
    /// pixel whose band has shown at least the steps that erase it.
    /// </summary>
    /// <exception cref="ArgumentException">The steps do not name every band, or one is below zero.</exception>
    public HiddenPicturePixels Hidden(ReadOnlySpan<int> bandSteps)
    {
        if (bandSteps.Length < BandCount)
            throw new ArgumentException($"The dissolve shows {BandCount} bands; the steps name {bandSteps.Length}.", nameof(bandSteps));
        foreach (int steps in bandSteps)
        {
            if (steps < 0)
                throw new ArgumentException($"A band shows {steps} steps of the dissolve, below none.", nameof(bandSteps));
        }
        var hidden = new bool[Pixels.Length];
        for (int index = 0; index < hidden.Length; index++)
            hidden[index] = Pixels[index] is { } pixel && bandSteps[pixel.Band] >= pixel.ErasedAfterSteps;
        return new HiddenPicturePixels(Size, ImmutableArray.Create(hidden));
    }
}
