namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>One Tourian statue's windows of the shared animated-tile strip.</summary>
/// <param name="First">The strip character the first oscillation picture starts at.</param>
/// <param name="Tiles">The width of each picture in characters.</param>
/// <param name="Released">The strip character the released picture starts at.</param>
internal readonly record struct TourianStatueTileWindow(int First, int Tiles, int Released);
