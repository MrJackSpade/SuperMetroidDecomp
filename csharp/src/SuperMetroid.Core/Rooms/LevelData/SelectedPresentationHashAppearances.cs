using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Hashes block appearances into a selected presentation's identity (#1469).</summary>
internal static class SelectedPresentationHashAppearances
{
    /// <summary>Appearance hashing for <see cref="SelectedPresentationHash"/>.</summary>
    extension(SelectedPresentationHash hash)
    {
        /// <summary>
        /// Block appearances hash as each block number followed by the mirroring's name, so the hash names what a
        /// block shows rather than how a file or the cartridge stores it.
        /// </summary>
        public void AppendAppearances(string label, ReadOnlySpan<RoomBlockAppearance> appearances)
        {
            hash.Append(label, appearances.Length);
            foreach (RoomBlockAppearance appearance in appearances)
            {
                hash.Append("block", appearance.Block);
                hash.AppendName("mirroring", appearance.Mirroring.ToString());
            }
        }
    }
}
