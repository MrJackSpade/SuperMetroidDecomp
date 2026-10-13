using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class ShutterScrewContactVerification
{
    // #1269: MainAI_ShutterShootable_ShutterDestroyable_Kamer ($A2:EED1) runs its function,
    // then, while idle, reacts when Samus's four solid-enemy collision words AND to this
    // enemy and her contact damage is nonzero ($A2:EEE9-EF04). In the 100% movie a screw
    // attack pressed against a Three Musketeers' shootable shutter starts it moving.
    internal static void VerifyShutterScrewContact()
    {
        Confirm(contactDamage: SamusContactDamageIndex.ScrewAttack, reacts: true);
        Confirm(contactDamage: SamusContactDamageIndex.None, reacts: false);
        Console.WriteLine("Shutter screw contact: an idle shutter touched with contact damage reacts like a shot.");

        static void Confirm(SamusContactDamageIndex contactDamage, bool reacts)
        {
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.ThreeMusketeers);
            var samus = runtime.Player.Samus!;
            RoomEnemySlot shutter = runtime.Enemies.SlotTable.Slots[10];
            VerticalShutterEnemyState state = runtime.Enemies.VerticalShutterStates[10]!;
            AssertEqual((ushort)6, state.InitialFunctionTableOffset, "the movie's shutter waits only for a shot");

            samus.HorizontalSpeed.ContactDamageIndex = contactDamage;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            samus.Kinematics.RecordSolidEnemyCollision(SamusCollisionDirection.Left, (ushort?)shutter.NativeIndex);
            foreach (var direction in new[] { SamusCollisionDirection.Right, SamusCollisionDirection.Up, SamusCollisionDirection.Down })
                samus.Kinematics.RecordSolidEnemyCollision(direction, (ushort?)null);

            runtime.Enemies.ShuttersBehavior.RunVerticalShutterMain(shutter, state, samus, Position16<RoomPixels>.Zero);
            string context = $"contact damage {contactDamage}";
            AssertEqual(reacts, state.Function is VerticalShutterFunction.MovingUp or VerticalShutterFunction.MovingDown,
                $"{context}: the idle shutter starts moving");
            AssertEqual(reacts, state.ShotActivated, $"{context}: the shot-activation flag");
        }
    }

    [VerificationCommand("--shutter-screw-contact")]
    internal static int ShutterScrewContactCommand()
    {
        VerifyShutterScrewContact();
        return 0;
    }
}
