using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static class DeadTorizoCollisionVerification
{
    internal static int VerifyDeadTorizoCollision()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        foreach (string trigger in new[] { "contact", "shot", "power bomb", "solid collision delay" })
        {
            var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
            enemies.BindFixtureBus(installation.OpenRuntimeAddressSpace());
            enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 0);
            var slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.CorpseTorizo;
            slot.Position = slot.Position.WithWhole(new(120, 100));
            slot.Radius = new(16, 32);
            enemies.DeadTorizoBehavior.InitializeDeadTorizo(slot);
            Require(IsInteractive(), "The intact corpse must initially be interactive.");
            Require(slot.Properties.HasAny(EnemyProperties.SolidToSamus),
                "Native initial solidity must be retained.");
            var samus = new SamusState();
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
                    samus.Kinematics.RecordSolidEnemyCollision(SamusCollisionDirection.Right, slot.NativeIndex);
                    // The wait handler reads Samus's collision indexes.
                    enemies.DeadTorizoBehavior.RunDeadTorizoMain(slot, samus);
                    for (int frame = 0; frame < 15; frame++) enemies.DeadTorizoBehavior.RunDeadTorizoMain(slot, null);
                    Require(IsInteractive(), "The native 16-frame pre-rot delay must retain collision.");
                    enemies.DeadTorizoBehavior.RunDeadTorizoMain(slot, null);
                    break;
            }
            Require(slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision),
                $"{trigger}: rotting must set the native ignore-collision bit.");
            Require(!IsInteractive(), $"{trigger}: rotting corpse must leave the interaction list.");
            samus.Kinematics.Position = samus.Kinematics.Position.WithWholeX(90);
            Require(!Probe().Collided, $"{trigger}: the same movement must pass the rotting corpse.");
            ushort phase = slot.VariableA;
            slot.VariableA = 0xd3c7; // Native completed/no-operation phase.
            enemies.DeadTorizoBehavior.TriggerDeadTorizoPowerBomb(slot);
            Require(slot.VariableA == 0xd3c7, "An ignored corpse must not restart from another power bomb.");
            Require(phase == 0xd3e6, "Each trigger must enter the native rotting phase.");

            bool IsInteractive()
            {
                enemies.ProcessingWindow.DetermineWhichEnemiesToProcess(Position16<RoomPixels>.Zero);
                return ((IReadOnlyList<ushort>)enemies.ProcessingWindow._interactiveEnemyIndexes).Contains(slot.NativeIndex);
            }
            SolidEnemyCollisionResult Probe() => SamusSolidEnemyCollision.Probe(samus.Kinematics,
                IsInteractive() ? [new SolidEnemyCollisionBody(slot.NativeIndex, slot.Position.Whole, new(slot.Radius.X, slot.Radius.Y), slot.FrozenTimer, slot.Properties)] : [],
                SamusCollisionDirection.Right, 16, 0);
        }
        Console.WriteLine("Dead Torizo: contact, shot, power bomb and delayed solid contact release collision correctly.");
        return 0;

        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidDataException(message);
        }
    }

    [VerificationCommand("--dead-torizo-collision")]
    internal static int DeadTorizoCollisionCommand()
    {
        return VerifyDeadTorizoCollision();
    }
}
