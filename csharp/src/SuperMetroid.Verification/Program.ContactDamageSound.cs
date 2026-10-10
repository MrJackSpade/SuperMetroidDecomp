using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// When a speed-boosting, sparking or screw-attacking Samus damages an enemy by touch,
    /// NormalEnemyTouchAI queues library-two $0B with QueueSound_Lib2_Max1 ($A0:A55A). In the
    /// 13% movie Samus speed-boosts through an enemy before a door; the queued sound holds
    /// the door's sound wait for an extra frame.
    /// </summary>
    private static void VerifyContactDamageSound()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xd0b9);
        SamusState samus = runtime.Samus!;
        samus.HorizontalSpeed.ContactDamageIndex = 1;
        RoomEnemySlot sciser = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Sciser);
        ushort healthBefore = sciser.Health;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetMethod("ResolveNormalEnemyTouch", flags)!
            .Invoke(runtime.Enemies, [sciser, samus, false]);

        AssertTrue(sciser.Health < healthBefore, "the speed booster damages the Sciser");
        EnemySoundRequest request = runtime.Enemies.SoundRequests.Single();
        AssertEqual(SoundEffectLibrary2Sounds.EnemyContactDamaged, request.SoundEffect, "the hit queues library-two $0B");
        AssertEqual((byte)1, request.MaximumQueued, "through QueueSound_Lib2_Max1");
        Console.WriteLine("  Contact-damage sound: Samus's damaging touch queues library-two $0B.");
    }
}
