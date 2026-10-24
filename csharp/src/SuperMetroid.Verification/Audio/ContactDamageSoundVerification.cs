using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Verification.Audio;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Audio;

/// <summary>Checks the Sciser contact-damage sound that delayed a door transition in the 13% recording.</summary>
internal static class ContactDamageSoundVerification
{
    /// <summary>
    /// When a speed-boosting, sparking or screw-attacking Samus damages an enemy by touch,
    /// NormalEnemyTouchAI queues library-two $0B with QueueSound_Lib2_Max1 ($A0:A55A). In the
    /// 13% movie Samus speed-boosts through an enemy before a door; the queued sound holds
    /// the door's sound wait for an extra frame.
    /// </summary>
    internal static void VerifyContactDamageSound()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MtEverest);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.SpeedBooster;
        RoomEnemySlot sciser = runtime.Enemies.SlotTable.Slots.First(slot => slot.DefinitionId == EnemyDefinitionId.Sciser);
        int healthBefore = sciser.Health;

        runtime.Enemies.Collision.ResolveNormalEnemyTouch(sciser, samus, TouchDeathAnimation.Play);

        AssertTrue(sciser.Health < healthBefore, "the speed booster damages the Sciser");
        SoundRequest request = runtime.Enemies.Audio.SoundRequests.Single();
        AssertEqual(Library2Sound.ContactKill, request.SoundEffect, "the hit queues library-two $0B");
        AssertEqual(1, request.MaximumQueued, "through QueueSound_Lib2_Max1");
        Console.WriteLine("  Contact-damage sound: Samus's damaging touch queues library-two $0B.");
    }

    /// <summary>Runs the focused Speed Booster contact-sound check from the verification command line.</summary>
    [VerificationCommand("--contact-damage-sound")]
    internal static int ContactDamageSoundCommand()
    {
        Suite(nameof(VerifyContactDamageSound), () => VerifyContactDamageSound());
        return 0;
    }
}
