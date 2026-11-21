using SuperMetroid.Core.Frontend.Options;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>An options row with the name the installed layout keys its cursor anchor by.</summary>
/// <param name="Name">The row's installed name, unique on its page.</param>
/// <param name="Row">The row the cursor points at.</param>
internal sealed record GameOptionsCursorRow(string Name, GameOptionsRow Row);
