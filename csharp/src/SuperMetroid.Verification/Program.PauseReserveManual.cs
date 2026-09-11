using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Audio;
using SuperMetroid.Desktop;
using System.Reflection;

internal static partial class Program
{
    private static void VerifyPauseReserveManual()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Health = 20, MaxHealth = 99, ReserveEnergy = 10,
            MaxReserveEnergy = 100, ReserveTankMode = 1,
            CollectedBeams = (ushort)SamusBeamFlags.Charge };
        var pause = new PauseMenuState(bus, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0);
        pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
        for (int i = 0; i < 32; i++) pause.Step(0, 0);
        pause.Step(0, (ushort)SnesButton.Up);
        AssertEqual(PauseEquipmentCategories.Reserves, pause.SelectedCategory, "Up selects owned tank controls");
        pause.Step(0, (ushort)SnesButton.A);
        AssertEqual(2, samus.ReserveTankMode, "A changes AUTO to MANUAL");
        pause.Step(0, (ushort)SnesButton.Down);
        AssertEqual(PauseEquipmentCategories.Reserves, pause.SelectedCategory, "manual Down stays in tanks");
        AssertEqual(1, pause.SelectedItem, "manual Down selects transfer");
        pause.Step(0, (ushort)SnesButton.A);
        AssertEqual(21, samus.Health, "first manual transfer occurs on A call");
        AssertEqual(9, samus.ReserveEnergy, "first manual transfer consumes one reserve");
        pause.Step(0, (ushort)SnesButton.Down);
        AssertEqual(22, samus.Health, "transfer precedes leaving reserve item");
        AssertEqual(PauseEquipmentCategories.Beams, pause.SelectedCategory, "Down leaves reserve item");
        for (int i = 0; i < 5; i++) pause.Step(0, 0);
        AssertEqual(22, samus.Health, "transfer pauses while another category owns input");
        pause.Step(0, (ushort)SnesButton.Up);
        pause.Step(0, (ushort)SnesButton.Down);
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
        pause.Step(0, (ushort)SnesButton.A);
        AssertEqual(1, samus.ReserveTankMode, "A restores AUTO");
        VerifyManualReserveSoundAndClamp(bus);
        var fields = typeof(PauseMenuState).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .OrderBy(field => field.MetadataToken).ToArray();
        var oldFields = DebuggerStateFieldMigrations.SelectSerializedFields(typeof(PauseMenuState), fields, fields.Length - 1);
        AssertTrue(oldFields.SequenceEqual(fields.Where(field => field.Name != "reserveTransferSoundDelay")),
            "legacy pause layout omits only the new manual transfer timer");
        Console.WriteLine("Manual reserves: mode, selection, transfer ordering, suspended/resumed refill and visible supply pass.");
    }

    private static void VerifyManualReserveSoundAndClamp(SuperMetroidAddressSpace bus)
    {
        foreach (ushort startingHealth in new ushort[] { 20, 98 })
        {
            var samus = new SamusState { Health = startingHealth, MaxHealth = 99, ReserveEnergy = 10,
                MaxReserveEnergy = 100, ReserveTankMode = 2, CollectedBeams = (ushort)SamusBeamFlags.Charge };
            var audio = new CartridgeAudioState();
            var pause = new PauseMenuState(bus, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0, audio);
            pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
            for (int i = 0; i < 32; i++) pause.Step(0, 0);
            pause.Step(0, (ushort)SnesButton.Up);
            pause.Step(0, (ushort)SnesButton.Down);
            audio.Reset();
            var positions = (byte[])typeof(CartridgeAudioState).GetField("_soundWritePositions", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(audio)!;
            var queues = (byte[,])typeof(CartridgeAudioState).GetField("_soundQueues", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(audio)!;
            int count = startingHealth == 98 ? 1 : 10;
            for (int frame = 0; frame < count; frame++)
            {
                pause.Step(0, frame == 0 ? (ushort)SnesButton.A : (ushort)0);
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
                    var timer = typeof(PauseMenuState).GetField("reserveTransferSoundDelay", BindingFlags.Instance | BindingFlags.NonPublic)!;
                    AssertEqual((ushort)15, (ushort)timer.GetValue(restored)!, "debugger state preserves pending transfer delay");
                }
            }
            AssertEqual(0, samus.ReserveEnergy, "retail consumes all reserves on exhaustion or max health");
            AssertEqual(0, pause.SelectedItem, "completed refill returns to mode");
        }
    }
}
