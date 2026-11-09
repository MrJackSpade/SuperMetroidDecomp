namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Statue movement and Samus joint offsets for Wrecked Ship and Lower Norfair.</summary>
public static class ChozoCarryMotionDefinitions
{
    /// <summary>X velocity in 8.8; its absolute magnitude also drives downward collision.</summary>
    /// <remarks>
    /// All 32 signed speeds are authored values. The nonzero
    /// first-half speeds are negative; the second half is their positive mirror.
    /// Each half holds four stationary poses, repeats the stride burst of
    /// 2, 3, 14 and 8 pixels per frame twice, then holds four more zeros.
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
