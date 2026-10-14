using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static MapPresentationVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Checks pause-map save marker frames and timing against the native animation.</summary>
internal static class MapIndicatorAnimationVerification
{
    /// <summary>Compares all four sprite phases to bank $82 and rejects invalid phase indices.</summary>
    internal static void VerifyMapIndicatorSprites(ISnesAddressSpace rom)
    {
        for (int frame = 0; frame < 4; frame++)
            AssertEqual(ReadVerificationWord(rom, 0x82ba2d + frame * 2),
                (ushort)PauseMapIndicatorAnimation.SpritemapId(frame), "original marker sprite phase");
        foreach (int invalid in new[] { int.MinValue, -1, 4, 256, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PauseMapIndicatorAnimation.SpritemapId(invalid),
                "unsupported marker sprite phase");
    }

    /// <summary>Compares native frame delays and 49 ticks of marker phase and backing alternation.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for map indicator delays")]
    internal static void VerifyMapIndicatorDelays(ISnesAddressSpace rom)
    {
        AssertEqual((byte)0xe0, rom.ReadCartridgeByte(0x82ba06), "native marker frame CPX");
        AssertEqual((int)ReadVerificationWord(rom, 0x82ba07), PauseMapIndicatorAnimation.FrameCount * 2,
            "native marker word-offset wrap boundary");
        for (int frame = 0; frame < 4; frame++)
            AssertEqual((int)ReadVerificationWord(rom, 0x82ba25 + frame * 2),
                PauseMapIndicatorAnimation.FrameDelay(frame), "original marker phase delay");
        foreach (int invalid in new[] { int.MinValue, -1, 4, 256, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PauseMapIndicatorAnimation.FrameDelay(invalid),
                "unsupported marker delay phase");

        using var json = new MemoryStream(MapSaveMarkerExtractor.Extract(rom), writable: false);
        var marker = new FileSelectStationMarker(NewForbiddenMapBus(), AreaId.Crateria, 0,
            MapSaveMarkerLayout.Load(json));
        int nativeOffset = 0, nativeTimer = 0, loops = 0;
        AssertEqual(ReadVerificationWord(rom, 0x82ba2d), (ushort)marker.SpritemapId, "marker initial phase before first tick");
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
            AssertEqual(ReadVerificationWord(rom, 0x82ba2d + nativeOffset), (ushort)marker.SpritemapId,
                $"actual marker phase at tick {tick}");
            AssertEqual((loops & 1) == 0, marker.ShowBacking, $"marker backing parity at tick {tick}");
        }
    }
}
