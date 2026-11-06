using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The glyphs a typed text's steps add (#1513): each step shows every part of the step before it and the parts of one more
/// glyph, so each glyph is the parts its step adds, placed from the text's origin as the step placed them. A step that adds
/// nothing types a space.
/// </summary>
internal static class TypedTextGlyphs
{
    /// <summary>The parts each of <paramref name="steps"/> adds to the step before it, in typing order.</summary>
    /// <param name="text">The text's name, which a failure names.</param>
    /// <param name="steps">The text's steps in typing order, each the whole of what it shows.</param>
    /// <exception cref="InvalidDataException">A step does not show every part of the step before it, so it is no typed step.</exception>
    internal static SpriteFramePart[][] Of(string text, IReadOnlyList<SpriteFrame> steps)
    {
        Ensure.NotNull(steps);
        var glyphs = new SpriteFramePart[steps.Count][];
        List<SpriteFramePart> before = [];
        for (int step = 0; step < steps.Count; step++)
        {
            var added = new List<SpriteFramePart>();
            foreach (SpriteFramePart part in steps[step].Parts)
            {
                if (!before.Remove(part))
                    added.Add(part);
            }
            if (before.Count > 0)
                throw new InvalidDataException($"Step {step} of typed text {text} leaves out {before.Count} parts of the step before it.");
            glyphs[step] = [.. added];
            before = [.. steps[step].Parts];
        }
        return glyphs;
    }
}
