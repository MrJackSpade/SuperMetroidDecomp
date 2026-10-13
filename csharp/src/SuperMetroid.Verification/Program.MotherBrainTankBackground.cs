using System.Buffers.Binary;
using System.Runtime.InteropServices;
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
using static MotherBrainAscentCaptureVerification;
/// <summary>Checks Mother Brain's tank background staging and phase-two BG2 uploads.</summary>
internal static class MotherBrainTankBackgroundVerification
{
    /// <summary>Verifies pipe-room VRAM survives initialization and the crouched body updates only its authored BG2 words.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static int VerifyMotherBrainTankBackground(bool ascentMaskOnly = false)
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        var projectiles=InstalledFixtures.Projectiles;
        InstalledFixtures.BindGame(game);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime",flags)!.Invoke(game,new object[]{false});
        var runtime=(SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime",flags)!.GetValue(game)!;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        var expectedVram = new SnesVram();
        LibraryBackgroundLoader.Execute(installation.OpenRuntimeAddressSpace(), expectedVram, 0xe48a, 0, new LibraryBackgroundArtwork { TilemapArt = installation.LoadRoomBackgroundTilemaps() });
        runtime.LoadCartridgeRoomForDebug(0xdd58);
        if (ascentMaskOnly) return VerifyMotherBrainAscentCapture(runtime);
        const int bg2ByteAddress = 0x9000;
        AssertTrue(runtime.Vram.Bytes.Slice(bg2ByteAddress, 0x1000).SequenceEqual(
            expectedVram.Bytes.Slice(bg2ByteAddress, 0x1000)),
            "Mother Brain initialization must preserve both pages of the actual pipe-room background");
        for (int index = 0; index < 0x800; index++)
            AssertEqual((ushort)0x0338, ReadStaged(index), "Initializer prepares all enemy BG2 staging words");
        var state = runtime.Enemies.MotherBrainEncounter.MotherBrain!;
        AssertEqual((ushort)0x800, state.EnemyBg2TilemapSize, "Native default transfer size is bytes, not words");
        runtime.Enemies.MotherBrainBehavior.SetupMotherBrainPhaseTwoGraphics(state);
        var blank = Enumerable.Repeat((ushort)0x0338, 0x400).ToArray();
        expectedVram.ExecuteWordTransfer(blank, 0x4800, 1);
        AssertTrue(runtime.Vram.Bytes.Slice(bg2ByteAddress, 0x1000).SequenceEqual(
            expectedVram.Bytes.Slice(bg2ByteAddress, 0x1000)),
            "Phase-two setup publishes the staged first page at the native transition, retaining page two");
        var artwork = runtime.Enemies.TileArtwork!;
        AssertTrue(artwork.MotherBrainBodyBg2Frames!.TryGet(0xa252, out var writes),
            "Crouched body BG2 artwork is installed");
        state.Body.ExtraProperties = state.Body.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
        runtime.Enemies.Instructions.ApplyInstalledEnemyBg2Frame(state.Body, (ushort)0xa252);
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
        ushort ReadStaged(int index) => new EnemyScratchMemory(bus).EnemyBg2Tilemap[index];
    }

    /// <summary>Runs the ascent mask capture using the initialized Mother Brain room.</summary>
    [VerificationCommand("--mother-brain-ascent-capture")]
    internal static int MotherBrainAscentCaptureCommand()
    {
        return VerifyMotherBrainTankBackground(ascentMaskOnly: true);
    }

    /// <summary>Runs the tank background staging and VRAM transfer check.</summary>
    [VerificationCommand("--mother-brain-tank-background")]
    internal static int MotherBrainTankBackgroundCommand()
    {
        return VerifyMotherBrainTankBackground();
    }
}
