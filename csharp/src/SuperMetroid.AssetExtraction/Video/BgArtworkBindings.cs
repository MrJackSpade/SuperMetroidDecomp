namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// The background character artwork a room's load leaves (#1350, #1406, #1466): room tilesets, library backgrounds,
/// collectibles, animated tiles and the HUD's characters, bound by identity over the characters of the set each layer
/// reads, as the import pipeline replays what the cartridge copies. A later binding shows over an earlier one where they
/// share characters of one set. Gameplay names what it shows over a room's load by appearance inputs instead.
/// </summary>
public sealed class BgArtworkBindings
{
    /// <summary>The live bindings, oldest first.</summary>
    internal readonly List<BgArtworkBinding> bindings = [];

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

    /// <summary>
    /// What shows over <paramref name="characterCount"/> of <paramref name="set"/>'s characters from
    /// <paramref name="firstCharacter"/> (#1392): the newest binding over them, which must cover them all, and how far into
    /// its artwork they begin; false where no binding covers any of them.
    /// </summary>
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
}
