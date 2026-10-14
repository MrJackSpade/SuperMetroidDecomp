using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Options;

/// <summary>Checks options-screen cursor positions and controller label transfer geometry against bank-$82 words.</summary>
internal static class OptionsGeometryLookupsVerification
{
    /// <summary>Checks five four-byte-stride primary-menu cursor heights and their index bounds.</summary>
    internal static void VerifyOptionsPrimaryCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f309, 4, 5,
            GameOptionsRomData.Cursors.PrimaryY, "primary cursor Y"));

    /// <summary>Checks nine four-byte-stride controller-menu cursor heights and their index bounds.</summary>
    internal static void VerifyOptionsControllerCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f31d, 4, 9,
            GameOptionsRomData.Cursors.ControllerY, "controller cursor Y"));

    /// <summary>Checks three four-byte-stride special-menu cursor heights and their index bounds.</summary>
    internal static void VerifyOptionsSpecialCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f341, 4, 3,
            GameOptionsRomData.Cursors.SpecialY, "special cursor Y"));

    /// <summary>Checks seven destination words used by controller-label transfers.</summary>
    internal static void VerifyOptionsLabelDestinations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f639, 2, 7,
            GameOptionsRomData.ControllerLabels.Destination, "controller label destination"));

    /// <summary>Checks seven source words used by controller-label transfers.</summary>
    internal static void VerifyOptionsLabelSources(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f647, 2, 7,
            GameOptionsRomData.ControllerLabels.Source, "controller label source"));

    /// <summary>Compares an indexed native geometry sequence and rejects indexes beyond its declared count.</summary>
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
