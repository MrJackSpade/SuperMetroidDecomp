namespace SuperMetroid.Core.Escape;

/// <summary>
/// What a Zebes escape explosion's random draw chooses (#2499): one of the explosions it names, or none, leaving the kind to
/// the explosion's place (<see cref="ZebesEscapeExplosionDefinitions.Draw"/>).
/// </summary>
internal abstract record ZebesEscapeExplosionDraw
{
    /// <summary>Only the draws below exist.</summary>
    private ZebesEscapeExplosionDraw()
    {
    }

    /// <summary>The draw names <paramref name="Explosion"/>, which goes off with its own kind and sound.</summary>
    /// <param name="Explosion">The explosion named.</param>
    internal sealed record Named(ZebesEscapeExplosionDefinition Explosion) : ZebesEscapeExplosionDraw;

    /// <summary>The draw names no explosion: the explosion's place gives it its kind, and it makes no sound.</summary>
    internal sealed record LeftOpen : ZebesEscapeExplosionDraw
    {
        /// <summary>The one draw that leaves the kind open.</summary>
        internal static LeftOpen Instance { get; } = new();

        /// <summary>Only <see cref="Instance"/> exists.</summary>
        private LeftOpen()
        {
        }
    }
}
