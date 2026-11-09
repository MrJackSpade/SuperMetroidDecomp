namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>The edges of one file-select map reveal rectangle a check compares.</summary>
/// <param name="Left">The first screen column inside the rectangle.</param>
/// <param name="Top">The first screen line inside the rectangle.</param>
/// <param name="Right">The first screen column past the rectangle.</param>
/// <param name="Bottom">The first screen line past the rectangle.</param>
internal readonly record struct VerifyWorldMapLabelsCase(int Left, int Top, int Right, int Bottom);
