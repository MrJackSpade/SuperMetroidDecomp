using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The page file select has selected and the slots its prompt names (#1467, #2553): a copy's confirmation and completion
/// pages name the copy's two slots, the copy-destination and clear pages name the slot they act on, and every other page
/// names none, so a page holds a slot only where it shows one.
/// </summary>
internal abstract partial class SelectedFileSelectPage
{
    /// <summary>Allows only the pages below.</summary>
    private SelectedFileSelectPage(FileSelectPage page) => Page = page;

    /// <summary>The page.</summary>
    internal FileSelectPage Page { get; }

    /// <summary>The slot letters the page's prompt shows, and where.</summary>
    internal abstract IReadOnlyList<FileSelectPromptLetter> Letters { get; }

    /// <summary>The main page with its Copy and Clear entries.</summary>
    internal static SelectedFileSelectPage MainWithData { get; } = new NamingNoSlot(FileSelectPage.MainWithData);

    /// <summary>The main page without its Copy and Clear entries, as every slot is empty.</summary>
    internal static SelectedFileSelectPage MainEmpty { get; } = new NamingNoSlot(FileSelectPage.MainEmpty);

    /// <summary>The page choosing a copy's source.</summary>
    internal static SelectedFileSelectPage CopySource { get; } = new NamingNoSlot(FileSelectPage.CopySource);

    /// <summary>The page choosing the slot to clear.</summary>
    internal static SelectedFileSelectPage ClearSelection { get; } = new NamingNoSlot(FileSelectPage.ClearSelection);

    /// <summary>The page choosing where to copy <paramref name="source"/>, which it names.</summary>
    internal static SelectedFileSelectPage CopyDestination(SaveSlot source) =>
        new NamingSource(FileSelectPage.CopyDestination, FileSelectAnchor.CopyDestinationSource, source);

    /// <summary>The page confirming the clear of <paramref name="slot"/>, which it names.</summary>
    internal static SelectedFileSelectPage ClearConfirm(SaveSlot slot) =>
        new NamingSource(FileSelectPage.ClearConfirm, FileSelectAnchor.ClearConfirmSource, slot);

    /// <summary>The page showing <paramref name="slot"/> cleared, which it names.</summary>
    internal static SelectedFileSelectPage ClearCompleted(SaveSlot slot) =>
        new NamingSource(FileSelectPage.ClearCompleted, FileSelectAnchor.ClearConfirmSource, slot);

    /// <summary>The page confirming the copy <paramref name="route"/>, which names its two slots.</summary>
    internal static SelectedFileSelectPage CopyConfirm(FileCopyRoute route) => new NamingCopy(FileSelectPage.CopyConfirm, route);

    /// <summary>The page showing the copy <paramref name="route"/> done, which names its two slots.</summary>
    internal static SelectedFileSelectPage CopyCompleted(FileCopyRoute route) => new NamingCopy(FileSelectPage.CopyCompleted, route);

    /// <summary>A page whose prompt names no slot.</summary>
    private sealed partial class NamingNoSlot : SelectedFileSelectPage
    {
        /// <summary>Creates <paramref name="page"/>, naming no slot.</summary>
        /// <param name="page">The page.</param>
        internal NamingNoSlot(FileSelectPage page)
            : base(page)
        {
        }

        /// <inheritdoc/>
        internal override IReadOnlyList<FileSelectPromptLetter> Letters => [];
    }

    /// <summary>A page whose prompt names the slot it acts on.</summary>
    private sealed partial class NamingSource : SelectedFileSelectPage
    {
        /// <summary>Creates <paramref name="page"/>, naming <paramref name="slot"/> at <paramref name="anchor"/>.</summary>
        /// <param name="page">The page.</param>
        /// <param name="anchor">Where the prompt shows the slot's letter.</param>
        /// <param name="slot">The slot the page acts on.</param>
        internal NamingSource(FileSelectPage page, FileSelectAnchor anchor, SaveSlot slot)
            : base(page) => Letters = [new(anchor, slot)];

        /// <inheritdoc/>
        internal override IReadOnlyList<FileSelectPromptLetter> Letters { get; }
    }

    /// <summary>A page whose prompt names a copy's source and destination.</summary>
    private sealed partial class NamingCopy : SelectedFileSelectPage
    {
        /// <summary>Creates <paramref name="page"/>, naming the two slots of <paramref name="route"/>.</summary>
        /// <param name="page">The page.</param>
        /// <param name="route">The copy whose source and destination the prompt names.</param>
        internal NamingCopy(FileSelectPage page, FileCopyRoute route)
            : base(page) =>
            Letters =
            [
                new(FileSelectAnchor.CopyConfirmSource, FileCopyRoutes.SourceOf(route)),
                new(FileSelectAnchor.CopyConfirmDestination, FileCopyRoutes.DestinationOf(route)),
            ];

        /// <inheritdoc/>
        internal override IReadOnlyList<FileSelectPromptLetter> Letters { get; }
    }
}
