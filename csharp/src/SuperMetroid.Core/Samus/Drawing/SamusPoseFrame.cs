using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>One animation frame of one of Samus's poses.</summary>
/// <param name="Pose">The pose.</param>
/// <param name="Frame">The pose's animation frame.</param>
internal readonly record struct SamusPoseFrame(SamusPoseId Pose, int Frame);
