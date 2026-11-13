using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// The typed texts' steps in typing order (#1513), by which the import pictures each text's glyphs once: the step at each
/// place shows every glyph up to it.
/// </summary>
internal static class CinematicTypedTextSteps
{
    /// <summary>Listing the texts and their steps.</summary>
    extension(CinematicTypedTexts typed)
    {
        /// <summary>The texts typed, by their programs, in the order they were given.</summary>
        internal IEnumerable<CinematicSpriteSegment> Texts => typed.steps.Values.Select(step => step.Text).Distinct();

        /// <summary>The poses <paramref name="text"/> shows, in typing order: the step at each place shows every glyph up to it.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The program types no text here.</exception>
        internal IReadOnlyList<CinematicSpritePose> Steps(CinematicSpriteSegment text)
        {
            CinematicSpritePose[] poses = [.. typed.steps.Where(entry => entry.Value.Text == text)
                .OrderBy(entry => entry.Value.Glyph).Select(entry => entry.Key)];
            return poses.Length > 0 ? poses : throw new ArgumentOutOfRangeException(nameof(text), text, "Not a typed text.");
        }
    }
}
