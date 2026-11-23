using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Assets;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>The ending shooting star's single-character appearances, which its sheet frames picture (#1463).</summary>
internal static partial class EndingShootingStarParts
{
    /// <summary>
    /// $8B:E9A7 / Handle_ShootingStars.tilemapValues: the lead-in is the blank piece (character 0, palette 0), the specks and
    /// the dim star grow through characters $1F0 to $1F2, and the full star is $1F3, all in sprite palette 4.
    /// </summary>
    internal static SpritePart Appearance(EndingShootingStarLook look) => look switch
    {
        EndingShootingStarLook.LeadIn => SpritePart.Single(0, 0, ActorDepth.BehindRoom),
        EndingShootingStarLook.FaintSpeck => SpritePart.Single(CharacterSheetGrid.Cell(31, 0), 4, ActorDepth.BehindRoom),
        EndingShootingStarLook.BrightSpeck => SpritePart.Single(CharacterSheetGrid.Cell(31, 1), 4, ActorDepth.BehindRoom),
        EndingShootingStarLook.DimStar => SpritePart.Single(CharacterSheetGrid.Cell(31, 2), 4, ActorDepth.BehindRoom),
        EndingShootingStarLook.FullStar => SpritePart.Single(CharacterSheetGrid.Cell(31, 3), 4, ActorDepth.BehindRoom),
        _ => throw new ArgumentOutOfRangeException(nameof(look)),
    };
}
