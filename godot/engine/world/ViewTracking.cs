namespace Engine.World;

/// <summary>
/// How a room keeps the player in view on one update: where in the view the camera makes for having them.
/// </summary>
/// <param name="PlaceAcross">How many pixels from the view's left edge the player is to stand.</param>
/// <param name="Rising">Whether the player is moving up.</param>
/// <param name="AboveWhileRising">How many pixels above the player the view's top is to stand while they move up.</param>
/// <param name="AboveOtherwise">How many pixels above the player the view's top is to stand while they move any other way.</param>
public readonly record struct ViewTracking(int PlaceAcross, bool Rising, int AboveWhileRising, int AboveOtherwise);
