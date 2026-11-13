using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Shutters;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.AssetExtraction.Enemies;

/// <summary>Checks contact-damage activation of the Three Musketeers' shootable shutter.</summary>
internal static class ShutterScrewContactVerification
{
    /// <summary>The Three Musketeers shutter's place in the room's enemy table.</summary>
    private const int ShutterPlace = 10;

    /// <summary>Samus's four contact directions, in the order her records keep them.</summary>
    private static readonly SamusCollisionDirection[] ContactDirections =
        [SamusCollisionDirection.Left, SamusCollisionDirection.Right, SamusCollisionDirection.Up, SamusCollisionDirection.Down];

    /// <summary>Confirms an idle shutter reacts to screw-attack contact and ignores contact with no damage.</summary>
    /// <remarks>
    /// #1269: native $A2:EED1 checks Samus's solid-enemy contact words and nonzero contact damage after its function; this
    /// starts the shutter in the 100% movie. #1702: the contacts hold the enemies Samus met, and the shutter reads them by
    /// reference through the room's places.
    /// </remarks>
    internal static void VerifyShutterScrewContact()
    {
        VerifySolidContactSlotCombination();
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // Places 11 and 14 share only the shutter's position bits; the shutter's and place 5's share none of them.
        Confirm(runtime, SamusContactDamageIndex.ScrewAttack, [ShutterPlace, null, null, null], reacts: true, "the shutter met moving left");
        Confirm(runtime, SamusContactDamageIndex.None, [ShutterPlace, null, null, null], reacts: false, "the shutter met without contact damage");
        Confirm(runtime, SamusContactDamageIndex.ScrewAttack, [11, null, null, 14], reacts: true, "two other enemies whose places combine to the shutter's");
        Confirm(runtime, SamusContactDamageIndex.ScrewAttack, [ShutterPlace, null, 5, null], reacts: false, "the shutter and an enemy whose places combine to another");
        Console.WriteLine("Shutter screw contact: an idle shutter reacts like a shot when Samus's contacts, combined, name it and she has contact damage.");

        // Each case loads the room afresh, so its shutter starts idle.
        static void Confirm(SuperMetroidRuntime runtime, SamusContactDamageIndex contactDamage, int?[] contactPlaces, bool reacts, string contacts)
        {
            runtime.LoadCartridgeRoomForDebug(RoomDefinitions.ThreeMusketeers);
            var samus = runtime.Player.IntroducedSamus;
            RoomEnemySlot shutter = runtime.Enemies.SlotTable.Slots[ShutterPlace];
            VerticalShutterEnemyState state = (runtime.Enemies.VerticalShutterStates[ShutterPlace] ?? throw new InvalidOperationException("Verification requires runtime.Enemies.VerticalShutterStates[10]."));
            AssertEqual(ShutterTriggerMode.OneShotReaction, state.TriggerMode, "the movie's shutter waits only for a shot");

            samus.HorizontalSpeed.ContactDamageIndex = contactDamage;
            for (int direction = 0; direction < ContactDirections.Length; direction++)
            {
                samus.Kinematics.RecordSolidEnemyCollision(ContactDirections[direction],
                    contactPlaces[direction] is { } place ? runtime.Enemies.SlotTable.Slots[place] : null);
            }

            runtime.Enemies.ShuttersBehavior.RunVerticalShutterMain(shutter, state, samus, Position16<RoomPixels>.Zero);
            string context = $"{contacts}, contact damage {contactDamage}";
            AssertEqual(reacts, state.Action is VerticalShutterAction.MovingUp or VerticalShutterAction.MovingDown,
                $"{context}: the idle shutter starts moving");
            AssertEqual(reacts, state.ShotActivated, $"{context}: the shot-activation flag");
        }
    }

    /// <summary>
    /// Checks <see cref="SolidContactSlotCombination"/> (#1471, #1702) over every combination of contacts the shutters'
    /// idle test reads: each of the four directions meeting no enemy or the enemy in any of the room's places, recorded
    /// as Samus's movement records them. The place named is the one the cartridge reaches by ANDing the four contact
    /// words, each the enemy's slot offset or all ones, and no place when she met none.
    /// </summary>
    private static void VerifySolidContactSlotCombination()
    {
        RoomEnemySystem room = new(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        IReadOnlyList<RoomEnemySlot> places = room.SlotTable.Slots;
        var kinematics = new SamusKinematicsState();
        int choices = places.Count + 1;
        int[] choice = new int[ContactDirections.Length];
        int combinations = 0;
        for (choice[0] = 0; choice[0] < choices; choice[0]++)
        for (choice[1] = 0; choice[1] < choices; choice[1]++)
        for (choice[2] = 0; choice[2] < choices; choice[2]++)
        for (choice[3] = 0; choice[3] < choices; choice[3]++)
        {
            int nativeWord = EnemySlotOffsets.NoEnemy;
            for (int direction = 0; direction < ContactDirections.Length; direction++)
            {
                int? place = choice[direction] == 0 ? null : choice[direction] - 1;
                kinematics.RecordSolidEnemyCollision(ContactDirections[direction], place is { } met ? places[met] : null);
                nativeWord &= EnemySlotOffsets.WordOf(place);
            }
            RoomEnemySlot? expected = nativeWord == EnemySlotOffsets.NoEnemy ? null : places[EnemySlotOffsets.SlotOf(nativeWord)];
            ISolidEnemy? named = SolidContactSlotCombination.NamedPlace<ISolidEnemy>(kinematics.SolidEnemyContacts, places);
            if (!ReferenceEquals(expected, named))
            {
                throw new InvalidDataException(
                    $"Contacts left/right/up/down {string.Join("/", choice.Select(c => c == 0 ? "none" : (c - 1).ToString(System.Globalization.CultureInfo.InvariantCulture)))}: " +
                    $"the cartridge's combined word {nativeWord:X4} names {expected?.SlotIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "no enemy"}, " +
                    $"the combination names {(named as RoomEnemySlot)?.SlotIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "no enemy"}.");
            }
            combinations++;
        }
        AssertEqual(choices * choices * choices * choices, combinations, "every combination of the four directions' contacts is checked");
    }

    /// <summary>Runs the focused shutter screw-contact verification command.</summary>
    [VerificationCommand("--shutter-screw-contact")]
    internal static int ShutterScrewContactCommand()
    {
        VerifyShutterScrewContact();
        return 0;
    }
}
