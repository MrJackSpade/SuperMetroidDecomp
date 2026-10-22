using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The pre-coloured sheet of each enemy as a room graphics set loads it (#1392).</summary>
public interface IEnemySheetSource
{
    /// <summary>The sheet of <paramref name="enemy"/> as graphics set <paramref name="set"/> loads it.</summary>
    /// <exception cref="InvalidDataException">The set does not load that enemy.</exception>
    SpriteSheet For(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy);
}
