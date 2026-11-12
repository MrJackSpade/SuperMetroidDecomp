using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using System.Buffers.Binary;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction;

/// <summary>Verification access to <see cref="SelectedPresentationHash"/> members production does not use.</summary>
internal static class SelectedPresentationHashAccess
{
    /// <summary>Appends raw word rows with boundaries preserved, including empty rows.</summary>
    extension(SelectedPresentationHash hash)
    {
        /// <summary>Ordered raw word rows retain their boundaries, including empty rows: the independent oracle of <see cref="AppendColorFrames"/>.</summary>
        internal void AppendWordFrames(string label, IReadOnlyList<ushort[]> frames)
        {
            hash.Append(label, frames.Count);
            foreach (ushort[] frame in frames)
                hash.AppendWords("row", frame);
        }

        /// <summary>
        /// Adds native 16-bit words in little-endian order, identical to
        /// <see cref="SelectedPresentationHash.AppendWords(string, ReadOnlySpan{int})"/> of the same words: the checks'
        /// content-identity oracles hash the cartridge's words this way (#1595).
        /// </summary>
        internal void AppendWords(string label, ReadOnlySpan<ushort> words)
        {
            var bytes = new byte[checked(words.Length * sizeof(ushort))];
            for (int index = 0; index < words.Length; index++)
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * sizeof(ushort)), words[index]);
            hash.Append(label, bytes);
        }
    }

    /// <summary>Builds stable hashes for single runs, single blocks, and multiple runs per frame.</summary>
    extension(SelectedPresentationHash)
    {
        /// <summary>Wraps each appearance-array frame as one run before the canonical keyed hash.</summary>
        internal static string FromAppearanceFrames(string domain, IReadOnlyDictionary<string, RoomBlockAppearance[]> frames) =>
            SelectedPresentationHash.FromAppearanceFrames(domain, frames.ToDictionary(pair => pair.Key, pair => new[] { pair.Value }));

        /// <summary>Wraps each single-block frame as a one-block run before the canonical keyed hash.</summary>
        internal static string FromAppearanceFrames(string domain, IReadOnlyDictionary<string, RoomBlockAppearance> frames) =>
            SelectedPresentationHash.FromAppearanceFrames(domain, frames.ToDictionary(pair => pair.Key, pair => new[] { pair.Value }));

        /// <summary>
        /// Preserves draw-run and block order while canonicalizing frame order: frames are keyed by their artwork identity
        /// and hashed in identity order, as the PLM visual catalogs frame them.
        /// </summary>
        internal static string FromAppearanceFrames(string domain, IReadOnlyDictionary<string, RoomBlockAppearance[][]> frames) =>
            SelectedPresentationHash.Create(domain, content =>
            {
                foreach ((string id, RoomBlockAppearance[][] runs) in frames.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                {
                    content.AppendName("frame", id);
                    content.Append("runs", runs.Length);
                    foreach (RoomBlockAppearance[] run in runs)
                        content.AppendAppearances("blocks", run);
                }
            });
    }

    /// <summary>Exposes <see cref="SelectedPresentationHash"/> state that only verification reads.</summary>
    extension(SelectedPresentationHash self)
    {
        /// <summary>Ordered color rows retain their boundaries, including empty rows; each row hashes its encoded words.</summary>
        public void AppendColorFrames(string label, IReadOnlyList<RgbColor[]> frames)
        {
            self.Append(label, frames.Count);
            foreach (RgbColor[] frame in frames)
                self.AppendColors("row", frame);
        }
        /// <summary>Null fixture domains differ from installed content without storing a derived hash.</summary>
        public void AppendIdentity(string label, string? identity) =>
            self.Append(label, identity is null ? ReadOnlySpan<byte>.Empty : Convert.FromHexString(identity));
    }
}
