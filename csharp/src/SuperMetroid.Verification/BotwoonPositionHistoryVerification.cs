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

/// <summary>Checks Botwoon's initial body history and intangible head collision property.</summary>
internal static class BotwoonPositionHistoryVerification
{
    /// <summary>Confirms delayed head positions start at spawn while body history remains zeroed and hidden/dying heads use property $0400.</summary>
    /// <remarks>#1269: <c>Initialise_Enemies</c> zeroes $7E:9000-$93FF; seeding body history with head position moved initial segments. Native $0400 ignores Samus collision, unlike $8000.</remarks>
    internal static void VerifyBotwoonPositionHistory()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Botwoon);

        RoomEnemySlot head = runtime.Enemies.SlotTable.Slots.Single(slot =>
            slot.EnemyDefinitionPointer == EnemyDefinitionId.Botwoon);
        BotwoonEnemyState state = runtime.Enemies.Botwoon
            ?? throw new InvalidOperationException("Botwoon's room did not initialize its state.");
        AssertTrue(state.History.All(position => position == Position16<RoomPixels>.Zero),
            "Botwoon's position history starts as Initialise_Enemies' zeroed RAM");
        AssertTrue(state.HeadHistory.All(position => position == head.Position.Whole),
            "InitAI_Botwoon seeds its four delayed head positions with the spawn position");

        // $B3:961B and $B3:96F5 are ORA #$0400 (ignore Samus collision), never $8000. The
        // dying head must leave the shot pass or a late hit interrupts its fall.
        AssertTrue(head.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
            !head.Properties.HasAny(EnemyProperties.SolidToSamus),
            "InitAI_Botwoon hides the head from collision with property $0400");
        head.Properties = head.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        head.Health = 0;
        runtime.Enemies.BotwoonBehavior.ResolveBotwoonCombatAfterCommon(head);
        AssertTrue(head.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
            !head.Properties.HasAny(EnemyProperties.SolidToSamus),
            "a lethal hit makes the head intangible with property $0400");
        Console.WriteLine("Botwoon position history and collision bit: ring starts zeroed; $0400 hides the head.");
    }

    /// <summary>Runs the focused Botwoon history and collision-bit verification command.</summary>
    [VerificationCommand("--botwoon-position-history")]
    internal static int BotwoonPositionHistoryCommand()
    {
        VerifyBotwoonPositionHistory();
        return 0;
    }
}
