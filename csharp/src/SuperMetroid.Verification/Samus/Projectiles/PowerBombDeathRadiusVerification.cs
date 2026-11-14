using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the native Power Bomb radius alias after a higher enemy slot dies.</summary>
internal static class PowerBombDeathRadiusVerification
{
    /// <summary>
    /// Confirms EnemyDeath overwrites the shared horizontal radius word with the killed
    /// slot's respawn flag before lower slots are checked that same frame.
    /// </summary>
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
            higher.Contact = EnemyContact.None;
            higher.Processing = EnemyProcessing.None;
            higher.Presence = killerRespawns ? EnemyPresence.RespawnsIfKilled : EnemyPresence.None;
            var lower = Place(enemies.SlotTable.Slots[0], unchecked((ushort)(explosionX + lowerXOffset)), 1000);

            enemies.Collision.ResolveOrdinaryPowerBombHits(new(explosionX, explosionY), radius, RoomEnemyFrameFixtures.OpenRoom(new(16, 16)), new(0, 0));

            if (killerLethal)
            {
                AssertEqual(EnemyDefinitionId.None, higher.DefinitionId, label + ": higher slot dies first");
                AssertEqual(killerRespawns, higher.AwaitsRespawn, label + ": a respawning killer's slot awaits its respawn");
            }
            AssertEqual(expectLowerHit ? (ushort)48 : (ushort)0, lower.InvincibilityTimer,
                label + ": lower slot invincibility");
            AssertEqual(expectLowerHit, lower.Health < 1000, label + ": lower slot damage");
        }

        static RoomEnemySlot Place(RoomEnemySlot actor, ushort x, ushort health)
        {
            actor.Bind(EnemyDefinitionId.Atomic, actor.Definition);
            actor.Bind(actor.DefinitionId, RoomEnemyDefinitionLookup.Get(actor.DefinitionId));
            actor.Health = health;
            actor.Position = actor.Position.WithWhole(new(x, explosionY));
            actor.Radius = new(actor.Definition.Radius.X, actor.Definition.Radius.Y);
            actor.Contact = EnemyContact.None;
            actor.Processing = EnemyProcessing.None;
            actor.Presence = EnemyPresence.None;
            return actor;
        }
    }

    /// <summary>Runs the focused Power Bomb collision-order check.</summary>
    [VerificationCommand("--power-bomb-death-radius")]
    internal static int PowerBombDeathRadiusCommand()
    {
        VerifyPowerBombDeathRadius();
        return 0;
    }
}
