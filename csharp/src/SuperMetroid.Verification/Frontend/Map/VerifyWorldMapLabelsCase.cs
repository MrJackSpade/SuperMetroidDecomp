namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>The top left and bottom right of one verify world map labels case.</summary>
/// <param name="TopLeft">The top left.</param>
/// <param name="BottomRight">The bottom right.</param>
internal readonly record struct VerifyWorldMapLabelsCase(int Left, int Top, int Right, int Bottom);
