namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The Baby Metroid's drain stops Samus's bombs by holding the count of her bombs out full on every update it drains her,
/// and clears the count as it lets her go (#2350). The count it holds is the one every bomb rule reads: a full count
/// refuses another bomb and a bomb spread, X-ray reads it among the drained values (<see cref="XrayRefusedAsDrained"/>),
/// and Mother Brain's bombs look for Samus's bombs only while it is not zero. A bomb still burning from before the drain
/// that ends during it takes one off the held count until the drain's next update holds it full again. The release clears
/// the count to zero whatever is out, and a bomb that ends after that leaves it at zero, so the count can stand below the
/// bombs out until they end; in play the drain lasts far longer than a bomb burns, so none is out when it lets her go.
/// </summary>
/// <remarks>
/// Consulted on every update the drain holds Samus and once as it releases her, in the Baby Metroid's update; a static rule
/// with no state. Evidence: the drain (<c>$A9:F21B</c>) stores a bomb counter of five on every update it drains Samus
/// (<c>$A9:F22C</c>) and stores zero as it releases her below two energy (<c>$A9:F269</c>); a bomb's deletion
/// (<c>$90:ADB7</c>) decrements the counter and clears it when that passes zero (<c>$90:ADFC</c>); the bomb placement
/// (<c>$90:C0AB</c>), the bomb spread (<c>$90:D849</c>), X-ray's setup (<c>$91:E17B</c>) and the enemy projectiles'
/// bomb check (<c>$86:C1B8</c>) read the same counter.
/// </remarks>
internal static partial class DrainHoldsBombCount
{
    /// <summary>The count of Samus's bombs out the drain holds on every update it drains her: a bomb in each of her five places.</summary>
    internal const int Held = 5;

    /// <summary>The count of Samus's bombs out the drain leaves as it lets her go.</summary>
    internal const int Released = 0;
}
