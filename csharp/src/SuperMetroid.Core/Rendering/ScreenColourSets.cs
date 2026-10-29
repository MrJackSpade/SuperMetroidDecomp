using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// One of a frontend screen's two colour owners (#1464): its background sets, which its background images show, or its
/// sprite sets, which its actors draw in. An image colour group the owner colours shows the set of the same identity.
/// </summary>
public sealed class ScreenColourSets : ColourSets;
