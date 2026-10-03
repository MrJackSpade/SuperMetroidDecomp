using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static byte[] OriginalBlackBlendIds() => [0x02, 0x42, 0x48, 0xe2, 0xe8, 0xee];

    private static void VerifyFxBlendBlackRed(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxBlendBlackComponent(rom, stock, 0);
    private static void VerifyFxBlendBlackGreen(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxBlendBlackComponent(rom, stock, 5);
    private static void VerifyFxBlendBlackBlue(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxBlendBlackComponent(rom, stock, 10);

    private static void VerifyFxBlendBlackComponent(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock, int shift)
    {
        foreach (byte id in OriginalBlackBlendIds())
        {
            int expected = (ReadVerificationWord(rom, 0x89aa06 + id) >> shift) & 31;
            AssertEqual(expected, (RoomFxPaletteBlendDefinitions.CalculatedThirdColor(id)!.Value >> shift) & 31, "Original third-color component");
            AssertEqual(expected, (stock.Resolve(id)[2] >> shift) & 31, "Resolved third-color component");
            var cgram = new SnesCgram();
            stock.Apply(cgram, id);
            AssertEqual(expected, (cgram.Colors[27] >> shift) & 31, "Applied third-color component");
        }
    }

    private static void VerifyFxBlendBlackStorageAndEdits(RoomFxPaletteBlendCatalog stock)
    {
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte id = (byte)value;
            if (OriginalBlackBlendIds().Contains(id)) AssertEqual((ushort)0,
                RoomFxPaletteBlendDefinitions.CalculatedThirdColor(id)!.Value, "Six liquid selections calculate black");
            else if (id is 0x22 or 0x62) AssertTrue(RoomFxPaletteBlendDefinitions.CalculatedThirdColor(id) is null, "Weather third color stays independent");
            else AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedThirdColor(id), "Unknown calculated-color selector rejects");
        }
        var overrideField = typeof(RoomFxBlendColors).GetField("thirdOverride", BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (var field in typeof(RoomFxPaletteBlendCatalog).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                     .Where(field => field.FieldType == typeof(RoomFxBlendColors)))
        {
            var owner = (RoomFxBlendColors)field.GetValue(stock)!;
            bool weather = field.Name is "landingSiteRain" or "fog";
            AssertEqual(weather, overrideField.GetValue(owner) is not null, "Stock black values are not stored");
            _ = owner.CreateColors();
            AssertEqual(weather, overrideField.GetValue(owner) is not null, "Output creation does not cache black");
        }
        foreach (byte id in OriginalBlackBlendIds())
        foreach (ushort third in new ushort[] { 0, 1, 31, 32, 1023, 1024, 32767 })
        {
            var owner = new RoomFxBlendColors(id, 0x1234, 0x2345, third);
            var colors = owner.CreateColors();
            AssertEqual(third, colors[2], "Custom third-color components survive");
            colors[2] ^= 1;
            AssertEqual(third, owner.CreateColors()[2], "Output edits do not mutate loaded colors");
        }
    }

    private sealed class BlackBlendSourceGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadByte(int address)
        {
            foreach (byte id in OriginalBlackBlendIds())
                if (address == 0x89aa06 + id || address == 0x89aa07 + id)
                    throw new InvalidOperationException("FX blend extraction read a calculated black color.");
            return source.ReadByte(address);
        }
        public byte ReadCartridgeByte(int address) => ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
