namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy whose visual frames may include a BG2 half the instruction interpreter writes.</summary>
internal interface IEnemyBg2Frames : IEnemyEntity
{
    /// <summary>The installed BG2 half of <paramref name="selected"/>, or null when that frame has none.</summary>
    /// <param name="selected">The resolved visual frame.</param>
    /// <param name="room">The room binding whose installed artwork holds the BG2 frames.</param>
    EnemyBg2FrameLookup? FindBg2Frame(ushort selected, EnemyRoomBinding room);
}
