using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// A title demonstration's input (#1300, #1554): the recording it plays, and the recording it continues into on its first
/// step that plays, when it has one (#2385).
/// </summary>
/// <param name="Animation">The recording the playback starts.</param>
/// <param name="Continuation">
/// The recording the playback switches to on its first step that plays, the diagonal shinespark's release (#2190); null for a
/// demonstration that plays its one recording to the end.
/// </param>
public readonly record struct AttractDemoInputDefinition(DemoInputRecordingId Animation, DemoInputRecordingId? Continuation = null);
