namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The three linked bomb-block restoration draw lists.</summary>
internal enum BombBlockRestoreDraw
{
    /// <summary>Horizontal parent and child restoration.</summary>
    Horizontal,
    /// <summary>Vertical parent and child restoration.</summary>
    Vertical,
    /// <summary>Four-block parent and child restoration.</summary>
    Square,
}
