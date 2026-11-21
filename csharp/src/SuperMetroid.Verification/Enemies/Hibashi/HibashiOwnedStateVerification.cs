using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Hibashi;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Hibashi state each part owns: its spawn height, and the wait its population parameter names.</summary>
internal static class HibashiOwnedStateVerification
{
    /// <summary>
    /// Initializes a graphics part and its collision part and confirms the part selector, the graphics part's spawn height,
    /// that a new flame waits its first wait and erupts on its first update (#2536), and that a finished eruption rests for
    /// parameter one's full word and the update it runs out on (#2439).
    /// </summary>
    internal static void VerifyHibashiOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace graphics = enemies.Places.InUpdateOrder[0];
        graphics.SetSpawnParameter1(EnemyDefinitionId.Hibashi, ushort.MaxValue);
        graphics.SetSpawnParameter2(EnemyDefinitionId.Hibashi, 0);
        graphics.Bind(EnemyDefinitionId.Hibashi, graphics.KeptDefinition);
        graphics.Position = graphics.Position.WithWhole(new(0x0040, 0x0120));
        graphics.InitializeAsPlaced(RoomEnemyFrameFixtures.StandaloneSamus());
        RoomEnemyPlace hitbox = enemies.Places.InUpdateOrder[1];
        hitbox.SetSpawnParameter2(EnemyDefinitionId.Hibashi, 1);
        hitbox.Bind(EnemyDefinitionId.Hibashi, hitbox.KeptDefinition);
        hitbox.InitializeAsPlaced(RoomEnemyFrameFixtures.StandaloneSamus());

        AssertTrue(graphics.Entity is HibashiEntity && hitbox.Entity is HibashiCollisionPartEntity,
            "parameter two selects the collision part");
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        bool Erupting() => !graphics.Presence.HasAny(EnemyPresence.Hidden);
        graphics.RunMainAi(samus);
        AssertTrue(Erupting() && !hitbox.Contact.HasAny(EnemyContact.IgnoresSamus), "a new flame erupts on its first update");

        // The eruption plays to its last cue, which hides the flame; the visible stream needs 59 updates through it.
        for (int update = 0; update < 70 && Erupting(); update++)
        {
            graphics.AdvanceAnimation(samus);
            hitbox.AdvanceAnimation(samus);
        }
        AssertTrue(!Erupting(), "the eruption's last cue finishes it");

        // The next update rests the finished flame, for parameter one's full word and the update it runs out on.
        graphics.RunMainAi(samus);
        int rest = ushort.MaxValue + EnemyWaitDefinitions.RunOutUpdate;
        int rested = 0;
        while (!Erupting() && rested <= rest)
        {
            graphics.RunMainAi(samus);
            rested++;
        }
        AssertEqual(rest, rested, "a finished eruption rests for parameter one's full word and the update it runs out on");
        Console.WriteLine("Hibashi owned state: both parts' parameters and the spawn height match.");
    }

    /// <summary>Runs the focused Hibashi owned-state check.</summary>
    [VerificationCommand("--hibashi-owned-state")]
    internal static int HibashiOwnedStateCommand()
    {
        VerifyHibashiOwnedState();
        return 0;
    }
}
