namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>When an admitted x-ray begins.</summary>
public enum XrayActivation
{
    /// <summary>The beam begins at once.</summary>
    Immediate,

    /// <summary>The beam begins at the HUD-item handler's deferred point in the frame.</summary>
    Deferred,
}
