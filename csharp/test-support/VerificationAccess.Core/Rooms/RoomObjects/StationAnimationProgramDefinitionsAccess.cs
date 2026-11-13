using SuperMetroid.AssetExtraction.Rooms;
namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="StationAnimationDefinitions"/> members production does not use.</summary>
internal static class StationAnimationProgramDefinitionsAccess
{
    /// <summary>Pairs station animation list identities with every frame's native duration and draw word.</summary>
    extension(StationAnimationDefinitions)
    {
        /// <summary>Enumerates the station animation list identities for the native-word audit.</summary>
        internal static IEnumerable<StationAnimationList> Lists() => Enum.GetValues<StationAnimationList>();

        /// <summary>Visits each station frame's duration and draw pointer in list order.</summary>
        internal static IEnumerable<NativeWord> NativeWords()
        {
            foreach (StationAnimationList list in StationAnimationDefinitions.Lists())
            for (int index = 0; index < StationAnimationDefinitions.Frames[list].Count; index++)
            {
                StationAnimationDefinitions.Frame frame = StationAnimations.Resolve(list, index);
                yield return new NativeWord(checked((ushort)(StationNativeWords.AnimationLists.Word(list) + 4 * index)), checked((ushort)frame.Duration));
                yield return new NativeWord(checked((ushort)(StationNativeWords.AnimationLists.Word(list) + 4 * index + 2)), RoomObjectDrawAddressCodec.Of(frame.Draw));
            }
        }
    }
}
