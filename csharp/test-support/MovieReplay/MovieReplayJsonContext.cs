using System.Text.Json.Serialization;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The source-generated JSON metadata of every type test-support/MovieReplay's documents and reports serialize (#2602), and of
/// every type they reach, each under its own name; <see cref="MovieReplayJson"/> binds a call's options to it.
/// </summary>
[JsonSerializable(typeof(global::SuperMetroid.TestSupport.MovieReplay.CompactReplayTraceManifest), TypeInfoPropertyName = "SuperMetroid_TestSupport_MovieReplay_CompactReplayTraceManifest")]
internal sealed partial class MovieReplayJsonContext : JsonSerializerContext;
