namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What a gameplay blend looks like (#1407, #1514): Fireflea's darkness (<see cref="FirefleaDarknessBlend"/>), Phantoon's
/// translucent body (<see cref="PhantoonTranslucenceBlend"/>), a room effect lying behind the room
/// (<see cref="EffectBehindRoomBlend"/>), or the X-ray's dimming outside its cone (<see cref="XrayDimmingBlend"/>). Each
/// describes the effect it shows; the backend draws it.
/// </summary>
public abstract record GameplayBlend
{
    /// <summary>Only the effects declared beside it derive from this.</summary>
    private protected GameplayBlend()
    {
    }
}
