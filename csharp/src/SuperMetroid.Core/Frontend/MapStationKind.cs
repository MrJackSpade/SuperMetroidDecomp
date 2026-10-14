namespace SuperMetroid.Core.Frontend;

/// <summary>Mutually exclusive station marker families in the normal file-select map.</summary>
public enum MapStationKind
{
    /// <summary>Missile-refill markers selected through the area-pointer table at $82:C7DB.</summary>
    Missile,
    /// <summary>Energy-refill markers selected through the area-pointer table at $82:C7EB.</summary>
    Energy,
    /// <summary>Map-download station markers selected through the area-pointer table at $82:C7FB.</summary>
    Map
}
