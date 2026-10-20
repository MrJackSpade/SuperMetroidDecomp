using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable ordered visual parts. No collision, attack or attachment mechanics are stored here.</summary>
public sealed class SpriteComposition
{
    /// <summary>Ordered immutable OAM parts, including per-part palette inheritance choices.</summary>
    internal readonly IReadOnlyList<CompiledSpritePart> parts;
    /// <summary>Copies authored parts so later document edits cannot alter a compiled composition.</summary>
    internal SpriteComposition(CompiledSpritePart[] parts) => this.parts = (CompiledSpritePart[])parts.Clone();
    /// <summary>Retains an already immutable calculated view without another array copy.</summary>
    private SpriteComposition(IReadOnlyList<CompiledSpritePart> parts) => this.parts = parts;
    /// <summary>Wraps an immutable calculated part view. The caller must supply an immutable view, never a mutable document/list.</summary>
    internal static SpriteComposition FromCalculated(IReadOnlyList<CompiledSpritePart> parts) => new(parts);
    /// <summary>Number of OAM parts emitted in composition order.</summary>
    internal int PartCount => parts.Count;
    /// <summary>Selects one compiled OAM part by its drawing ordinal.</summary>
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
            content.Append("x-and-size", part.X.Raw);
            content.Append("y", part.Y.Raw);
            content.Append("attributes", part.Attributes.Raw);
            content.Append("inherit-palette", part.InheritPalette ? 1 : 0);
        }
    }
    /// <summary>
    /// The composition as a whole frame over artwork whose character 0 is OBJ character <paramref name="firstCharacter"/>.
    /// </summary>
    internal SpriteFrame ToFrame(string name, int firstCharacter)
    {
        var converted = ImmutableArray.CreateBuilder<SpriteFramePart>(parts.Count);
        foreach (CompiledSpritePart part in parts)
            converted.Add(ObjPartFrames.Part(part.X, part.Y, part.Attributes, firstCharacter, part.InheritPalette));
        return new(name, converted.MoveToImmutable());
    }

    /// <summary>Places every part as a sprite entry relative to <paramref name="origin"/> (#1358).</summary>
    /// <param name="sprites">The screen's sprite entries.</param>
    /// <param name="origin">Composition origin in screen-space pixels.</param>
    /// <param name="paletteBits">OBJ palette bits applied only to parts configured to inherit them.</param>
    public void Draw(ScreenSprites sprites, Position16<OamPixels> origin, ushort paletteBits)
    {
        Ensure.NotNull(sprites);
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        foreach (var part in parts)
            sprites.AddPart(part.X, part.Y, part.InheritPalette ? part.Attributes.WithPaletteBits(paletteBits) : part.Attributes, origin);
    }

    /// <summary>
    /// Draws the installed composition <paramref name="pointer"/> names, choosing the native on-screen or off-screen
    /// origin clipping; <paramref name="owner"/> names the presentation in a rejection.
    /// </summary>
    /// <exception cref="InvalidDataException">No composition is installed for <paramref name="pointer"/>.</exception>
    internal static void DrawInstalled(IReadOnlyDictionary<ushort, SpriteComposition> frames, string owner, ushort pointer,
        ScreenSprites objects, Position16<OamPixels> origin, ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException($"{owner} frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.Draw(objects, origin, paletteBits);
        else
            frame.DrawOffScreen(objects, origin, paletteBits);
    }

    /// <summary>Places all parts as sprite entries using the native off-screen origin's reversed vertical-wrap clipping (#1358).</summary>
    public void DrawOffScreen(ScreenSprites sprites, Position16<OamPixels> origin, ushort paletteBits)
    {
        Ensure.NotNull(sprites);
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        foreach (var part in parts)
            sprites.AddOffScreenPart(part.X, part.Y, part.InheritPalette ? part.Attributes.WithPaletteBits(paletteBits) : part.Attributes, origin);
    }
}
