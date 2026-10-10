using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFxWeatherThirdGreen(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock)
    {
        foreach (byte id in new byte[] { 0x22, 0x62 })
        {
            ushort native = ReadVerificationWord(rom, 0x89aa06 + id);
            int expected = (native >> 5) & 31;
            AssertEqual(expected, RoomFxPaletteBlendDefinitions.CalculatedThirdGreen((RoomFxPaletteBlend)id)!.Value, "Native weather third green");
            AssertEqual(expected, (stock.Resolve((RoomFxPaletteBlend)id)[2].ToWord() >> 5) & 31, "Installed weather third green");
            var cgram = new SnesCgram(); stock.Apply(cgram, (RoomFxPaletteBlend)id);
            AssertEqual(expected, (cgram.Colors[27].ToWord() >> 5) & 31, "Applied weather third green");
            var owner = new RoomFxThirdColor((RoomFxPaletteBlend)id, Bgr555.FromWord(checked((ushort)(native))));
            var field = typeof(RoomFxThirdColor).GetField("greenOverride", BindingFlags.Instance | BindingFlags.NonPublic)!;
            AssertTrue(field.GetValue(owner) is null, "Stock weather third green is not stored");
            _ = owner.CreateColor();
            AssertTrue(field.GetValue(owner) is null, "Weather output does not cache green");
        }
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte id = (byte)value;
            if (!OriginalFxBlendIds().Contains(id))
                AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedThirdGreen((RoomFxPaletteBlend)id), "Unknown third-green selector rejects");
            else if (id is not (0x22 or 0x62))
                AssertTrue(RoomFxPaletteBlendDefinitions.CalculatedThirdGreen((RoomFxPaletteBlend)id) is null, "Liquid override green remains independent");
        }
    }
    private static void VerifyFxWeatherThirdBlue(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock)
    {
        foreach (byte id in new byte[] { 0x22, 0x62 })
        {
            ushort native = ReadVerificationWord(rom, 0x89aa06 + id);
            int expected = (native >> 10) & 31;
            AssertEqual(expected, RoomFxPaletteBlendDefinitions.CalculatedThirdBlue((RoomFxPaletteBlend)id, native & 31)!.Value, "Native weather third blue");
            AssertEqual(expected, (stock.Resolve((RoomFxPaletteBlend)id)[2].ToWord() >> 10) & 31, "Installed weather third blue");
            var cgram = new SnesCgram();
            stock.Apply(cgram, (RoomFxPaletteBlend)id);
            AssertEqual(expected, (cgram.Colors[27].ToWord() >> 10) & 31, "Applied weather third blue");
            var owner = new RoomFxThirdColor((RoomFxPaletteBlend)id, Bgr555.FromWord(checked((ushort)(native))));
            var field = typeof(RoomFxThirdColor).GetField("blueOverride", BindingFlags.Instance | BindingFlags.NonPublic)!;
            AssertTrue(field.GetValue(owner) is null, "Stock weather third blue is not stored");
            _ = owner.CreateColor();
            AssertTrue(field.GetValue(owner) is null, "Weather output does not create a blue cache");
        }
        foreach (byte id in OriginalFxBlendIds())
        {
            for (int color = 0; color < 0x8000; color++)
                AssertEqual((ushort)color, new RoomFxThirdColor((RoomFxPaletteBlend)id, Bgr555.FromWord(checked((ushort)((ushort)color)))).CreateColor(), "Every RGB5 third-color edit round-trips");
            foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxPaletteBlendDefinitions.CalculatedThirdBlue((RoomFxPaletteBlend)id, invalid), "Third blue RGB5 input bounds");
        }
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte id = (byte)value;
            if (!OriginalFxBlendIds().Contains(id))
                AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedThirdBlue((RoomFxPaletteBlend)id, 0), "Unknown third-blue selector rejects");
            else if (id is not (0x22 or 0x62))
                AssertTrue(RoomFxPaletteBlendDefinitions.CalculatedThirdBlue((RoomFxPaletteBlend)id, 0) is null, "Independent liquid third blue is not overridden by weather rules");
        }
    }
}
