using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Sbug;

/// <summary>
/// Checks the plain reversal #1620 gave the Sbug: no authored Sbug reaches the reversal whose separate negations the
/// original applies, and the reversal turns every custom speed to its exact opposite.
/// </summary>
internal static class SbugReversalVerification
{
    /// <summary>The headings a turn holds.</summary>
    private const int HeadingCount = 256;

    /// <summary>
    /// Every authored Sbug and Sbug2 activates as something other than the random wander that reverses when far from
    /// Samus, the only behaviour that reaches the reversal; and the production reversal, run at every custom heading and
    /// every authored speed with Samus far away on both axes, leaves each custom speed at its exact opposite and moves the
    /// Sbug by it.
    /// </summary>
    internal static void VerifySbugReversal()
    {
        SbugSpawnSettings[] placements = [.. RoomEnemyPopulationDefinitions.Definitions
            .SelectMany(list => list.Records.ToArray())
            .Where(placement => placement.DefinitionId is EnemyDefinitionId.Sbug or EnemyDefinitionId.Sbug2)
            .Select(placement => (SbugSpawnSettings)placement.SpawnSettings)];
        if (placements.Length == 0)
            throw new InvalidOperationException("No authored Sbug was found.");
        if (placements.Any(placement => placement.Behavior == SbugActivationBehavior.RandomAndReverseWhenFar))
            throw new InvalidOperationException("An authored Sbug reverses when far from Samus, so the original's separate negations are reached.");

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Sbug, slot.Definition);
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(Position16<RoomPixels>.Zero),
        };
        var start = FixedPosition<RoomPixels>.FromWhole(new(512, 512));
        foreach (int speed in placements.Select(placement => placement.Speed).Distinct().Order())
        for (int heading = 0; heading < HeadingCount; heading++)
        {
            slot.SetSpawnSettings(new SbugSpawnSettings(speed, RotationAngle.FromSteps(0), ActivationRadius: 48, SbugActivationBehavior.RandomAndReverseWhenFar));
            slot.Position = start;
            SbugBehavior.InitializeSbug(slot);
            SbugEnemyState state = slot.RequireSpeciesState<SbugEnemyState>();
            state.FaceCustomHeading(RotationAngle.FromSteps(heading));
            var angle = new TrigonometryAngle(heading);
            SignedSixteenSixteen x = SbugBehavior.CalculateSignedSbugComponent(angle, speed, EnemyTrigonometryAngleDefinitions.QuarterTurn);
            SignedSixteenSixteen y = SbugBehavior.CalculateSignedSbugComponent(angle, speed, EnemyTrigonometryAngleDefinitions.HalfTurn);
            state.CustomXVelocity = x;
            state.CustomYVelocity = y;
            state.StraightMovementTimer = WrappingCountdown.Start(512);
            state.MovementTimer = 32;
            state.Action = SbugAction.MoveStraightAndReverseWhenFar;

            enemies.SbugBehavior.RunSbugMain(slot, samus, null);
            var expected = new FixedPosition<RoomPixels>(start.X.MovedBy(-x), start.Y.MovedBy(-y));
            if (state.CustomXVelocity != -x || state.CustomYVelocity != -y || slot.Position != expected)
                throw new InvalidOperationException(
                    $"An Sbug heading {heading} at speed {speed} reversed {x}, {y} to {state.CustomXVelocity}, {state.CustomYVelocity} and moved to {slot.Position}.");
        }
        Console.WriteLine($"  Sbug reversal: none of the {placements.Length} authored Sbugs reverses when far; every heading at each authored speed reverses exactly.");
    }
}
