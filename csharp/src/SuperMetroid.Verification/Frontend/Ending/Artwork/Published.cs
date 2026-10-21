using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Verification.Frontend.Ending.Artwork;

/// <summary>The role and name of one published.</summary>
/// <param name="Role">The role.</param>
/// <param name="Name">The name.</param>
internal readonly record struct Published(EndingPaletteId Role, string Name);
