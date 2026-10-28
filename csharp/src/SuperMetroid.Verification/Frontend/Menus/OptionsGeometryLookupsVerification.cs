using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Menus;

/// <summary>Checks options-screen cursor positions and controller label transfer geometry against bank-$82 words.</summary>
internal static class OptionsGeometryLookupsVerification
{
    /// <summary>Checks five four-byte-stride primary-menu cursor heights and their index bounds.</summary>
    internal static void VerifyOptionsPrimaryCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f309, 4, 5,
            GameOptionsPositions.PrimaryY, "primary cursor Y"));

    /// <summary>Checks nine four-byte-stride controller-menu cursor heights and their index bounds.</summary>
    internal static void VerifyOptionsControllerCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f31d, 4, 9,
            GameOptionsPositions.ControllerY, "controller cursor Y"));

    /// <summary>Checks three four-byte-stride special-menu cursor heights and their index bounds.</summary>
    internal static void VerifyOptionsSpecialCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f341, 4, 3,
            GameOptionsPositions.SpecialY, "special cursor Y"));

    /// <summary>Checks the seven controller-label boxes against the native byte offsets their labels are drawn at.</summary>
    internal static void VerifyOptionsLabelDestinations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f639, 2, 7,
            action => checked((ushort)((GameOptionsPositions.ControllerLabelAnchor(action).Y * GameOptionsDefinitions.MenuTilemapWidth
                + GameOptionsPositions.ControllerLabelAnchor(action).X) * sizeof(ushort))), "controller label box"));

    /// <summary>Checks seven source words used by controller-label transfers.</summary>
    internal static void VerifyOptionsLabelSources(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f647, 2, 7,
            index => GameOptionsSourceDefinitions.ControllerLabelSources[index], "controller label source"));

    /// <summary>Compares an indexed native geometry sequence and rejects indexes beyond its declared count.</summary>
    private static void VerifyOptionsGeometryWords(ISnesAddressSpace rom, int address,
        int stride, int count, Func<int, ushort> actual, string name)
    {
        for (int index = 0; index < count; index++)
            AssertEqual(ReadVerificationWord(rom, address + stride * index), actual(index),
                $"{name} native word {index}");
        foreach (int index in new[] { int.MinValue, -1, count, count + 1, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => actual(index),
                $"{name} rejects {index}");
    }
}
