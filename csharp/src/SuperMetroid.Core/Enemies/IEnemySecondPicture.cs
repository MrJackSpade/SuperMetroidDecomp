using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy whose frames show a second picture drawn right after its own, at that picture's own depth (#2633, #2634):
/// Mother Brain's far leg, which stands behind the room's front tiles and her body while her near leg stands in front of
/// them. The common draw places it where the enemy's picture lies, in the enemy's shading.
/// </summary>
internal interface IEnemySecondPicture
{
    /// <summary>The sheet frame showing <paramref name="frame"/>'s second picture, or null where it shows none.</summary>
    /// <param name="frame">The frame the enemy shows.</param>
    string? SecondPictureOf(EnemyFrameId frame);
}
