using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Audio;

/// <summary>Checks manual reserve transfer through pause-menu selection, audio, and saved state.</summary>
internal static class PauseReserveManualVerification
{
    /// <summary>Verifies mode changes and frame-by-frame refill when leaving and returning to the reserve item.</summary>
    internal static void VerifyPauseReserveManual()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 20, MaxHealth = 99, ReserveEnergy = 10,
            MaxReserveEnergy = 100, ReserveTankMode = ReserveTankMode.Auto,
            CollectedBeams = SamusBeamFlags.Charge };
        var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = new SharedGameState(new SharedRandom()), Area = AreaId.Crateria, RoomMapPosition = new(0, 0) });
        EnterPauseEquipment(pause);
        AssertEqual<PauseEquipmentControl>(PauseEquipmentControls.ReserveMode, pause.SelectedControl, "equipment entry selects the owned reserve mode control without Up");
        pause.Step(0, ControllerButton.A);
        AssertEqual(ReserveTankMode.Manual, samus.ReserveTankMode, "A changes AUTO to MANUAL");
        pause.Step(0, ControllerButton.Down);
        AssertEqual<PauseEquipmentControl>(PauseEquipmentControls.ReserveTransfer, pause.SelectedControl, "manual Down stays in tanks and selects transfer");
        pause.Step(0, ControllerButton.A);
        AssertEqual(21, samus.Health, "first manual transfer occurs on A call");
        AssertEqual(9, samus.ReserveEnergy, "first manual transfer consumes one reserve");
        pause.Step(0, ControllerButton.Down);
        AssertEqual(22, samus.Health, "transfer precedes leaving reserve item");
        AssertEqual(PauseEquipmentCategory.Beams, pause.SelectedCategory, "Down leaves reserve item");
        for (int i = 0; i < 5; i++) pause.Step(0, 0);
        AssertEqual(22, samus.Health, "transfer pauses while another category owns input");
        pause.Step(0, ControllerButton.Up);
        pause.Step(0, ControllerButton.Down);
        AssertEqual(22, samus.Health, "selecting transfer does not dispatch it early");
        for (int i = 0; i < 8; i++)
        {
            pause.Step(0, 0);
            AssertEqual(23 + i, samus.Health, "returning resumes saved transfer without another A");
            AssertEqual(7 - i, samus.ReserveEnergy, "reserve counts down each resumed frame");
            AssertEqual(PauseMenuLayoutConstants.ReserveSupplyDigitZeroTile + 7 - i,
                pause.ReadReserveSupplyDigit(RetailPresentationFixture(), 2).Raw, "visible supply digit updates on transfer frame");
        }
        AssertEqual<PauseEquipmentControl>(PauseEquipmentControls.ReserveMode, pause.SelectedControl, "exhaustion returns to the mode control");
        pause.Step(0, ControllerButton.A);
        AssertEqual(ReserveTankMode.Auto, samus.ReserveTankMode, "A restores AUTO");
        Suite(nameof(VerifyManualReserveSoundAndClamp), () => VerifyManualReserveSoundAndClamp());
        Console.WriteLine("Manual reserves: mode, selection, transfer ordering, suspended/resumed refill and visible supply pass.");
    }

    /// <summary>Checks refill sound cadence, health clamping, and debugger restoration of a pending delay.</summary>
    private static void VerifyManualReserveSoundAndClamp()
    {
        foreach (ushort startingHealth in new ushort[] { 20, 98 })
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = startingHealth, MaxHealth = 99, ReserveEnergy = 10,
                MaxReserveEnergy = 100, ReserveTankMode = ReserveTankMode.Manual, CollectedBeams = SamusBeamFlags.Charge };
            var audio = AudioQueues.Standalone();
            var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = new SharedGameState(new SharedRandom()), Area = AreaId.Crateria, RoomMapPosition = new(0, 0), Audio = audio });
            EnterPauseEquipment(pause);
            pause.Step(0, ControllerButton.Up);
            pause.Step(0, ControllerButton.Down);
            audio.Reset();
            // The standalone pause's audio is never advanced, so the room library's queue keeps every request since the reset.
            IReadOnlyList<SoundEffectId> roomRequests = audio.WaitingSounds(SoundEffectLibrary.Room);
            int count = startingHealth == 98 ? 1 : 10;
            for (int frame = 0; frame < count; frame++)
            {
                pause.Step(0, frame == 0 ? ControllerButton.A : 0);
                // The shared pause palette loop also uses library three. Count
                // refill identities, not unrelated requests in the same queue.
                int refills = roomRequests.Count(sound => sound == SoundEffectCommands.Decode(SoundEffectLibrary.Room, 0x2d));
                AssertEqual(frame < 8 ? 1 : 2, refills, "refill sound published on transfer calls zero and eight");
                AssertEqual(Math.Min(99, startingHealth + frame + 1), samus.Health, "manual energy progression");
                if (frame == 0 && startingHealth == 20)
                {
                    // The capture names the installed content by path, as a host's store does, instead of copying it (#1532).
                    var restored = SuperMetroid.Verification.Harness.GraphDigestVerification.RoundTrip(pause);
                    AssertEqual((ushort)15, restored.ReserveTransferSoundDelay.RawValue, "debugger state preserves pending transfer delay");
                }
            }
            AssertEqual(0, samus.ReserveEnergy, "retail consumes all reserves on exhaustion or max health");
            AssertEqual<PauseEquipmentControl>(PauseEquipmentControls.ReserveMode, pause.SelectedControl, "completed refill returns to mode");
        }
    }

    /// <summary>Runs the manual reserve transfer regression.</summary>
    [VerificationCommand("--pause-reserve-manual")]
    internal static int PauseReserveManualCommand()
    {
        Suite(nameof(VerifyPauseReserveManual), () => VerifyPauseReserveManual());
        return 0;
    }
}
