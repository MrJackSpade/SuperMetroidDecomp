using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Progression;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Checks the enemy compatibility rules #1620 named, and the plain form it gave the Yapping Maw's link offset, each over
/// its whole input range against the original's arithmetic.
/// </summary>
internal static class EnemyRuleRangesVerification
{
    /// <summary>The values a sixteen-bit coordinate or length takes.</summary>
    private const int WordRange = 65536;

    /// <summary>
    /// Every difference of two room coordinates, from several starting points: the closing Metroid's distance is the
    /// difference's place within 256 read as a signed eight-bit number, -128 through 127.
    /// </summary>
    internal static void VerifyMetroidClosingDistance()
    {
        foreach (int current in new[] { 0, 1, 127, 128, 255, 256, 4000, 65535 })
        {
            for (int target = 0; target < WordRange; target++)
            {
                int original = unchecked((sbyte)(target - current));
                int reckoned = MetroidClosingDistance.Between(target, current);
                if (reckoned != original)
                    throw new InvalidOperationException($"Closing distance from {current} to {target} is {reckoned}, not {original}.");
            }
        }
        AssertEqual(-56, MetroidClosingDistance.Between(200, 0), "a target 200 pixels right reads as 56 pixels left");
        AssertEqual(127, MetroidClosingDistance.Between(127, 0), "a target 127 pixels right reads as it lies");
        Console.WriteLine("  Metroid closing distance: every coordinate difference matches the signed span.");
    }

    /// <summary>
    /// Every angle with every length the short product tells apart, and a length from each later 256 and the longest:
    /// the Yapping Maw's plain floor of the signed short product equals the original's construction, which negates the
    /// product, keeps its whole part, doubles it and fills the rest with ones.
    /// </summary>
    internal static void VerifyYappingMawLinkOffset()
    {
        int[] lengths = [.. Enumerable.Range(0, 512).Concat(Enumerable.Range(2, 254).Select(page => page * 256 + page)).Append(WordRange - 1)];
        for (int raw = 0; raw < 256; raw++)
        {
            var angle = new TrigonometryAngle(raw);
            int signedSample = Trigonometry.FineSignedSine(TrigonometryAngle.Wrapped(-raw));
            int sampleWhole = Math.Abs(signedSample) / 256;
            foreach (int length in lengths)
            {
                int product = sampleWhole * (length % 256);
                int original = product == 0 ? 0
                    : signedSample >= 0 ? 2 * (product / 256)
                    : 2 * ((WordRange - product) / 256) % 256 - 256;
                int offset = YappingMawBehavior.CalculateYappingMawY(angle, length);
                if (offset != original)
                    throw new InvalidOperationException($"Link offset at angle {raw}, length {length} is {offset}, not {original}.");
            }
        }
        Console.WriteLine("  Yapping Maw link offset: every angle and length matches the original construction.");
    }

    /// <summary>
    /// Every Spazer and Plasma combination takes the response the table stores in its place after the twelve beam rows,
    /// and a combination the table authors is refused by the rule.
    /// </summary>
    internal static void VerifyUnauthoredBeamVulnerability()
    {
        string[] combinations = ["Spazer-Plasma", "Spazer-Plasma-Wave", "Spazer-Plasma-Ice", "Spazer-Plasma-Ice-Wave"];
        string[] responses = ["missile", "Super Missile", "bomb", "Power Bomb"];
        for (int place = 0; place < combinations.Length; place++)
            AssertEqual(responses[place], UnauthoredBeamVulnerability.Response(combinations[place], combinations, responses),
                $"{combinations[place]} takes the {responses[place]} response");
        AssertThrows<ArgumentOutOfRangeException>(() => UnauthoredBeamVulnerability.Response("Plasma", combinations, responses),
            "an authored combination is refused");

        EnemyVulnerabilities standard = EnemyVulnerabilityDefinitions.Standard;
        AssertEqual(standard.Ordnance.Missile, standard.ForBeam(SamusBeamCombination.SpazerPlasma), "Spazer-Plasma takes the missile response");
        AssertEqual(standard.Ordnance.SuperMissile, standard.ForBeam(SamusBeamCombination.SpazerPlasmaWave), "with Wave, the Super Missile response");
        AssertEqual(standard.Ordnance.Bomb, standard.ForBeam(SamusBeamCombination.SpazerPlasmaIce), "with Ice, the bomb response");
        AssertEqual(standard.Ordnance.PowerBomb, standard.ForBeam(SamusBeamCombination.SpazerPlasmaIceWave), "with Ice and Wave, the Power Bomb response");
        Console.WriteLine("  Unauthored beam vulnerability: the four Spazer and Plasma combinations take the ordnance responses.");
    }

    /// <summary>A follower reads its parent's current state while one occupies the place, and the state it took once the parent was deleted.</summary>
    internal static void VerifyDeletedParentLeftovers()
    {
        object left = new();
        object current = new();
        AssertTrue(ReferenceEquals(current, DeletedParentLeftovers.Parent(current, left)), "a living parent is read as it is");
        AssertTrue(ReferenceEquals(left, DeletedParentLeftovers.Parent<object>(null, left)), "a deleted parent is read as it left itself");
        Console.WriteLine("  Deleted parents: followers read what a deleted parent left.");
    }
}
