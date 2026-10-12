using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static DoorDefinitionsVerification;

internal static class LookupFxBlendBlackColorsVerification
{
    private static byte[] OriginalBlackBlendIds() => [0x02, 0x42, 0x48, 0xe2, 0xe8, 0xee];

    internal static void VerifyFxBlendBlackRed(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxBlendBlackComponent(rom, stock, 0);
    internal static void VerifyFxBlendBlackGreen(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxBlendBlackComponent(rom, stock, 5);
    internal static void VerifyFxBlendBlackBlue(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxBlendBlackComponent(rom, stock, 10);

    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void VerifyFxBlendBlackComponent(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock, int shift)
    {
        foreach (byte id in OriginalBlackBlendIds())
        {
            int expected = (ReadVerificationWord(rom, 0x89aa06 + id) >> shift) & 31;
            AssertEqual(expected, (RoomFxPaletteBlendDefinitions.CalculatedThirdColor((RoomFxPaletteBlend)id)!.Value.ToWord() >> shift) & 31, "Original third-color component");
            AssertEqual(expected, (stock.Resolve((RoomFxPaletteBlend)id)[2].ToWord() >> shift) & 31, "Resolved third-color component");
            var cgram = new SnesCgram();
            stock.Apply(cgram, (RoomFxPaletteBlend)id);
            AssertEqual(expected, (cgram.Colors[27].ToWord() >> shift) & 31, "Applied third-color component");
        }
    }

    internal static void VerifyFxBlendBlackStorageAndEdits(RoomFxPaletteBlendCatalog stock)
    {
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte id = (byte)value;
            if (OriginalBlackBlendIds().Contains(id)) AssertEqual((ushort)0,
                RoomFxPaletteBlendDefinitions.CalculatedThirdColor((RoomFxPaletteBlend)id)!.Value, "Six liquid selections calculate black");
            else if (id is 0x22 or 0x62) AssertTrue(RoomFxPaletteBlendDefinitions.CalculatedThirdColor((RoomFxPaletteBlend)id) is null, "Weather third color stays independent");
            else AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedThirdColor((RoomFxPaletteBlend)id), "Unknown calculated-color selector rejects");
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
            var owner = new RoomFxBlendColors((RoomFxPaletteBlend)id, Bgr555.FromWord(0x1234), Bgr555.FromWord(0x2345), Bgr555.FromWord(checked((ushort)(third))));
            var colors = owner.CreateColors();
            AssertEqual(third, colors[2], "Custom third-color components survive");
            colors[2] = Bgr555.FromWord((ushort)(colors[2].ToWord() ^ (1)));
            AssertEqual(third, owner.CreateColors()[2], "Output edits do not mutate loaded colors");
        }
    }

    internal static CartridgeReadGuard NewBlackBlendSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => OriginalBlackBlendIds().Any(id => address == 0x89aa06 + id || address == 0x89aa07 + id),
            _ => "FX blend extraction read a calculated black color.");
}
