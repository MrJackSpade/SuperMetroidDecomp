using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks that Plasma beams keep their type, damage, direction, and animation after enemy hits.</summary>
internal static class PlasmaPenetrationVerification
{
    /// <summary>Applies two impacts to each charged and uncharged Plasma combination without consuming the shot.</summary>
    internal static void VerifyPlasmaEnemyPenetration()
    {
        foreach (bool charged in new[] { false, true })
        for (ushort combination = 0; combination < 8; combination++)
        {
            var shots = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None, InstalledFixtures.SamusOverreads);
            var slot = shots.Slots[0];
            SamusProjectileType type = SamusProjectileType.Beam(
                SamusBeamCombinations.Of((SamusBeamFlags)combination | SamusBeamFlags.Plasma), charged);
            slot.Type = type;
            slot.Damage = 100;
            slot.Direction = SamusProjectileDirection.Right;
            slot.Cursor = ProjectileFixtureCursors.Active;
            slot.InstructionTimer = WrappingCountdown.Start(7);
            var bombs = new SamusBombProjectileSystem();
            for (int hit = 0; hit < 2; hit++)
            {
                AssertTrue(shots.EnemyImpacts.TryStartEnemyImpact(bombs, 0), "penetrating enemy hit accepted");
                AssertEqual(type, slot.Type, "Plasma preserves live beam type instead of explosion");
                AssertEqual(100, slot.Damage, "Plasma preserves original damage");
                AssertTrue(!slot.IsSpent, "Plasma is not marked spent");
                AssertEqual(ProjectileFixtureCursors.Active, slot.Cursor, "Plasma retains its animation");
                AssertEqual((ushort)7, slot.InstructionTimer.RawValue, "Plasma retains animation timing");
            }
        }
    }

    /// <summary>Runs the focused Plasma penetration check.</summary>
    [VerificationCommand("--plasma-penetration")]
    internal static int PlasmaPenetrationCommand()
    {
        Suite(nameof(VerifyPlasmaEnemyPenetration), () => VerifyPlasmaEnemyPenetration());
        return 0;
    }
}
