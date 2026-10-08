using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // #1269: MainAI_ShutterShootable_ShutterDestroyable_Kamer ($A2:EED1) runs its function,
    // then, while idle, reacts when Samus's four solid-enemy collision words AND to this
    // enemy and her contact damage is nonzero ($A2:EEE9-EF04). In the 100% movie a screw
    // attack pressed against a Three Musketeers' shootable shutter starts it moving.
    private static void VerifyShutterScrewContact()
    {
        Confirm(contactDamage: 3, reacts: true);
        Confirm(contactDamage: 0, reacts: false);
        Console.WriteLine("Shutter screw contact: an idle shutter touched with contact damage reacts like a shot.");

        static void Confirm(ushort contactDamage, bool reacts)
        {
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.ThreeMusketeers);
            var samus = runtime.Samus!;
            RoomEnemySlot shutter = runtime.Enemies.Slots[10];
            VerticalShutterEnemyState state = runtime.Enemies.VerticalShutterStates[10]!;
            AssertEqual((ushort)6, state.InitialFunctionTableOffset, "the movie's shutter waits only for a shot");

            samus.HorizontalSpeed.ContactDamageIndex = contactDamage;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var record = typeof(SamusKinematicsState).GetMethod("RecordSolidEnemyCollision", flags)!;
            record.Invoke(samus.Kinematics, [SamusCollisionDirection.Left, (ushort?)shutter.NativeIndex]);
            foreach (var direction in new[] { SamusCollisionDirection.Right, SamusCollisionDirection.Up, SamusCollisionDirection.Down })
                record.Invoke(samus.Kinematics, [direction, (ushort?)null]);

            typeof(RoomEnemySystem).GetMethod("RunVerticalShutterMain", flags)!
                .Invoke(runtime.Enemies, [shutter, state, samus, (ushort)0, (ushort)0]);
            string context = $"contact damage {contactDamage}";
            AssertEqual(reacts, state.Function is VerticalShutterFunction.MovingUp or VerticalShutterFunction.MovingDown,
                $"{context}: the idle shutter starts moving");
            AssertEqual(reacts, state.ShotActivated, $"{context}: the shot-activation flag");
        }
    }
}
