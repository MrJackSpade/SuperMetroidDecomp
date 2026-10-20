namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Where Phantoon reappears for one flame-rain pattern: its path cursor and room position.</summary>
/// <param name="Cursor">The body's path cursor.</param>
/// <param name="Position">The body's whole-pixel room position.</param>
public readonly record struct PhantoonRainPlacementDefinition(ushort Cursor, Position16<RoomPixels> Position);
