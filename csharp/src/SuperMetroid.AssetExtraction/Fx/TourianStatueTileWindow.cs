using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>One Tourian statue's windows of the shared statue strip, and the animated-tile object that copies them.</summary>
/// <param name="Object">The animated-tile object whose header copies the statue's pictures.</param>
/// <param name="First">The strip character the first oscillation picture starts at.</param>
/// <param name="Tiles">The width of each picture in characters.</param>
/// <param name="Released">The strip character the released picture starts at.</param>
internal readonly record struct TourianStatueTileWindow(BlockAnimation Object, int First, int Tiles, int Released);
