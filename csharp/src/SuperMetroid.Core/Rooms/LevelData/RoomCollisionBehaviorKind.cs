namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What a collision kind's blocks' behaviour describes.</summary>
internal enum RoomCollisionBehaviorKind
{
    /// <summary>Nothing: the kind's blocks carry no behaviour.</summary>
    None,

    /// <summary>A reaction of the kind's reaction family.</summary>
    Reaction,

    /// <summary>A slope's shape and mirroring.</summary>
    Slope,

    /// <summary>The signed distance to the block the extension continues.</summary>
    Extension,
}
