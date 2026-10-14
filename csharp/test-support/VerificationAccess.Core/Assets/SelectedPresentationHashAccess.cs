using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="SelectedPresentationHash"/> members production does not use.</summary>
internal static class SelectedPresentationHashAccess
{
    /// <summary>Appends raw word rows with boundaries preserved, including empty rows.</summary>
    extension(SelectedPresentationHash hash)
    {
        /// <summary>Ordered raw word rows retain their boundaries, including empty rows: the independent oracle of <see cref="SelectedPresentationHash.AppendColorFrames"/>.</summary>
        internal void AppendWordFrames(string label, IReadOnlyList<ushort[]> frames)
        {
            hash.Append(label, frames.Count);
            foreach (ushort[] frame in frames)
                hash.AppendWords("row", frame);
        }
    }

    /// <summary>Builds stable hashes for single runs, scalar words, and multiple runs per frame.</summary>
    extension(SelectedPresentationHash)
    {
        /// <summary>Wraps each word-array frame as one run before the canonical keyed hash.</summary>
        internal static string FromWordFrames(string domain, IReadOnlyDictionary<ushort, ushort[]> frames) =>
            SelectedPresentationHash.FromWordFrames(domain, frames.ToDictionary(pair => pair.Key, pair => new[] { pair.Value }));

        /// <summary>Wraps each scalar frame as a one-word run before the canonical keyed hash.</summary>
        internal static string FromWordFrames(string domain, IReadOnlyDictionary<ushort, ushort> frames) =>
            SelectedPresentationHash.FromWordFrames(domain, frames.ToDictionary(pair => pair.Key, pair => new[] { pair.Value }));

        /// <summary>Preserves draw-run and word order while canonicalizing frame-key insertion order.</summary>
        internal static string FromWordFrames(string domain, IReadOnlyDictionary<ushort, ushort[][]> frames) =>
            SelectedPresentationHash.Create(domain, content =>
            {
                foreach ((ushort pointer, ushort[][] runs) in frames.OrderBy(pair => pair.Key))
                {
                    content.Append("frame", pointer);
                    content.Append("runs", runs.Length);
                    foreach (ushort[] run in runs)
                        content.AppendWords("words", run);
                }
            });
    }
}
