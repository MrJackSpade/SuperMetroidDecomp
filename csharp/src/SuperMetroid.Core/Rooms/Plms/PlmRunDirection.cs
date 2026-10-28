namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The direction a PLM block run writes its level words in, starting from the run's first block.</summary>
internal enum PlmRunDirection
{
    /// <summary>Each following block is one column to the right, along a row.</summary>
    Horizontal,
    /// <summary>Each following block is one row below, down a column.</summary>
    Vertical,
}
