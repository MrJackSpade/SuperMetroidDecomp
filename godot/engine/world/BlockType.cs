using System.Collections.Immutable;
using Engine.Content;
using Engine.Shared;

namespace Engine.World;

/// <summary>
/// A kind of reactive tile, as a combination of the block rules and nothing else: whether it is solid, what breaks
/// it, whether it comes back, what hides it, and what touching it does. The rules are the engine's; the hit kinds a
/// block type names are the game's.
/// </summary>
/// <remarks>
/// <code>
/// {
///   "solid": true,
///   "broken_by": ["beam", "missile"],
///   "breaks_when_stood_on": false,
///   "regrows_after": 240,
///   "records": "wall_broken",
///   "hidden_until": ["bomb"],
///   "revealed_by_sight": true,
///   "contact": "nothing", "contact_hit": 0, "contact_speed": 0, "carries_only_level": false,
///   "pictures": { "intact": "tiles/{graphics}/87.png" },
///   "hides_again": true,
///   "breaking": { "steps": [ { "picture": "tiles/{graphics}/83.png", "updates": 4 } ] },
///   "regrowing": { "steps": [ { "picture": "tiles/{graphics}/85.png", "updates": 4 } ] },
///   "break_sound": "enemy_block_crumble",
///   "break_sound_by_kind": { "power_bomb": "enemy_block_crumble:low" }
/// }
/// </code>
/// A block is drawn with the picture of the tile it lies on, at each tile it covers. One whose type gives an "intact"
/// picture starts hidden and shows that picture, at each of its tiles, once it has been revealed or has broken and
/// come back; a "hidden" picture is shown in place of its tiles' own until then. A block of several tiles whose
/// tiles differ once it is known gives, in place of "intact", "intact_parts": a picture for each tile, row by row.
/// One that "hides_again" comes back from breaking looking like plain terrain once more. "breaking" is the timeline shown,
/// at each of its tiles, from the moment it breaks, and "regrowing" the one whose end is the moment it comes back;
/// "regrows_after" counts from the break to the return and takes in both. One that "blocks_while_crumbling" goes on
/// blocking for as long as either timeline's pictures show, and lets things through only between them. "break_sound" is heard as it breaks,
/// unless the hit that broke it carries a kind "break_sound_by_kind" names, the first listed of them: then that
/// kind's sound is.
/// A block that pulls down may say more, as quicksand does: "moving_contact_speed" for what is rising or falling in
/// it, "rise_limit", "holds_up", "feet_only", "stops_momentum", and under "eased" the equipment ("by") that eases
/// it with the three numbers it has for one who wears it.
/// </remarks>
public sealed class BlockType
{
    /// <summary>The data file this type was read from.</summary>
    public string File { get; private init; } = "";

    /// <summary>Whether an intact block is solid.</summary>
    public bool Solid { get; private init; }

    /// <summary>The hit kinds that break it. A hit carrying any of them does.</summary>
    public ImmutableHashSet<HitKind> BrokenBy { get; private init; } = [];

    /// <summary>Whether being stood on breaks it.</summary>
    public bool BreaksWhenStoodOn { get; private init; }

    /// <summary>How many updates it lasts once stood on, before it breaks.</summary>
    public int StoodOnUpdates { get; private init; }

    /// <summary>
    /// For a block that can be latched onto once: how many updates after it is first latched onto it breaks, whether
    /// or not it is still held; zero for one that breaks when it is let go of.
    /// </summary>
    public int LatchedUpdates { get; private init; }

    /// <summary>How many updates after breaking it regrows; zero means it stays gone until its room is next set up.</summary>
    public int RegrowsAfter { get; private init; }

    /// <summary>The hit kinds that reveal it while hidden; empty when no hit does.</summary>
    public ImmutableHashSet<HitKind> HiddenUntil { get; private init; } = [];

    /// <summary>Whether a revealing ability shows it for what it is, while hidden, for as long as it sees it.</summary>
    public bool RevealedBySight { get; private init; }

    /// <summary>
    /// Whether it starts hidden: looking like plain terrain until revealed. One that something reveals does, and so does
    /// one with a picture to show for what it is.
    /// </summary>
    public bool StartsHidden => HiddenUntil.Count > 0 || RevealedBySight || IntactPicture.Length > 0;

    /// <summary>What touching it does.</summary>
    public BlockContact Contact { get; private init; }

    /// <summary>For a block that hurts, the damage each update, in hundredths of a point; for one that carries or pulls, unused.</summary>
    public int ContactDamageHundredths { get; private init; }

    /// <summary>
    /// For a block that hurts, the damage of the hit touching it delivers; zero for one that only does ongoing damage.
    /// What was hit is safe for a moment afterwards, as after any hit, so the block does not hit again at once.
    /// </summary>
    public int ContactHit { get; private init; }

    /// <summary>For a block that carries or pulls, how far each update; rightward or downward when positive.</summary>
    public Pixels ContactSpeed { get; private init; }

    /// <summary>For a block that carries: whether it carries only what is not rising or falling by a whole pixel or more an update.</summary>
    public bool CarriesOnlyLevel { get; private init; }

    /// <summary>For a block that pulls: how far each update it pulls what is rising or falling in it; what stands still it pulls by <see cref="ContactSpeed"/>.</summary>
    public Pixels MovingContactSpeed { get; private init; }

    /// <summary>For a block that pulls: the fastest it lets what is in it rise; zero for no limit.</summary>
    public Pixels RiseLimit { get; private init; }

    /// <summary>For a block that pulls: whether what has its feet in it stands there as on ground, and does not fall through it.</summary>
    public bool HoldsUp { get; private init; }

    /// <summary>For a block that pulls: whether it pulls only what has its feet in it.</summary>
    public bool PullsFeetOnly { get; private init; }

    /// <summary>For a block that pulls: whether what is in it loses its momentum.</summary>
    public bool StopsMomentum { get; private init; }

    /// <summary>For a block that pulls: the equipment that eases it; empty for none.</summary>
    public IReadOnlyList<string> EasedBy { get; private init; } = [];

    /// <summary>What <see cref="ContactSpeed"/>, <see cref="MovingContactSpeed"/> and <see cref="RiseLimit"/> are for one whose equipment eases it.</summary>
    public (Pixels ContactSpeed, Pixels MovingContactSpeed, Pixels RiseLimit) Eased { get; private init; }

    /// <summary>
    /// The pictures shown while intact and revealed: one for every tile of it, or one for each of its tiles, row by
    /// row; none for the pictures of the tiles it lies on.
    /// </summary>
    public IReadOnlyList<string> IntactPictures { get; private init; } = [];

    /// <summary>The picture the first of its tiles shows while intact and revealed; empty for the picture of the tile it lies on.</summary>
    public string IntactPicture => IntactPictures.Count > 0 ? IntactPictures[0] : "";

    /// <summary>The picture one of its tiles shows while intact and revealed, by the tile's place in it, row by row.</summary>
    public string IntactPictureOf(int part) => IntactPictures.Count == 0 ? "" : IntactPictures.Count == 1 ? IntactPictures[0]
        : part >= 0 && part < IntactPictures.Count ? IntactPictures[part]
        : throw new ContentException(File, $"gives {IntactPictures.Count} pictures for the tiles of a block that has more of them");

    /// <summary>Whether it is hidden again when it comes back from breaking.</summary>
    public bool HidesAgain { get; private init; }

    /// <summary>Whether it goes on blocking, broken, for as long as the pictures of its breaking or of its regrowing show.</summary>
    public bool BlocksWhileCrumbling { get; private init; }

    /// <summary>The fact recorded as it breaks, by name ("records"); empty for none: a wall whose breaking the game remembers.</summary>
    public string Records { get; private init; } = "";

    /// <summary>The picture shown while hidden; empty for the pictures of the tiles it lies on.</summary>
    public string HiddenPicture { get; private init; } = "";

    /// <summary>The pictures shown from the moment it breaks; null for one that is gone at once.</summary>
    public FrameTimeline? Breaking { get; private init; }

    /// <summary>The pictures shown up to the moment it comes back; null for one that is back at once.</summary>
    public FrameTimeline? Regrowing { get; private init; }

    /// <summary>The sound of it breaking, by name, where what broke it has none of its own here; empty for none.</summary>
    public string BreakSound { get; private init; } = "";

    /// <summary>The sounds of it breaking under a hit of a kind, by name, in the order its data lists them.</summary>
    public IReadOnlyList<(HitKind Kind, string Sound)> BreakSoundsByKind { get; private init; } = [];

    /// <summary>
    /// The sound of it breaking, by name: under <paramref name="hit"/>, the sound of the first listed kind the hit
    /// carries, and otherwise, or broken by nothing that is a hit, its plain one. Empty for none.
    /// </summary>
    public string BreakSoundFor(Hit? hit)
    {
        if (hit is not null)
        {
            foreach ((HitKind kind, string sound) in BreakSoundsByKind)
            {
                if (hit.Carries(kind))
                    return sound;
            }
        }

        return BreakSound;
    }

    /// <summary>Reads a block type from its data file.</summary>
    public static BlockType Read(DataNode data)
    {
        ImmutableHashSet<HitKind> Kinds(string name) =>
            data.Has(name) ? [.. data.List(name).Select(kind => new HitKind(kind.AsText()))] : [];

        DataNode? pictures = data.Has("pictures") ? data.Get("pictures") : null;
        DataNode? eased = data.Has("eased") ? data.Get("eased") : null;
        var type = new BlockType
        {
            File = data.File,
            Solid = data.Flag("solid"),
            BrokenBy = Kinds("broken_by"),
            BreaksWhenStoodOn = data.Has("breaks_when_stood_on") && data.Flag("breaks_when_stood_on"),
            StoodOnUpdates = data.Has("stood_on_updates") ? data.Int("stood_on_updates") : 0,
            LatchedUpdates = data.Has("latched_updates") ? data.Int("latched_updates") : 0,
            RegrowsAfter = data.Has("regrows_after") ? data.Int("regrows_after") : 0,
            BlocksWhileCrumbling = data.Has("blocks_while_crumbling") && data.Flag("blocks_while_crumbling"),
            Records = data.Has("records") ? data.Text("records") : "",
            HiddenUntil = Kinds("hidden_until"),
            RevealedBySight = data.Has("revealed_by_sight") && data.Flag("revealed_by_sight"),
            Contact = data.Has("contact") ? data.Get("contact").AsEnum<BlockContact>() : BlockContact.Nothing,
            ContactDamageHundredths = data.Has("contact_damage_hundredths") ? data.Int("contact_damage_hundredths") : 0,
            ContactHit = data.Has("contact_hit") ? data.Int("contact_hit") : 0,
            ContactSpeed = data.Has("contact_speed") ? data.Distance("contact_speed") : Pixels.Zero,
            CarriesOnlyLevel = data.Has("carries_only_level") && data.Flag("carries_only_level"),
            MovingContactSpeed = data.Has("moving_contact_speed") ? data.Distance("moving_contact_speed")
                : data.Has("contact_speed") ? data.Distance("contact_speed") : Pixels.Zero,
            RiseLimit = data.Has("rise_limit") ? data.Distance("rise_limit") : Pixels.Zero,
            HoldsUp = data.Has("holds_up") && data.Flag("holds_up"),
            PullsFeetOnly = data.Has("feet_only") && data.Flag("feet_only"),
            StopsMomentum = data.Has("stops_momentum") && data.Flag("stops_momentum"),
            EasedBy = eased is null ? [] : [.. eased.List("by").Select(item => item.AsText())],
            Eased = eased is null ? default
                : (eased.Distance("contact_speed"), eased.Distance("moving_contact_speed"), eased.Distance("rise_limit")),
            IntactPictures = pictures is null ? []
                : pictures.Has("intact_parts") ? [.. pictures.List("intact_parts").Select(picture => picture.AsText())]
                : pictures.Has("intact") ? [pictures.Text("intact")] : [],
            HidesAgain = data.Has("hides_again") && data.Flag("hides_again"),
            HiddenPicture = pictures is not null && pictures.Has("hidden") ? pictures.Text("hidden") : "",
            Breaking = data.Has("breaking") ? FrameTimeline.Read(data.Get("breaking")) : null,
            Regrowing = data.Has("regrowing") ? FrameTimeline.Read(data.Get("regrowing")) : null,
            BreakSound = data.Has("break_sound") ? data.Text("break_sound") : "",
            BreakSoundsByKind = data.Has("break_sound_by_kind")
                ? [.. data.Get("break_sound_by_kind").Entries().Select(entry => (new HitKind(entry.Key), entry.Value.AsText()))]
                : [],
        };
        if (type.RegrowsAfter < 0 || type.StoodOnUpdates < 0 || type.LatchedUpdates < 0)
            throw data.Problem("gives a negative number of updates");
        return type;
    }
}
