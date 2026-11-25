using Godot;

namespace Engine.World;

/// <summary>
/// A yes-or-no question about the game's state, held as data. Barriers, placed things, room versions, triggers and
/// boss encounters hold conditions; nothing writes its own check of the game's state. It is made in the editor
/// too, as the kinds of node that hold one and are shown there are (a backdrop, a liquid, a trigger): a node made in
/// the editor cannot be given a condition that is not.
/// </summary>
[Tool]
[GlobalClass]
public partial class Condition : Resource
{
    /// <summary>The question asked.</summary>
    [Export]
    public ConditionKind Kind { get; set; }

    /// <summary>The fact, the equipment or the placed thing the question is about, where it is about one.</summary>
    [Export]
    public string About { get; set; } = "";

    /// <summary>The conditions the question is about, where it is about several.</summary>
    [Export]
    public Godot.Collections.Array<Condition> Parts { get; set; } = [];

    /// <summary>A condition asking <paramref name="kind"/> about <paramref name="about"/>.</summary>
    public static Condition Of(ConditionKind kind, string about = "") => new() { Kind = kind, About = about };

    /// <summary>A condition asking <paramref name="kind"/> of several conditions.</summary>
    public static Condition Of(ConditionKind kind, params Condition[] parts) => new() { Kind = kind, Parts = [.. parts] };

    /// <summary>The answer, asked in <paramref name="room"/>.</summary>
    public bool IsTrue(Room room) => Kind switch
    {
        ConditionKind.FactRecorded => room.Session.Progress.Has(RequireAbout()),
        ConditionKind.RoomHasNoEnemies => !room.HasLiveEnemies,
        ConditionKind.PlayerOwns => room.Player.Inventory.Owns(RequireAbout()),
        ConditionKind.PlacedThingGone => room.IsGone(RequireAbout()),
        ConditionKind.All => Parts.All(part => part.IsTrue(room)),
        ConditionKind.Any => Parts.Any(part => part.IsTrue(room)),
        ConditionKind.None => !Parts.Any(part => part.IsTrue(room)),
        ConditionKind.PlayerCarries => room.Player.Inventory.Capacity(RequireAbout()) > 0,
        ConditionKind.EnteredAt => room.Entry?.EntryPoint == RequireAbout(),
        ConditionKind.PlayerIsAbove => room.Player.Position.Y.Whole < (int.TryParse(RequireAbout(), out int height)
            ? height
            : throw new InvalidOperationException($"A condition on the player's height is about \"{About}\", which is not a number.")),
        ConditionKind.EnemiesKilled => room.EnemiesKilled >= (int.TryParse(RequireAbout(), out int kills)
            ? kills
            : throw new InvalidOperationException($"A condition on enemies killed is about \"{About}\", which is not a number.")),
        _ => throw new InvalidOperationException($"Undefined condition kind {Kind}."),
    };

    /// <summary>What the question is about, which must be named.</summary>
    private string RequireAbout() =>
        About.Length > 0 ? About : throw new InvalidOperationException($"A {Kind} condition does not say what it is about.");
}
