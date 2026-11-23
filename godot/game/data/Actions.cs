using Engine.Input;

namespace Game.Data;

/// <summary>The actions this game adds to the engine's own. Samus's states read these, never buttons.</summary>
public static class Actions
{
    /// <summary>Run.</summary>
    public static GameAction Run { get; } = new("run");

    /// <summary>Move on to the next item.</summary>
    public static GameAction ItemSelect { get; } = new("item_select");

    /// <summary>Put the selected item away.</summary>
    public static GameAction ItemCancel { get; } = new("item_cancel");

    /// <summary>Aim diagonally up.</summary>
    public static GameAction AngleUp { get; } = new("angle_up");

    /// <summary>Aim diagonally down.</summary>
    public static GameAction AngleDown { get; } = new("angle_down");

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<GameAction> All { get; } = [Run, ItemSelect, ItemCancel, AngleUp, AngleDown];

    /// <summary>The buttons each is bound to until the player chooses otherwise.</summary>
    public static Dictionary<string, List<string>> StandardBindings => new()
    {
        [Run.Name] = ["key:C", "pad:2"],
        [ItemSelect.Name] = ["key:A", "pad:4"],
        [ItemCancel.Name] = ["key:S", "pad:3"],
        [AngleUp.Name] = ["key:D", "pad:10"],
        [AngleDown.Name] = ["key:F", "pad:9"],
    };
}
