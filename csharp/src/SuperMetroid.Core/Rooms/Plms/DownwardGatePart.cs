namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Which part of a downward gate a draw list writes.</summary>
internal enum DownwardGatePart
{
    /// <summary>The five-cell gate column.</summary>
    Column,

    /// <summary>A trigger on the left, with a separate neighbouring block.</summary>
    LeftTrigger,

    /// <summary>A trigger on the right.</summary>
    RightTrigger,
}
