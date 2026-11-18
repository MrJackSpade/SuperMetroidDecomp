namespace SuperMetroid.Core.Fx;

/// <summary>
/// What each kind of effect overlay maps to (#2505): a consumer implements one method for each, and an overlay calls the
/// one for its kind (<see cref="RoomEffectOverlay.Map"/>), so every kind is mapped and none is left to a fallback.
/// </summary>
/// <typeparam name="TResult">What each kind of overlay maps to.</typeparam>
public interface IRoomEffectOverlayMapping<out TResult>
{
    /// <summary>The value of an overlay showing nothing.</summary>
    TResult Nothing();

    /// <summary>The value of an atmosphere.</summary>
    /// <param name="atmosphere">The atmosphere.</param>
    TResult Atmosphere(AtmosphereEffectOverlay atmosphere);

    /// <summary>The value of a liquid.</summary>
    /// <param name="liquid">The liquid.</param>
    TResult Liquid(LiquidEffectOverlay liquid);
}
