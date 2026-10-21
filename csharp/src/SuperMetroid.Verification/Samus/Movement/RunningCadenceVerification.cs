using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks compiled Samus running-animation cadence and speed-booster stage selection against native bytes.</summary>
internal static class RunningCadenceVerification
{
    /// <summary>Checks authored delay streams, queue-selected aliases, and the production animation-stage interceptor.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for running cadence")]
    internal static void VerifyRunningCadence(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        var bus = new RunningCadenceReadGuard(rom);
        // Stage five's delay list aliases the Mode 7 queue residue; it mirrors the guard's mutable delay byte.
        var residue = new SuperMetroid.Core.Hardware.Mode7TransferQueueResidue();
        int stageFiveOffset = SamusRunningCadenceDefinitions.ReadSpeedBoostDelayListPointer(
            SamusRunningCadenceDefinitions.MaximumSoundQueueSelection) - SnesAddressMath.OffsetInBank(SuperMetroid.Core.Hardware.Mode7TransferQueueLayout.QueueStart);
        residue.Write(stageFiveOffset, bus.MutableDelay);
        Suite(nameof(VerifyOrdinaryRunningPointer), () => VerifyOrdinaryRunningPointer(rom));
        Suite(nameof(VerifyOrdinaryRunningStream), () => VerifyOrdinaryRunningStream(rom));
        Suite(nameof(VerifyBoostRunningPointers), () => VerifyBoostRunningPointers(rom));
        Suite(nameof(VerifyBoostRunningStream0), () => VerifyBoostRunningStream0(rom));
        Suite(nameof(VerifyBoostRunningStream1), () => VerifyBoostRunningStream1(rom));
        Suite(nameof(VerifyBoostRunningStream2), () => VerifyBoostRunningStream2(rom));
        Suite(nameof(VerifyBoostRunningStream3), () => VerifyBoostRunningStream3(rom));
        Suite(nameof(VerifyBoostRunningStream4), () => VerifyBoostRunningStream4(rom));
        Suite(nameof(VerifyBoostRunningResets), () => VerifyBoostRunningResets(rom));
        var ordinary = new Func<SuperMetroid.Core.Hardware.Mode7TransferQueueResidue?, int, byte>(SamusPresentation.ReadDefaultRunningAnimationByte);
        var speed = new SamusHorizontalSpeedState();

        // Every authored frame and loop command remains byte-exact. Reads may cross into
        // another member of this bounded catalog or wrap to the genuine mutable alias,
        // but unrelated high-bank code is not accepted as animation cadence.
        for (ushort index = 0; index <= 10; index++)
            AssertEqual(rom.ReadCartridgeByte(0x91b5d3 + index), ordinary(residue, index), "Ordinary authored cadence");
        AssertEqual(rom.ReadCartridgeByte(0x91b5de), ordinary(residue, 11), "Ordinary restored index can reach the next compiled definition");
        AssertEqual(bus.MutableDelay, ordinary(residue, 0x4d30), "Ordinary restored index retains low-bank wrapping");
        AssertThrows<InvalidDataException>(
            () => ordinary(residue, 0x0100),
            "Ordinary restored index cannot parse unrelated high-bank ROM");

        for (byte stage = 0; stage <= SamusRunningCadenceDefinitions.MaximumSoundQueueSelection; stage++)
        {
            speed.SpeedBoostCounter = new SamusSpeedBoostCounter(SnesLittleEndian.Word(0, unchecked(stage)));
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x91b5de + stage * 2));
            for (ushort index = 0; index <= 10; index++)
            {
                int address = 0x910000 | unchecked((ushort)(pointer + index));
                byte expected = (SnesAddressMath.OffsetInBank(address)) < 0x8000
                    ? bus.ReadUncompiledByte(address)
                    : rom.ReadCartridgeByte(address);
                AssertEqual(expected, speed.ReadSpeedBoosterAnimationByte(residue, index), "Native Max6 cadence byte selection");
            }
        }
        speed.SpeedBoostCounter = new SamusSpeedBoostCounter(0x0600);
        AssertThrows<InvalidDataException>(
            () => speed.ReadSpeedBoosterAnimationByte(residue, 0),
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
            int raw = SnesLittleEndian.Word(unchecked((byte)low), unchecked((byte)stage));
            bool admitted = momentum && running && dash;
            ushort decremented = unchecked((ushort)(raw - 1));
            bool intercept = admitted && (byte)decremented == 0;
            ushort next = admitted ? decremented : (ushort)raw;
            bool sound = intercept && (next & 0x0400) == 0 && ((next + 0x0100) & 0x0400) != 0;
            if (intercept && (next & 0x0400) == 0) next = unchecked((ushort)(next + 0x0100));
            int selection = sound ? 5 : SnesLittleEndian.HighByte(next);
            ushort expectedTimer = intercept
                ? unchecked((ushort)(raw + (selection == 5 ? bus.MutableDelay : rom.ReadCartridgeByte(0x910000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0x91b5de + selection * 2))))))
                : (ushort)0;
            if (intercept) next = (ushort)((SnesLittleEndian.HighByteInPlace(next)) | CartridgeImportSource.Require(rom).ReadCartridgeWord((0x91b61f + selection * 2)));
            int calls = 0;
            speed.HasRunningMomentum = momentum; speed.SpeedBoostCounter = new SamusSpeedBoostCounter((ushort)raw);
            speed.EchoSoundRequested = false; speed.ContactDamageIndex = SamusContactDamageIndex.None;
            ushort frame = 7;
            bool actual = speed.TryAdvanceSpeedBoosterAnimationStage(residue,
                running ? SamusMovementType.Running : SamusMovementType.Standing,
                dash ? SnesButton.B : 0, (ushort)raw, ref frame, out ushort timer,
                () => { calls++; return 0x0500; });
            AssertEqual(intercept, actual, "Native boost interception gates and low-byte countdown");
            AssertEqual(next, speed.SpeedBoostCounter.RawValue, "Native counter decrement, stage bit and reset-word OR");
            AssertEqual(intercept ? 0 : 7, frame, "Only intercepted command resets frame");
            AssertEqual(expectedTimer, timer, "Selected first-frame delay and wrapping frame buffer");
            AssertEqual(sound ? 1 : 0, calls, "Echo call only on bit-two stage entry");
            AssertTrue(!speed.EchoSoundRequested, "Synchronous queue path does not also defer sound");
            AssertEqual(SamusContactDamageIndex.None, speed.ContactDamageIndex, "Animation cannot publish movement-owned boost contact damage");
        }

        // The queue's returned accumulator, not the already-published stage word, owns both
        // reads. Exhaust the six high-byte values that Max6 can really return and every
        // incidental low byte; selection five must retain the live WRAM delay alias.
        for (int stage = 0; stage <= SamusRunningCadenceDefinitions.MaximumSoundQueueSelection; stage++)
        for (int low = 0; low <= byte.MaxValue; low++)
        {
            int accumulator = SnesLittleEndian.Word(unchecked((byte)low), unchecked((byte)stage));
            bus.MutableDelay = (byte)accumulator;
            residue.Write(stageFiveOffset, bus.MutableDelay);
            speed.HasRunningMomentum = true; speed.SpeedBoostCounter = new SamusSpeedBoostCounter(0x0301);
            ushort frame = 9;
            speed.TryAdvanceSpeedBoosterAnimationStage(residue, SamusMovementType.Running, SnesButton.B,
                (ushort)accumulator, ref frame, out ushort timer, () => (ushort)accumulator);
            AssertEqual(0x0400 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0x91b61f + stage * 2)), speed.SpeedBoostCounter.RawValue, "All returned queue words retain native reset selection");
            byte expectedDelay = stage == 5
                ? bus.MutableDelay
                : rom.ReadCartridgeByte(0x910000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0x91b5de + stage * 2)));
            AssertEqual(unchecked((ushort)(accumulator + expectedDelay)), timer,
                "Queue-selected delay includes live mutable aliases");
        }

        speed.HasRunningMomentum = true;
        speed.SpeedBoostCounter = new SamusSpeedBoostCounter(0x0301);
        ushort invalidFrame = 9;
        AssertThrows<InvalidDataException>(
            () => speed.TryAdvanceSpeedBoosterAnimationStage(
                residue,
                SamusMovementType.Running,
                SnesButton.B,
                0,
                ref invalidFrame,
                out _,
                () => 0x0600),
            "Impossible Max6 accumulator fails before adjacent code becomes cadence metadata");

        speed = new SamusHorizontalSpeedState();
        speed.HandleExtraRunSpeed(SamusMovementType.Running, SnesButton.B, true);
        AssertEqual(1, speed.SpeedBoostCounter.RawValue, "First running frame seeds native countdown");
        AssertEqual(0x1000, speed.ExtraRunSpeed.Fraction, "Cadence migration leaves run acceleration intact");
        speed.SpeedBoostCounter = new SamusSpeedBoostCounter(0); speed.SpecialPaletteTimer = NativeCountdown.Start(8); speed.SpecialPaletteFrame = 6;
        speed.ReconcilePauseSpeedBoosterState(true);
        AssertEqual(1, speed.SpeedBoostCounter.RawValue, "Pause equipment reconciliation seeds native countdown");
        AssertEqual(0, speed.SpecialPaletteTimer.RawValue, "Pause reconciliation palette timer differs from initial run");
        AssertEqual(0, speed.SpecialPaletteFrame, "Pause reconciliation palette frame reset");
        Console.WriteLine("Running cadence: 90 bounded bytes, all native Max6 selections, 10240 command/gate cases, 1536 sound-return words, mutable stage-five alias, and loud non-native rejection pass.");
    }

    /// <summary>Compares the ordinary running-list pointer with its native bank-$91 word.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ordinary running pointer")]
    private static void VerifyOrdinaryRunningPointer(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyCadenceBytes), () => VerifyCadenceBytes(rom, 0x91b5d1, 2));
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x91b5d1),
            SamusRunningCadenceDefinitions.DefaultRunningDelayListPointer, "Original ordinary pointer");
    }

    /// <summary>Checks the eleven ordinary running delay and loop bytes.</summary>
    private static void VerifyOrdinaryRunningStream(SuperMetroidAddressSpace rom) => VerifyCadenceBytes(rom, 0x91b5d3, 11);
    /// <summary>Checks the first speed-booster stage's eleven cadence bytes.</summary>
    private static void VerifyBoostRunningStream0(SuperMetroidAddressSpace rom) => VerifyCadenceBytes(rom, 0x91b5e8, 11);
    /// <summary>Checks the second speed-booster stage's eleven cadence bytes.</summary>
    private static void VerifyBoostRunningStream1(SuperMetroidAddressSpace rom) => VerifyCadenceBytes(rom, 0x91b5f3, 11);
    /// <summary>Checks the third speed-booster stage's eleven cadence bytes.</summary>
    private static void VerifyBoostRunningStream2(SuperMetroidAddressSpace rom) => VerifyCadenceBytes(rom, 0x91b5fe, 11);
    /// <summary>Checks the fourth speed-booster stage's eleven cadence bytes.</summary>
    private static void VerifyBoostRunningStream3(SuperMetroidAddressSpace rom) => VerifyCadenceBytes(rom, 0x91b609, 11);
    /// <summary>Checks the fifth speed-booster stage's eleven cadence bytes.</summary>
    private static void VerifyBoostRunningStream4(SuperMetroidAddressSpace rom) => VerifyCadenceBytes(rom, 0x91b614, 11);

    /// <summary>Checks all five stage pointers and the sixth sound-queue alias against native words.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for boost running pointers")]
    private static void VerifyBoostRunningPointers(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyCadenceBytes), () => VerifyCadenceBytes(rom, 0x91b5de, 10));
        for (byte selection = 0; selection <= 5; selection++)
        {
            int address = 0x91b5de + 2 * selection;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                SamusRunningCadenceDefinitions.ReadSpeedBoostDelayListPointer(selection), "Original pointer including stage5 alias");
        }
        for (int selection = 6; selection <= byte.MaxValue; selection++)
            AssertThrows<InvalidDataException>(() => SamusRunningCadenceDefinitions.ReadSpeedBoostDelayListPointer((byte)selection), "Pointer unsupported stage");
    }

    /// <summary>Checks stage reset words, including the adjacent pose-zero alias, and rejects unsupported selections.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for boost running resets")]
    private static void VerifyBoostRunningResets(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyCadenceBytes), () => VerifyCadenceBytes(rom, 0x91b61f, 12));
        for (byte selection = 0; selection <= 5; selection++)
        {
            int address = 0x91b61f + 2 * selection;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                SamusRunningCadenceDefinitions.ReadResetWord(selection), "Original reset including pose0 alias");
        }
        for (int selection = 6; selection <= byte.MaxValue; selection++)
            AssertThrows<InvalidDataException>(() => SamusRunningCadenceDefinitions.ReadResetWord((byte)selection), "Reset unsupported stage");
    }

    /// <summary>Compares one bounded compiled cadence byte range with retail cartridge bytes.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for running cadence bytes")]
    private static void VerifyCadenceBytes(SuperMetroidAddressSpace rom, int start, int count)
    {
        for (int address = start; address < start + count; address++)
            AssertEqual(rom.ReadCartridgeByte(address), SamusRunningCadenceDefinitions.ReadCompiledByte(address), "Original bounded cadence byte");
    }
    /// <summary>Forbids compiled cadence ROM reads while exposing the live WRAM delay alias and fallback address signature.</summary>
    [CpuBusBoundary("exact bus test: live WRAM delay alias at $91:0303 plus a constructed unimplemented-hardware byte signature that witnesses running-cadence operand routing")]
    internal sealed class RunningCadenceReadGuard(ISnesAddressSpace source) :
        ISnesWritableAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Mutable delay byte observed when the native stage-five pointer wraps into low-bank memory.</summary>
        public byte MutableDelay = 0x35;
        /// <summary>Routes cartridge reads through the compiled-range guard.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects native cadence bytes and delegates other addresses to the source or constructed alias.</summary>
        public byte ReadByte(int address)
        {
            if (address is >= 0x91b5d1 and < 0x91b62b)
                throw new InvalidOperationException($"Compiled running cadence read ROM ${address:X6}.");
            return ReadUncompiledByte(address);
        }
        /// <summary>Uses a constructed signature for unimplemented hardware addresses reached by restored indexes, and preserves the mutable delay alias.</summary>
        public byte ReadUncompiledByte(int address) => address == 0x910303 ? MutableDelay :
            address is >= 0x912000 and < 0x918000 ? unchecked((byte)(address ^ SnesLittleEndian.HighByte(unchecked((ushort)address)))) : ReadSourceByte(address);
        /// <summary>Exposes the mutable delay alias and otherwise forwards WRAM reads.</summary>
        public byte ReadWorkRamByte(int address) => address == 0x7e0303 ? MutableDelay : SourceMemory.ReadWorkRamByte(address);
        /// <summary>Forwards writes without changing the constructed read behavior.</summary>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        /// <summary>Requires the source to expose WRAM and SRAM for low-bank fallback reads.</summary>
        private ISnesMutableMemory SourceMemory => source as ISnesMutableMemory ??
            throw new InvalidOperationException("Running-cadence guard requires WRAM and SRAM.");

        /// <summary>The source byte the CPU maps at <paramref name="address"/>: its cartridge ROM, WRAM or SRAM.</summary>
        private byte ReadSourceByte(int address) => SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.WorkRam => SourceMemory.ReadWorkRamByte(address),
            SnesDmaSourceKind.Cartridge => source.ReadCartridgeByte(address),
            SnesDmaSourceKind.Unmapped => throw new InvalidOperationException(
                $"CPU read ${SnesAddressMath.BankNumber(address):X2}:{SnesAddressMath.OffsetInBank(address):X4} is outside the runtime address map."),
            _ => throw new InvalidOperationException(
                $"CPU read ${SnesAddressMath.BankNumber(address):X2}:{SnesAddressMath.OffsetInBank(address):X4} is outside the runtime address map."),
        };
    }
}
