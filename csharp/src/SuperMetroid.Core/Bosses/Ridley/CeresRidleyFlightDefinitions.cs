namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How Ceres Ridley flies (#2171, #2232): his lift-off, how hard each of his flights seeks its target, how long each
/// attack lasts and how fast his swoop and retreat carry him.
/// </summary>
internal static class CeresRidleyFlightDefinitions
{
    /// <summary>The least vertical speed Ridley keeps, either way, while he moves into his fireball position: half a pixel an update.</summary>
    internal static readonly SignedEightEight FireballMoveMinimumSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(128);

    /// <summary>The fastest 1/256-pixel speed his seek clamps to toward a lower target: five pixels per update.</summary>
    internal static readonly SignedEightEight MaximumSeekSpeed = SignedEightEight.FromPixels(5);

    /// <summary>The fastest 1/256-pixel speed toward a higher target: five pixels per update the other way.</summary>
    internal static readonly SignedEightEight MinimumSeekSpeed = -MaximumSeekSpeed;

    /// <summary>The upward speed his lift-off's climb gains each update: a sixteenth of a pixel an update.</summary>
    internal static readonly SignedEightEight LiftoffClimbStep = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16);

    /// <summary>The upward speed his lift-off loses each update once it is slowing.</summary>
    internal static readonly SignedEightEight LiftoffSlowingStep = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(20);

    /// <summary>The seek divisor of his hover and his flights into and through his fireball position: his slowest seek.</summary>
    internal const int HoverDivisor = 16;

    /// <summary>The seek divisor of his flight to the swoop's start and of both his retreats.</summary>
    internal const int SweepDivisor = 15;

    /// <summary>The seek divisor of his lunge at Samus.</summary>
    internal const int LungeDivisor = 3;

    /// <summary>The seek divisor of his hand's flight to the falling Baby.</summary>
    internal const int RetrievalDivisor = 4;

    /// <summary>The updates his hover waits outside the hover point before he attacks anyway.</summary>
    internal const int HoverWaitLimit = 124;

    /// <summary>The updates above the hover line after which his move into his fireball position gives up for a swoop.</summary>
    internal const int FireballMoveGiveUp = 48;

    /// <summary>The updates of his fireballing, during which he hovers about his fireball position.</summary>
    internal const int FireballingUpdates = 225;

    /// <summary>The updates left of his fireballing once it is cut short as Samus's energy runs low.</summary>
    internal const int FireballingCutShortUpdates = 9;

    /// <summary>The updates of his lunge at Samus, unless he reaches her first.</summary>
    internal const int LungeUpdates = 65;

    /// <summary>The updates of his swoop's first dive, aiming down, as his swoop is set up and again as it begins.</summary>
    internal const int SwoopDiveUpdates = 11;

    /// <summary>The updates of his swoop's sweep, aiming left along the bottom of the swoop.</summary>
    internal const int SwoopSweepUpdates = 37;

    /// <summary>The updates of his swoop's climb, aiming up.</summary>
    internal const int SwoopClimbUpdates = 29;

    /// <summary>The updates of his swoop's last, faster climb.</summary>
    internal const int SwoopFinalClimbUpdates = 2;

    /// <summary>The speed every stage of his swoop moves toward: three pixels an update.</summary>
    internal static readonly SignedEightEight SwoopSpeed = SignedEightEight.FromPixels(3);

    /// <summary>The updates he waits after the Baby falls; on the last he flies to retrieve it.</summary>
    internal const int BabyRetrievalDelayUpdates = 22;

    /// <summary>The upward speed his real retreat starts at as he catches the Baby: two pixels an update.</summary>
    internal static readonly SignedEightEight RetreatStartSpeed = SignedEightEight.FromPixels(-2);

    /// <summary>The updates he waits once he is above the chamber; on the last the getaway begins.</summary>
    internal const int RetreatDelayUpdates = 65;
}
