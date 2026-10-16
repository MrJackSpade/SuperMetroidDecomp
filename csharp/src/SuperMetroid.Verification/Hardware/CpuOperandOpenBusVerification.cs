using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks the indirect-long operand read's region routing.</summary>
internal static class CpuOperandOpenBusVerification
{
    /// <summary>Checks indirect-long reads across WRAM, SRAM and forbidden cartridge space.</summary>
    [CpuBusBoundary("exact bus test: absolute-indexed and indirect-long operand reads, open-bus MDR retention and region routing")]
    internal static void VerifyCpuOperandOpenBus()
    {
        var bus = new OperandReadWitness();
        AssertEqual((ushort)0xabab, SnesIndirectLongDataRead.ReadWord(bus, 0x7e, 0x4000, 0),
            "indirect long operand reads WRAM through the typed mutable-memory contract");
        AssertEqual((ushort)0xabab, SnesIndirectLongDataRead.ReadWord(bus, 0x70, 0x4000, 0),
            "indirect long operand reads SRAM through the typed mutable-memory contract");
        AssertThrows<InvalidOperationException>(() =>
                SnesIndirectLongDataRead.ReadWord(bus, 0x90, 0x8000, 0),
            "indirect long operand cannot read cartridge bytes through the gameplay bus");
    }

    /// <summary>Marks every mapped read with a fixed byte so open-bus retention remains observable.</summary>
    [CpuBusBoundary("exact bus test: witness answering every WRAM, SRAM, cartridge and peripheral operand read with a marker byte")]
    internal sealed class OperandReadWitness : ISnesMemoryBus, IImportCartridgeSource, ISnesCpuPeripheralSource
    {
        /// <summary>Returns the marker byte for mapped work RAM reads.</summary>
        public byte ReadWorkRamByte(int address) => 0xab;
        /// <summary>Returns the marker byte for mapped save RAM reads.</summary>
        public byte ReadSaveRamByte(int address) => 0xab;
        /// <summary>Returns the marker byte for cartridge reads attempted by the operand helper.</summary>
        public byte ReadCartridgeByte(int address) => 0xab;
        /// <summary>Returns the marker byte for mapped peripheral reads.</summary>
        public byte ReadPeripheralByte(int address) => 0xab;
        /// <summary>Rejects writes because this witness only models operand reads.</summary>
        public void WriteByte(int address, byte value) => throw new NotSupportedException();
    }
}
