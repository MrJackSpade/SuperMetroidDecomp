using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Confirms the #1684 typed combo orbit offset against the angle-unit arithmetic it replaces: for every angle and every
/// orbit radius, the offset's X is the scaled sine at the angle and its Y the scaled sine a quarter turn back, each read
/// at the angle's units wrapped to a byte.
/// </summary>
internal static class ComboSineOffsetVerification
{
    /// <summary>Compares every angle and orbit radius with the unit arithmetic.</summary>
    internal static void VerifyComboSineOffset()
    {
        for (int units = 0; units < RotationAngle.StepsPerTurn; units++)
        for (int amplitude = 0; amplitude < 256; amplitude++)
        {
            Offset16<RoomPixels> expected = new(Scaled(units, amplitude), Scaled((units - 64) & 255, amplitude));
            Offset16<RoomPixels> actual = SamusComboMechanics.GetSineOffset(new TrigonometryAngle(units), amplitude);
            if (actual != expected)
                throw new InvalidDataException($"Combo offset at angle {units}, radius {amplitude} is {actual}, not {expected}.");
        }

        Console.WriteLine("  Combo sine offset: 65536 angle and radius pairs equal the unit arithmetic.");
    }

    /// <summary>The sine at <paramref name="units"/> scaled by <paramref name="amplitude"/>, truncated toward zero.</summary>
    private static int Scaled(int units, int amplitude)
    {
        int sample = Trigonometry.SignedSine(new TrigonometryAngle(units));
        int magnitude = Math.Abs(sample) * amplitude >> 8;
        return sample < 0 ? -magnitude : magnitude;
    }
}
