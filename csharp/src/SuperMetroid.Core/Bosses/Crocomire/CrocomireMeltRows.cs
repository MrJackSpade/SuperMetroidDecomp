namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>How many of Crocomire's melting rows stand above the acid as he sinks (#2553): his top row to his top four.</summary>
internal enum CrocomireMeltRows
{
    /// <summary>Only his top row stands above the acid.</summary>
    TopRow,
    /// <summary>His top two rows stand above the acid.</summary>
    TopTwoRows,
    /// <summary>His top three rows stand above the acid.</summary>
    TopThreeRows,
    /// <summary>His top four rows stand above the acid.</summary>
    TopFourRows,
}
