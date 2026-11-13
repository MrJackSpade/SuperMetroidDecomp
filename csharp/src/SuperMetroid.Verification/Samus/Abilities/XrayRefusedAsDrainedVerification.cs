using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks X-ray's refusal of a Samus who reads as drained (<see cref="XrayRefusedAsDrained"/>, #1692).</summary>
internal static class XrayRefusedAsDrainedVerification
{
    /// <summary>The most bombs Samus counts: the morph ball lays a bomb only while fewer than five are counted.</summary>
    private const int MostBombs = 5;

    /// <summary>The most a speed divisor reaches: each stuck Draygon goop raises it by one, to at most five.</summary>
    private const int MostSpeedDivisor = 5;

    /// <summary>
    /// The rule against the original's test, for every input that reaches it: every cooldown a wrapping countdown holds,
    /// every bomb count and every speed divisor; the X-ray setup refuses exactly when the cooldown equals 7, the bombs 5 and
    /// the divisor 2. Then X-ray's own start, for a standing Samus who is otherwise admitted, at every bomb count and divisor
    /// and the cooldowns around seven: refused exactly where the rule refuses, admitted everywhere else.
    /// </summary>
    internal static void VerifyXrayRefusedAsDrained()
    {
        for (int cooldown = 0; cooldown <= ushort.MaxValue; cooldown++)
        {
            WrappingCountdown countdown = WrappingCountdown.Start(cooldown);
            for (int bombs = 0; bombs <= MostBombs; bombs++)
            {
                for (int divisor = 0; divisor <= MostSpeedDivisor; divisor++)
                {
                    bool original = cooldown == 7 && bombs == 5 && divisor == 2;
                    if (XrayRefusedAsDrained.Refuses(countdown, bombs, divisor) != original)
                    {
                        throw new InvalidOperationException(
                            $"X-ray's drained refusal at cooldown {cooldown}, {bombs} bombs and divisor {divisor} reads " +
                            $"{!original}; the original's test reads {original}.");
                    }
                }
            }
        }

        foreach (int cooldown in new[] { 0, 6, 7, 8 })
        {
            for (int bombs = 0; bombs <= MostBombs; bombs++)
            {
                for (int divisor = 0; divisor <= MostSpeedDivisor; divisor++)
                {
                    SamusState samus = StandingSamus();
                    samus.XSpeedDivisor = divisor;
                    bool admitted = samus.Xray.TryBegin(samus, previousMovementType: SamusMovementType.Standing,
                        conditions: new XrayBeginConditions
                        {
                            GameState = SuperMetroidGameState.MainGameplay,
                            ProjectileCooldownTimer = WrappingCountdown.Start(cooldown),
                            BombCounter = bombs,
                        });
                    bool refused = cooldown == 7 && bombs == 5 && divisor == 2;
                    AssertEqual(!refused, admitted,
                        $"X-ray starts at cooldown {cooldown}, {bombs} bombs and divisor {divisor} unless Samus reads as drained");
                }
            }
        }

        Console.WriteLine("X-ray drained refusal: refuses exactly at cooldown 7, five bombs and divisor 2, as the original's test does.");
    }

    /// <summary>A standing, right-facing Samus whose X-ray every other admission test admits.</summary>
    private static SamusState StandingSamus()
    {
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(),
            new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(100, 200)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        return samus;
    }
}
