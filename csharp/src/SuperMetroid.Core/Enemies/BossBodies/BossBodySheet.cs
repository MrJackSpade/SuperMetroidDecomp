using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One boss's installed body sheet (#1466, #1515): every frame it shows on its body, each one whole pre-coloured picture
/// placed on the body's area, with the pictures of the pixels room animations paint.
/// </summary>
public sealed class BossBodySheet
{
    /// <summary>The frames by name.</summary>
    private readonly Dictionary<string, BossBodyFrame> frames;

    /// <summary>Holds <paramref name="owner"/>'s frames cut from <paramref name="sheet"/>.</summary>
    /// <exception cref="InvalidDataException">A frame repeats a name, or a picture of it is not on the sheet.</exception>
    public BossBodySheet(BossBodyOwner owner, SpriteSheet sheet, IEnumerable<BossBodyFrame> frames)
    {
        Owner = owner;
        Sheet = Ensure.NotNull(sheet);
        var named = new Dictionary<string, BossBodyFrame>(StringComparer.Ordinal);
        foreach (BossBodyFrame frame in Ensure.NotNull(frames))
        {
            if (frame.Picture is { } picture && picture.Sheet != sheet.Image)
                throw new InvalidDataException($"{owner}'s body frame {frame.Name} is pictured off its sheet.");
            if (!named.TryAdd(frame.Name, frame))
                throw new InvalidDataException($"{owner}'s body sheet repeats frame {frame.Name}.");
        }
        this.frames = named;
    }

    /// <summary>The boss.</summary>
    public BossBodyOwner Owner { get; }

    /// <summary>The installed sheet the pictures are cut from.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The frame enemy frame <paramref name="frame"/> shows.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame.</exception>
    public BossBodyFrame Frame(EnemyFrameId frame) => Frame(BossBodyFrameNames.Of(frame));

    /// <summary>The frame named <paramref name="name"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame.</exception>
    public BossBodyFrame Frame(string name) =>
        frames.TryGetValue(name, out BossBodyFrame? frame) ? frame : throw new InvalidDataException($"{Owner}'s body sheet has no frame {name}.");
}
