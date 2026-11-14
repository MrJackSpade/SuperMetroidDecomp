using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// A room state's effect selection (#1546): the room effect each entry door selects, in the game's order, and the
/// default effect a room entered through any other door shows, or none when that entry shows no effect.
/// </summary>
public sealed class RoomFxSelection
{
    /// <summary>Holds a selection's door effects in the game's order and its default effect.</summary>
    /// <param name="name">The selection's name, or a fixture's description.</param>
    /// <param name="doorEffects">The effects entry doors select, in the game's order.</param>
    /// <param name="defaultEffect">The effect any other entry shows, or null when that entry shows no effect.</param>
    public RoomFxSelection(string name, DoorRoomEffect[] doorEffects, RoomEffectDefinition? defaultEffect)
    {
        Name = Ensure.NotNull(name);
        DoorEffects = Array.AsReadOnly(Ensure.NotNull(doorEffects).ToArray());
        Default = defaultEffect;
        var effects = new List<RoomEffectDefinition>(DoorEffects.Count + 1);
        effects.AddRange(DoorEffects.Select(doorEffect => doorEffect.Effect));
        if (defaultEffect is not null)
            effects.Add(defaultEffect);
        Effects = effects.AsReadOnly();
    }

    /// <summary>The selection's name.</summary>
    public string Name { get; }

    /// <summary>The effects entry doors select, in the game's order: the first naming the entry door is shown.</summary>
    public IReadOnlyList<DoorRoomEffect> DoorEffects { get; }

    /// <summary>The effect a room entered through a door no door effect names shows, or null for no effect.</summary>
    public RoomEffectDefinition? Default { get; }

    /// <summary>Every effect the selection can show: its door effects in the game's order, then its default effect.</summary>
    public IReadOnlyList<RoomEffectDefinition> Effects { get; }

    /// <summary>Selects the first door effect naming <paramref name="door"/>, or else the default effect, or none.</summary>
    public RoomEffectDefinition? Select(DoorDefinition? door)
    {
        foreach (DoorRoomEffect doorEffect in DoorEffects)
        {
            if (doorEffect.Door == door)
                return doorEffect.Effect;
        }
        return Default;
    }
}
