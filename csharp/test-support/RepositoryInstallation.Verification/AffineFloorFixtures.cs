using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Turns a fixture's memory image into a turning floor (#1403, #1467): its map, the low byte of each word, over its 256
/// characters, the high byte of each word, composed into the floor's picture under the fixture's palette as the import
/// composes installed floors.
/// </summary>
internal static class AffineFloorFixtures
{
    /// <summary>The floor <paramref name="image"/>'s map and characters show, coloured from <paramref name="palette"/>.</summary>
    internal static AffineFloor Floor(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette) =>
        AffineFloorConstants.Capture(image, palette);
}
