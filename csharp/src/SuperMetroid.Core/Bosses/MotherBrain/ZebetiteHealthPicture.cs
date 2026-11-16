using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A picture a Zebetite shows while its health is at least <paramref name="MinimumHealth"/> (#2250).</summary>
/// <param name="MinimumHealth">The least health the picture shows at.</param>
/// <param name="Picture">The installed frame the Zebetite shows.</param>
internal readonly record struct ZebetiteHealthPicture(int MinimumHealth, EnemyFrameId Picture);
