namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// A copy from one save slot onto another (#2553): one of the six routes between two different slots, so no copy names its
/// source as its destination. <see cref="FileCopyRoutes"/> names each route's slots and arrow.
/// </summary>
internal enum FileCopyRoute
{
    /// <summary>SAMUS A copied onto SAMUS B.</summary>
    AToB,

    /// <summary>SAMUS A copied onto SAMUS C.</summary>
    AToC,

    /// <summary>SAMUS B copied onto SAMUS A.</summary>
    BToA,

    /// <summary>SAMUS B copied onto SAMUS C.</summary>
    BToC,

    /// <summary>SAMUS C copied onto SAMUS A.</summary>
    CToA,

    /// <summary>SAMUS C copied onto SAMUS B.</summary>
    CToB,
}
