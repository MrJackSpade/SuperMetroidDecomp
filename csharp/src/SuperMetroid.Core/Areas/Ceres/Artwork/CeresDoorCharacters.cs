using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The characters the Ceres door shows over the OBJ characters as its second variant spawns (#1406): its installed tiles,
/// which <c>CeresDoor_Func_1</c> copies there for that variant alone.
/// </summary>
internal sealed record CeresDoorCharacters : SpriteArtworkSource
{
    /// <summary>The door's characters, which its second variant shows.</summary>
    internal static CeresDoorCharacters Shown { get; } = new();

    /// <summary>Names the source for diagnostics and pictures.</summary>
    public override string ToString() => "Ceres door characters";
}
