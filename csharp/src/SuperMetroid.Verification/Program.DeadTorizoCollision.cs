using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static int VerifyDeadTorizoCollision()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        foreach (string trigger in new[] { "contact", "shot", "power bomb", "solid collision delay" })
        {
            var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, installation.OpenRuntimeAddressSpace());
            typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => 0));
            var slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.CorpseTorizo;
            slot.XPosition = 120;
            slot.YPosition = 100;
            slot.XRadius = 16;
            slot.YRadius = 32;
            Call("InitializeDeadTorizo", slot);
            Require(IsInteractive(), "The intact corpse must initially be interactive.");
            Require(slot.Properties.HasAny(EnemyProperties.SolidToSamus),
                "Native initial solidity must be retained.");
            var samus = new SamusState();
            samus.Kinematics.XPosition = 90;
            samus.Kinematics.YPosition = 100;
            samus.Kinematics.XRadius = 8;
            samus.Kinematics.YRadius = 16;
            Require(Probe().Collided, "Approaching the intact corpse must initially collide.");
            switch (trigger)
            {
                case "contact":
                    samus.Kinematics.XPosition = 120;
                    Call("RunDeadTorizoMain", slot, samus);
                    Require(samus.Kinematics.ExtraXDisplacement >= 4 &&
                        samus.Kinematics.ExtraYDisplacement == 4,
                        "The custom touch rectangles must publish native displacement.");
                    break;
                case "shot": Call("TriggerDeadTorizoRotting", slot); break;
                case "power bomb": Call("TriggerDeadTorizoPowerBomb", slot); break;
                default:
                    samus.Kinematics.XPosition = 500; // Exercise solid-contact delay independently of custom overlap.
                    samus.Kinematics.RecordSolidEnemyCollision(SamusCollisionDirection.Right, slot.NativeIndex);
                    // The wait handler reads Samus's collision indexes.
                    Call("RunDeadTorizoMain", slot, samus);
                    for (int frame = 0; frame < 15; frame++) Call("RunDeadTorizoMain", slot, null);
                    Require(IsInteractive(), "The native 16-frame pre-rot delay must retain collision.");
                    Call("RunDeadTorizoMain", slot, null);
                    break;
            }
            Require(slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision),
                $"{trigger}: rotting must set the native ignore-collision bit.");
            Require(!IsInteractive(), $"{trigger}: rotting corpse must leave the interaction list.");
            samus.Kinematics.XPosition = 90;
            Require(!Probe().Collided, $"{trigger}: the same movement must pass the rotting corpse.");
            ushort phase = slot.VariableA;
            slot.VariableA = 0xd3c7; // Native completed/no-operation phase.
            Call("TriggerDeadTorizoPowerBomb", slot);
            Require(slot.VariableA == 0xd3c7, "An ignored corpse must not restart from another power bomb.");
            Require(phase == 0xd3e6, "Each trigger must enter the native rotting phase.");

            void Call(string name, params object?[] arguments) =>
                typeof(RoomEnemySystem).GetMethod(name, flags)!.Invoke(enemies, arguments);
            bool IsInteractive()
            {
                Call("DetermineWhichEnemiesToProcess", (ushort)0, (ushort)0);
                return ((IReadOnlyList<ushort>)typeof(RoomEnemySystem)
                    .GetField("_interactiveEnemyIndexes", flags)!.GetValue(enemies)!).Contains(slot.NativeIndex);
            }
            SolidEnemyCollisionResult Probe() => SamusSolidEnemyCollision.Probe(samus.Kinematics,
                IsInteractive() ? [new SolidEnemyCollisionBody(slot.NativeIndex, slot.XPosition,
                    slot.YPosition, slot.XRadius, slot.YRadius, slot.FrozenTimer, slot.Properties)] : [],
                SamusCollisionDirection.Right, 16, 0);
        }
        Console.WriteLine("Dead Torizo: contact, shot, power bomb and delayed solid contact release collision correctly.");
        return 0;

        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidDataException(message);
        }
    }
}
