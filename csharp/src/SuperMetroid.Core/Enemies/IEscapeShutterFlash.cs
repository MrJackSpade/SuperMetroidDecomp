using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Enemies;

/// <summary>The enemies that show the escape's shutter flash (#1512, #2288, #2571).</summary>
internal interface IEscapeShutterFlash
{
    /// <summary>
    /// Shows <paramref name="frame"/> of the escape's shutter flash on the destroyable shutters the room holds, and on
    /// nothing once both are destroyed.
    /// </summary>
    /// <param name="frame">The flash's frame.</param>
    void ShowEscapeShutterFlash(ColourEffectFrame frame);
}
