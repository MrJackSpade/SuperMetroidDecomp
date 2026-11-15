using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="StationAnimationDefinitions"/> members production does not use.</summary>
internal static class StationAnimationProgramDefinitionsAccess
{
    /// <summary>Pairs station animations with every image's native duration and draw word.</summary>
    extension(StationAnimationDefinitions)
    {
        /// <summary>Visits each station image's duration and draw pointer in the cartridge's order.</summary>
        /// <exception cref="InvalidOperationException">A station animation's beat is not an image held for a time.</exception>
        internal static IEnumerable<NativeWord> NativeWords()
        {
            foreach (StationAnimation animation in Enum.GetValues<StationAnimation>())
            {
                IReadOnlyList<AnimationBeat<StationDraw, NoAnimationCue>> beats = StationAnimationDefinitions.Segments.Beats(animation);
                for (int beat = 0; beat < beats.Count; beat++)
                {
                    if (beats[beat] is not AnimationBeat<StationDraw, NoAnimationCue>.Frame frame)
                        throw new InvalidOperationException($"Station animation {animation} beat {beat} is not an image held for a time.");
                    ushort record = StationNativeWords.Record(animation, beat);
                    yield return new NativeWord(record, checked((ushort)frame.Duration));
                    yield return new NativeWord(checked((ushort)(record + 2)), RoomObjectDrawAddressCodec.Of(StationObjectDraws.Family.Of(frame.Shown)));
                }
            }
        }
    }
}
