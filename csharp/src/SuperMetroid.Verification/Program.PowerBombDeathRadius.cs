using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Game;
using static EnemyDropsVerification;

internal static class PowerBombDeathRadiusVerification
{
    // #1269: `$A0:A306` keeps its horizontal radius in `$12`, which EnemyDeath overwrites
    // with the dying actor's respawn bit. The 100% movie's Ki-Hunter therefore survives
    // the frame on which a higher slot dies, and takes the hit one frame later.
    internal static void VerifyPowerBombDeathRadius()
    {
        const ushort explosionX = 128;
        const ushort explosionY = 128;
        const byte radius = 64;

        // Ordinary kill: the radius becomes zero, so the lower in-range slot is untouched.
        Confirm(killerRespawns: false, killerLethal: true, lowerXOffset: 8,
            expectLowerHit: false, "ordinary kill zeroes the remaining horizontal radius");
        // Respawning kill: $4000 admits any horizontal distance; Y still uses `$14`.
        Confirm(killerRespawns: true, killerLethal: true, lowerXOffset: 200,
            expectLowerHit: true, "respawning kill widens the remaining horizontal radius");
        // No kill: the lower slot sees the unmodified radius.
        Confirm(killerRespawns: false, killerLethal: false, lowerXOffset: 8,
            expectLowerHit: true, "surviving higher slot leaves the radius unchanged");
        Confirm(killerRespawns: false, killerLethal: false, lowerXOffset: 200,
            expectLowerHit: false, "unmodified radius still rejects a distant lower slot");
        Console.WriteLine("Power-bomb death radius: EnemyDeath's $12 respawn word resizes the explosion for lower slots that frame.");

        void Confirm(bool killerRespawns, bool killerLethal, int lowerXOffset, bool expectLowerHit, string label)
        {
            var fixture = CreateEnemyDropFixture(CreateDropTestSamus(), [1]);
            var enemies = fixture.System;
            var higher = Place(enemies.SlotTable.Slots[1], explosionX, killerLethal ? (ushort)1 : (ushort)1000);
            higher.Properties = killerRespawns ? (ushort)EnemyProperties.RespawnIfKilled : (ushort)0;
            var lower = Place(enemies.SlotTable.Slots[0], unchecked((ushort)(explosionX + lowerXOffset)), 1000);

            enemies.Collision.ResolveOrdinaryPowerBombHits(fixture.Bus, new(explosionX, explosionY), radius);

            if (killerLethal)
            {
                AssertEqual(killerRespawns ? EnemyDefinitionId.Respawn : (ushort)0,
                    higher.EnemyDefinitionPointer, label + ": higher slot dies first");
            }
            AssertEqual(expectLowerHit ? (ushort)48 : (ushort)0, lower.InvincibilityTimer,
                label + ": lower slot invincibility");
            AssertEqual(expectLowerHit, lower.Health < 1000, label + ": lower slot damage");
        }

        static RoomEnemySlot Place(RoomEnemySlot actor, ushort x, ushort health)
        {
            actor.EnemyDefinitionPointer = EnemyDefinitionId.Atomic;
            actor.Definition = RoomEnemyDefinitionCatalog.Get(actor.EnemyDefinitionPointer);
            actor.AiBank = actor.Definition.Bank;
            actor.Health = health;
            actor.Position = actor.Position.WithWhole(new(x, explosionY));
            actor.Radius = new(actor.Definition.Radius.X, actor.Definition.Radius.Y);
            actor.Properties = 0;
            return actor;
        }
    }

    [VerificationCommand("--power-bomb-death-radius")]
    internal static int PowerBombDeathRadiusCommand()
    {
        VerifyPowerBombDeathRadius();
        return 0;
    }
}
