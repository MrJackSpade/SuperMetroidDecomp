using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Input;

/// <summary>A typed demo input that plays a run of identical consecutive native records (#2557).</summary>
/// <param name="Recording">The recording holding the input.</param>
/// <param name="Input">The input's place in the typed recording.</param>
/// <param name="Records">How many native records the input stands for.</param>
internal readonly record struct DemoInputNativeRun(DemoInputRecordingId Recording, int Input, int Records);
