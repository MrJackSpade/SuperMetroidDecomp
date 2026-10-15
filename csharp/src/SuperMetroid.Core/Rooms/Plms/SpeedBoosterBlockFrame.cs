namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>What a Speed Booster block's program shows (#1296).</summary>
internal enum SpeedBoosterBlockFrame
{
    /// <summary>The first crumble image.</summary>
    Crumble0,
    /// <summary>The second crumble image.</summary>
    Crumble1,
    /// <summary>The third crumble image.</summary>
    Crumble2,
    /// <summary>The last crumble image: the block is gone.</summary>
    Crumble3,
    /// <summary>The block's saved level word, restored.</summary>
    RestoredBlock,
    /// <summary>The bomb reveal's block image.</summary>
    BombReveal,
}
