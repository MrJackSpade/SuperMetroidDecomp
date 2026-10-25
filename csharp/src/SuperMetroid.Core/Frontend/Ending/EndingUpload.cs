namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// One recorded ending transfer (#1358): its kind, the chunk or subject that distinguishes transfers of that kind, and
/// the words a tilemap transfer wrote. A later transfer with the same kind, chunk and subject replaces it, since it
/// covers exactly the same VRAM words.
/// </summary>
/// <param name="Kind">The transfer.</param>
/// <param name="Key">The chunk the transfer names; zero when its kind has only one or names a subject instead.</param>
/// <param name="Words">The tilemap words the transfer wrote, or null for a transfer of installed artwork.</param>
/// <param name="Subject">The floor scene, explosion fragment or text tilemap the transfer names, or null when it names none.</param>
internal sealed record EndingUpload(EndingUploadKind Kind, int Key, ushort[]? Words, Enum? Subject = null)
{
    /// <summary>Whether <paramref name="later"/> covers exactly the VRAM words this transfer wrote.</summary>
    internal bool IsReplacedBy(EndingUpload later) =>
        Kind == Ensure.NotNull(later).Kind && Key == later.Key && Equals(Subject, later.Subject);

    /// <summary>The subject of type <typeparamref name="T"/> this transfer names.</summary>
    /// <exception cref="InvalidOperationException">The transfer names no such subject.</exception>
    internal T SubjectAs<T>() where T : struct, Enum =>
        Subject is T subject ? subject : throw new InvalidOperationException($"The {Kind} transfer names no {typeof(T).Name}.");
}
