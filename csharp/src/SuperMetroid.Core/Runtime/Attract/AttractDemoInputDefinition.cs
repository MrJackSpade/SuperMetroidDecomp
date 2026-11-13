using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>A title demonstration's input (#1300, #1554): the recording it plays and what its playback first watches for.</summary>
/// <param name="FirstWatch">What the playback watches for before its first step.</param>
/// <param name="Animation">The recording the playback starts.</param>
public readonly record struct AttractDemoInputDefinition(AttractDemoInputWatch FirstWatch, DemoInputRecordingId Animation);
