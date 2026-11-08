namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A set of the four boss statues in Tourian's entrance room, such as the ones still animating.</summary>
[Flags]
public enum TourianGreyStatues
{
    /// <summary>No statue.</summary>
    None = 0,
    /// <summary>Phantoon's statue.</summary>
    Phantoon = 1 << 0,
    /// <summary>Ridley's statue.</summary>
    Ridley = 1 << 1,
    /// <summary>Kraid's statue.</summary>
    Kraid = 1 << 2,
    /// <summary>Draygon's statue.</summary>
    Draygon = 1 << 3,
}
