namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed word and bank each enemy frame identity shows (#1395). The binding composes every enemy-bank family's
/// frames, bosses and areas included, so it is supplied from above them.
/// </summary>
internal interface IEnemyFrameWords
{
    /// <summary>The enemy bank holding installed composition <paramref name="frame"/>.</summary>
    byte Bank(EnemyFrameId frame);

    /// <summary>The bank-local word of the frame an enemy shows, zero before it shows any.</summary>
    ushort Spritemap(EnemyFrameId? frame);

    /// <summary>The bank-local word of the spritemap or extended frame <paramref name="frame"/> shows, in the enemy's bank.</summary>
    ushort Spritemap(EnemyFrameId frame);
}
