using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One whole frame a part of a boss shows on its body (#1466, #1515, #1540): a body pose, Kraid's head at a mouth stage, an
/// eye, a mouth or a melt image, as one pre-coloured picture placed on the body's area and drawn whole at one depth among the
/// room's layers (#1634). The part showing it draws the picture over the parts drawn before it, and a pixel the picture
/// leaves clear shows them. Each state a room animation gives a frame is a frame of its own, pictured whole (#1513).
/// </summary>
public sealed class BossBodyFrame
{
    /// <summary>Holds the frame's picture, where it lies and the depth it is drawn at.</summary>
    /// <param name="name">The frame's name in its sheet.</param>
    /// <param name="picture">The frame's whole picture, or null when the frame shows nothing.</param>
    /// <param name="corner">The body pixel the picture's top-left corner lies on.</param>
    /// <param name="depth">The depth the picture is drawn at; null exactly when the frame shows nothing.</param>
    /// <param name="dissolving">The picture's cells a column dissolve erases (#1652); empty for a frame no dissolve erases.</param>
    /// <exception cref="ArgumentException">
    /// The frame has no name, a depth without a picture or a picture without one, or dissolving cells but no picture.
    /// </exception>
    public BossBodyFrame(string name, SpriteSheetFrame? picture, Position16<BackgroundPixels> corner, BossBodyDepth? depth,
        ImmutableArray<DissolvingCell> dissolving = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (picture is null != depth is null)
            throw new ArgumentException($"Boss body frame {name} needs a depth exactly when it has a picture.", nameof(depth));
        Dissolving = dissolving.IsDefault ? [] : dissolving;
        if (picture is null && !Dissolving.IsEmpty)
            throw new ArgumentException($"Boss body frame {name} dissolves cells but shows no picture.", nameof(dissolving));
        Name = name;
        Picture = picture;
        Corner = corner;
        Depth = depth;
    }

    /// <summary>This frame showing its picture with <paramref name="hidden"/>'s pixels hidden (#1652).</summary>
    private BossBodyFrame(BossBodyFrame shown, BossBodyDepth? depth, HiddenPictureColumns? hidden)
        : this(shown.Name, shown.Picture, shown.Corner, depth, shown.Dissolving) =>
        Hidden = hidden;

    /// <summary>The frame's name in its sheet.</summary>
    public string Name { get; }

    /// <summary>
    /// The frame's whole picture; null when the frame shows nothing, as a melt image the dissolve has erased whole does: the
    /// part showing it is not drawn (#1513, #1540).
    /// </summary>
    public SpriteSheetFrame? Picture { get; }

    /// <summary>The body pixel the picture's top-left corner lies on.</summary>
    public Position16<BackgroundPixels> Corner { get; }

    /// <summary>
    /// The depth the picture is drawn at among the room's layers (#1634), as the import decided it from where the game draws
    /// the frame; null when the frame shows nothing.
    /// </summary>
    public BossBodyDepth? Depth { get; }

    /// <summary>
    /// The picture's cells a column dissolve erases (#1652), as the import found them: Crocomire's melt images. Empty for
    /// every other frame.
    /// </summary>
    public ImmutableArray<DissolvingCell> Dissolving { get; }

    /// <summary>The pixels the picture hides as this frame shows it (#1652), or null where it shows them all.</summary>
    public HiddenPictureColumns? Hidden { get; }

    /// <summary>
    /// This frame with <paramref name="hidden"/>'s pixels of its picture hidden, as a dissolve's step leaves its picture
    /// (#1652).
    /// </summary>
    /// <exception cref="InvalidOperationException">The frame shows no picture.</exception>
    public BossBodyFrame Hiding(HiddenPictureColumns hidden) => Picture is null
        ? throw new InvalidOperationException($"Boss body frame {Name} shows no picture to hide pixels of.")
        : new(this, Depth, Ensure.NotNull(hidden));

    /// <summary>
    /// This frame drawn at <paramref name="depth"/> instead, as Kraid's growth brings his body and the head it rises with in
    /// front of the sprites (#1634); a frame showing nothing stays as it is.
    /// </summary>
    public BossBodyFrame At(BossBodyDepth depth) => Picture is null || Depth == depth ? this : new(this, depth, Hidden);
}
