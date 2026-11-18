using SuperMetroid.Core.Enemies.Beetom;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Beetom;

/// <summary>
/// Confirms that the chance a distant Beetom faces right (#2479) and its choice of what to do next (#2510) decide every one
/// of the 65536 samples exactly as the test of the sample's lowest bit and the lookup of its lowest three bits they replace
/// did.
/// </summary>
internal static class BeetomRandomChancesVerification
{
    /// <summary>The distant Beetom's choices as the replaced lookup listed them, by the sample's lowest three bits.</summary>
    private static readonly BeetomDistantChoice[] ReplacedDistantChoices =
    [
        new(BeetomAction.StartIdling, null),
        new(BeetomAction.StartIdling, null),
        new(BeetomAction.StartShortHop, HorizontalDirection.Left),
        new(BeetomAction.StartLongHop, HorizontalDirection.Right),
        new(BeetomAction.StartShortHop, HorizontalDirection.Left),
        new(BeetomAction.StartShortHop, HorizontalDirection.Right),
        new(BeetomAction.StartLongHop, HorizontalDirection.Left),
        new(BeetomAction.StartLongHop, HorizontalDirection.Right),
    ];

    /// <summary>Compares every sample's named answers with the bit test and the lookup they replace.</summary>
    internal static void VerifyBeetomRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
        {
            AssertSameAt((sample & 1) != 0, BeetomDecisions.FacesRight.IsTakenBy(sample), "distant Beetom faces right", sample);
            AssertSameAt(ReplacedDistantChoices[sample & 7], BeetomDecisions.DistantChoice.Choose(sample), "distant Beetom's next action", sample);
        }

        Console.WriteLine("Beetom random chances: all 65536 samples decide as the bit test and the lookup they replace.");
    }
}
