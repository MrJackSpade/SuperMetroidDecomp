using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.TestSupport.SamusNativeCodecs;

/// <summary>
/// Reads the arm cannon's cover picture from the native frame word a snapshot records, for snapshot imports and
/// comparisons: zero is the closed cover and one through three the pictures the import's cover codec numbers.
/// </summary>
internal static class SamusArmCannonCoverNativeCodec
{
    /// <summary>The cover picture a recorded frame word holds, or null for the closed cover.</summary>
    /// <exception cref="InvalidDataException">The word is no frame a snapshot between updates records.</exception>
    internal static SamusArmCannonCoverFrame? FromNative(int word)
    {
        if (word == 0)
            return null;
        foreach (SamusArmCannonCoverFrame frame in SamusArmCannonCoverFrameCodec.InOpeningOrder)
        {
            if (SamusArmCannonCoverFrameCodec.CartridgeFrame(frame) == word)
                return frame;
        }

        throw new InvalidDataException($"Arm cannon frame word {word:X4} is no cover picture a snapshot between updates records.");
    }
}
