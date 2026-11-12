namespace SuperMetroid.Core.Assets;

/// <summary>Pixel columns of one 8x8 picture cell (#1652), any number of which a picture can hide together.</summary>
[Flags]
public enum CellColumns
{
    /// <summary>No column.</summary>
    None = 0,

    /// <summary>The cell's leftmost pixel column.</summary>
    First = 1,

    /// <summary>The cell's second pixel column from the left.</summary>
    Second = 2,

    /// <summary>The cell's third pixel column from the left.</summary>
    Third = 4,

    /// <summary>The cell's fourth pixel column from the left.</summary>
    Fourth = 8,

    /// <summary>The cell's fifth pixel column from the left.</summary>
    Fifth = 16,

    /// <summary>The cell's sixth pixel column from the left.</summary>
    Sixth = 32,

    /// <summary>The cell's seventh pixel column from the left.</summary>
    Seventh = 64,

    /// <summary>The cell's rightmost pixel column.</summary>
    Eighth = 128,
}
