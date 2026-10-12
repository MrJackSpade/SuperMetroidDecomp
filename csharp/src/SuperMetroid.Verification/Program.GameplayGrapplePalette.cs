using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
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
using SuperMetroid.Core.Spatial;
/// <summary>Represents gameplay grapple palette verification.</summary>
internal static class GameplayGrapplePaletteVerification
{
    /// <summary>Verifies gameplay grapple palette.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyGameplayGrapplePalette()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation=RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        var projectiles=installation.LoadProjectiles();
        InstalledFixtures.BindGame(game);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime",flags)!.Invoke(game,new object[]{false});
        var runtime=(SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime",flags)!.GetValue(game)!;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.ParlorAndAlcatraz);
        foreach (var enemy in runtime.Enemies.SlotTable.Slots) enemy.Clear();
        var words = new ushort[32 * 16];
        for (int i = 12 * 32; i < 13 * 32; i++) words[i] = 0x8000;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 16), new RoomLevelPlanes(words, new byte[512], new ushort[512]), new byte[8]);
        typeof(ActiveRoomState).GetProperty(nameof(ActiveRoomState.LevelData))!.SetValue(runtime.Room, level);
        var samus = runtime.Player.Samus!;
        var expected = new SnesCgram();
        var palettes = projectiles.BeamTiles.Palettes ?? throw new InvalidDataException("Missing beam palettes.");
        palettes.LoadTo(expected, SamusBeamCombination.Ice);
        foreach (ushort beam in new ushort[] { 0, 4 })
        {
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            samus.InputLocked = false;
            samus.EquippedItems = samus.CollectedItems = (ushort)SamusEquipmentFlags.GrappleBeam;
            samus.EquippedBeams = beam;
            samus.SelectedHudItem = SamusHudRomData.GrappleSelectedItem;
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus);
            samus.CommitPoseHistory(bus);
            samus.Position = samus.Position.WithWhole(new(128, (ushort)(192 - samus.Kinematics.Radius.Y)));
            samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
            runtime.StepFrame(0);
            palettes.LoadTo(runtime.Cgram, SamusBeamCombinations.FromTableIndex(beam));
            runtime.Cgram.SetColor(223, Bgr555.FromWord(0x1234));
            runtime.StepFrame((ushort)SnesButton.X);
            AssertTrue(runtime.Player.LastGrappleMovement is { Fired: true }, "normal HUD input fires grapple");
            AssertPalette("normal firing");
            samus.Pose = SamusPoseId.StandingAimDiagonalUpRightPose;
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus);
            samus.CommitPoseHistory(bus);
            palettes.LoadTo(runtime.Cgram, SamusBeamCombinations.FromTableIndex(beam));
            runtime.Cgram.SetColor(223, Bgr555.FromWord(0x1234));
            runtime.StepFrame((ushort)SnesButton.X);
            AssertTrue(runtime.Player.LastGrappleMovement is { Fired: true }, "native pose-change window refires grapple");
            AssertPalette("pose-change refiring");
            for (int frame = 0; frame < 4 && samus.Grapple.Phase != GrapplePhase.Inactive; frame++)
                runtime.StepFrame(0);
            AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "release cancels extending grapple");
            var ordinary = new SnesCgram();
            palettes.LoadTo(ordinary, SamusBeamCombinations.FromTableIndex(beam));
            AssertTrue(runtime.Cgram.Colors.Slice(224, 16).SequenceEqual(ordinary.Colors.Slice(224, 16)),
                "cancel restores equipped beam palette");
        }
        Console.WriteLine("Gameplay grapple palette: normal HUD firing, refire, fixed flare color and cancellation restoration verified.");
        void AssertPalette(string phase)
        {
            AssertTrue(runtime.Cgram.Colors.Slice(224, 16).SequenceEqual(expected.Colors.Slice(224, 16)),
                phase + " loads native palette two");
            AssertEqual((ushort)0x7f91, runtime.Cgram.Colors[223], phase + " sets native flare color");
        }
    }

    /// <summary>Gameplay grapple palette command.</summary>
    [VerificationCommand("--gameplay-grapple-palette")]
    internal static int GameplayGrapplePaletteCommand()
    {
        Suite(nameof(VerifyGameplayGrapplePalette), () => VerifyGameplayGrapplePalette());
        return 0;
    }
}
