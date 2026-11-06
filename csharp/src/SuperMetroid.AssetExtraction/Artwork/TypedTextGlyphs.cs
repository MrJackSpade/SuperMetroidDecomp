using SuperMetroid.Core.Spatial;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The glyphs a typed text's steps add (#1513): each step shows every part of the step before it, moved by the text's
/// scroll, and the parts of one more glyph, so each glyph is the parts its step adds, placed from the text's origin as the
/// step placed them; a glyph is drawn moved by the scroll once for each step typed after it. A step that adds nothing types
/// a space.
/// </summary>
internal static class TypedTextGlyphs
{
    /// <summary>The parts each of <paramref name="steps"/> adds to the step before it, in typing order.</summary>
    /// <param name="text">The text's name, which a failure names.</param>
    /// <param name="steps">The text's steps in typing order, each the whole of what it shows.</param>
    /// <param name="scroll">How far each step moves the parts of the step before it; zero for a text typed in place.</param>
    /// <exception cref="InvalidDataException">A step does not show every part of the step before it moved by the scroll, so it is no typed step.</exception>
    internal static SpriteFramePart[][] Of(string text, IReadOnlyList<SpriteFrame> steps, Offset16<ScreenPixels> scroll)
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
                throw new InvalidDataException($"Step {step} of typed text {text} leaves out {before.Count} parts of the step before it, moved by its scroll.");
            glyphs[step] = [.. added];
            before = [.. steps[step].Parts.Select(part => part with { Offset = part.Offset + scroll })];
        }
        return glyphs;
    }
}
