using System.Text.Json.Serialization;

namespace SuperMetroid.Diagnostics;

/// <summary>
/// The source-generated JSON metadata of every type SuperMetroid.Diagnostics's documents and reports serialize (#2602), and of
/// every type they reach, each under its own name; <see cref="DiagnosticsJson"/> binds a call's options to it.
/// </summary>
[JsonSerializable(typeof(global::SuperMetroid.Diagnostics.Saves.LegacyGameSaveJson.LegacySramPage), TypeInfoPropertyName = "SuperMetroid_Diagnostics_Saves_LegacyGameSaveJson_LegacySramPage")]
internal sealed partial class DiagnosticsJsonContext : JsonSerializerContext;
