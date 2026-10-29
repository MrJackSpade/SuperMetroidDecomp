namespace SuperMetroid.Core.Assets;

/// <summary>An owner of some of the colours its sprites' image colour groups show (#1405, #1464).</summary>
public interface IImageGroupColours
{
    /// <summary>
    /// Lays the owner's colours for its images' colour group <paramref name="group"/> over <paramref name="colours"/>,
    /// which start as the image's own; the owner leaves alone the colours it does not hold.
    /// </summary>
    void Overlay(ImageColourGroup group, Span<RgbColor> colours);
}
