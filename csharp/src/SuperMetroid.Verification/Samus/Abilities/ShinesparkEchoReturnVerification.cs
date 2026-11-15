using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks that a released crash echo is placed at its radius taken within 256 pixels (#2051).</summary>
internal static class ShinesparkEchoReturnVerification
{
    /// <summary>
    /// An echo is placed at the radius it has flown until 248 pixels, comes back to Samus at 256 and flies out again
    /// from there.
    /// </summary>
    internal static void VerifyShinesparkEchoReturn()
    {
        AssertEqual(0, ShinesparkEchoReturn.PlacementRadius(0), "a fresh echo is placed on Samus");
        AssertEqual(248, ShinesparkEchoReturn.PlacementRadius(248), "an echo is placed at the radius it has flown");
        AssertEqual(0, ShinesparkEchoReturn.PlacementRadius(256), "an echo that has flown 256 pixels is placed back on Samus");
        AssertEqual(8, ShinesparkEchoReturn.PlacementRadius(264), "a returned echo flies out again");
        AssertThrows<ArgumentOutOfRangeException>(() => ShinesparkEchoReturn.PlacementRadius(-8), "an echo never flies a negative distance");
        Console.WriteLine("Shinespark echo return: an echo is placed at its radius within 256 pixels and returns to Samus at 256.");
    }
}
