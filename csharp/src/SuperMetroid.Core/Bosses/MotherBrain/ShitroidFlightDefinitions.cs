namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How sharply the Tourian Shitroid pulls toward each point it flies to: the divisor its gradual acceleration divides the
/// distance to the point by (a larger divisor is a gentler pull), and the extra push a reversal takes while it is off
/// screen.
/// </summary>
internal static class ShitroidFlightDefinitions
{
    /// <summary>The pull of a rush (to the middle of the room, onto its Sidehopper and onto Samus): the whole distance.</summary>
    internal const int RushDivisor = 1;

    /// <summary>The pull of its stare-down while it hovers near Samus: a sixth of the distance.</summary>
    internal const int StareDownDivisor = 6;

    /// <summary>The pull of its remorseful follow of the released Samus: an eighth of the distance.</summary>
    internal const int RemorseDivisor = 8;

    /// <summary>
    /// The pull of a drift (rising after it feeds and after it drains Samus, backing off left and right, and fleeing): a
    /// sixteenth of the distance.
    /// </summary>
    internal const int DriftDivisor = 16;

    /// <summary>
    /// The extra speed a reversal adds while the Shitroid is off screen, in 256ths of a pixel an update: four pixels an
    /// update.
    /// </summary>
    internal const int OffScreenReversalKick = 1024;
}
