namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Identity of a compact replay trace directory (<c>compact-manifest.json</c>): its format, the
/// <see cref="ReplayTraceLayout"/> it was packed with, the source capture it came from and the
/// SHA-256 of each compact file.
/// </summary>
/// <param name="Format">Always <see cref="ReplayTrace.CompactFormat"/>.</param>
/// <param name="Layout">The layout name.</param>
/// <param name="LayoutSha256">The layout's <see cref="ReplayTraceLayout.Sha256"/>.</param>
/// <param name="PayloadBytes">Bytes of WRAM in each record's payload.</param>
/// <param name="RecordCount">Checkpoint records, the initial and terminal records included.</param>
/// <param name="InitialRecord">Index of the record kept whole in <c>initial.wram.gz</c>.</param>
/// <param name="MovieSha256">The source movie's SHA-256, as in <c>updates.json</c>.</param>
/// <param name="SourceCheckpointsSha256">SHA-256 of the full capture stream the trace was compacted from.</param>
/// <param name="UpdatesSha256">SHA-256 of <c>updates.json.gz</c>.</param>
/// <param name="InitialSha256">SHA-256 of <c>initial.wram.gz</c>.</param>
/// <param name="ExpectedSha256">SHA-256 of <c>expected.bin.gz</c>.</param>
internal sealed record CompactReplayTraceManifest(
    string Format, string Layout, string LayoutSha256, int PayloadBytes, int RecordCount, int InitialRecord,
    string MovieSha256, string SourceCheckpointsSha256, string UpdatesSha256, string InitialSha256, string ExpectedSha256);
