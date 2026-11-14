using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks pause-map save marker frames and timing against the native animation.</summary>
internal static class MapIndicatorAnimationVerification
{
    /// <summary>Compares the pulse's four pictures, in order, to bank $82.</summary>
    internal static void VerifyMapIndicatorSprites(ISnesAddressSpace rom)
    {
        AssertEqual(4, PauseMapIndicatorAnimation.Pulse.Count, "the marker pulse has four steps");
        for (int step = 0; step < PauseMapIndicatorAnimation.Pulse.Count; step++)
            AssertEqual(ReadVerificationWord(rom, 0x82ba2d + step * 2),
                MapSpriteOrdinals.Spritemaps.Word(PauseMapIndicatorAnimation.Pulse[step].Sprite), "original marker sprite phase");
    }

    /// <summary>Compares native frame delays and 49 ticks of marker phase and backing alternation.</summary>
    internal static void VerifyMapIndicatorDelays(ISnesAddressSpace rom)
    {
        AssertEqual((byte)0xe0, rom.ReadCartridgeByte(0x82ba06), "native marker frame CPX");
        AssertEqual(ReadVerificationWord(rom, 0x82ba07), PauseMapIndicatorAnimation.Pulse.Count * 2,
            "native marker word-offset wrap boundary");
        for (int step = 0; step < PauseMapIndicatorAnimation.Pulse.Count; step++)
            AssertEqual(ReadVerificationWord(rom, 0x82ba25 + step * 2),
                PauseMapIndicatorAnimation.Pulse[step].Duration, "original marker phase delay");

        using var json = new MemoryStream(MapSaveMarkerExtractor.Extract(rom), writable: false);
        var marker = new FileSelectStationMarker(LoadStationDefinitions.Gunship,
            MapSaveMarkerFile.Read(json));
        int nativeOffset = 0, nativeTimer = 0, loops = 0;
        AssertEqual(ReadVerificationWord(rom, 0x82ba2d), MapSpriteOrdinals.Spritemaps.Word(marker.SpriteId), "marker initial phase before first tick");
        for (int tick = 0; tick < 49; tick++)
        {
            // Original $82:B9FC uses a word byte-offset and the native delay data,
            // independently of the replacement's ordinal phase and alternating rule.
            if (nativeTimer == 0)
            {
                nativeOffset += 2;
                if (nativeOffset == 8) { nativeOffset = 0; loops++; }
                nativeTimer = ReadVerificationWord(rom, 0x82ba25 + nativeOffset);
            }
            nativeTimer--;
            marker.Step();
            AssertEqual(ReadVerificationWord(rom, 0x82ba2d + nativeOffset), MapSpriteOrdinals.Spritemaps.Word(marker.SpriteId),
                $"actual marker phase at tick {tick}");
            AssertEqual((loops & 1) == 0, marker.ShowBacking, $"marker backing parity at tick {tick}");
        }
    }
}
