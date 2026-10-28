using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>$8C:A57C silhouette: lower body/tip pieces followed by four right-to-left
/// rows of body and ray tiles. The packed atlas regions are reordered vertically
/// to reconstruct the original streaked image. Priority3, no flips, inherited palette.</summary>
internal sealed class EndingExplosionSilhouetteParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Right-hand tile and top Y of the four streaked rows, top to bottom in draw
    /// order: body, inner rays, middle rays, outer rays.</summary>
    private static readonly EndingSilhouetteRow[] Rows = [new EndingSilhouetteRow(CharacterSheetGrid.Cell(11, 14), 0), new EndingSilhouetteRow(CharacterSheetGrid.Cell(14, 6), -16), new EndingSilhouetteRow(CharacterSheetGrid.Cell(13, 6), -24), new EndingSilhouetteRow(CharacterSheetGrid.Cell(11, 6), -40)];

    /// <summary>Uses the calculated silhouette only when the selected pose and all supplied OAM parts match it exactly.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied) =>
        shown == EndingExplosionSpriteDefinitions.PoseOf(EndingExplosionSpriteDefinitions.Pose.Silhouette)
            ? supplied.CalculateIfMatching(new EndingExplosionSilhouetteParts()) : supplied;

    /// <summary>Four tip/body pieces followed by four rows of four streaked tiles.</summary>
    public int Count => 20;
    /// <summary>Places a lower-body piece or reorders one atlas row right-to-left at the native silhouette coordinates.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int x, y, tile;
            bool large = true;
            if (index < 2) { x = index * 16; y = 8; tile = CharacterSheetGrid.Cell(12, 12) + index * 2; }
            else if (index == 2) { x = 0; y = 24; tile = CharacterSheetGrid.Cell(14, 12); large = false; }
            else if (index == 3) { x = -32; y = 16; tile = CharacterSheetGrid.Cell(13, 8); }
            else
            {
                int column = (index - 4) % 4;
                x = 16 - column * 16;
                (int rightTile, int top) = Rows[(index - 4) / 4];
                tile = rightTile - column * 2;
                y = top;
            }
            return new(SpritePart.Create(x, y, large,
                tile, 0, 3), true);
        }
    }
    /// <summary>Yields the twenty silhouette pieces in native draw order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Preserves silhouette draw order for non-generic composition consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
