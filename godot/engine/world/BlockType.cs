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
///   "contact": "nothing",
///   "pictures": { "intact": "tiles/shot_block.png", "hidden": "tiles/rock.png" }
/// }
/// </code>
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

    /// <summary>Whether it starts hidden: looking like plain terrain until revealed.</summary>
    public bool StartsHidden => HiddenUntil.Count > 0 || RevealedBySight;

    /// <summary>What touching it does.</summary>
    public BlockContact Contact { get; private init; }

    /// <summary>For a block that hurts, the damage each update, in hundredths of a point; for one that carries or pulls, unused.</summary>
    public int ContactDamageHundredths { get; private init; }

    /// <summary>For a block that carries or pulls, how far each update; rightward or downward when positive.</summary>
    public Pixels ContactSpeed { get; private init; }

    /// <summary>The picture shown while intact and revealed; empty for none.</summary>
    public string IntactPicture { get; private init; } = "";

    /// <summary>The picture shown while hidden; empty for none.</summary>
    public string HiddenPicture { get; private init; } = "";

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
            ContactSpeed = data.Has("contact_speed") ? data.Distance("contact_speed") : Pixels.Zero,
            IntactPicture = pictures is not null && pictures.Has("intact") ? pictures.Text("intact") : "",
            HiddenPicture = pictures is not null && pictures.Has("hidden") ? pictures.Text("hidden") : "",
        };
        if (type.RegrowsAfter < 0 || type.StoodOnUpdates < 0)
            throw data.Problem("gives a negative number of updates");
        return type;
    }
}
