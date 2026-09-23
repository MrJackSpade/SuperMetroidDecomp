using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyDebuggerVersionCompatibility()
    {
        VerifyRoomCallbackStateIdentity();
        VerifyLegacyShinesparkGraph();
        VerifyFieldIdentityRestorationIgnoresMetadataOrder();
        VerifyLegacyRoomVisualLayoutState();
        MethodInfo expected = typeof(Program).GetMethod(nameof(DebuggerSignatureProbe), BindingFlags.NonPublic | BindingFlags.Static)!;
        foreach (bool wrongParameter in new[] { false, true })
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(expected.Name);
                writer.Write(true);
                writer.Write(LegacyIdentity(typeof(SamusState)));
                writer.Write(0); // Non-generic callback.
                writer.Write(1);
                writer.Write(wrongParameter ? typeof(int).AssemblyQualifiedName! : LegacyIdentity(typeof(SamusState)));
            }
            stream.Position = 0;
            using var reader = new BinaryReader(stream);
            if (wrongParameter)
            {
                bool rejected = false;
                try { DebuggerDelegateIdentity.Read(reader, typeof(Program), Resolve); }
                catch (InvalidDataException) { rejected = true; }
                AssertTrue(rejected, "cross-version delegate still rejects a changed parameter type");
            }
            else
                AssertEqual(expected, DebuggerDelegateIdentity.Read(reader, typeof(Program), Resolve),
                    "unchanged callback signature accepts the published assembly version");
        }

        var fields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(SamusState)])!;
        FieldInfo[] legacy = fields.Where(field => field.Name is not
            "<StationaryScriptControlLocked>k__BackingField" and not
            "_poseCollisionPreviousYPosition" and not "_poseAlignmentPreviousYDelta").ToArray();
        FieldInfo[] selected = DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusState), fields, legacy.Length);
        AssertTrue(selected.SequenceEqual(legacy), "0.2.1 Samus layout omits only the three verified additions");
        AssertTrue(selected.Any(field => field.Name == "_healthWarning") && selected.Any(field => field.Name == "_poseHistory"),
            "0.2.1 migration retains existing health and pose history rather than guessing from count");
        FieldInfo[] preBombLockFields = legacy.Where(field => field.Name is not "_healthWarning" and not "_poseHistory"
            and not "<AutoJumpTimer>k__BackingField" and not "<PreviousDrawHeldInput>k__BackingField"
            and not "<AutoJumpInputPending>k__BackingField" and not "<BombJumpPoseInputLocked>k__BackingField").ToArray();
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusState), fields,
            preBombLockFields.Length).SequenceEqual(preBombLockFields), "b944f1b5 Samus layout retains the saved draw-input latch");
        FieldInfo[] earlyPlayerFields = fields.Where(field => field.Name is not
            "_poseCollisionPreviousYPosition" and not "_poseAlignmentPreviousYDelta" and not
            "<BombJumpPoseInputLocked>k__BackingField" and not "_healthWarning" and not
            "<PreviousDrawNewInput>k__BackingField" and not "<AutoJumpTimer>k__BackingField" and not
            "<PreviousDrawHeldInput>k__BackingField" and not "<AutoJumpInputPending>k__BackingField" and not
            "<ShinesparkPoseInputLocked>k__BackingField" and not "<CrystalFlashPoseInputLocked>k__BackingField" and not
            "_poseHistory" and not "<StationaryScriptControlLocked>k__BackingField").ToArray();
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusState), fields,
            earlyPlayerFields.Length).SequenceEqual(earlyPlayerFields),
            "early player Samus layout restores exactly its twelve known transient omissions");
        var grappleResultFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod(
            "GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(GrappleMovementResult)])!;
        FieldInfo[] legacyGrappleResultFields = grappleResultFields.Where(field => field.Name is not
            "<PendingDropPose>k__BackingField" and not "<PendingConnection>k__BackingField").ToArray();
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(
            typeof(GrappleMovementResult), grappleResultFields, legacyGrappleResultFields.Length)
            .SequenceEqual(legacyGrappleResultFields),
            "legacy grapple result restores with no deferred pose handoff");

        var gameType = typeof(SuperMetroid.Core.Frontend.SuperMetroidGame);
        var bankType = typeof(SuperMetroid.Core.Audio.ManagedPcmSampleBank);
        var bankFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [bankType])!;
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(bankType, bankFields, 3)
            .SequenceEqual(bankFields.Where(field => field.Name != "loopEntrySources")),
            "legacy PCM bank retains samples and upload identity");
        var sample = new SuperMetroid.Core.Audio.ManagedPcmSample("legacy-loop", 32000, new short[32], 16);
        var bank = new SuperMetroid.Core.Audio.ManagedPcmSampleBank("legacy-bank", 0,
            new Dictionary<byte, SuperMetroid.Core.Audio.ManagedPcmSample> { [0] = sample });
        bankType.GetField("loopEntrySources", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(bank, null);
        DebuggerStateFieldMigrations.InitializeMissingFields(bank, 3);
        AssertEqual((sample, 16), bank.ResolveLoopEntry(0), "legacy PCM initializer retains the saved self-loop cursor");
        var voiceType = typeof(SuperMetroid.Core.Audio.ManagedSnesDsp).GetNestedType("Voice", BindingFlags.NonPublic)!;
        var voiceFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [voiceType])!;
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(voiceType, voiceFields, 22)
            .SequenceEqual(voiceFields.Where(field => field.Name != "ReleasedBrrCursor")),
            "legacy DSP voice retains envelope, PCM cursor, and interpolation state");
        var suitFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(SamusSuitPickupState)])!;
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusSuitPickupState), suitFields, 9)
            .SequenceEqual(suitFields.Where(field => field.Name is not "_transformationSoundPending" and not "<TransformationSoundSuppressed>k__BackingField")),
            "legacy suit pickup retains its saved transformation phase");
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusSuitPickupState), suitFields, 10)
            .SequenceEqual(suitFields.Where(field => field.Name != "<TransformationSoundSuppressed>k__BackingField")),
            "pre-guard suit pickup retains its pending sound and transformation phase");
        var projectileResultFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(SamusProjectileFrameResult)])!;
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusProjectileFrameResult), projectileResultFields, 5)
            .SequenceEqual(projectileResultFields.Where(field => field.Name is not "<AdditionalSoundRequests>k__BackingField"
                and not "<QueuedSoundSuppressed>k__BackingField"
                and not "<PersistentMemoryCorrupted>k__BackingField")),
            "legacy projectile result retains its saved sound and collision results");
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(
                typeof(SamusProjectileFrameResult), projectileResultFields, 7)
            .SequenceEqual(projectileResultFields.Where(field =>
                field.Name != "<PersistentMemoryCorrupted>k__BackingField")),
            "pre-SpaceTime projectile result retains every prior publication field");
        foreach (var (type, addedField) in new[] {
            (typeof(SamusSoundRequest), "<SoundSuppressed>k__BackingField"),
            (typeof(EnemySoundRequest), "<SoundSuppressed>k__BackingField"),
            (typeof(RoomFxSoundRequest), "<SoundSuppressed>k__BackingField"),
            (typeof(PaletteFxSoundRequest), "<SoundSuppressed>k__BackingField"),
            (typeof(SuperMetroid.Core.Rooms.PlmSoundRequest), "<SoundSuppressed>k__BackingField"),
            (typeof(HudState), "<SelectionSoundSuppressedThisFrame>k__BackingField"),
            (typeof(SamusBombProjectileSystem), "<SoundSuppressedBeforeProjectileHandling>k__BackingField") })
        {
            var suppressionFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
                BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [type])!;
            AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(type, suppressionFields, suppressionFields.Length - 1)
                .SequenceEqual(suppressionFields.Where(field => field.Name != addedField)),
                $"legacy {type.Name} preserves every existing field before suppression metadata");
        }
        var projectileSlotFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(SamusProjectileSlot)])!;
        var shineFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(SamusShinesparkState)])!;
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusShinesparkState), shineFields, shineFields.Length - 3)
            .SequenceEqual(shineFields.Where(field => field.Name is not "<StoredShineWarningSoundSuppressed>k__BackingField"
                and not "<LaunchSoundSuppressed>k__BackingField" and not "<CrashSoundSuppressed>k__BackingField")),
            "legacy shinespark retains native timers and pending sounds before suppression metadata");
        var xrayFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(SamusXrayState)])!;
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusXrayState), xrayFields,
                xrayFields.Length - 3).SequenceEqual(xrayFields.Where(field => field.Name is not
                    "<PendingActivationPose>k__BackingField" and not "<OwnsSamusControl>k__BackingField"
                    and not "<SuspendedSubsystems>k__BackingField")),
            "0.2.0 X-Ray layout omits activation-pose, control, and subsystem ownership fields");
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusXrayState), xrayFields,
                xrayFields.Length - 2).SequenceEqual(xrayFields.Where(field => field.Name is not
                    "<OwnsSamusControl>k__BackingField" and not "<SuspendedSubsystems>k__BackingField")),
            "legacy X-Ray preserves its HDMA, freeze, phase, and palette state before explicit owners");
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusXrayState), xrayFields,
                xrayFields.Length - 1).SequenceEqual(xrayFields.Where(field =>
                    field.Name != "<SuspendedSubsystems>k__BackingField")),
            "immediately previous X-Ray layout omits only subsystem-disable ownership");
        var legacyXray = new SamusXrayState();
        typeof(SamusXrayState).GetField("<IsActive>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(legacyXray, true);
        typeof(SamusXrayState).GetField("<TimeIsFrozen>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(legacyXray, true);
        DebuggerStateFieldMigrations.InitializeMissingFields(legacyXray, xrayFields.Length - 2);
        AssertTrue(legacyXray.OwnsSamusControl,
            "legacy active frozen X-Ray restores its dedicated Samus handlers");
        AssertEqual(XraySuspendedSubsystems.All, legacyXray.SuspendedSubsystems,
            "legacy active X-Ray reconstructs all four native subsystem disables");
        var legacyInactiveXray = new SamusXrayState();
        DebuggerStateFieldMigrations.InitializeMissingFields(legacyInactiveXray, xrayFields.Length - 2);
        AssertTrue(!legacyInactiveXray.OwnsSamusControl,
            "legacy inactive X-Ray does not invent Samus handler ownership");
        AssertEqual(XraySuspendedSubsystems.None, legacyInactiveXray.SuspendedSubsystems,
            "legacy inactive X-Ray does not invent suspended subsystems");
        var draygonGrabFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(SamusDraygonGrabbedState)])!;
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusDraygonGrabbedState),
                draygonGrabFields, draygonGrabFields.Length - 1).SequenceEqual(draygonGrabFields.Where(field =>
                    field.Name != "<MovementHandlerReplaced>k__BackingField")),
            "0.2.0 Draygon-grab layout omits only explicit movement-handler ownership");
        var layer3FxFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(RoomLayer3FxState)])!;
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(RoomLayer3FxState), layer3FxFields,
                layer3FxFields.Length - 1).SequenceEqual(layer3FxFields.Where(field =>
                    field.Name != "lavaAcidBg3PreInstructionInstalled")),
            "0.2.0 room-FX layout omits only the BG3 pre-instruction latch");
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusProjectileSlot), projectileSlotFields, 19)
            .SequenceEqual(projectileSlotFields.Where(field => field.Name != "<AuxiliaryPhase>k__BackingField")),
            "legacy projectile slot retains the actual projectile type and trajectory");
        var projectileFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(SamusProjectileSystem)])!;
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusProjectileSystem), projectileFields, 16)
            .SequenceEqual(projectileFields.Where(field => field.Name != "<ComboState>k__BackingField")),
            "legacy projectile state retains slots, charge, trails, and timers");
        var kinematicsFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(SamusKinematicsState)])!;
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusKinematicsState), kinematicsFields, 22)
            .SequenceEqual(kinematicsFields.Where(field => field.Name != "<ProbeContactDamageIndex>k__BackingField")),
            "legacy kinematics retains owner and all fixed-point movement words");
        var plmSlotType = typeof(SuperMetroid.Core.Rooms.RoomPlmSystem).GetNestedType("PlmSlot", BindingFlags.NonPublic)!;
        var plmFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [plmSlotType])!;
        FieldInfo[] oldPlmFields = plmFields.Where(field => field.Name is not "<PlantHeldX>k__BackingField" and not
            "<PlantHeldY>k__BackingField").ToArray();
        AssertEqual(20, oldPlmFields.Length, "preserved pre-plant-capture PLM slot count");
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(plmSlotType, plmFields, 20).SequenceEqual(oldPlmFields),
            "legacy PLM slot preserves active header, instructions, timers, and block owner fields");
        var gameFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [gameType])!;
        FieldInfo[] preSpacetimeGameFields = gameFields.Where(field =>
            field.Name != "spacetimeIntroRestartSlot").ToArray();
        AssertEqual(48, preSpacetimeGameFields.Length,
            "preserved pre-SpaceTime frontend field count");
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(
                gameType, gameFields, 48).SequenceEqual(preSpacetimeGameFields),
            "pre-SpaceTime frontend preserves every prior saved field in order");
        FieldInfo[] preRandomGameFields = gameFields.Where(field =>
            field.Name is not "menuRandom" and not "spacetimeIntroRestartSlot").ToArray();
        AssertEqual(47, preRandomGameFields.Length, "preserved pre-menu-RNG frontend field count");
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(gameType, gameFields, 47).SequenceEqual(preRandomGameFields),
            "pre-menu-RNG frontend preserves every saved field in order");
        FieldInfo[] oldGameFields = gameFields.Where(field => field.Name is not
            "pauseFadeCounter" and not "menuRandom" and not "spacetimeIntroRestartSlot").ToArray();
        AssertEqual(46, oldGameFields.Length, "preserved #391 frontend field count");
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(gameType, gameFields, 46).SequenceEqual(oldGameFields),
            "pre-pause-cadence frontend preserves every saved field in order");

        var enemyFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(RoomEnemySystem)])!;
        foreach (bool beforeStatueFields in new[] { false, true })
        {
            FieldInfo[] oldEnemyFields = enemyFields.Where(field => field.Name != "_samusProjectilesForEnemyFrame" &&
                (!beforeStatueFields || field.Name is not "<TourianEntranceStatueVerticalOffset>k__BackingField" and not
                    "<TourianStatueWaterY>k__BackingField")).ToArray();
            AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(RoomEnemySystem), enemyFields,
                    oldEnemyFields.Length).SequenceEqual(oldEnemyFields),
                "legacy enemy owner composes projectile-context and statue migrations without reordering fields");
        }

        static string LegacyIdentity(Type type) => $"{type.FullName}, {type.Assembly.GetName().Name}, Version=0.2.1.0, Culture=neutral, PublicKeyToken=null";
        static Type Resolve(string name) => DebuggerStateTypeIdentity.Resolve(name) ?? throw new InvalidDataException(name);
    }

    private static void VerifyLegacyRoomVisualLayoutState()
    {
        var type = typeof(RoomLevelData);
        var fields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer)
            .GetMethod("GetSerializableFields", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [type])!;
        FieldInfo[] selected = DebuggerStateFieldMigrations.SelectSerializedFields(
            type, fields, fields.Length - 2);
        AssertTrue(selected.Length == fields.Length - 2 &&
            selected.All(field => field.Name is not
                "_visualStreamingForegroundAllocation" and not
                "_visualStreamingBackgroundAllocation"),
            "pre-layout debugger state selects exactly its prior room fields");
        var level = new RoomLevelData(1, 1, [0x8000], [0], [0x8000], new byte[8]);
        foreach (string name in new[] { "_visualStreamingForegroundAllocation",
                     "_visualStreamingBackgroundAllocation" })
            type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(level, null);
        DebuggerStateFieldMigrations.InitializeMissingFields(level, fields.Length - 2);
        _ = level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(0, 0);
        AssertTrue(type.GetField("_visualStreamingForegroundAllocation",
                    BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(level) is ushort[] &&
            type.GetField("_visualStreamingBackgroundAllocation",
                    BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(level) is ushort[],
            "pre-layout debugger state restores native visual streams without a ROM read");
    }

    /// <summary>
    /// Reconstructs a valid field payload in the opposite order to prove that state
    /// compatibility follows serialized field identity, not compiler metadata order.
    /// </summary>
    private static void VerifyFieldIdentityRestorationIgnoresMetadataOrder()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(DebuggerGraphWireDefinitions.NewObjectMarker);
            writer.Write(1);
            writer.Write(DebuggerStateTypeIdentity.GetSerializedName(typeof(DebuggerFieldOrderProbe)));
            writer.Write(DebuggerGraphWireDefinitions.FieldsPayloadKind);
            writer.Write(2);
            WriteIntField(writer, nameof(DebuggerFieldOrderProbe.Second), 22);
            WriteIntField(writer, nameof(DebuggerFieldOrderProbe.First), 11);
        }
        stream.Position = 0;
        DebuggerFieldOrderProbe restored = DebuggerObjectGraphSerializer.Deserialize<DebuggerFieldOrderProbe>(stream);
        AssertEqual(11, restored.First, "debugger state restores the first field by identity after reordering");
        AssertEqual(22, restored.Second, "debugger state restores the second field by identity after reordering");

        static void WriteIntField(BinaryWriter writer, string fieldName, int value)
        {
            writer.Write(DebuggerStateTypeIdentity.GetSerializedName(typeof(DebuggerFieldOrderProbe)));
            writer.Write(fieldName);
            writer.Write(DebuggerGraphWireDefinitions.NewObjectMarker);
            writer.Write(0); // Value types do not receive reference IDs.
            writer.Write(DebuggerStateTypeIdentity.GetSerializedName(typeof(int)));
            writer.Write(DebuggerGraphWireDefinitions.PrimitivePayloadKind);
            writer.Write(value);
        }
    }

    private static SamusState DebuggerSignatureProbe(SamusState state) => state;

    private sealed class DebuggerFieldOrderProbe
    {
        public int First = 0;
        public int Second = 0;
    }
}

/// <summary>Stable wire identifiers emitted by the debugger object-graph serializer.</summary>
internal static class DebuggerGraphWireDefinitions
{
    /// <summary>Introduces an object/value not previously emitted in the graph.</summary>
    public const byte NewObjectMarker = 2;

    /// <summary>Identifies a primitive-value payload.</summary>
    public const byte PrimitivePayloadKind = 0;

    /// <summary>Identifies an object payload serialized as named instance fields.</summary>
    public const byte FieldsPayloadKind = 5;
}
