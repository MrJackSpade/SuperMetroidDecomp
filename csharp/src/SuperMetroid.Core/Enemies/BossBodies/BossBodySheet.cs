using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One boss's installed body sheet (#1466): every frame it draws on its body layer, pre-assembled and pre-coloured over
/// the characters its room shows, split into still and animated parts, and the blank it clears its layer with.
/// </summary>
public sealed class BossBodySheet
{
    /// <summary>The frames by name.</summary>
    private readonly IReadOnlyDictionary<string, BossBodyFrame> frames;

    /// <summary>Every part of every frame, and the blank, by name.</summary>
    private readonly IReadOnlyDictionary<string, BossBodyPart> parts;

    /// <summary>Holds <paramref name="owner"/>'s frames cut from <paramref name="sheet"/>.</summary>
    /// <exception cref="InvalidDataException">
    /// A frame or part repeats a name, a frame has no parts, or a part's picture is not on the sheet.
    /// </exception>
    public BossBodySheet(BossBodyOwner owner, SpriteSheet sheet, BossBodyPart blank, IEnumerable<BossBodyFrame> frames)
    {
        Owner = owner;
        Sheet = Ensure.NotNull(sheet);
        Blank = Ensure.NotNull(blank);
        var named = new Dictionary<string, BossBodyFrame>(StringComparer.Ordinal);
        var partsByName = new Dictionary<string, BossBodyPart>(StringComparer.Ordinal) { [blank.Name] = blank };
        foreach (BossBodyFrame frame in Ensure.NotNull(frames))
        {
            if (frame.Parts.IsDefaultOrEmpty)
                throw new InvalidDataException($"{owner}'s body frame {frame.Name} has no parts.");
            if (frame.Parts.Any(part => part.Picture.Sheet != sheet.Image))
                throw new InvalidDataException($"{owner}'s body frame {frame.Name} has a part off its sheet.");
            if (!named.TryAdd(frame.Name, frame))
                throw new InvalidDataException($"{owner}'s body sheet repeats frame {frame.Name}.");
            foreach (BossBodyPart part in frame.Parts)
            {
                if (!partsByName.TryAdd(part.Name, part))
                    throw new InvalidDataException($"{owner}'s body sheet repeats part {part.Name}.");
            }
        }
        this.frames = named;
        parts = partsByName;
    }

    /// <summary>The boss.</summary>
    public BossBodyOwner Owner { get; }

    /// <summary>The installed sheet the parts are cut from.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The part the boss clears its layer with: one cell, repeated.</summary>
    public BossBodyPart Blank { get; }

    /// <summary>The part named <paramref name="name"/>, of any frame or the blank.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such part.</exception>
    public BossBodyPart Part(string name) =>
        parts.TryGetValue(name, out BossBodyPart? part) ? part : throw new InvalidDataException($"{Owner}'s body sheet has no part {name}.");

    /// <summary>The frame enemy frame <paramref name="frame"/> draws.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame.</exception>
    public BossBodyFrame Frame(EnemyFrameId frame) => Frame(BossBodyFrameNames.Of(frame));

    /// <summary>The frame named <paramref name="name"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame.</exception>
    public BossBodyFrame Frame(string name) =>
        frames.TryGetValue(name, out BossBodyFrame? frame) ? frame : throw new InvalidDataException($"{Owner}'s body sheet has no frame {name}.");
}
