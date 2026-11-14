using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>
/// Checks #1800: the setup of the escape room before the old Tourian escape shaft ($8F:C953) places room object $84:B968,
/// which starts the room's acid rising once Samus passes below and right of its trigger point.
/// </summary>
internal static class EscapeAcidRiseVerification
{
    /// <summary>The level the room's effect starts the acid at, the room effect's $05E0.</summary>
    private const int AcidStartLevel = 1504;

    /// <summary>
    /// The trigger's definitions match the original's setup and room object; loading the room through the room loader
    /// places the trigger, which wakes on the update Samus passes its point, gives the acid its speed and delay and
    /// leaves. The acid's motion arms on the next update, waits sixteen updates and first rises on the eighteenth update
    /// after the trigger's, from the level it started at.
    /// </summary>
    internal static void VerifyEscapeAcidRise()
    {
        VerifyDefinitionsAgainstRom(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(StoryEvent.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.TourianEscape4);
        bool TriggerPresent() => runtime.RoomObjects.PopulationSlots.Any(slot => slot.Behaviour is EscapeAcidRiseObject);
        AssertTrue(TriggerPresent(), "the room's setup places the acid trigger");

        RoomFxOverlayState acid = runtime.RoomFxOverlay;
        AssertEqual(RoomFxType.Acid, acid.Type, "the room's liquid is acid");
        FixedCoordinate loaded = acid.BaseY ?? throw new InvalidOperationException("The escape room's acid has no surface.");
        AssertEqual(AcidStartLevel, (int)loaded.Whole, "the acid starts at the room effect's level");
        AssertEqual(SignedEightEight.Zero, acid.LiquidLevelSpeed, "the acid starts still");

        SamusState samus = runtime.Player.IntroducedSamus;
        // Each step places Samus, steps one update, and reports whether, where the update left her, she had passed the
        // trigger point; the trigger reads her after her own movement.
        bool StepWithSamusAt(int x, int y)
        {
            samus.Position = samus.Position.WithWhole(new(x, y));
            runtime.StepFrame(0);
            return EscapeAcidRiseObjectDefinitions.TriggerPoint.IsPassedAt(samus.Position.Whole);
        }

        AssertTrue(!StepWithSamusAt(0x00e8, 0x05a0), "Samus left of X $F0 has not passed the point");
        AssertTrue(TriggerPresent(), "Samus left of the point leaves the trigger waiting");
        AssertTrue(!StepWithSamusAt(0x00f8, 0x0500), "Samus above Y $540 has not passed the point");
        AssertTrue(TriggerPresent(), "Samus above the point leaves the trigger waiting");
        AssertEqual(SignedEightEight.Zero, acid.LiquidLevelSpeed, "the acid stays still until Samus passes the point");
        AssertEqual<FixedCoordinate?>(loaded, acid.BaseY, "the acid holds its level until Samus passes the point");

        AssertTrue(StepWithSamusAt(0x00f8, 0x05a0), "Samus right of X $F0 and below Y $540 has passed the point");
        AssertTrue(!TriggerPresent(), "the trigger leaves on the update it wakes");
        AssertEqual(EscapeAcidRiseObjectDefinitions.RiseSpeed, acid.LiquidLevelSpeed, "the trigger gives the acid its rising speed");
        AssertEqual(EscapeAcidRiseObjectDefinitions.RiseDelay, acid.LiquidMoveDelay.RemainingFrames, "the trigger gives the acid its delay");
        AssertEqual<FixedCoordinate?>(loaded, acid.BaseY, "the acid has not moved on the trigger's update");

        // The next update arms the acid's wait, which counts the delay down over the sixteen updates after it.
        for (int update = 1; update <= 17; update++)
        {
            runtime.StepFrame(0);
            AssertEqual<FixedCoordinate?>(loaded, acid.BaseY, $"the acid holds its level {update} updates after the trigger");
            AssertEqual(update == 1 ? EscapeAcidRiseObjectDefinitions.RiseDelay : EscapeAcidRiseObjectDefinitions.RiseDelay - (update - 1),
                acid.LiquidMoveDelay.RemainingFrames, $"the acid's delay {update} updates after the trigger");
        }

        runtime.StepFrame(0);
        AssertEqual<FixedCoordinate?>(new FixedCoordinate(AcidStartLevel, loaded.Fraction - 104 * 256), acid.BaseY,
            "the acid first rises 104/256 of a pixel on the eighteenth update after the trigger");
        Console.WriteLine("Escape acid rise: setup places the trigger; Samus past ($F0,$540) starts the acid rising 18 updates later.");
    }

    /// <summary>
    /// The trigger's block, point, speed and delay against the original: setup $8F:C953 spawns room object $84:B968 at
    /// block ($10,$10); its entry's list installs pre-instruction $84:B948, sleeps and deletes; the pre-instruction loads
    /// the point ($F0,$540), then the speed $FF98 and the delay $10.
    /// </summary>
    private static void VerifyDefinitionsAgainstRom(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(address);
        const int Setup = 0x8fc953;
        const int Entry = 0x84b968;
        AssertEqual(EscapeAcidRiseObjectDefinitions.BlockX, Word(Setup + 4) & 0xff, "the setup's spawn column");
        AssertEqual(EscapeAcidRiseObjectDefinitions.BlockY, Word(Setup + 4) >> 8, "the setup's spawn row");
        AssertEqual((ushort)(Entry & 0xffff), Word(Setup + 6), "the setup spawns the acid trigger");
        int list = 0x840000 | Word(Entry + 2);
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, (RoomObjectInstruction)Word(list), "the trigger first installs its wait");
        int wait = 0x840000 | Word(list + 2);
        AssertEqual(RoomObjectInstruction.Sleep, (RoomObjectInstruction)Word(list + 4), "the trigger sleeps while it waits");
        AssertEqual(RoomObjectInstruction.Delete, (RoomObjectInstruction)Word(list + 6), "past its sleep the trigger ends");
        AssertEqual(EscapeAcidRiseObjectDefinitions.TriggerPoint.Point.X, Word(wait + 1), "the trigger point's X");
        AssertEqual(EscapeAcidRiseObjectDefinitions.TriggerPoint.Point.Y, Word(wait + 6), "the trigger point's Y");
        AssertEqual(EscapeAcidRiseObjectDefinitions.RiseSpeed, SignedEightEight.FromTwoHundredFiftySixthsOfPixel(unchecked((short)Word(wait + 0x10))),
            "the acid's rising speed");
        AssertEqual(EscapeAcidRiseObjectDefinitions.RiseDelay, (int)Word(wait + 0x16), "the acid's delay");
    }
}
