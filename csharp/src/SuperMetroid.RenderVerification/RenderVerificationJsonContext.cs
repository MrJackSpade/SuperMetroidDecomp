using System.Text.Json.Serialization;

namespace SuperMetroid.RenderVerification;

/// <summary>
/// The source-generated JSON metadata of every type SuperMetroid.RenderVerification's documents and reports serialize (#2602), and of
/// every type they reach, each under its own name; <see cref="RenderVerificationJson"/> binds a call's options to it.
/// </summary>
internal sealed partial class RenderVerificationJsonContext : JsonSerializerContext;
