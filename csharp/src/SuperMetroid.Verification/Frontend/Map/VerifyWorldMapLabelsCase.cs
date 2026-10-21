namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>The top left and bottom right of one verify world map labels case.</summary>
/// <param name="TopLeft">The top left.</param>
/// <param name="BottomRight">The bottom right.</param>
internal readonly record struct VerifyWorldMapLabelsCase(Position16<ScreenPixels> TopLeft, Position16<ScreenPixels> BottomRight);
