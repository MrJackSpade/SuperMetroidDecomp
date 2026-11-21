using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// One row of the controller page (#2206): a row that assigns a button to an action, then Exit and Reset;
/// <see cref="GameOptionsPageRows.Controller"/> places them from the top.
/// </summary>
public abstract partial record GameOptionsControllerRow
{
    /// <summary>Closes the hierarchy to the rows declared here.</summary>
    private GameOptionsControllerRow()
    {
    }

    /// <summary>Assigns the physical button pressed on it to <see cref="Action"/>, swapping the button it displaces.</summary>
    /// <param name="Action">The action the row assigns.</param>
    public sealed partial record AssignsAction(ControllerAction Action) : GameOptionsControllerRow;

    /// <summary>Returns to the primary page.</summary>
    public sealed partial record Exit : GameOptionsControllerRow
    {
        /// <summary>The one Exit row.</summary>
        public static Exit Instance { get; } = new();
    }

    /// <summary>Restores the default button permutation.</summary>
    public sealed partial record Reset : GameOptionsControllerRow
    {
        /// <summary>The one Reset row.</summary>
        public static Reset Instance { get; } = new();
    }
}
