using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>
/// Checks the Yapping Maw's bend step (#1632): a quarter of the curl, the curl shifted right two, for every curl the
/// extension runs with, and refused for a curl outside that range.
/// </summary>
internal static class YappingMawBendStepVerification
{
    /// <summary>The largest curl the extension runs with, in whole angle units.</summary>
    private const int MaximumCurl = 128;

    /// <summary>Compares every admitted curl's bend step with the curl shifted right two and refuses the curls outside them.</summary>
    internal static void VerifyYappingMawBendStep()
    {
        for (int curl = 0; curl <= MaximumCurl; curl++)
            AssertEqual(curl >> 2, YappingMawBehavior.BendStepOf(curl), $"a curl of {curl} bends each link by the curl shifted right two");
        AssertThrows<ArgumentOutOfRangeException>(() => YappingMawBehavior.BendStepOf(-1), "a negative curl is refused");
        AssertThrows<ArgumentOutOfRangeException>(() => YappingMawBehavior.BendStepOf(MaximumCurl + 1), "a curl past the clamp is refused");
        Console.WriteLine("Yapping Maw bend step: every curl from zero to the clamp bends by a quarter of the curl.");
    }
}
