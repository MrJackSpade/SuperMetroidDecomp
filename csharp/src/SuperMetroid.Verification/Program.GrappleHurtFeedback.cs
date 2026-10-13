using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Rendering;
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
using static SuiteTimingVerification;
using static VerificationHelpersVerification;
internal static class GrappleHurtFeedbackVerification
{
    internal static void VerifyGrappleHurtFeedback()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation=RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        InstalledFixtures.BindGame(game);
        game.CreateGameplayRuntime(false);
        var runtime=game.runtime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.ParlorAndAlcatraz);
        foreach (var enemy in runtime.Enemies.SlotTable.Slots) enemy.Clear();
        var level = CreateRoom(new(32, 16), new ushort[512], new byte[512]);
        runtime.Room.LevelData = level;
        var samus = runtime.Player.Samus!;
        samus.InputLocked = false;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.GrappleBeam;
        samus.SelectedHudItem = SamusHudRomData.GrappleSelectedItem;
        runtime.Artwork.BindGrapplePresentation();
        SamusGrappleMovement.ConnectUnobstructedSwing(bus, samus, new Position16<RoomPixels>(160, 64), 52,
            SnesAngle.QuarterTurn, 0, faceRight: true);
        samus.CommitPoseHistory(bus);
        runtime.StepFrame((ushort)SnesButton.X);
        AssertEqual((ushort)0, samus.HurtFlashCounter, "undamaged hanging Samus has no hurt flash");
        AssertEqual(GrapplePhase.ConnectedSwinging, samus.Grapple.Phase, "fixture is hanging from an attached grapple");
        var target = runtime.Enemies.SlotTable.Slots[0];
        target.EnemyDefinitionPointer = EnemyDefinitionId.Ripper;
        target.Definition = RoomEnemyDefinitionCatalog.Get(target.EnemyDefinitionPointer);
        target.Position = target.Position.WithWhole(samus.Position.Whole);
        target.Radius = new(8, 8);
        target.SpritemapPointer = 1;
        var interactive = (List<ushort>)runtime.Enemies.ProcessingWindow._interactiveEnemyIndexes;
        interactive.Clear();
        interactive.Add(target.NativeIndex);
        ushort health = samus.Health;
        AssertTrue(runtime.Enemies.Collision.ResolveOrdinarySamusContact(samus, (ushort)SnesButton.X), "ordinary enemy contact hits hanging Samus");
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
        Console.WriteLine("Grapple hurt feedback: real enemy contact retains hanging/no knockback and starts one native counter-two gasp.");
    }

    private static void VerifyHurtHealthHistoryRestoration()
    {
        var state = new SamusState { Health = 77, PreviousHealthForHurtCheck = 99 };
        using var current = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(current, state);
        current.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SamusState>(current);
        AssertEqual((ushort)99, restored.PreviousHealthForHurtCheck, "current captures preserve pending health-loss history");
        restored.InputSnapshot.UpdateHurtFlashFromHealthLoss();
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
        migrated.InputSnapshot.UpdateHurtFlashFromHealthLoss();
        AssertEqual((ushort)0, migrated.HurtFlashCounter, "legacy load does not invent a damage event");
    }

    [VerificationCommand("--grapple-hurt-feedback")]
    internal static int GrappleHurtFeedbackCommand()
    {
        Suite(nameof(VerifyGrappleHurtFeedback), () => VerifyGrappleHurtFeedback());
        return 0;
    }
}
