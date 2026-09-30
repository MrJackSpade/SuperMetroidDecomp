using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>Explicit old-layout reproduction, with authored bytes rather than a cartridge.</summary>
    private static void VerifyLegacyMutableMemoryGraph()
    {
        foreach (bool aliasRetiredRom in new[] { false, true })
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(DebuggerGraphWireDefinitions.NewObjectMarker);
                writer.Write(1);
                writer.Write(DebuggerStateTypeIdentity.GetSerializedName(typeof(SuperMetroidAddressSpace)));
                writer.Write(DebuggerGraphWireDefinitions.FieldsPayloadKind);
                writer.Write(3);
                Field(writer, "_rom");
                Bytes(writer, 2, Enumerable.Repeat((byte)0xee, LegacyCartridgeStateFormat.BankByteCount).ToArray());
                Field(writer, "_workRam");
                var work = new byte[SuperMetroidAddressSpace.WorkRamByteCount];
                work[0] = 0x12;
                work[^1] = 0x34;
                Bytes(writer, 3, work);
                Field(writer, "_saveRam");
                if (aliasRetiredRom)
                {
                    writer.Write(DebuggerGraphWireDefinitions.ReferenceObjectMarker);
                    writer.Write(2);
                }
                else
                {
                    var save = new byte[SuperMetroidAddressSpace.SaveRamByteCount];
                    save[0] = 0x56;
                    save[^1] = 0x78;
                    Bytes(writer, 4, save);
                }
            }
            stream.Position = 0;
            if (aliasRetiredRom)
            {
                AssertThrows<InvalidDataException>(
                    () => DebuggerObjectGraphSerializer.Deserialize<SuperMetroidAddressSpace>(stream),
                    "retired cartridge aliases cannot reenter live memory");
                continue;
            }
            SuperMetroidAddressSpace restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidAddressSpace>(stream);
            AssertEqual((byte)0x12, restored.WorkRam[0], "legacy graph preserves first WRAM byte");
            AssertEqual((byte)0x34, restored.WorkRam[^1], "legacy graph preserves last WRAM byte");
            AssertEqual((byte)0x56, restored.SaveRam[0], "legacy graph preserves first SRAM byte");
            AssertEqual((byte)0x78, restored.SaveRam[^1], "legacy graph preserves last SRAM byte");
            AssertTrue(restored.GetType().GetFields(System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic).All(field => field.Name != "_rom"),
                "legacy restoration never recreates cartridge storage");
        }
        AssertThrows<InvalidDataException>(() => LegacyCartridgeStateImport.DiscardPayload(
            new MemoryStream(), int.MaxValue), "legacy cartridge length is bounded before reading");
        AssertThrows<EndOfStreamException>(() => LegacyCartridgeStateImport.DiscardPayload(
            new MemoryStream(new byte[1]), LegacyCartridgeStateFormat.BankByteCount),
            "truncated retired payload fails inside the harness");

        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var manager = new SuperMetroidSaveRam(memory);
        typeof(SuperMetroidSaveRam).GetField("mutableMemory", System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic)!.SetValue(manager, null);
        DebuggerStateFieldMigrations.InitializeMissingFields(manager, 1);
        AssertTrue(manager.ReadSlot(0) is null, "legacy save manager rebinds its original SRAM owner");

        static void Field(BinaryWriter writer, string name)
        {
            writer.Write(DebuggerStateTypeIdentity.GetSerializedName(typeof(SuperMetroidAddressSpace)));
            writer.Write(name);
        }
        static void Bytes(BinaryWriter writer, int id, byte[] bytes)
        {
            writer.Write(DebuggerGraphWireDefinitions.NewObjectMarker);
            writer.Write(id);
            writer.Write(DebuggerStateTypeIdentity.GetSerializedName(typeof(byte[])));
            writer.Write(DebuggerGraphWireDefinitions.PrimitiveArrayPayloadKind);
            writer.Write(1);
            writer.Write(bytes.Length);
            writer.Write(0);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }
    }
}
