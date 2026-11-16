namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// What an Evir's spit does each update (#2063); its body and arms have one behaviour each and keep no action.
/// Its values are arbitrary.
/// </summary>
public enum EvirSpitAction
{
    /// <summary>Keeps the reusable spit at the body's mouth until a launch is selected.</summary>
    Idle,
    /// <summary>Checks for regeneration beyond the viewport, then advances the aimed spit's fixed-point position.</summary>
    Moving,
    /// <summary>Anchors the growing spit to the mouth plus its animation-controlled offset; a frozen body holds this state.</summary>
    Regenerating,
}
