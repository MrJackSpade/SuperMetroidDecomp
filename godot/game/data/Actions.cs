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

    /// <summary>
    /// The control each action of play is on in a new game: the cartridge's own layout of its controller's seven
    /// buttons that can be given an action. The cartridge keeps which is on which with each saved game, and so does
    /// this game (<see cref="Engine.Core.SaveData.Controls"/>).
    /// </summary>
    public static Dictionary<string, string> StandardControls => new()
    {
        [GameAction.Attack.Name] = Controls.X,
        [GameAction.Jump.Name] = Controls.A,
        [Run.Name] = Controls.B,
        [ItemSelect.Name] = Controls.Select,
        [ItemCancel.Name] = Controls.Y,
        [AngleUp.Name] = Controls.R,
        [AngleDown.Name] = Controls.L,
    };

    /// <summary>
    /// The buttons each of the cartridge's controls is bound to until the player chooses otherwise: a key, and the
    /// button of a controller that lies where the control lies on the cartridge's own (its A to the right, its B at
    /// the bottom, its X at the top, its Y to the left). The menus' confirming and cancelling are the engine's actions,
    /// and are put where the cartridge reads them: confirming on its A, cancelling on its B. (Start, the engine's pause,
    /// confirms too, as the menus themselves say.)
    /// </summary>
    public static Dictionary<string, List<string>> StandardBindings => new()
    {
        [GameAction.Confirm.Name] = ["key:X", "pad:1"],
        [GameAction.Cancel.Name] = ["key:Z", "pad:0"],
        [Controls.A] = ["key:X", "pad:1"],
        [Controls.B] = ["key:C", "pad:0"],
        [Controls.X] = ["key:Z", "pad:3"],
        [Controls.Y] = ["key:S", "pad:2"],
        [Controls.Select] = ["key:A", "pad:4"],
        [Controls.R] = ["key:D", "pad:10"],
        [Controls.L] = ["key:F", "pad:9"],
    };
}

/// <summary>
/// The buttons of the cartridge's controller that an action of play can be on, by the names the options keep them
/// under. The directions and Start are not among them: the cartridge keeps those as they are.
/// </summary>
public static class Controls
{
    /// <summary>The A button.</summary>
    public const string A = "a";

    /// <summary>The B button.</summary>
    public const string B = "b";

    /// <summary>The X button.</summary>
    public const string X = "x";

    /// <summary>The Y button.</summary>
    public const string Y = "y";

    /// <summary>The Select button.</summary>
    public const string Select = "select";

    /// <summary>The left shoulder button.</summary>
    public const string L = "l";

    /// <summary>The right shoulder button.</summary>
    public const string R = "r";

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<string> All { get; } = [A, B, X, Y, Select, L, R];
}
