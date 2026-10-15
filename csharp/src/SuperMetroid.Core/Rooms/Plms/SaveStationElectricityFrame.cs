namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One save-station electricity pose; its installed artwork key is confined to <see cref="Artwork.SaveStationElectricityFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct SaveStationElectricityFrame(int Pose);
