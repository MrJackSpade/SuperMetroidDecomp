namespace SuperMetroid.Core.Samus;

/// <summary>
/// Whether the scenario has introduced Samus (#2554): not until it explicitly does, then the Samus it introduced, whom
/// every later scenario step moves, draws and damages.
/// </summary>
internal abstract class SamusIntroduction
{
    /// <summary>Only the two states below exist.</summary>
    private SamusIntroduction() { }

    /// <summary>The scenario has not introduced Samus yet.</summary>
    internal sealed class NotIntroduced : SamusIntroduction
    {
        /// <summary>The one state before her introduction.</summary>
        internal static NotIntroduced Instance { get; } = new();

        /// <summary>The state has no data, so one instance serves every runtime.</summary>
        private NotIntroduced() { }
    }

    /// <summary>The scenario has introduced Samus.</summary>
    /// <param name="samus">The Samus it introduced.</param>
    internal sealed class Introduced(SamusState samus) : SamusIntroduction
    {
        /// <summary>The Samus the scenario introduced.</summary>
        internal SamusState Samus { get; } = Ensure.NotNull(samus);
    }
}
