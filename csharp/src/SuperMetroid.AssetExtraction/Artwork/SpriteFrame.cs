using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One whole, named sprite frame: the ordered parts that together show an actor in one pose or glyph.
/// Earlier parts are in front of later ones. It holds no position, collision or animation state.
/// </summary>
public sealed class SpriteFrame
{
    /// <summary>Validates and keeps an immutable part sequence.</summary>
    /// <param name="name">The frame's identity within its family, such as <c>Digit.3</c>.</param>
    /// <param name="parts">The parts, frontmost first.</param>
    /// <exception cref="ArgumentException">The name is empty, or a part has an unsupported size, palette or priority, or a negative character.</exception>
    public SpriteFrame(string name, ImmutableArray<SpriteFramePart> parts)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("A sprite frame needs a name.", nameof(name));
        if (parts.IsDefault) throw new ArgumentException($"Sprite frame {name} has no part list.", nameof(parts));
        foreach (SpriteFramePart part in parts)
        {
            if (part.Size.X is not (8 or 16 or 32 or 64) || part.Size.Y is not (8 or 16 or 32 or 64))
                throw new ArgumentException($"Sprite frame {name} has a {part.Size.X}x{part.Size.Y} part; sizes are 8, 16, 32 or 64.", nameof(parts));
            if (part.Character < 0 || part.Palette is < 0 or > 7 || part.Priority is < 0 or > 3)
                throw new ArgumentException($"Sprite frame {name} has a part with an invalid character, palette or priority.", nameof(parts));
        }
        Name = name;
        this.parts = ImmutableCollectionsMarshal.AsArray(parts) ?? throw new ArgumentException($"Sprite frame {name} has no part list.", nameof(parts));
    }

    /// <summary>The frame's identity within its family.</summary>
    public string Name { get; }

    /// <summary>Owned parts; a plain array so debugger state can carry published frames.</summary>
    private readonly SpriteFramePart[] parts;

    /// <summary>The parts, frontmost first.</summary>
    public ImmutableArray<SpriteFramePart> Parts => ImmutableCollectionsMarshal.AsImmutableArray(parts);

    /// <summary>The highest character any part reads, or -1 for an empty frame.</summary>
    public int LastCharacter => Parts.IsEmpty ? -1 : Parts.Max(part => part.LastCharacter);
}
