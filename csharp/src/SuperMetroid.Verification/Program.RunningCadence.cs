using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyRunningCadence(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        var bus = new RunningCadenceReadGuard(rom);
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        VerifyOrdinaryRunningPointer(rom);
        VerifyOrdinaryRunningStream(rom);
        VerifyBoostRunningPointers(rom);
        VerifyBoostRunningStream0(rom);
        VerifyBoostRunningStream1(rom);
        VerifyBoostRunningStream2(rom);
        VerifyBoostRunningStream3(rom);
        VerifyBoostRunningStream4(rom);
        VerifyBoostRunningResets(rom);
        var ordinary = typeof(SamusState).GetMethod("ReadDefaultRunningAnimationByte", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<ISnesAddressSpace, ushort, byte>>();
        var speed = new SamusHorizontalSpeedState();

        // Every authored frame and loop command remains byte-exact. Reads may cross into
        // another member of this bounded catalog or wrap to the genuine mutable alias,
        // but unrelated high-bank code is not accepted as animation cadence.
        for (ushort index = 0; index <= 10; index++)
            AssertEqual(rom.ReadByte(0x91b5d3 + index), ordinary(bus, index), "Ordinary authored cadence");
        AssertEqual(rom.ReadByte(0x91b5de), ordinary(bus, 11), "Ordinary restored index can reach the next compiled definition");
        AssertEqual(bus.MutableDelay, ordinary(bus, 0x4d30), "Ordinary restored index retains low-bank wrapping");
        AssertThrows<InvalidDataException>(
            () => ordinary(bus, 0x0100),
            "Ordinary restored index cannot parse unrelated high-bank ROM");

        for (byte stage = 0; stage <= SamusRunningCadenceDefinitions.MaximumSoundQueueSelection; stage++)
        {
            speed.SpeedBoostCounter = (ushort)(stage << 8);
            ushort pointer = Word(0x91b5de + stage * 2);
            for (ushort index = 0; index <= 10; index++)
            {
                int address = 0x910000 | unchecked((ushort)(pointer + index));
                byte expected = (address & 0xffff) < 0x8000
                    ? bus.ReadUncompiledByte(address)
                    : rom.ReadByte(address);
                AssertEqual(expected, speed.ReadSpeedBoosterAnimationByte(bus, index), "Native Max6 cadence byte selection");
            }
        }
        speed.SpeedBoostCounter = 0x0600;
        AssertThrows<InvalidDataException>(
            () => speed.ReadSpeedBoosterAnimationByte(bus, 0),
            "Restored non-native stage cannot reinterpret adjacent code as a pointer");

        if (definitionsOnly) return;

        // Exercise every stored counter word through the real command interceptor.
        // For the sound-call case, A=$0500 deliberately selects the adjacent-data bug.
        foreach (bool momentum in new[] { false, true })
        foreach (bool running in new[] { false, true })
        foreach (bool dash in new[] { false, true })
        for (int stage = 0; stage <= 4; stage++)
        for (int low = 0; low <= byte.MaxValue; low++)
        {
            int raw = stage << 8 | low;
            bool admitted = momentum && running && dash;
            ushort decremented = unchecked((ushort)(raw - 1));
            bool intercept = admitted && (byte)decremented == 0;
            ushort next = admitted ? decremented : (ushort)raw;
            bool sound = intercept && (next & 0x0400) == 0 && ((next + 0x0100) & 0x0400) != 0;
            if (intercept && (next & 0x0400) == 0) next = unchecked((ushort)(next + 0x0100));
            int selection = sound ? 5 : next >> 8;
            ushort expectedTimer = intercept
                ? unchecked((ushort)(raw + (selection == 5 ? bus.MutableDelay : rom.ReadByte(0x910000 | Word(0x91b5de + selection * 2)))))
                : (ushort)0;
            if (intercept) next = (ushort)((next & 0xff00) | Word(0x91b61f + selection * 2));
            int calls = 0;
            speed.HasRunningMomentum = momentum; speed.SpeedBoostCounter = (ushort)raw;
            speed.EchoSoundRequested = false; speed.ContactDamageIndex = 0;
            ushort frame = 7;
            bool actual = speed.TryAdvanceSpeedBoosterAnimationStage(bus,
                running ? SamusMovementType.Running : SamusMovementType.Standing,
                dash ? (ushort)SnesButton.B : (ushort)0, (ushort)raw, ref frame, out ushort timer,
                () => { calls++; return 0x0500; });
            AssertEqual(intercept, actual, "Native boost interception gates and low-byte countdown");
            AssertEqual(next, speed.SpeedBoostCounter, "Native counter decrement, stage bit and reset-word OR");
            AssertEqual(intercept ? 0 : 7, frame, "Only intercepted command resets frame");
            AssertEqual(expectedTimer, timer, "Selected first-frame delay and wrapping frame buffer");
            AssertEqual(sound ? 1 : 0, calls, "Echo call only on bit-two stage entry");
            AssertTrue(!speed.EchoSoundRequested, "Synchronous queue path does not also defer sound");
            AssertEqual(0, speed.ContactDamageIndex, "Animation cannot publish movement-owned boost contact damage");
        }

        // The queue's returned accumulator, not the already-published stage word, owns both
        // reads. Exhaust the six high-byte values that Max6 can really return and every
        // incidental low byte; selection five must retain the live WRAM delay alias.
        for (int stage = 0; stage <= SamusRunningCadenceDefinitions.MaximumSoundQueueSelection; stage++)
        for (int low = 0; low <= byte.MaxValue; low++)
        {
            int accumulator = stage << 8 | low;
            bus.MutableDelay = (byte)accumulator;
            speed.HasRunningMomentum = true; speed.SpeedBoostCounter = 0x0301;
            ushort frame = 9;
            speed.TryAdvanceSpeedBoosterAnimationStage(bus, SamusMovementType.Running, (ushort)SnesButton.B,
                (ushort)accumulator, ref frame, out ushort timer, () => (ushort)accumulator);
            AssertEqual(0x0400 | Word(0x91b61f + stage * 2), speed.SpeedBoostCounter, "All returned queue words retain native reset selection");
            byte expectedDelay = stage == 5
                ? bus.MutableDelay
                : rom.ReadByte(0x910000 | Word(0x91b5de + stage * 2));
            AssertEqual(unchecked((ushort)(accumulator + expectedDelay)), timer,
                "Queue-selected delay includes live mutable aliases");
        }

        speed.HasRunningMomentum = true;
        speed.SpeedBoostCounter = 0x0301;
        ushort invalidFrame = 9;
        AssertThrows<InvalidDataException>(
            () => speed.TryAdvanceSpeedBoosterAnimationStage(
                bus,
                SamusMovementType.Running,
                (ushort)SnesButton.B,
                0,
                ref invalidFrame,
                out _,
                () => 0x0600),
            "Impossible Max6 accumulator fails before adjacent code becomes cadence metadata");

        speed = new SamusHorizontalSpeedState();
        speed.HandleExtraRunSpeed(SamusMovementType.Running, (ushort)SnesButton.B, true);
        AssertEqual(1, speed.SpeedBoostCounter, "First running frame seeds native countdown");
        AssertEqual(0x1000, speed.ExtraRunSubspeed, "Cadence migration leaves run acceleration intact");
        speed.SpeedBoostCounter = 0; speed.SpecialPaletteTimer = 8; speed.SpecialPaletteFrame = 6;
        speed.ReconcilePauseSpeedBoosterState(true);
        AssertEqual(1, speed.SpeedBoostCounter, "Pause equipment reconciliation seeds native countdown");
        AssertEqual(0, speed.SpecialPaletteTimer, "Pause reconciliation palette timer differs from initial run");
        AssertEqual(0, speed.SpecialPaletteFrame, "Pause reconciliation palette frame reset");
        Console.WriteLine("Running cadence: 90 bounded bytes, all native Max6 selections, 10240 command/gate cases, 1536 sound-return words, mutable stage-five alias, and loud non-native rejection pass.");
    }

    private static void VerifyOrdinaryRunningPointer(SuperMetroidAddressSpace rom)
    {
        VerifyCadenceBytes(rom, 0x91b5d1, 2);
        AssertEqual((ushort)(rom.ReadByte(0x91b5d1) | rom.ReadByte(0x91b5d2) << 8),
            SamusRunningCadenceDefinitions.DefaultRunningDelayListPointer, "Original ordinary pointer");
    }

    private static void VerifyOrdinaryRunningStream(SuperMetroidAddressSpace rom) => VerifyCadenceBytes(rom, 0x91b5d3, 11);
    private static void VerifyBoostRunningStream0(SuperMetroidAddressSpace rom) => VerifyCadenceBytes(rom, 0x91b5e8, 11);
    private static void VerifyBoostRunningStream1(SuperMetroidAddressSpace rom) => VerifyCadenceBytes(rom, 0x91b5f3, 11);
    private static void VerifyBoostRunningStream2(SuperMetroidAddressSpace rom) => VerifyCadenceBytes(rom, 0x91b5fe, 11);
    private static void VerifyBoostRunningStream3(SuperMetroidAddressSpace rom) => VerifyCadenceBytes(rom, 0x91b609, 11);
    private static void VerifyBoostRunningStream4(SuperMetroidAddressSpace rom) => VerifyCadenceBytes(rom, 0x91b614, 11);

    private static void VerifyBoostRunningPointers(SuperMetroidAddressSpace rom)
    {
        VerifyCadenceBytes(rom, 0x91b5de, 10);
        for (byte selection = 0; selection <= 5; selection++)
        {
            int address = 0x91b5de + 2 * selection;
            AssertEqual((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8),
                SamusRunningCadenceDefinitions.ReadSpeedBoostDelayListPointer(selection), "Original pointer including stage5 alias");
        }
        for (int selection = 6; selection <= byte.MaxValue; selection++)
            AssertThrows<InvalidDataException>(() => SamusRunningCadenceDefinitions.ReadSpeedBoostDelayListPointer((byte)selection), "Pointer unsupported stage");
    }

    private static void VerifyBoostRunningResets(SuperMetroidAddressSpace rom)
    {
        VerifyCadenceBytes(rom, 0x91b61f, 12);
        for (byte selection = 0; selection <= 5; selection++)
        {
            int address = 0x91b61f + 2 * selection;
            AssertEqual((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8),
                SamusRunningCadenceDefinitions.ReadResetWord(selection), "Original reset including pose0 alias");
        }
        for (int selection = 6; selection <= byte.MaxValue; selection++)
            AssertThrows<InvalidDataException>(() => SamusRunningCadenceDefinitions.ReadResetWord((byte)selection), "Reset unsupported stage");
    }

    private static void VerifyCadenceBytes(SuperMetroidAddressSpace rom, int start, int count)
    {
        for (int address = start; address < start + count; address++)
            AssertEqual(rom.ReadByte(address), SamusRunningCadenceDefinitions.ReadCompiledByte(address), "Original bounded cadence byte");
    }
    private sealed class RunningCadenceReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        public byte MutableDelay = 0x35;
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (address is >= 0x91b5d1 and < 0x91b62b)
                throw new InvalidOperationException($"Compiled running cadence read ROM ${address:X6}.");
            return ReadUncompiledByte(address);
        }
        // Restored indexes can reach unimplemented hardware. A constructed address
        // signature checks exact fallback routing, not native I/O-register semantics.
        public byte ReadUncompiledByte(int address) => address == 0x910303 ? MutableDelay :
            address is >= 0x912000 and < 0x918000 ? unchecked((byte)(address ^ (address >> 8))) : source.ReadByte(address);
        public byte ReadWorkRamByte(int address) => address == 0x910303 ? MutableDelay :
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Running-cadence guard requires WRAM.")).ReadWorkRamByte(address);
        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Running-cadence guard requires SRAM.")).ReadSaveRamByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
