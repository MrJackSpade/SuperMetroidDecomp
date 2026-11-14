using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Resource identity, supported JSON version, and fixed dimensions derived from the Crystal Flash palette programs.</summary>
/// <remarks>
/// The stock body colours are six ten-colour cycle steps, <c>r = 0..5</c>. The first colour is always 14336; the other
/// nine in each step are the same grey with every channel at <c>16 + floor(11*min(r,6-r)/3)</c>, channel levels 16, 19,
/// 23, 27, 23, 19, matching all 60 colours in the cartridge. The body colour animation selects steps 0,1,2,3,4,5,4,3,2,1
/// and copies only ten colours per transition; the following six bubble colours have a separate cursor and timer
/// (#625, #902). For bubble colour <c>c = 0..5</c>, let <c>k = (c-r+6)%6</c> and <c>q = k + (k &gt;= 3 ? 1 : 0)</c>:
/// the stock colour is 32767 less 1056 times <c>q</c>, except <c>r = 5, c = 0</c> is 32767 rather than 31711. All 36
/// colours, that exception included, match the cartridge. The bubble cursor visits steps 0..5 and wraps independently of
/// the body steps (#625, #903).
/// </remarks>
public static class CrystalFlashColorFormat
{
    /// <summary>Ten editable body frames, one per pointer/timer record in playback order.</summary>
    public const int BodyFrameCount = SamusPaletteDefinitions.CrystalFlash.BodyStepCount;
    /// <summary>Six editable bubble frames selected independently by the pointer table.</summary>
    public const int BubbleFrameCount = SamusPaletteDefinitions.CrystalFlash.BubblePaletteCount;
    /// <summary>Ten colors in each body frame, occupying sprite palette six colors 0..9.</summary>
    public const int BodyColorCount = 10;
    /// <summary>Six colors in each bubble frame, occupying sprite palette six colors 10..15.</summary>
    public const int BubbleColorCount = 6;
}
