using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyDebuggerVersionCompatibility()
    {
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

        static string LegacyIdentity(Type type) => $"{type.FullName}, {type.Assembly.GetName().Name}, Version=0.2.1.0, Culture=neutral, PublicKeyToken=null";
        static Type Resolve(string name) => DebuggerStateTypeIdentity.Resolve(name) ?? throw new InvalidDataException(name);
    }

    private static SamusState DebuggerSignatureProbe(SamusState state) => state;
}
