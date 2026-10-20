using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;

/// <summary>Checks manual reserve transfer through pause-menu selection, audio, and saved state.</summary>
internal static class PauseReserveManualVerification
{
    /// <summary>Verifies mode changes and frame-by-frame refill when leaving and returning to the reserve item.</summary>
    internal static void VerifyPauseReserveManual()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Health = 20, MaxHealth = 99, ReserveEnergy = 10,
            MaxReserveEnergy = 100, ReserveTankMode = ReserveTankMode.Auto,
            CollectedBeams = SamusBeamFlags.Charge };
        var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0) });
        EnterPauseEquipment(pause);
        AssertEqual(PauseEquipmentCategory.Reserves, pause.SelectedCategory, "equipment entry selects owned reserve controls without Up");
        AssertEqual(PauseReserveTransferRomData.ModeItem, pause.SelectedItem, "equipment entry selects reserve mode item");
        pause.Step(0, SnesButton.A);
        AssertEqual(ReserveTankMode.Manual, samus.ReserveTankMode, "A changes AUTO to MANUAL");
        pause.Step(0, SnesButton.Down);
        AssertEqual(PauseEquipmentCategory.Reserves, pause.SelectedCategory, "manual Down stays in tanks");
        AssertEqual(1, pause.SelectedItem, "manual Down selects transfer");
        pause.Step(0, SnesButton.A);
        AssertEqual(21, samus.Health, "first manual transfer occurs on A call");
        AssertEqual(9, samus.ReserveEnergy, "first manual transfer consumes one reserve");
        pause.Step(0, SnesButton.Down);
        AssertEqual(22, samus.Health, "transfer precedes leaving reserve item");
        AssertEqual(PauseEquipmentCategory.Beams, pause.SelectedCategory, "Down leaves reserve item");
        for (int i = 0; i < 5; i++) pause.Step(0, 0);
        AssertEqual(22, samus.Health, "transfer pauses while another category owns input");
        pause.Step(0, SnesButton.Up);
        pause.Step(0, SnesButton.Down);
        AssertEqual(22, samus.Health, "selecting transfer does not dispatch it early");
        for (int i = 0; i < 8; i++)
        {
            pause.Step(0, 0);
            AssertEqual(23 + i, samus.Health, "returning resumes saved transfer without another A");
            AssertEqual(7 - i, samus.ReserveEnergy, "reserve counts down each resumed frame");
            AssertEqual(PauseMenuLayout.ReserveSupplyDigitZeroTile + 7 - i,
                pause.ReadReserveSupplyDigit(2).Raw, "visible supply digit updates on transfer frame");
        }
        AssertEqual(0, pause.SelectedItem, "exhaustion returns to mode item");
        pause.Step(0, SnesButton.A);
        AssertEqual(ReserveTankMode.Auto, samus.ReserveTankMode, "A restores AUTO");
        Suite(nameof(VerifyManualReserveSoundAndClamp), () => VerifyManualReserveSoundAndClamp());
        Console.WriteLine("Manual reserves: mode, selection, transfer ordering, suspended/resumed refill and visible supply pass.");
    }

    /// <summary>Checks refill sound cadence, health clamping, and debugger restoration of a pending delay.</summary>
    private static void VerifyManualReserveSoundAndClamp()
    {
        foreach (ushort startingHealth in new ushort[] { 20, 98 })
        {
            var samus = new SamusState { Health = startingHealth, MaxHealth = 99, ReserveEnergy = 10,
                MaxReserveEnergy = 100, ReserveTankMode = ReserveTankMode.Manual, CollectedBeams = SamusBeamFlags.Charge };
            var audio = new CartridgeAudioState();
            var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), Audio = audio });
            EnterPauseEquipment(pause);
            pause.Step(0, SnesButton.Up);
            pause.Step(0, SnesButton.Down);
            audio.Reset();
            var positions = audio.SoundWritePositions;
            var queues = audio.SoundQueues;
            int count = startingHealth == 98 ? 1 : 10;
            for (int frame = 0; frame < count; frame++)
            {
                pause.Step(0, frame == 0 ? SnesButton.A : 0);
                // The shared pause palette loop also uses library three. Count
                // refill identities, not unrelated requests in the same ring.
                int refills = Enumerable.Range(0, positions[2]).Count(i => queues[2, i] == 0x2d);
                AssertEqual(frame < 8 ? 1 : 2, refills, "refill sound published on transfer calls zero and eight");
                AssertEqual(Math.Min(99, startingHealth + frame + 1), samus.Health, "manual energy progression");
                if (frame == 0 && startingHealth == 20)
                {
                    using var state = new MemoryStream();
                    DebuggerObjectGraphSerializer.Serialize(state, pause);
                    state.Position = 0;
                    var restored = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(state);
                    AssertEqual((ushort)15, restored.ReserveTransferSoundDelay.RawValue, "debugger state preserves pending transfer delay");
                }
            }
            AssertEqual(0, samus.ReserveEnergy, "retail consumes all reserves on exhaustion or max health");
            AssertEqual(0, pause.SelectedItem, "completed refill returns to mode");
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
