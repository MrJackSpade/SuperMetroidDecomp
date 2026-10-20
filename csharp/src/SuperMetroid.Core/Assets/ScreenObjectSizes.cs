namespace SuperMetroid.Core.Assets;

/// <summary>The two object sizes a screen's OBJ size selection offers: what a part's small or large flag draws (#1358).</summary>
/// <param name="Small">The size of a small object.</param>
/// <param name="Large">The size of a large object.</param>
public readonly record struct ScreenObjectSizes(Extent16<ScreenPixels> Small, Extent16<ScreenPixels> Large);
