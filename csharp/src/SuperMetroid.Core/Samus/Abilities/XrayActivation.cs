using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>X-ray's setup taking control of Samus in a frame (#2562), as the input phase records it for the frame's later phases.</summary>
/// <param name="Pose">
/// The pose its activation installs, which the frame's pose update commits unless a higher-priority pose supersedes it; null
/// when the setup ended a stranded beam at once.
/// </param>
internal readonly partial record struct XrayActivation(SamusPoseId? Pose);
