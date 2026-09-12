using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyDebuggerVersionCompatibility()
    {
        VerifyLegacyShinesparkGraph();
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
            .SequenceEqual(suitFields.Where(field => field.Name != "_transformationSoundPending")),
            "legacy suit pickup retains its saved transformation phase");
        var projectileResultFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(SamusProjectileFrameResult)])!;
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusProjectileFrameResult), projectileResultFields, 5)
            .SequenceEqual(projectileResultFields.Where(field => field.Name != "<AdditionalSoundRequests>k__BackingField")),
            "legacy projectile result retains its saved sound and collision results");
        var projectileSlotFields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [typeof(SamusProjectileSlot)])!;
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
        FieldInfo[] oldGameFields = gameFields.Where(field => field.Name != "pauseFadeCounter").ToArray();
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

    private static SamusState DebuggerSignatureProbe(SamusState state) => state;
}
