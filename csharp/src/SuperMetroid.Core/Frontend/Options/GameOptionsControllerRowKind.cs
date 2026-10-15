namespace SuperMetroid.Core.Frontend.Options;

/// <summary>The roles of controller-page rows: seven assignable actions, then Exit and Reset.</summary>
public enum GameOptionsControllerRowKind
{
    /// <summary>Rows zero through six assign a physical button to one action.</summary>
    Action,
    /// <summary>Row seven exits to the primary page.</summary>
    Exit,
    /// <summary>Row eight restores the retail default button permutation.</summary>
    Reset,
}
