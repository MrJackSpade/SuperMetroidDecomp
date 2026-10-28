namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Statue movement and Samus joint offsets for Wrecked Ship and Lower Norfair.</summary>
public static class ChozoCarryMotionDefinitions
{
    /// <summary>$AA:E630: X velocity in 8.8; its absolute magnitude also drives downward collision.</summary>
    /// <remarks>
    /// All 32 signed words match the pinned NTSC J/U v1.0 ROM. The nonzero
    /// first-half words are negative; the second half is their positive mirror.
    /// Each half holds four stationary poses, repeats the stride
    /// <c>0200,0300,0E00,0800</c> burst twice, then holds four more zeros.
    /// Adjacent stock foot origins derive planted-foot displacement. Transfer advance2
    /// and final plant slide1 specify the authored pose-transition trajectory: changing
    /// them changes the foot-transfer advance and final planted-foot shift. They are not
    /// continuous physical rates. No independent planted3/14/7 magnitude is retained.
    /// Investigation: #625 / #665.
    /// </remarks>
    internal const int WeightShiftAdvance = 2, FinalPlantSlide = 1;

    /// <summary>The carry-motion records: sixteen facing left, then their sixteen mirrors facing right.</summary>
    public const int RecordCount = 32;
}
