using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One boss's installed body sheet (#1466, #1515): every frame it shows on its body, each one whole pre-coloured picture
/// placed against its boss (#2031); each state a room animation gives a frame is a whole frame of its own (#1513).
/// </summary>
public sealed class BossBodySheet
{
    /// <summary>The frames by name.</summary>
    private readonly Dictionary<string, BossBodyFrame> frames;

    /// <summary>The frames a dissolve erases, by name.</summary>
    private readonly Dictionary<string, DissolvingBossBodyFrame> dissolving;

    /// <summary>Holds <paramref name="owner"/>'s frames cut from <paramref name="sheet"/>.</summary>
    /// <exception cref="InvalidDataException">A frame repeats a name, or a picture of it is not on the sheet.</exception>
    public BossBodySheet(BossBodyOwner owner, SpriteSheet sheet, IEnumerable<BossBodyFrame> frames)
    {
        Owner = owner;
        Sheet = Ensure.NotNull(sheet);
        var named = new Dictionary<string, BossBodyFrame>(StringComparer.Ordinal);
        var dissolvingFrames = new Dictionary<string, DissolvingBossBodyFrame>(StringComparer.Ordinal);
        foreach (BossBodyFrame frame in Ensure.NotNull(frames))
        {
            if (frame.Shown is { } shown && shown.Picture.Sheet != sheet.Image)
                throw new InvalidDataException($"{owner}'s body frame {frame.Name} is pictured off its sheet.");
            if (!named.TryAdd(frame.Name, frame))
                throw new InvalidDataException($"{owner}'s body sheet repeats frame {frame.Name}.");
            // A frame's constructor admits a dissolve only of the picture it shows.
            if (frame.Dissolve is { } dissolve)
                dissolvingFrames.Add(frame.Name, new DissolvingBossBodyFrame(frame, dissolve));
        }
        this.frames = named;
        dissolving = dissolvingFrames;
    }

    /// <summary>The boss.</summary>
    public BossBodyOwner Owner { get; }

    /// <summary>The installed sheet the pictures are cut from.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The frame enemy frame <paramref name="frame"/> shows.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame.</exception>
    public BossBodyFrame Frame(EnemyFrameId frame) => Frame(EnemyFrameSheetKeys.Of(frame));

    /// <summary>The frame enemy frame <paramref name="frame"/> shows on the body, or null when the body shows no picture of it.</summary>
    public BossBodyFrame? Find(EnemyFrameId frame) =>
        frames.TryGetValue(EnemyFrameSheetKeys.Of(frame), out BossBodyFrame? found) ? found : null;

    /// <summary>The frame named <paramref name="name"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame.</exception>
    public BossBodyFrame Frame([InstalledKey] string name) =>
        frames.TryGetValue(name, out BossBodyFrame? frame) ? frame : throw new InvalidDataException($"{Owner}'s body sheet has no frame {name}.");

    /// <summary>The frame named <paramref name="name"/>, which a dissolve erases.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame that a dissolve erases.</exception>
    public DissolvingBossBodyFrame Dissolving([InstalledKey] string name) =>
        dissolving.TryGetValue(name, out DissolvingBossBodyFrame? frame) ? frame
            : throw new InvalidDataException($"{Owner}'s body sheet has no frame {name} that a dissolve erases.");
}
