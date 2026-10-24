using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Checks the background character artwork a room publishes as bindings instead of VRAM writes (#1350).</summary>
internal static class BgArtworkBindingsVerification
{
    /// <summary>
    /// A loaded room binds its CRE and tileset characters, and a library background its character transfer, at their
    /// VRAM words; VRAM itself holds none of them, and the published memory image shows them composed in bind order.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks the room characters' native VRAM words")]
    internal static void VerifyRoomCharacterBindings()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Kraid);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        CartridgeRoomAssets assets = runtime.Room.LoadedScope.Assets;
        // The room's characters are bound, not written: VRAM lacks them and the published image composes them in the
        // cartridge's transfer order, the area tileset over the CRE (Kraid's tileset covers the whole allocation).
        byte[] published = runtime.ComposedVram().ToArray();
        int area = RoomGraphicsLayout.AreaCharacters.Offset * 2, cre = RoomGraphicsLayout.CommonRoomElementCharacters.Offset * 2;
        var expected = new byte[RoomGraphicsLayout.AreaCharacters.Count * 2];
        assets.CreCharacters.CopyTo(expected.AsSpan(cre - area));
        assets.RoomCharacters.AsSpan(0, Math.Min(assets.RoomCharacters.Length, expected.Length)).CopyTo(expected);
        // Kraid's library background binds its own characters from word $2000, over the tileset.
        int library = 0x2000 * 2 - area;
        AssertTrue(published.AsSpan(area, library).SequenceEqual(expected.AsSpan(0, library)),
            "the published image shows the room's characters in transfer order");
        AssertTrue(!published.AsSpan(area + library, 0x40).SequenceEqual(expected.AsSpan(library, 0x40)),
            "the library background's characters show over the tileset where they bind later");
        AssertTrue(!runtime.Vram.Bytes.Slice(area, library).SequenceEqual(expected.AsSpan(0, library)),
            "VRAM itself holds no room characters");
        Console.WriteLine("  Background artwork: the room's characters publish from bindings, not VRAM.");
    }

    /// <summary>Runs the background artwork binding checks.</summary>
    [VerificationCommand("--bg-artwork-bindings")]
    internal static int BgArtworkBindingsCommand()
    {
        Suite(nameof(VerifyRoomCharacterBindings), () => VerifyRoomCharacterBindings());
        return 0;
    }
}
