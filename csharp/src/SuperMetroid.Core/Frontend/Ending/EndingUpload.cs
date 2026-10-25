namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// One recorded ending transfer (#1358). Each kind of transfer is its own record holding only what distinguishes it: the
/// chunk, the subject or the words a tilemap transfer wrote. A later transfer that covers exactly the same VRAM words
/// replaces it.
/// </summary>
/// <param name="Kind">The transfer.</param>
internal abstract record EndingUpload(EndingUploadKind Kind)
{
    /// <summary>Whether <paramref name="later"/> covers exactly the VRAM words this transfer wrote.</summary>
    internal virtual bool IsReplacedBy(EndingUpload later) => Equals(Ensure.NotNull(later));

    /// <summary>This transfer as the record of type <typeparamref name="T"/> its kind is recorded as.</summary>
    /// <exception cref="InvalidOperationException">The transfer is not recorded as <typeparamref name="T"/>.</exception>
    internal T As<T>() where T : EndingUpload =>
        this as T ?? throw new InvalidOperationException($"The {Kind} transfer is not a {typeof(T).Name}.");
}
