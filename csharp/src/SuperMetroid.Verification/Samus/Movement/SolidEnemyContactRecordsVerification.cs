using System.Globalization;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
namespace SuperMetroid.Verification.Samus.Movement;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks Samus's per-direction solid-enemy contact records (#1702): they hold the enemies she met, the corpses' contact
/// test finds exactly the enemies the cartridge's slot-word comparison finds, and a room that unloads takes its enemies
/// out of them.
/// </summary>
internal static class SolidEnemyContactRecordsVerification
{
    /// <summary>Samus's four contact directions, in the order her records keep them.</summary>
    private static readonly SamusCollisionDirection[] ContactDirections =
        [SamusCollisionDirection.Left, SamusCollisionDirection.Right, SamusCollisionDirection.Up, SamusCollisionDirection.Down];

    /// <summary>Runs the contact-test and room-change checks.</summary>
    internal static void VerifySolidEnemyContactRecords()
    {
        VerifyEveryContactCombination();
        VerifyRoomChangeForgetsContacts();
        Console.WriteLine("Solid-enemy contact records: the contact test matches the cartridge's for every combination, and a room change forgets the replaced room's enemies.");
    }

    /// <summary>
    /// Over every combination of the four directions meeting no enemy or the enemy in any of the room's places, and for
    /// every place, the corpses' contact test (<see cref="SamusKinematicsState.DidCollideWithSolidEnemy"/>) finds the
    /// enemy exactly when one of the cartridge's four contact words, each the met enemy's slot offset or all ones, equals
    /// that enemy's offset; the records hold the enemies recorded, and clearing them leaves none.
    /// </summary>
    private static void VerifyEveryContactCombination()
    {
        RoomEnemySystem room = new(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        IReadOnlyList<RoomEnemySlot> places = room.SlotTable.Slots;
        var kinematics = new SamusKinematicsState();
        int choices = places.Count + 1;
        int[] choice = new int[ContactDirections.Length];
        int[] nativeWords = new int[ContactDirections.Length];
        int combinations = 0;
        for (choice[0] = 0; choice[0] < choices; choice[0]++)
        for (choice[1] = 0; choice[1] < choices; choice[1]++)
        for (choice[2] = 0; choice[2] < choices; choice[2]++)
        for (choice[3] = 0; choice[3] < choices; choice[3]++)
        {
            for (int direction = 0; direction < ContactDirections.Length; direction++)
            {
                int? place = choice[direction] == 0 ? null : choice[direction] - 1;
                RoomEnemySlot? met = place is { } ordinal ? places[ordinal] : null;
                kinematics.RecordSolidEnemyCollision(ContactDirections[direction], met);
                nativeWords[direction] = EnemySlotOffsets.WordOf(place);
                if (!ReferenceEquals(met, kinematics.SolidEnemyContacts[direction]))
                    throw new InvalidDataException($"{Describe(choice)}: direction {direction} does not hold the enemy recorded.");
            }
            foreach (RoomEnemySlot enemy in places)
            {
                int offset = EnemySlotOffsets.OffsetOf(enemy.SlotIndex);
                bool cartridge = nativeWords[0] == offset || nativeWords[1] == offset || nativeWords[2] == offset || nativeWords[3] == offset;
                if (kinematics.DidCollideWithSolidEnemy(enemy) != cartridge)
                {
                    throw new InvalidDataException(
                        $"{Describe(choice)}: the cartridge's contact test {(cartridge ? "finds" : "does not find")} the enemy in place " +
                        $"{enemy.SlotIndex.ToString(CultureInfo.InvariantCulture)}, the port's disagrees.");
                }
            }
            combinations++;
        }
        AssertEqual(choices * choices * choices * choices, combinations, "every combination of the four directions' contacts is checked");

        kinematics.ClearSolidEnemyCollisions();
        foreach (RoomEnemySlot enemy in places)
            AssertTrue(!kinematics.DidCollideWithSolidEnemy(enemy), $"cleared records find no enemy, place {enemy.SlotIndex}");
        for (int direction = 0; direction < ContactDirections.Length; direction++)
            AssertEqual<ISolidEnemy?>(null, kinematics.SolidEnemyContacts[direction], $"cleared records hold no enemy, direction {direction}");
    }

    /// <summary>
    /// A room that unloads takes its enemies out of Samus's records: contacts and a waiting launch shake naming the Three
    /// Musketeers' enemies are gone once the room loads again, and the new room's enemy in the same place is not met.
    /// </summary>
    private static void VerifyRoomChangeForgetsContacts()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.ThreeMusketeers);
        var samus = runtime.Player.IntroducedSamus;
        RoomEnemySlot replaced = runtime.Enemies.SlotTable.Slots[10];
        foreach (SamusCollisionDirection direction in ContactDirections)
            samus.Kinematics.RecordSolidEnemyCollision(direction, replaced);
        samus.Kinematics.RequestWallJumpLaunchShake(replaced);

        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.ThreeMusketeers);
        RoomEnemySlot loaded = runtime.Enemies.SlotTable.Slots[10];
        AssertTrue(!ReferenceEquals(replaced, loaded), "the reloaded room's enemies are new places");
        for (int direction = 0; direction < ContactDirections.Length; direction++)
            AssertEqual<ISolidEnemy?>(null, samus.Kinematics.SolidEnemyContacts[direction], $"the room change forgets direction {direction}'s contact");
        AssertTrue(!samus.Kinematics.DidCollideWithSolidEnemy(loaded), "the new room's enemy in the same place is not met");
        AssertEqual<ISolidEnemy?>(null, samus.Kinematics.TakeWallJumpLaunchShake(), "the room change withdraws the waiting launch shake");
    }

    /// <summary>The four directions' contacts of a combination, for a failure message.</summary>
    private static string Describe(int[] choice) =>
        "contacts left/right/up/down " + string.Join("/", choice.Select(c => c == 0 ? "none" : (c - 1).ToString(CultureInfo.InvariantCulture)));

    /// <summary>Runs the focused solid-enemy contact records verification command.</summary>
    [VerificationCommand("--solid-enemy-contact-records")]
    internal static int SolidEnemyContactRecordsCommand()
    {
        VerifySolidEnemyContactRecords();
        return 0;
    }
}
