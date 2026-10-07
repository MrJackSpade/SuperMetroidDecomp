using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
internal static partial class Program
{
    private static int VerifyMotherBrainTankBackground(bool ascentMaskOnly = false)
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation = runtimeFixtureInstallation.Value;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        var maps = installation.LoadMaps();
        game.BindMapPresentation(maps);
        game.BindGameplayBasePalettes(installation.LoadGameplayBasePalettes());
        game.BindStandardObjectArt(installation.LoadStandardObjects());
        game.BindIntroCinematicArt(installation.LoadIntroCinematicArt());
        game.BindSamusBodyArt(installation.LoadSamusBodyArt());
        game.BindEndingMode7Art(installation.LoadEndingMode7Art());
        game.BindEndingObjectArt(installation.LoadEndingObjectArt());
        game.BindEndingPaletteArt(installation.LoadEndingPalettes());
        game.BindRoomCharacterArt(installation.LoadRoomCharacters());
        game.BindRoomPaletteArt(installation.LoadRoomPalettes());
        game.BindRoomMetatileArt(installation.LoadRoomMetatiles());
        game.BindRoomVisualLayouts(installation.LoadRoomVisualLayouts());
        game.BindRoomPlmShotBlockVisuals(installation.LoadRoomPlmShotBlockVisuals());
        game.BindRoomPlmGrappleBlockVisuals(installation.LoadRoomPlmGrappleBlockVisuals());
        game.BindRoomPlmStationVisuals(installation.LoadRoomPlmStationVisuals());
        game.BindRoomPlmBlueDoorVisuals(installation.LoadRoomPlmBlueDoorVisuals());
        game.BindRoomPlmColoredDoorVisuals(installation.LoadRoomPlmColoredDoorVisuals());
        game.BindRoomPlmGreyDoorVisuals(installation.LoadRoomPlmGreyDoorVisuals());
        game.BindRoomPlmEyeDoorVisuals(installation.LoadRoomPlmEyeDoorVisuals());
        game.BindRoomPlmMotherBrainGlassVisuals(installation.LoadRoomPlmMotherBrainGlassVisuals());
        game.BindRoomPlmNoobTubeVisuals(installation.LoadRoomPlmNoobTubeVisuals());
        game.BindRoomPlmDownwardGateVisuals(installation.LoadRoomPlmDownwardGateVisuals());
        game.BindRoomPlmElevatorPlatformVisuals(installation.LoadRoomPlmElevatorPlatformVisuals());
        game.BindRoomPlmEscapeGateVisuals(installation.LoadRoomPlmEscapeGateVisuals());
        game.BindRoomPlmBombTorizoHandVisuals(installation.LoadRoomPlmBombTorizoHandVisuals());
        game.BindRoomPlmDraygonCannonVisuals(installation.LoadRoomPlmDraygonCannonVisuals());
        game.BindRoomPlmChozoStatueVisuals(installation.LoadRoomPlmChozoStatueVisuals());
        game.BindRoomPlmLinkedRestoreVisuals(installation.LoadRoomPlmLinkedRestoreVisuals());
        game.BindRoomPlmTourianAccessVisuals(installation.LoadRoomPlmTourianAccessVisuals());
        game.BindRoomPlmSpeedBoosterVisuals(installation.LoadRoomPlmSpeedBoosterVisuals());
        game.BindRoomPlmMaridiaElevatubeVisuals(installation.LoadRoomPlmMaridiaElevatubeVisuals());
        game.BindRoomPlmSporeSpawnCeilingVisuals(installation.LoadRoomPlmSporeSpawnCeilingVisuals());
        game.BindRoomPlmSamusEaterVisuals(installation.LoadRoomPlmSamusEaterVisuals());
        game.BindRoomPlmBotwoonWallVisuals(installation.LoadRoomPlmBotwoonWallVisuals());
        game.BindRoomPlmKraidVisuals(installation.LoadRoomPlmKraidVisuals());
        game.BindRoomPlmCrocomireVisuals(installation.LoadRoomPlmCrocomireVisuals());
        game.BindRoomPlmMotherBrainFakeDeathVisuals(installation.LoadRoomPlmMotherBrainFakeDeathVisuals());
        game.BindRoomPlmCollectibleVisuals(installation.LoadRoomPlmCollectibleVisuals());
        game.BindRoomPlmDynamicCollectibleArt(installation.LoadRoomPlmDynamicCollectibleArt());
        game.BindXrayRevealVisuals(installation.LoadXrayRevealVisuals());
        game.BindRoomBackgroundTilemapArt(installation.LoadRoomBackgroundTilemaps());
        game.BindRoomSkyTilemapArt(installation.LoadRoomSkyTilemaps());
        var projectiles=installation.LoadProjectiles();
        game.BindProjectileCompositions(projectiles.Catalog);
        game.BindProjectileFrameBindings(projectiles.FrameBindings);
        game.BindBeamArtwork(projectiles.BeamTiles);
        game.BindEnemyTileArtwork(installation.LoadEnemyTiles());
        game.BindTrailArtwork(projectiles.Trails);
        game.BindChargeFlarePlacement(projectiles.FlarePlacement);
        game.BindChargeFlareCompositions(projectiles.FlareCompositions);
        game.BindGrappleArtwork(projectiles.GrappleTiles);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime",flags)!.Invoke(game,new object[]{false});
        var runtime=(SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime",flags)!.GetValue(game)!;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        var expectedVram = new SnesVram();
        LibraryBackgroundLoader.Execute(installation.OpenRuntimeAddressSpace(), expectedVram,
            0xe48a, 0, installation.LoadRoomBackgroundTilemaps());
        runtime.LoadCartridgeRoomForDebug(0xdd58);
        if (ascentMaskOnly) return VerifyMotherBrainAscentCapture(runtime);
        const int bg2ByteAddress = 0x9000;
        AssertTrue(runtime.Vram.Bytes.Slice(bg2ByteAddress, 0x1000).SequenceEqual(
            expectedVram.Bytes.Slice(bg2ByteAddress, 0x1000)),
            "Mother Brain initialization must preserve both pages of the actual pipe-room background");
        for (int index = 0; index < 0x800; index++)
            AssertEqual((ushort)0x0338, ReadStaged(index), "Initializer prepares all enemy BG2 staging words");
        var state = runtime.Enemies.MotherBrain!;
        AssertEqual((ushort)0x800, state.EnemyBg2TilemapSize, "Native default transfer size is bytes, not words");
        typeof(RoomEnemySystem).GetMethod("SetupMotherBrainPhaseTwoGraphics", flags)!
            .Invoke(runtime.Enemies, [state]);
        var blank = Enumerable.Repeat((ushort)0x0338, 0x400).ToArray();
        expectedVram.ExecuteWordTransfer(blank, 0x4800, 1);
        AssertTrue(runtime.Vram.Bytes.Slice(bg2ByteAddress, 0x1000).SequenceEqual(
            expectedVram.Bytes.Slice(bg2ByteAddress, 0x1000)),
            "Phase-two setup publishes the staged first page at the native transition, retaining page two");
        var artwork = runtime.Enemies.TileArtwork!;
        AssertTrue(artwork.MotherBrainBodyBg2Frames!.TryGet(0xa252, out var writes),
            "Crouched body BG2 artwork is installed");
        state.Body.ExtraProperties = state.Body.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
        typeof(RoomEnemySystem).GetMethod("ApplyInstalledEnemyBg2Frame", flags)!
            .Invoke(runtime.Enemies, [state.Body, (ushort)0xa252]);
        foreach (var write in writes.Span)
        {
            expectedVram.ExecuteWordTransfer(write.Tiles.Span, (ushort)(0x4800 + write.DestinationWord), 1);
            for (int index = 0; index < write.Tiles.Length; index++)
                AssertEqual(write.Tiles.Span[index], ReadStaged(write.DestinationWord + index),
                    "Body frame updates the native staging image as well as visible BG2");
        }
        AssertTrue(runtime.Vram.Bytes.Slice(bg2ByteAddress, 0x1000).SequenceEqual(
            expectedVram.Bytes.Slice(bg2ByteAddress, 0x1000)),
            "The later crouched body upload contains exactly its authored BG2 words");
        Console.WriteLine("Mother Brain tank background: real room pipes retained, native staging clear and later body upload passed.");
        return 0;
        ushort ReadStaged(int index) => (ushort)(bus.ReadByte(0x7e2000 + index * 2) |
            bus.ReadByte(0x7e2001 + index * 2) << 8);
    }
}
