namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One powered colour of the Wrecked Ship (#1512): colour <paramref name="Colour"/>, zero through fifteen, of <paramref name="Part"/>.</summary>
/// <param name="Part">The part of the ship it paints.</param>
/// <param name="Colour">Its place among the part's sixteen colours.</param>
public readonly record struct WreckedShipPoweredColour(WreckedShipPoweredPart Part, int Colour);
