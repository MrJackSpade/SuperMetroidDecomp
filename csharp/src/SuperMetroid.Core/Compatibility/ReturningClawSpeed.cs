namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A ninja Space Pirate's claw's speed along its throw (#1846, #2284): it slows by an eighth of a pixel each update while
/// the claw flies out, and speeds up by as much each update once the claw comes back, where a claw that keeps flying back
/// past 255 and 255/256 pixels an update starts again from rest. Only its whole pixels move the claw.
/// </summary>
/// <remarks>
/// Consulted once per update of a thrown claw, after the claw has moved by the whole pixels of its speed before the
/// change. Evidence: the original keeps the claw's speed as a sixteen-bit count of 256ths of a pixel that its return adds
/// to without a limit; the ninja claw flight check steps a thrown claw for 2200 updates, past that wrap, against that
/// arithmetic.
/// </remarks>
[CompatibilityService("a ninja pirate's returning claw that keeps flying speeds up to 255 pixels an update and then starts again from rest")]
public readonly partial record struct ReturningClawSpeed
{
    /// <summary>How much a claw slows each update flying out, and speeds up each update coming back, in 256ths of a pixel.</summary>
    private const int ChangePerUpdate = 32;

    /// <summary>The 256ths of a pixel in one pixel.</summary>
    private const int PartsPerPixel = 256;

    /// <summary>The speeds, in 256ths of a pixel per update, a returning claw goes round: just short of 256 pixels.</summary>
    private const int SpeedRange = PartsPerPixel * PartsPerPixel;

    /// <summary>The speed in 256ths of a pixel per update, 0 through 65535.</summary>
    private readonly int _twoHundredFiftySixths;

    /// <summary>Keeps <paramref name="twoHundredFiftySixths"/> as the speed.</summary>
    private ReturningClawSpeed(int twoHundredFiftySixths) =>
        _twoHundredFiftySixths = Ensure.BetweenInclusive(0, SpeedRange - 1, twoHundredFiftySixths);

    /// <summary>A claw's speed of <paramref name="pixels"/> whole pixels an update, 0 through 255.</summary>
    public static ReturningClawSpeed FromPixels(int pixels) => new(Ensure.BetweenInclusive(0, PartsPerPixel - 1, pixels) * PartsPerPixel);

    /// <summary>The whole pixels the claw moves in an update at this speed.</summary>
    public int WholePixels => _twoHundredFiftySixths / PartsPerPixel;

    /// <summary>Whether the claw is at rest: a claw flying out turns back once its speed reaches rest.</summary>
    public bool IsAtRest => _twoHundredFiftySixths == 0;

    /// <summary>The speed after one more update of a claw flying out, an eighth of a pixel slower.</summary>
    /// <exception cref="InvalidOperationException">The claw would slow past rest without stopping.</exception>
    public ReturningClawSpeed SlowedFlyingOut() => _twoHundredFiftySixths >= ChangePerUpdate
        ? new(_twoHundredFiftySixths - ChangePerUpdate)
        : throw new InvalidOperationException($"A claw flying out at {_twoHundredFiftySixths}/256 pixels an update would pass rest without stopping.");

    /// <summary>
    /// The speed after one more update of a returning claw, an eighth of a pixel faster: a claw past 255 and 255/256 pixels
    /// an update starts again from rest.
    /// </summary>
    public ReturningClawSpeed SpedUpComingBack() => new((_twoHundredFiftySixths + ChangePerUpdate) % SpeedRange);
}
