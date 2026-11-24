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
///   "hidden_until": ["bomb"],
///   "revealed_by_sight": true,
///   "contact": "nothing", "contact_hit": 0,
///   "pictures": { "intact": "tiles/{graphics}/87.png" },
///   "breaking": { "steps": [ { "picture": "tiles/{graphics}/83.png", "updates": 4 } ] },
///   "regrowing": { "steps": [ { "picture": "tiles/{graphics}/85.png", "updates": 4 } ] },
///   "break_sound": "enemy_block_crumble"
/// }
/// </code>
/// A block is drawn with the picture of the tile it lies on, at each tile it covers. One whose type gives an "intact"
/// picture starts hidden and shows that picture, at each of its tiles, once it has been revealed or has broken and
/// come back; a "hidden" picture is shown in place of its tiles' own until then. "breaking" is the timeline shown,
/// at each of its tiles, from the moment it breaks, and "regrowing" the one whose end is the moment it comes back;
/// "regrows_after" counts from the break to the return and takes in both. "break_sound" is heard as it breaks.
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

    /// <summary>How many updates after breaking it regrows; zero means it stays gone until its room is next set up.</summary>
    public int RegrowsAfter { get; private init; }

    /// <summary>The hit kinds that reveal it while hidden; empty when no hit does.</summary>
    public ImmutableHashSet<HitKind> HiddenUntil { get; private init; } = [];

    /// <summary>Whether a revealing ability reveals it while hidden.</summary>
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

    /// <summary>The picture shown while intact and revealed; empty for the pictures of the tiles it lies on.</summary>
    public string IntactPicture { get; private init; } = "";

    /// <summary>The picture shown while hidden; empty for the pictures of the tiles it lies on.</summary>
    public string HiddenPicture { get; private init; } = "";

    /// <summary>The pictures shown from the moment it breaks; null for one that is gone at once.</summary>
    public FrameTimeline? Breaking { get; private init; }

    /// <summary>The pictures shown up to the moment it comes back; null for one that is back at once.</summary>
    public FrameTimeline? Regrowing { get; private init; }

    /// <summary>The sound of it breaking, by name; empty for none.</summary>
    public string BreakSound { get; private init; } = "";

    /// <summary>Reads a block type from its data file.</summary>
    public static BlockType Read(DataNode data)
    {
        ImmutableHashSet<HitKind> Kinds(string name) =>
            data.Has(name) ? [.. data.List(name).Select(kind => new HitKind(kind.AsText()))] : [];

        DataNode? pictures = data.Has("pictures") ? data.Get("pictures") : null;
        var type = new BlockType
        {
            File = data.File,
            Solid = data.Flag("solid"),
            BrokenBy = Kinds("broken_by"),
            BreaksWhenStoodOn = data.Has("breaks_when_stood_on") && data.Flag("breaks_when_stood_on"),
            StoodOnUpdates = data.Has("stood_on_updates") ? data.Int("stood_on_updates") : 0,
            RegrowsAfter = data.Has("regrows_after") ? data.Int("regrows_after") : 0,
            HiddenUntil = Kinds("hidden_until"),
            RevealedBySight = data.Has("revealed_by_sight") && data.Flag("revealed_by_sight"),
            Contact = data.Has("contact") ? data.Get("contact").AsEnum<BlockContact>() : BlockContact.Nothing,
            ContactDamageHundredths = data.Has("contact_damage_hundredths") ? data.Int("contact_damage_hundredths") : 0,
            ContactHit = data.Has("contact_hit") ? data.Int("contact_hit") : 0,
            ContactSpeed = data.Has("contact_speed") ? data.Distance("contact_speed") : Pixels.Zero,
            IntactPicture = pictures is not null && pictures.Has("intact") ? pictures.Text("intact") : "",
            HiddenPicture = pictures is not null && pictures.Has("hidden") ? pictures.Text("hidden") : "",
            Breaking = data.Has("breaking") ? FrameTimeline.Read(data.Get("breaking")) : null,
            Regrowing = data.Has("regrowing") ? FrameTimeline.Read(data.Get("regrowing")) : null,
            BreakSound = data.Has("break_sound") ? data.Text("break_sound") : "",
        };
        if (type.RegrowsAfter < 0 || type.StoodOnUpdates < 0)
            throw data.Problem("gives a negative number of updates");
        return type;
    }
}
