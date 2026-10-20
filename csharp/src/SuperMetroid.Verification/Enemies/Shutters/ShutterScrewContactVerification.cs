using System.Reflection;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Verification.Enemies.Shutters;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks contact-damage activation of the Three Musketeers' shootable shutter.</summary>
internal static class ShutterScrewContactVerification
{
    /// <summary>Confirms an idle shutter reacts to screw-attack contact and ignores contact with no damage.</summary>
    /// <remarks>#1269: native $A2:EED1 checks Samus's solid-enemy contact words and nonzero contact damage after its function; this starts the shutter in the 100% movie.</remarks>
    internal static void VerifyShutterScrewContact()
    {
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
            AssertEqual(6, (state ?? throw new InvalidOperationException("Verification requires state.")).InitialFunctionTableOffset, "the movie's shutter waits only for a shot");

            samus.HorizontalSpeed.ContactDamageIndex = contactDamage;
            samus.Kinematics.RecordSolidEnemyCollision(SamusCollisionDirection.Left, (ushort?)shutter.NativeIndex);
            foreach (var direction in new[] { SamusCollisionDirection.Right, SamusCollisionDirection.Up, SamusCollisionDirection.Down })
                samus.Kinematics.RecordSolidEnemyCollision(direction, null);

            runtime.Enemies.ShuttersBehavior.RunVerticalShutterMain(shutter, state, samus, Position16<RoomPixels>.Zero);
            string context = $"contact damage {contactDamage}";
            AssertEqual(reacts, state.Function is VerticalShutterFunction.MovingUp or VerticalShutterFunction.MovingDown,
                $"{context}: the idle shutter starts moving");
            AssertEqual(reacts, state.ShotActivated, $"{context}: the shot-activation flag");
        }
    }

    /// <summary>Runs the focused shutter screw-contact verification command.</summary>
    [VerificationCommand("--shutter-screw-contact")]
    internal static int ShutterScrewContactCommand()
    {
        VerifyShutterScrewContact();
        return 0;
    }
}
