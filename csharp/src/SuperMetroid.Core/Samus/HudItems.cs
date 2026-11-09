namespace SuperMetroid.Core.Samus;

/// <summary>Transitions of <see cref="HudItem"/>.</summary>
public static class HudItems
{
    /// <summary>The next entry of the selection ring, wrapping after X-ray.</summary>
    public static HudItem Next(HudItem item) => EnumOrder.NextWrapping(item);
}
