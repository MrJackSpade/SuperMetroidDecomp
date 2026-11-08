using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Assets;

/// <summary>An installed sprite sheet and the name a sheet rule reports it by (#1513).</summary>
/// <param name="Label">What the sheet pictures, such as its sheet identity or its graphics set and enemy.</param>
/// <param name="Sheet">The installed sheet.</param>
internal sealed record LabelledSheet(string Label, SpriteSheet Sheet);
