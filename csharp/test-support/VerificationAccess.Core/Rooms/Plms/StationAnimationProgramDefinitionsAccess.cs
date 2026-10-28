using SuperMetroid.AssetExtraction.Rooms;
namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="StationAnimationProgramDefinitions"/> members production does not use.</summary>
internal static class StationAnimationProgramDefinitionsAccess
{
    /// <summary>Pairs station animation list identities with every frame's native duration and draw word.</summary>
    extension(StationAnimationProgramDefinitions)
    {
        /// <summary>Enumerates the station animation list identities for the native-word audit.</summary>
        internal static IEnumerable<StationAnimationList> Lists() => Enum.GetValues<StationAnimationList>();

        /// <summary>Visits each station frame's duration and draw pointer in list order.</summary>
        internal static IEnumerable<NativeWord> NativeWords()
        {
            foreach (StationAnimationList list in StationAnimationProgramDefinitions.Lists())
            for (int index = 0; index < StationAnimationPrograms.FrameCount(list); index++)
            {
                StationAnimationProgramDefinitions.Frame frame = StationAnimationPrograms.Resolve(list, index);
                yield return new NativeWord(checked((ushort)(StationNativeWords.AnimationLists.Word(list) + 4 * index)), checked((ushort)frame.Duration));
                yield return new NativeWord(checked((ushort)(StationNativeWords.AnimationLists.Word(list) + 4 * index + 2)), PlmDrawAddressCodec.Of(frame.Draw));
            }
        }
    }
}
