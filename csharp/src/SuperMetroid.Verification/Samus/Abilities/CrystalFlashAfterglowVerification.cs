using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>
/// Confirms the #1684 typed Crystal Flash radius against the 256ths arithmetic it replaces: the expansion limit is
/// 8192 256ths of a pixel, and every radius below it fades for fourteen steps and one more for each 2048 256ths.
/// </summary>
internal static class CrystalFlashAfterglowVerification
{
    /// <summary>Compares every radius around the limit with the 256ths arithmetic.</summary>
    internal static void VerifyCrystalFlashAfterglow()
    {
        for (int radius = 0; radius <= ushort.MaxValue; radius++)
        {
            PowerBombRadius typed = PowerBombRadius.FromRaw(radius);
            bool belowLimit = typed < PowerBombExplosionDefinitions.CrystalFlashRadiusLimit;
            if (belowLimit != radius < 8192)
                throw new InvalidDataException($"Radius {radius} lies {(belowLimit ? "below" : "at or past")} the Crystal Flash limit.");
            if (belowLimit)
                AssertEqual(14 + (radius >> 11), PowerBombExplosionAfterglow.CrystalFlashAfterglowSteps(typed), $"afterglow of radius {radius}");
            else if (radius is 8192 or ushort.MaxValue)
                AssertThrows<ArgumentOutOfRangeException>(() => PowerBombExplosionAfterglow.CrystalFlashAfterglowSteps(typed), $"radius {radius} is past the expansion limit");
        }

        Console.WriteLine("  Crystal Flash afterglow: the expansion limit and every reachable radius's fade steps equal the 256ths arithmetic.");
    }
}
