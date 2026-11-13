using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the dead Torizo corpse releases solid contact after each native rot trigger.</summary>
internal static class DeadTorizoCollisionVerification
{
    /// <summary>Exercises contact, shot, Power Bomb, and delayed solid-contact triggers through real collision paths.</summary>
    internal static int VerifyDeadTorizoCollision()
    {
        var installation = RepositoryInstallation.Installation;
        foreach (string trigger in new[] { "contact", "shot", "power bomb", "solid collision delay" })
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { BossArtwork = InstalledFixtures.BossArtwork };
            enemies.VerificationServices().ReadRandom = () => 0;
            var slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.CorpseTorizo, slot.Definition);
            slot.Position = slot.Position.WithWhole(new(120, 100));
            slot.Radius = new(16, 32);
            enemies.DeadTorizoBehavior.InitializeDeadTorizo(slot);
            Require(IsInteractive(), "The intact corpse must initially be interactive.");
            Require(slot.Contact.HasAny(EnemyContact.SolidToSamus),
                "Native initial solidity must be retained.");
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
            samus.Kinematics.Position = samus.Kinematics.Position.WithWhole(new(90, 100));
            samus.Kinematics.Radius = new(8, 16);
            Require(Probe().Collided, "Approaching the intact corpse must initially collide.");
            switch (trigger)
            {
                case "contact":
                    samus.Kinematics.Position = samus.Kinematics.Position.WithWholeX(120);
                    enemies.DeadTorizoBehavior.RunDeadTorizoMain(slot, samus);
                    Require(samus.Kinematics.ExtraDisplacement.X.WholePixels >= 4 &&
                        samus.Kinematics.ExtraDisplacement.Y.WholePixels == 4,
                        "The custom touch rectangles must publish native displacement.");
                    break;
                case "shot": enemies.DeadTorizoBehavior.TriggerDeadTorizoRotting(slot); break;
                case "power bomb": enemies.DeadTorizoBehavior.TriggerDeadTorizoPowerBomb(slot); break;
                default:
                    samus.Kinematics.Position = samus.Kinematics.Position.WithWholeX(500); // Exercise solid-contact delay independently of custom overlap.
                    samus.Kinematics.RecordSolidEnemyCollision(SamusCollisionDirection.Right, slot);
                    // The wait handler reads which enemies Samus met.
                    enemies.DeadTorizoBehavior.RunDeadTorizoMain(slot, samus);
                    for (int frame = 0; frame < 15; frame++) enemies.DeadTorizoBehavior.RunDeadTorizoMain(slot, null);
                    Require(IsInteractive(), "The native 16-frame pre-rot delay must retain collision.");
                    enemies.DeadTorizoBehavior.RunDeadTorizoMain(slot, null);
                    break;
            }
            Require(slot.Contact.HasAny(EnemyContact.IgnoresSamus),
                $"{trigger}: rotting must set the native ignore-collision bit.");
            Require(!IsInteractive(), $"{trigger}: rotting corpse must leave the interaction list.");
            samus.Kinematics.Position = samus.Kinematics.Position.WithWholeX(90);
            Require(!Probe().Collided, $"{trigger}: the same movement must pass the rotting corpse.");
            DeadTorizoEnemyState corpse = slot.SpeciesState<DeadTorizoEnemyState>()
                ?? throw new InvalidOperationException("The initialized corpse has no state.");
            DeadTorizoPhase? phase = corpse.Phase;
            corpse.Phase = DeadTorizoPhase.NoOperation;
            enemies.DeadTorizoBehavior.TriggerDeadTorizoPowerBomb(slot);
            Require(corpse.Phase == DeadTorizoPhase.NoOperation, "An ignored corpse must not restart from another power bomb.");
            Require(phase == DeadTorizoPhase.Rotting, "Each trigger must enter the native rotting phase.");

            bool IsInteractive()
            {
                enemies.ProcessingWindow.DetermineWhichEnemiesToProcess(Position16<RoomPixels>.Zero);
                return enemies.ProcessingWindow.InteractiveEnemies.Contains(slot);
            }
            SolidEnemyCollisionResult Probe() => SamusSolidEnemyCollision.Probe(samus.Kinematics,
                IsInteractive() ? [new SolidEnemyCollisionBody(slot, slot.Position.Whole, new(slot.Radius.X, slot.Radius.Y), slot.FrozenTimer != 0 || slot.Contact.HasAny(EnemyContact.SolidToSamus))] : [],
                SamusCollisionDirection.Right, 16, 0);
        }
        Console.WriteLine("Dead Torizo: contact, shot, power bomb and delayed solid contact release collision correctly.");
        return 0;

        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidDataException(message);
        }
    }

    /// <summary>Runs the focused corpse collision release check.</summary>
    [VerificationCommand("--dead-torizo-collision")]
    internal static int DeadTorizoCollisionCommand()
    {
        return VerifyDeadTorizoCollision();
    }
}
