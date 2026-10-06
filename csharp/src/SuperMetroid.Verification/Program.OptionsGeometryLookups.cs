using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyOptionsPrimaryCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f309, 4, 5,
            GameOptionsRomData.Cursors.PrimaryY, "primary cursor Y"));

    private static void VerifyOptionsControllerCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f31d, 4, 9,
            GameOptionsRomData.Cursors.ControllerY, "controller cursor Y"));

    private static void VerifyOptionsSpecialCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f341, 4, 3,
            GameOptionsRomData.Cursors.SpecialY, "special cursor Y"));

    private static void VerifyOptionsLabelDestinations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f639, 2, 7,
            GameOptionsRomData.ControllerLabels.Destination, "controller label destination"));

    private static void VerifyOptionsLabelSources(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f647, 2, 7,
            GameOptionsRomData.ControllerLabels.Source, "controller label source"));

    private static void VerifyOptionsGeometryWords(ISnesAddressSpace rom, int address,
        int stride, int count, Func<int, ushort> actual, string name)
    {
        for (int index = 0; index < count; index++)
            AssertEqual(ReadVerificationWord(rom, address + stride * index), actual(index),
                $"{name} native word {index}");
        foreach (int index in new[] { int.MinValue, -1, count, count + 1, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => actual(index),
                $"{name} rejects {index}");
    }
}
