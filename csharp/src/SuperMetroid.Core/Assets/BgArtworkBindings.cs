namespace SuperMetroid.Core.Assets;

/// <summary>
/// The background character artwork a frame shows (#1350, #1406): room tilesets, library backgrounds, collectibles,
/// animated tiles and the HUD's characters, bound by identity over the characters of the set each layer reads. A later
/// binding shows over an earlier one where they share characters of one set.
/// </summary>
public sealed class BgArtworkBindings
{
    /// <summary>The live bindings, oldest first.</summary>
    private readonly List<BgArtworkBinding> bindings = [];

    /// <summary>What each set's characters show, as a layer reading them asks.</summary>
    [NonSerialized] private Dictionary<BgCharacterSet, IBgCharacterArtwork>? views;

    /// <summary>
    /// Binds the first <paramref name="characterCount"/> characters of <paramref name="identity"/> over
    /// <paramref name="set"/>'s characters from <paramref name="firstCharacter"/> onward.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The characters do not lie inside the set there.</exception>
    public void Bind(BgArtworkIdentity identity, BgCharacterSet set, int firstCharacter, int characterCount)
    {
        if (characterCount <= 0 || firstCharacter < 0 || firstCharacter + characterCount > set.CharacterCount())
            throw new ArgumentOutOfRangeException(nameof(firstCharacter), firstCharacter,
                $"{characterCount} characters do not lie inside the {set} characters there.");
        var binding = new BgArtworkBinding(identity, set, firstCharacter, characterCount);
        // A binding a newer one covers entirely can no longer show.
        bindings.RemoveAll(older => older.Set == set && older.FirstCharacter >= binding.FirstCharacter &&
            older.FirstCharacter + older.CharacterCount <= binding.FirstCharacter + binding.CharacterCount);
        bindings.Add(binding);
    }

    /// <summary>What <paramref name="set"/>'s characters show, as a layer reading them asks.</summary>
    public IBgCharacterArtwork Of(BgCharacterSet set)
    {
        views ??= [];
        if (!views.TryGetValue(set, out IBgCharacterArtwork? view))
            views.Add(set, view = new SetArtwork(this, set));
        return view;
    }

    /// <summary>
    /// What shows over <paramref name="characterCount"/> of <paramref name="set"/>'s characters from
    /// <paramref name="firstCharacter"/> (#1392): the newest binding over them, which must cover them all, and how far into
    /// its artwork they begin.
    /// </summary>
    /// <exception cref="InvalidDataException">No binding covers the characters, or the newest over them covers only some.</exception>
    public BgArtworkShown Showing(BgCharacterSet set, int firstCharacter, int characterCount) =>
        TryShowing(set, firstCharacter, characterCount, out BgArtworkShown shown) ? shown
            : throw new InvalidDataException($"No background artwork is bound over {set} characters {firstCharacter}..{firstCharacter + characterCount - 1}.");

    /// <summary>What shows over the characters, as <see cref="Showing"/> finds it; false where no binding covers any of them.</summary>
    /// <exception cref="InvalidDataException">The newest binding over the characters covers only some.</exception>
    public bool TryShowing(BgCharacterSet set, int firstCharacter, int characterCount, out BgArtworkShown shown)
    {
        int end = firstCharacter + characterCount;
        for (int index = bindings.Count - 1; index >= 0; index--)
        {
            BgArtworkBinding binding = bindings[index];
            int bindingEnd = binding.FirstCharacter + binding.CharacterCount;
            if (binding.Set != set || binding.FirstCharacter >= end || bindingEnd <= firstCharacter)
                continue;
            if (binding.FirstCharacter > firstCharacter || bindingEnd < end)
                throw new InvalidDataException($"{binding.Identity.Name} shows over only some of {set} characters {firstCharacter}..{end - 1}.");
            shown = new BgArtworkShown(binding.Identity, (firstCharacter - binding.FirstCharacter) * set.CharacterBytes() / sizeof(ushort));
            return true;
        }
        shown = default;
        return false;
    }

    /// <summary>The live bindings, oldest first, for verification's composition of what they show.</summary>
    internal IReadOnlyList<BgArtworkBinding> Bound => bindings;

    /// <summary>What one set's characters show.</summary>
    /// <param name="bindings">The bindings.</param>
    /// <param name="set">The set.</param>
    private sealed class SetArtwork(BgArtworkBindings bindings, BgCharacterSet set) : IBgCharacterArtwork
    {
        /// <inheritdoc />
        public BgArtworkShown Showing(int firstCharacter, int characterCount) => bindings.Showing(set, firstCharacter, characterCount);
    }
}
