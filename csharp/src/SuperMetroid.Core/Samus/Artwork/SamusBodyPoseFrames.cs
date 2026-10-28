using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Core.Samus.Artwork.SamusBodyPoseDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The first body frame each pose draws by default.</summary>
internal static class SamusBodyPoseFrames
{
    /// <summary>The first frame of each of the 253 real poses' lists; FD..FF are outside the installed body domain.</summary>
    internal static int DefaultFirstFrame(SamusPoseId pose) =>
        DefaultFirstFrames.TryGetValue(pose, out int value) ? value : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");
}
