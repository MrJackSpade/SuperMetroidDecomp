using System.Text.Json.Serialization;
using SuperMetroid.Android.Diagnostics;

namespace SuperMetroid.Android;

/// <summary>
/// The source-generated JSON metadata of every document the Android host writes or reads (#2602), indented as the host
/// writes them; no type is serialized by reflection.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AndroidSessionData.RecordingSeed))]
[JsonSerializable(typeof(AndroidDiagnosticBundle.BundleManifest))]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal sealed partial class AndroidJsonContext : JsonSerializerContext;
