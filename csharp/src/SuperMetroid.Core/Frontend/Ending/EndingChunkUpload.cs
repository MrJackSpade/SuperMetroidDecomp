namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A transfer of one chunk of installed artwork: a flyaway chunk, a reward chunk or a post-shot transfer.</summary>
/// <param name="Kind">The chunked transfer.</param>
/// <param name="Chunk">The chunk the transfer names.</param>
internal sealed record EndingChunkUpload(EndingUploadKind Kind, int Chunk) : EndingUpload(Kind);
