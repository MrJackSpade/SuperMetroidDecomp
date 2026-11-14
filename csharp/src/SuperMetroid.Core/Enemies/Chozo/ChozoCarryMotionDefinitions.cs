namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Statue movement and Samus joint offsets for Wrecked Ship and Lower Norfair.</summary>
public static class ChozoCarryMotionDefinitions
{
    /// <summary>X velocity in 1/256 pixels; its absolute magnitude also drives downward collision.</summary>
    /// <remarks>
    /// The signed speeds are authored values: negative facing left, their positive mirror facing right. The four poses
    /// that take and hold Samus are stationary; the stride's four steps burst 2, 3, 14 and 8 pixels per frame.
    /// Adjacent stock foot origins derive planted-foot displacement. Transfer advance2
    /// and final plant slide1 specify the authored pose-transition trajectory: changing
    /// them changes the foot-transfer advance and final planted-foot shift. They are not
    /// continuous physical rates. No independent planted3/14/7 magnitude is retained.
    /// Investigation: #625 / #665.
    /// </remarks>
    internal const int WeightShiftAdvance = 2, FinalPlantSlide = 1;
}
