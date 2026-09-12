using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyRunningCadence(SuperMetroidAddressSpace rom)
    {
        var bus = new RunningCadenceReadGuard(rom);
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        byte ExpectedByte(int address) => bus.ReadUncompiledByte(address);
        for (int address = 0x91b5d1; address < 0x91b629; address++)
            AssertEqual(rom.ReadByte(address), SamusRunningCadenceDefinitions.ReadByte(bus, address), "All 88 native cadence-definition bytes");

        var ordinary = typeof(SamusState).GetMethod("ReadDefaultRunningAnimationByte", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<ISnesAddressSpace, ushort, byte>>();
        var speed = new SamusHorizontalSpeedState();
        // Arbitrary restored indexes must preserve within-bank wrapping, including
        // adjacent definitions and low-bank mutable memory, not clamp to ten frames.
        for (int index = 0; index <= ushort.MaxValue; index++)
        {
            int ordinaryAddress = 0x910000 | (ushort)(Word(0x91b5d1) + index);
            AssertEqual(ExpectedByte(ordinaryAddress), ordinary(bus, (ushort)index), "Ordinary cadence byte-index wrapping");
            for (int stage = 0; stage <= byte.MaxValue; stage++)
            {
                speed.SpeedBoostCounter = (ushort)(stage << 8);
                int address = 0x910000 | (ushort)(Word(0x91b5de + stage * 2) + index);
                AssertEqual(ExpectedByte(address), speed.ReadSpeedBoosterAnimationByte(bus, (ushort)index), "Boost stage and byte-index native address selection");
            }
        }

        // Exercise every stored counter word through the real command interceptor.
        // For the sound-call case, A=$0500 deliberately selects the adjacent-data bug.
        foreach (bool momentum in new[] { false, true })
        foreach (bool running in new[] { false, true })
        foreach (bool dash in new[] { false, true })
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            bool admitted = momentum && running && dash;
            ushort decremented = unchecked((ushort)(raw - 1));
            bool intercept = admitted && (byte)decremented == 0;
            ushort next = admitted ? decremented : (ushort)raw;
            bool sound = intercept && (next & 0x0400) == 0 && ((next + 0x0100) & 0x0400) != 0;
            if (intercept && (next & 0x0400) == 0) next = unchecked((ushort)(next + 0x0100));
            int selection = sound ? 5 : next >> 8;
            ushort expectedTimer = intercept ? unchecked((ushort)(raw + ExpectedByte(0x910000 | Word(0x91b5de + selection * 2)))) : (ushort)0;
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
            AssertEqual(intercept && (next & 0xff00) == 0x0400 ? 1 : 0, speed.ContactDamageIndex, "Post-reset active-stage contact publication");
        }

        // The queue's returned accumulator, not a clamped boost stage, owns both reads.
        // Vary its entire word and a live WRAM alias used by queue occupancy five.
        for (int accumulator = 0; accumulator <= ushort.MaxValue; accumulator++)
        {
            bus.MutableDelay = (byte)accumulator;
            int stage = accumulator >> 8;
            speed.HasRunningMomentum = true; speed.SpeedBoostCounter = 0x0301;
            ushort frame = 9;
            speed.TryAdvanceSpeedBoosterAnimationStage(bus, SamusMovementType.Running, (ushort)SnesButton.B,
                (ushort)accumulator, ref frame, out ushort timer, () => (ushort)accumulator);
            AssertEqual(0x0400 | Word(0x91b61f + stage * 2), speed.SpeedBoostCounter, "All returned queue words retain native reset selection");
            AssertEqual(unchecked((ushort)(accumulator + ExpectedByte(0x910000 | Word(0x91b5de + stage * 2)))), timer,
                "Queue-selected delay includes live mutable aliases");
        }

        speed = new SamusHorizontalSpeedState();
        speed.HandleExtraRunSpeed(SamusMovementType.Running, (ushort)SnesButton.B, true, bus);
        AssertEqual(1, speed.SpeedBoostCounter, "First running frame seeds native countdown");
        AssertEqual(0x1000, speed.ExtraRunSubspeed, "Cadence migration leaves run acceleration intact");
        speed.SpeedBoostCounter = 0; speed.SpecialPaletteTimer = 8; speed.SpecialPaletteFrame = 6;
        speed.ReconcilePauseSpeedBoosterState(bus, true);
        AssertEqual(1, speed.SpeedBoostCounter, "Pause equipment reconciliation seeds native countdown");
        AssertEqual(0, speed.SpecialPaletteTimer, "Pause reconciliation palette timer differs from initial run");
        AssertEqual(0, speed.SpecialPaletteFrame, "Pause reconciliation palette frame reset");
        Console.WriteLine("Running cadence: 88 native bytes, all stage/index pairs, 524288 command/gate cases, and 65536 sound-return words pass with cadence ROM reads forbidden.");
    }

    private sealed class RunningCadenceReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte MutableDelay = 0x35;
        public byte ReadByte(int address)
        {
            if (address is >= 0x91b5d1 and < 0x91b629)
                throw new InvalidOperationException($"Compiled running cadence read ROM ${address:X6}.");
            return ReadUncompiledByte(address);
        }
        // Restored indexes can reach unimplemented hardware. A constructed address
        // signature checks exact fallback routing, not native I/O-register semantics.
        public byte ReadUncompiledByte(int address) => address == 0x910303 ? MutableDelay :
            address is >= 0x912000 and < 0x918000 ? unchecked((byte)(address ^ (address >> 8))) : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
