namespace SuperMetroid.Verification.Frontend.Menus;

/// <summary>The x, y, offset x, offset y and large of one menu screen publication part.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="OffsetX">The offset x.</param>
/// <param name="OffsetY">The offset y.</param>
/// <param name="Large">The large.</param>
internal readonly record struct MenuScreenPublicationPart(int X, int Y, int OffsetX, int OffsetY, bool Large);
