using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Verification.Enemies.Shutters;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.AssetExtraction.Enemies;

/// <summary>Checks contact-damage activation of the Three Musketeers' shootable shutter.</summary>
internal static class ShutterScrewContactVerification
{
    /// <summary>Confirms an idle shutter reacts to screw-attack contact and ignores contact with no damage.</summary>
    /// <remarks>#1269: native $A2:EED1 checks Samus's solid-enemy contact words and nonzero contact damage after its function; this starts the shutter in the 100% movie.</remarks>
    internal static void VerifyShutterScrewContact()
    {
        VerifySolidContactSlotCombination();
        Confirm(contactDamage: SamusContactDamageIndex.ScrewAttack, reacts: true);
        Confirm(contactDamage: SamusContactDamageIndex.None, reacts: false);
        Console.WriteLine("Shutter screw contact: an idle shutter touched with contact damage reacts like a shot.");

        static void Confirm(SamusContactDamageIndex contactDamage, bool reacts)
        {
            var runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.ThreeMusketeers);
            var samus = runtime.Player.IntroducedSamus;
            RoomEnemySlot shutter = runtime.Enemies.SlotTable.Slots[10];
            VerticalShutterEnemyState state = (runtime.Enemies.VerticalShutterStates[10] ?? throw new InvalidOperationException("Verification requires runtime.Enemies.VerticalShutterStates[10]."));
            AssertEqual(ShutterTriggerMode.OneShotReaction, (state ?? throw new InvalidOperationException("Verification requires state.")).TriggerMode, "the movie's shutter waits only for a shot");

            samus.HorizontalSpeed.ContactDamageIndex = contactDamage;
            samus.Kinematics.RecordSolidEnemyCollision(SamusCollisionDirection.Left, shutter.SlotIndex);
            foreach (var direction in new[] { SamusCollisionDirection.Right, SamusCollisionDirection.Up, SamusCollisionDirection.Down })
                samus.Kinematics.RecordSolidEnemyCollision(direction, null);

            runtime.Enemies.ShuttersBehavior.RunVerticalShutterMain(shutter, state, samus, Position16<RoomPixels>.Zero);
            string context = $"contact damage {contactDamage}";
            AssertEqual(reacts, state.Function is VerticalShutterFunction.MovingUp or VerticalShutterFunction.MovingDown,
                $"{context}: the idle shutter starts moving");
            AssertEqual(reacts, state.ShotActivated, $"{context}: the shot-activation flag");
        }
    }

    /// <summary>
    /// Checks <see cref="SolidContactSlotCombination"/> (#1471): no contact names no enemy, one enemy met in several
    /// directions names that enemy, and two enemies met at once name the slot their ordinals share, the same slot the
    /// cartridge reaches by ANDing the four contact words, each the enemy's slot offset or all ones.
    /// </summary>
    private static void VerifySolidContactSlotCombination()
    {
        AssertEqual(null, SolidContactSlotCombination.Combine([null, null, null, null], EnemySlotOrder.Capacity),
            "no contact names no enemy");
        AssertEqual(10, SolidContactSlotCombination.Combine([10, null, 10, null], EnemySlotOrder.Capacity),
            "one enemy met twice names that enemy");
        AssertEqual(31, SolidContactSlotCombination.Combine([null, null, null, 31], EnemySlotOrder.Capacity),
            "the last slot is named, not taken for no contact");
        int nativeCombination = EnemySlotOffsets.WordOf(6) & EnemySlotOffsets.WordOf(3) &
            EnemySlotOffsets.WordOf(null) & EnemySlotOffsets.WordOf(null);
        AssertEqual(EnemySlotOffsets.SlotOf(nativeCombination),
            SolidContactSlotCombination.Combine([6, 3, null, null], EnemySlotOrder.Capacity),
            "two enemies met at once name the slot the native contact words AND to");
        AssertEqual(2, SolidContactSlotCombination.Combine([6, 3, null, null], EnemySlotOrder.Capacity),
            "two enemies met at once can name a third");
    }

    /// <summary>Runs the focused shutter screw-contact verification command.</summary>
    [VerificationCommand("--shutter-screw-contact")]
    internal static int ShutterScrewContactCommand()
    {
        VerifyShutterScrewContact();
        return 0;
    }
}
