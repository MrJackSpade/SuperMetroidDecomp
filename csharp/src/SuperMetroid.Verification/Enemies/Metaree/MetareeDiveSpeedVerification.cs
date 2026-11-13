using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Metaree;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Metaree's owned dive speed, including the wrapped distance when Samus is above it.</summary>
internal static class MetareeDiveSpeedVerification
{
    /// <summary>
    /// A Samus 96 pixels below gives a dive of 96 / 24 + 4 = 8 pixels per update; a Samus one pixel above wraps the
    /// distance to 65535 and gives the cartridge's enormous 2734.
    /// </summary>
    internal static void VerifyMetareeDiveSpeed()
    {
        foreach ((int samusOffset, int expected) in new[] { new VerifyMetareeDiveSpeedCase(96, 8), new VerifyMetareeDiveSpeedCase(-1, 2734) })
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Metaree, slot.Definition);
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0080));
            MetareeBehavior.InitializeMetaree(slot);
            MetareeEnemyState state = enemies.MetareeStates[0] ?? throw new InvalidOperationException("The Metaree has no state.");
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0100, unchecked((ushort)(0x0080 + samusOffset)))),
            };
            MetareeBehavior.RunMetareeIdle(slot, state, samus);
            AssertEqual(expected, state.YVelocity, $"Samus {samusOffset} pixels below gives a dive of {expected}");
            AssertEqual(MetareeAction.PreparingAttack, state.Action, "the Metaree prepares its attack");
        }
        Console.WriteLine("Metaree dive speed: the ordinary and wrapped dives match.");
    }

    /// <summary>Runs the focused Metaree dive-speed check.</summary>
    [VerificationCommand("--metaree-dive-speed")]
    internal static int MetareeDiveSpeedCommand()
    {
        VerifyMetareeDiveSpeed();
        return 0;
    }
}
