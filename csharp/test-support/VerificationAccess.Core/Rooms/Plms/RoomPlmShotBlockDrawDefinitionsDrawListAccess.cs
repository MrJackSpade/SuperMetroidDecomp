using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmShotBlockDrawDefinitions.DrawList"/> members production does not use.</summary>
internal static class RoomPlmShotBlockDrawDefinitionsDrawListAccess
{
    /// <summary>Exposes a shot-block draw list's decoded visual words.</summary>
    extension(RoomPlmShotBlockDrawDefinitions.DrawList self)
    {
        /// <summary>A new array of every run's visual words, in run then word order.</summary>
        internal ushort[] VisualWords()
        {
            int count = 0;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in self.Runs.Span)
                count += run.LevelWords.Length;
            var words = new ushort[count];
            int next = 0;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in self.Runs.Span)
            {
                foreach (ushort word in run.LevelWords.Span)
                    words[next++] = new RoomLevelWord(word).VisualWord;
            }
            return words;
        }
    }
}
