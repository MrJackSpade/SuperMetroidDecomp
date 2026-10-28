using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Reads the BG3 plane's tiles as the cartridge's packed tilemap words, so verification can compare the plane with the
/// words the cartridge uploads.
/// </summary>
internal static class Layer3PlaneWords
{
    /// <summary>The packed words of the plane's tiles.</summary>
    extension(Layer3Plane plane)
    {
        /// <summary>Each cell's packed tilemap word, row-major.</summary>
        internal ushort[] Words => BackgroundTileWordBytes.Words(plane.Tiles);
    }
}
