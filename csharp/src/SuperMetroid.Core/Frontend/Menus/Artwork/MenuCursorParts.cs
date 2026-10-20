using System.Collections;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>$82:CBCB-CBFA: two eight-pixel tiles form the centered selection missile.
/// Stock glyph identities and native part order specify the selected missile drawings. Only this display design is retained; atlas/centering relationships calculate and supplied edits remain independent. Pixels, colors and timing are excluded.</summary>
internal sealed class MenuCursorParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses the native two-part missile geometry only for the four stock cursor identities; other or edited compositions retain their supplied layout.</summary>
    internal static SpriteComposition CalculateIfMatching(int frame, SpriteComposition supplied)
    {
        if (supplied.PartCount != 2)
            return supplied;
        return supplied.CalculateIfMatching(new MenuCursorParts(MenuMissileAnimationDefinitions.SpritemapId(frame) - FirstNativeSpritemap));
    }

    /// <summary>The first menu missile spritemap identity (<see cref="MenuMissileAnimationDefinitions.FirstNativeSpritemap"/>).</summary>
    private const int FirstNativeSpritemap = MenuMissileAnimationDefinitions.FirstNativeSpritemap;
    /// <summary>$82:CBCB: first missile pose left tileDF and right tileEF.</summary>
    private const int FirstLeftTile = SecondLeftTile + 1 - MapSpriteFormat.TileColumns;
    /// <summary>$82:CBCB: right tile $EF of the first missile pose.</summary>
    private const int FirstRightTile = SecondLeftTile + 1;
    /// <summary>$82:CBD7: second missile pose left tileEE is the top-left of the EE/EF/FE/FF two-by-two atlas block.</summary>
    private const int SecondLeftTile = MenuMissileAnimationDefinitions.SecondPoseLeftCell;
    /// <summary>$82:CBD7/CBEF: selected right tileFF shared by second and final missile poses.</summary>
    private const int SharedRightTile = SecondLeftTile + MapSpriteFormat.TileColumns + 1;
    /// <summary>$82:CBE3: third missile pose left tileFE and right tileCC.</summary>
    private const int ThirdLeftTile = SecondLeftTile + MapSpriteFormat.TileColumns;
    /// <summary>$82:CBE3: right tile $CC of the third missile pose.</summary>
    private const int ThirdRightTile = MenuMissileAnimationDefinitions.ThirdPoseRightCell;
    /// <summary>$82:CBEF: final missile pose left tileC8; native part order is left then right.</summary>
    private const int FinalLeftTile = MenuMissileAnimationDefinitions.FinalPoseLeftCell;
    /// <summary>Each selection missile pose uses two eight-pixel OBJ parts.</summary>
    public int Count => 2;
    /// <summary>Calculates the left or right tile and centered offset for one native missile pose, honoring its frame-specific part order.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            bool left = (index == 0) == (frame == 3);
            int leftTile = frame switch { 0 => FirstLeftTile, 1 => SecondLeftTile, 2 => ThirdLeftTile, _ => FinalLeftTile };
            int rightTile = frame switch { 0 => FirstRightTile, 2 => ThirdRightTile, _ => SharedRightTile };
            return new(SnesSpritemapXWord.Create(left ? -8 : 0, false), SnesSpritemapYByte.Create(-4),
                SnesObjAttributeWord.Create(left ? leftTile : rightTile, 0, 3, SnesTileFlipFlags.None), true);
        }
    }
    /// <summary>Yields both missile parts in the native order for the selected pose.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same two native parts through the non-generic collection interface.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
