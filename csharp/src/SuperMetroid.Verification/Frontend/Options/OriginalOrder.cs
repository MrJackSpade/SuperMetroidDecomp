using SuperMetroid.Core.Frontend.Options;

namespace SuperMetroid.Verification.Frontend.Options;

/// <summary>The page and description of one original order.</summary>
/// <param name="Page">The page.</param>
/// <param name="Description">The description.</param>
internal readonly record struct OriginalOrder(GameOptionsTilemap Page, string Description);
