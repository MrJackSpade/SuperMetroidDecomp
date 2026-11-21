using System.Text.Json.Serialization;

namespace SuperMetroid.DesktopVerification;

/// <summary>
/// The source-generated JSON metadata of every type SuperMetroid.DesktopVerification's documents and reports serialize (#2602), and of
/// every type they reach, each under its own name; <see cref="DesktopVerificationJson"/> binds a call's options to it.
/// </summary>
[JsonSerializable(typeof(global::SuperMetroid.DesktopVerification.Soak.DesktopTimerSoakTests.TimerSoakReport), TypeInfoPropertyName = "SuperMetroid_DesktopVerification_Soak_DesktopTimerSoakTests_TimerSoakReport")]
[JsonSerializable(typeof(global::SuperMetroid.DesktopVerification.Soak.HiddenSoakTests.HiddenSoakReport), TypeInfoPropertyName = "SuperMetroid_DesktopVerification_Soak_HiddenSoakTests_HiddenSoakReport")]
internal sealed partial class DesktopVerificationJsonContext : JsonSerializerContext;
