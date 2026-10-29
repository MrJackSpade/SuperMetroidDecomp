using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One pre-assembled, pre-coloured image a boss places on its body layer (#1466): a body pose, a part of one (an eye, a
/// mouth, a head), a melt image or the blank a boss clears its layer with, cut from the boss's installed body sheet. Its
/// image's top edge lies on row <see cref="Top"/> of the layer and its left edge on the layer's left; it covers
/// <see cref="Footprint"/>'s cells. The blank part's image is one cell, repeated over whatever it covers.
/// </summary>
/// <remarks>
/// A part whose cells show an animated piece of artwork (Crocomire's melt bands) carries a picture for each frame of
/// that animation; it shows the frame the room's animation selected, and its own picture before any is selected.
/// </remarks>
public sealed class BossBodyPart
{
    /// <summary>The frame pictures by animation frame name.</summary>
    private readonly IReadOnlyDictionary<string, SpriteSheetFrame> animationPictures;

    /// <summary>Holds a part's picture, its place and footprint, and its animation's pictures when it follows one.</summary>
    /// <exception cref="ArgumentException">An animated part has no frame pictures, or a still part has some.</exception>
    public BossBodyPart(string name, SpriteSheetFrame picture, int top, BossBodyFootprint footprint, ArtworkAnimation? animation = null,
        IReadOnlyDictionary<string, SpriteSheetFrame>? animationPictures = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Ensure.AtLeastZero(top);
        if ((animation is null) != (animationPictures is null or { Count: 0 }))
            throw new ArgumentException($"Boss body part {name} must picture every frame of its animation, and only an animated part has frames.",
                nameof(animationPictures));
        Name = name;
        Picture = Ensure.NotNull(picture);
        Top = top;
        Footprint = Ensure.NotNull(footprint);
        Animation = animation;
        this.animationPictures = animationPictures ?? new Dictionary<string, SpriteSheetFrame>(StringComparer.Ordinal);
    }

    /// <summary>The part's name in its sheet.</summary>
    public string Name { get; }

    /// <summary>The part's picture before any animation frame is selected.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>The layer row the picture's top edge lies on.</summary>
    public int Top { get; }

    /// <summary>The cells the part covers where it is placed whole.</summary>
    public BossBodyFootprint Footprint { get; }

    /// <summary>The animation whose selected frame the part shows, or null for a still part.</summary>
    public ArtworkAnimation? Animation { get; }

    /// <summary>The picture the part shows while <paramref name="selections"/> hold the frame each animation shows.</summary>
    /// <exception cref="InvalidDataException">The selected frame of the part's animation is not pictured.</exception>
    public SpriteSheetFrame Shown(ArtworkFrameSelections selections)
    {
        Ensure.NotNull(selections);
        if (Animation is not { } animation || !selections.TryShown(animation, out ArtworkFrame frame))
            return Picture;
        return animationPictures.TryGetValue(frame.Name, out SpriteSheetFrame? shown) ? shown
            : throw new InvalidDataException($"Boss body part {Name} pictures no frame {frame.Name} of {animation.Name}.");
    }
}
