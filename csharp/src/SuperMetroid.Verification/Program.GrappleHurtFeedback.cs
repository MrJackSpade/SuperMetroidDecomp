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
using SuperMetroid.Desktop;
internal static partial class Program
{
    /// <summary>Verifies that enemy damage starts Samus's hurt flash and gasp while an attached grapple suppresses knockback.</summary>
    private static void VerifyGrappleHurtFeedback()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation=RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        game.BindMapPresentation(installation.LoadMaps());
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
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.ParlorAndAlcatraz);
        foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
        var level = CreateRoom(32, 16, new ushort[512], new byte[512]);
        typeof(SuperMetroidRuntime).GetProperty(nameof(SuperMetroidRuntime.LevelData))!.SetValue(runtime, level);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.GrappleBeam;
        samus.SelectedHudItem = SamusHudRomData.GrappleSelectedItem;
        typeof(SuperMetroidRuntime).GetMethod("BindGrapplePresentation", flags)!.Invoke(runtime, null);
        SamusGrappleMovement.ConnectUnobstructedSwing(bus, samus, 160, 64, 52,
            SnesAngle.QuarterTurn, 0, faceRight: true);
        samus.CommitPoseHistory(bus);
        runtime.StepFrame((ushort)SnesButton.X);
        AssertEqual((ushort)0, samus.HurtFlashCounter, "undamaged hanging Samus has no hurt flash");
        AssertEqual(GrapplePhase.ConnectedSwinging, samus.Grapple.Phase, "fixture is hanging from an attached grapple");
        var target = runtime.Enemies.Slots[0];
        target.EnemyDefinitionPointer = RoomEnemySystem.RipperDefinition;
        target.Definition = RoomEnemyDefinitionCatalog.Get(target.EnemyDefinitionPointer);
        target.XPosition = samus.XPosition;
        target.YPosition = samus.YPosition;
        target.XRadius = target.YRadius = 8;
        target.SpritemapPointer = 1;
        var interactive = (List<ushort>)typeof(RoomEnemySystem).GetField("_interactiveEnemyIndexes", flags)!.GetValue(runtime.Enemies)!;
        interactive.Clear();
        interactive.Add(target.NativeIndex);
        ushort health = samus.Health;
        AssertTrue(runtime.Enemies.ResolveOrdinarySamusContact(samus, (ushort)SnesButton.X), "ordinary enemy contact hits hanging Samus");
        AssertEqual((ushort)(health - target.Definition.Damage), samus.Health, "native contact reduces health");
        target.Clear();
        runtime.StepFrame((ushort)SnesButton.X);
        AssertEqual(GrapplePhase.ConnectedSwinging, samus.Grapple.Phase, "damage preserves the attached grapple");
        AssertTrue(!samus.KnockbackActive, "grapple suppresses the knockback mover");
        AssertEqual((ushort)1, samus.HurtFlashCounter, "post-draw health loss starts hurt feedback independently of knockback");
        runtime.StepFrame((ushort)SnesButton.X);
        AssertEqual((ushort)2, samus.HurtFlashCounter, "first hurt palette call advances the counter");
        samus.Health--; // Existing feedback must not restart on further health loss.
        runtime.StepFrame((ushort)SnesButton.X);
        AssertEqual((ushort)3, samus.HurtFlashCounter, "active hurt feedback does not restart");
        AssertEqual(1, samus.LiquidPhysics.SoundRequests.Count(request =>
            request.SoundEffect.Library == SoundEffectLibrary.Library1 && request.SoundEffect.Value == 0x35 && request.MaximumQueued == 6),
            "counter two queues exactly one native gasp with Max6");
        runtime.StepFrame((ushort)SnesButton.X);
        AssertEqual(0, samus.LiquidPhysics.SoundRequests.Count(request =>
            request.SoundEffect.Library == SoundEffectLibrary.Library1 && request.SoundEffect.Value == 0x35),
            "unchanged health does not repeat the gasp");
        Suite(nameof(VerifyHurtHealthHistoryRestoration), () => VerifyHurtHealthHistoryRestoration());
        Suite(nameof(VerifyDebuggerVersionCompatibility), () => VerifyDebuggerVersionCompatibility());
        Console.WriteLine("Grapple hurt feedback: real enemy contact retains hanging/no knockback and starts one native counter-two gasp.");
    }

    /// <summary>Checks that serialized health-loss history restores pending hurt feedback and migrates older graphs without inventing damage.</summary>
    private static void VerifyHurtHealthHistoryRestoration()
    {
        var state = new SamusState { Health = 77, PreviousHealthForHurtCheck = 99 };
        using var current = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(current, state);
        current.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SamusState>(current);
        AssertEqual((ushort)99, restored.PreviousHealthForHurtCheck, "current captures preserve pending health-loss history");
        restored.UpdateHurtFlashFromHealthLoss();
        AssertEqual((ushort)1, restored.HurtFlashCounter, "restored pending health loss starts feedback");

        // Construct the immediately preceding graph layout by removing only this
        // primitive field. Reference IDs remain unchanged by primitive values.
        using var fieldBytes = new MemoryStream();
        using (var writer = new BinaryWriter(fieldBytes, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(typeof(SamusState).AssemblyQualifiedName!);
            writer.Write("<PreviousHealthForHurtCheck>k__BackingField");
            DebuggerObjectGraphSerializer.Serialize(fieldBytes, (ushort)99);
        }
        byte[] bytes = current.ToArray();
        byte[] omitted = fieldBytes.ToArray();
        int fieldOffset = bytes.AsSpan().IndexOf(omitted);
        AssertTrue(fieldOffset >= 0, "current graph contains the health-history word");
        bytes = [.. bytes.AsSpan(0, fieldOffset), .. bytes.AsSpan(fieldOffset + omitted.Length)];
        using var legacy = new MemoryStream(bytes);
        using var reader = new BinaryReader(legacy, System.Text.Encoding.UTF8, leaveOpen: true);
        reader.ReadByte(); reader.ReadInt32(); reader.ReadString(); reader.ReadByte();
        long countOffset = legacy.Position;
        int count = reader.ReadInt32();
        legacy.Position = countOffset;
        using (var writer = new BinaryWriter(legacy, System.Text.Encoding.UTF8, leaveOpen: true))
            writer.Write(count - 1);
        legacy.Position = 0;
        var migrated = DebuggerObjectGraphSerializer.Deserialize<SamusState>(legacy);
        AssertEqual((ushort)77, migrated.PreviousHealthForHurtCheck, "legacy capture initializes missing history from saved health");
        migrated.UpdateHurtFlashFromHealthLoss();
        AssertEqual((ushort)0, migrated.HurtFlashCounter, "legacy load does not invent a damage event");
    }
}
