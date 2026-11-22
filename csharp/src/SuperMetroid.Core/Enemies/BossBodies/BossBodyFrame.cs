using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One whole frame a part of a boss shows on its body (#1466, #1515, #1540): a body pose, Kraid's head at a mouth stage, an
/// eye, a mouth or a melt image, as one pre-coloured picture placed against its boss and drawn whole at one depth among the
/// room's layers (#1634, #2031). The part showing it draws the picture over the parts drawn before it, and a pixel the picture
/// leaves clear shows them. Each state a room animation gives a frame is a frame of its own, pictured whole (#1513).
/// </summary>
public sealed partial class BossBodyFrame
{
    /// <summary>Holds the frame's picture, where it lies and the depth it is drawn at.</summary>
    /// <param name="name">The frame's name in its sheet.</param>
    /// <param name="shown">The frame's whole picture with the depth it is drawn at, or null when the frame shows nothing.</param>
    /// <param name="anchor">Where the picture's top-left corner lies from where its boss places its body: its boss's position.</param>
    /// <param name="dissolve">How a dissolve erases the picture step by step (#1652, #2478); null for a frame no dissolve erases.</param>
    /// <exception cref="ArgumentException">The frame has no name, or a dissolve but no picture of its size.</exception>
    public BossBodyFrame(string name, BossBodyPicture? shown, Offset16<RoomPixels> anchor, PictureDissolve? dissolve = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (dissolve is not null && (shown is null || dissolve.Size != shown.Picture.Size))
            throw new ArgumentException($"Boss body frame {name} dissolves a picture it does not show at the dissolve's size.", nameof(dissolve));
        Name = name;
        Shown = shown;
        Anchor = anchor;
        Dissolve = dissolve;
    }

    /// <summary>This frame showing <paramref name="shown"/> with <paramref name="hidden"/>'s pixels hidden (#1652).</summary>
    private BossBodyFrame(BossBodyFrame frame, BossBodyPicture? shown, HiddenPicturePixels? hidden)
        : this(frame.Name, shown, frame.Anchor, frame.Dissolve) =>
        Hidden = hidden;

    /// <summary>The frame's name in its sheet.</summary>
    public string Name { get; }

    /// <summary>
    /// The frame's whole picture and the depth it is drawn at among the room's layers (#1634, #2560); null when the frame shows
    /// nothing, as a melt image the dissolve has erased whole does: the part showing it is not drawn (#1513, #1540).
    /// </summary>
    public BossBodyPicture? Shown { get; }

    /// <summary>
    /// Where the picture's top-left corner lies from where its boss places its body, its boss's position (#2031), as the import
    /// placed it against the boss.
    /// </summary>
    public Offset16<RoomPixels> Anchor { get; }


    /// <summary>
    /// How a dissolve erases the picture step by step (#1652, #2478), as the import found it: Crocomire's melt images. Null for
    /// every other frame.
    /// </summary>
    internal PictureDissolve? Dissolve { get; }

    /// <summary>The pixels the picture hides as this frame shows it (#1652), or null where it shows them all.</summary>
    public HiddenPicturePixels? Hidden { get; }

    /// <summary>
    /// This frame with <paramref name="hidden"/>'s pixels of its picture hidden, as a dissolve's step leaves its picture
    /// (#1652).
    /// </summary>
    /// <exception cref="InvalidOperationException">The frame shows no picture.</exception>
    /// <exception cref="ArgumentException">The hidden pixels are not of the picture's size.</exception>
    public BossBodyFrame Hiding(HiddenPicturePixels hidden)
    {
        if (Shown is null)
            throw new InvalidOperationException($"Boss body frame {Name} shows no picture to hide pixels of.");
        if (Ensure.NotNull(hidden).Size != Shown.Picture.Size)
            throw new ArgumentException($"Boss body frame {Name} hides pixels of a picture of another size.", nameof(hidden));
        return new(this, Shown, hidden);
    }

    /// <summary>
    /// This frame as its dissolve leaves its picture once each band has shown the steps <paramref name="bandSteps"/> gives
    /// it, by band (#1652, #2478).
    /// </summary>
    /// <exception cref="InvalidOperationException">The frame has no dissolve.</exception>
    public BossBodyFrame Dissolved(ReadOnlySpan<int> bandSteps) => Dissolve is { } dissolve
        ? Hiding(dissolve.Hidden(bandSteps))
        : throw new InvalidOperationException($"Boss body frame {Name} has no dissolve.");

    /// <summary>
    /// This frame drawn at <paramref name="depth"/> instead, as Kraid's growth brings his body and the head it rises with in
    /// front of the sprites (#1634); a frame showing nothing stays as it is.
    /// </summary>
    public BossBodyFrame At(BossBodyDepth depth) => Shown is null || Shown.Depth == depth ? this : new(this, Shown with { Depth = depth }, Hidden);
}
