namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Which of the Ceres self-destruct's two page lists a cursor stands in (#1406).</summary>
internal enum CeresEscapePageList
{
    /// <summary>No list: the self-destruct has not begun its warning.</summary>
    None,

    /// <summary>The timer-sprite list: the escape timer's and the warning text's pages over the OBJ characters.</summary>
    Objects,

    /// <summary>The background list, whose pages land in characters the Ridley arena does not show.</summary>
    Backgrounds,
}
