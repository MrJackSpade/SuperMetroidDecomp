using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hardware;

/// <summary>Reproduces Shaktool segment contact during the recorded 100% route.</summary>
internal static class ShaktoolSamusContactVerification
{
    /// <summary>
    /// Shaktool's header touch callback <c>$AA:DF2F</c> is the common normal-enemy touch: the 100% movie's
    /// Samus falls into a segment at update 292109 and native deals contact damage and knockback. A touch
    /// table without a Shaktool reaction skips the actor, so Samus passes through unharmed.
    /// </summary>
    internal static void VerifyShaktoolSamusContact()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.Shaktool);
        RoomEnemySlot segment = runtime.Enemies.SlotTable.Slots[0];
        AssertEqual(EnemyDefinitionId.Shaktool, segment.EnemyDefinitionPointer, "slot zero is a Shaktool segment");
        AssertEqual(EnemyInteractionCallback.ShaktoolTouch, EnemyCollision.TouchAi(segment), "Shaktool's header touch callback");
        AssertTrue(segment.SpritemapPointer != 0, "the loaded segment publishes a sprite map");

        SamusState samus = runtime.Player.Samus!;
        samus.Position = samus.Position.WithWhole(segment.Position.Whole);
        samus.Kinematics.Radius = new(5, 19);
        samus.InvincibilityTimer = NativeCountdown.Start(0);
        ushort healthBefore = samus.Health;

        runtime.Enemies.ProcessingWindow.DetermineWhichEnemiesToProcess(
            new Position16<RoomPixels>((ushort)0, (ushort)0));
        bool touched = runtime.Enemies.Collision.ResolveOrdinarySamusContact(
            samus, 0, runtime.Room.LevelData, onlyNativeEnemyIndex: 0);

        AssertTrue(touched, "the overlapping Shaktool segment runs its touch reaction");
        AssertTrue(samus.Health < healthBefore, "$AA:DF2F deals common contact damage");
        AssertTrue(!samus.InvincibilityTimer.IsZero, "Shaktool contact damage starts invincibility");
        Console.WriteLine("  Shaktool Samus contact: a touching segment damages Samus through the common touch.");
    }

    /// <summary>Runs the focused Shaktool damage and invincibility verification.</summary>
    [VerificationCommand("--shaktool-samus-contact")]
    internal static int ShaktoolSamusContactCommand()
    {
        Suite(nameof(VerifyShaktoolSamusContact), () => VerifyShaktoolSamusContact());
        return 0;
    }
}
