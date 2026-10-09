using System.Collections;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>$82:CBCB-CBFA: two eight-pixel tiles form the centered selection missile.
/// Stock glyph identities and native part order specify the selected missile drawings. Only this display design is retained; atlas/centering relationships calculate and supplied edits remain independent. Pixels, colors and timing are excluded.</summary>
internal sealed class MenuCursorParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Rebuilds a recognized menu-missile composition from its native frame identity and selected atlas tiles.</summary>
    /// <param name="name">Composition name identifying one of the four missile animation frames.</param>
    /// <param name="supplied">Composition whose parts are replaced only when its name and two-part shape match.</param>
    /// <returns>The calculated missile composition for a recognized two-part frame, or <paramref name="supplied"/> unchanged otherwise.</returns>
    internal static SpriteComposition CalculateIfMatching(string name, SpriteComposition supplied)
    {
        if (name is not ("Cursor.0" or "Cursor.1" or "Cursor.2" or "Cursor.3") || supplied.PartCount != 2)
            return supplied;
        int frame = name[^1] - '0';
        return supplied.CalculateIfMatching(new MenuCursorParts(MenuMissileAnimationDefinitions.SpritemapId(frame) - FirstNativeSpritemap));
    }

    /// <summary>$82:C5D1 first menu missile spritemap identity34; playback order comes from82:BAB2, not atlas ordering.</summary>
    private const int FirstNativeSpritemap = 0x34;
    /// <summary>$82:CBCB: first missile pose left tileDF and right tileEF.</summary>
    private const int FirstLeftTile = SecondLeftTile + 1 - MapSpriteFormat.TileColumns;
    /// <summary>$82:CBCB: right-hand tileEF paired with the first missile pose's left tile.</summary>
    private const int FirstRightTile = SecondLeftTile + 1;
    /// <summary>$82:CBD7: second missile pose left tileEE is the top-left of the EE/EF/FE/FF two-by-two atlas block.</summary>
    private const int SecondLeftTile = 0xee;
    /// <summary>$82:CBD7/CBEF: selected right tileFF shared by second and final missile poses.</summary>
    private const int SharedRightTile = SecondLeftTile + MapSpriteFormat.TileColumns + 1;
    /// <summary>$82:CBE3: third missile pose left tileFE and right tileCC.</summary>
    private const int ThirdLeftTile = SecondLeftTile + MapSpriteFormat.TileColumns;
    /// <summary>$82:CBE3: native right-hand tileCC paired with the third missile pose's left tile.</summary>
    private const int ThirdRightTile = 0xcc;
    /// <summary>$82:CBEF: final missile pose left tileC8; native part order is left then right.</summary>
    private const int FinalLeftTile = 0xc8;
    /// <summary>Gets the two sprite parts that make up one missile cursor frame.</summary>
    public int Count => 2;

    /// <summary>Gets the requested part, with index zero representing the first item in native draw order.</summary>
    /// <param name="index">Zero-based part index; only zero and one are valid.</param>
    /// <returns>The positioned and attributed sprite part for this frame.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the two-part frame.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            bool left = (index == 0) == (frame == 3);
            int leftTile = frame switch { 0 => FirstLeftTile, 1 => SecondLeftTile, 2 => ThirdLeftTile, _ => FinalLeftTile };
            int rightTile = frame switch { 0 => FirstRightTile, 2 => ThirdRightTile, _ => SharedRightTile };
            return new(SnesSpritemapXWord.Create(left ? -8 : 0, false), unchecked((byte)-4),
                SnesObjAttributeWord.Create(left ? leftTile : rightTile, 0, 3, SnesTileFlipFlags.None), true);
        }
    }
    /// <summary>Enumerates the frame's parts in index order.</summary>
    /// <returns>An enumerator yielding the two positioned sprite parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
