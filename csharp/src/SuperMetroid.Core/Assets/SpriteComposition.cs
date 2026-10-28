using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable ordered visual parts. No collision, attack or attachment mechanics are stored here.</summary>
public sealed class SpriteComposition
{
    /// <summary>Ordered immutable parts, including per-part palette inheritance choices.</summary>
    internal readonly IReadOnlyList<CompiledSpritePart> parts;
    /// <summary>Copies authored parts so later document edits cannot alter a compiled composition.</summary>
    internal SpriteComposition(CompiledSpritePart[] parts) => this.parts = (CompiledSpritePart[])parts.Clone();
    /// <summary>Retains an already immutable calculated view without another array copy.</summary>
    private SpriteComposition(IReadOnlyList<CompiledSpritePart> parts) => this.parts = parts;
    /// <summary>Number of parts drawn in composition order.</summary>
    [IntegerQuantity("number of sprite parts in the composition")]
    internal int PartCount => parts.Count;
    /// <summary>Selects one part by its drawing ordinal.</summary>
    internal CompiledSpritePart Part(int index) => parts[index];

    /// <summary>Use an immutable calculated view only when every supplied visual field matches.
    /// Independently edited compositions retain their compiled parts.</summary>
    internal SpriteComposition CalculateIfMatching(IReadOnlyList<CompiledSpritePart> calculated) =>
        parts.SequenceEqual(calculated) ? new SpriteComposition(calculated) : this;

    /// <summary>Hashes only compiled visual fields, preserving the draw order and palette inheritance.</summary>
    internal void AppendIdentity(SelectedPresentationHash content)
    {
        content.Append("parts", parts.Count);
        foreach (CompiledSpritePart part in parts)
        {
            part.Part.AppendIdentity(content);
            content.Append("inherit-palette", part.InheritPalette ? 1 : 0);
        }
    }
    /// <summary>
    /// The composition as a whole frame over artwork whose character 0 is sheet character <paramref name="firstCharacter"/>,
    /// on a screen drawing the <see cref="SpritePartFrames.StandardSizes"/>.
    /// </summary>
    internal SpriteFrame ToFrame(string name, int firstCharacter) => ToFrame(name, firstCharacter, SpritePartFrames.StandardSizes);

    /// <summary>
    /// The composition as a whole frame over artwork whose character 0 is sheet character <paramref name="firstCharacter"/>,
    /// on a screen whose objects draw at <paramref name="sizes"/>.
    /// </summary>
    internal SpriteFrame ToFrame(string name, int firstCharacter, ScreenObjectSizes sizes)
    {
        var converted = ImmutableArray.CreateBuilder<SpriteFramePart>(parts.Count);
        foreach (CompiledSpritePart part in parts)
            converted.Add(part.Part.ToFramePart(firstCharacter, sizes, part.InheritPalette));
        return new(name, converted.MoveToImmutable());
    }

}
