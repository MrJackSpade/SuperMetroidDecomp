namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Mutually exclusive station marker families in the normal file-select map.</summary>
public enum MapStationKind
{
    /// <summary>Missile-refill markers.</summary>
    Missile,
    /// <summary>Energy-refill markers.</summary>
    Energy,
    /// <summary>Map-download station markers.</summary>
    Map
}
