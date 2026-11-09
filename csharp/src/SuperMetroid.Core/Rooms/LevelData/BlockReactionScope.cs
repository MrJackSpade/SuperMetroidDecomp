namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Which list of reactions a reacting block's reaction is an entry of.</summary>
public enum BlockReactionScope
{
    /// <summary>The block's collision kind's own reactions, the same in every area.</summary>
    Common,

    /// <summary>The reactions the room's area gives the block's collision kind.</summary>
    AreaSpecific,
}
