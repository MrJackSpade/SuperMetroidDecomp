using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The warning a check's Ceres escape writes (#2554): production hands Ridley's step the foreground warning of the room it
/// runs in, which Ceres Ridley holds. A check that drives the step itself names the warning once, the loaded room's or a
/// fresh fixture one, and every step of that escape writes on it.
/// </summary>
internal static class CeresEscapeWarningAccess
{
    /// <summary>The warning each escape a check drives writes on, by escape.</summary>
    private static readonly ConditionalWeakTable<CeresEscape, RoomWarning> warnings = new();

    /// <summary>The warning a check's escape writes on.</summary>
    /// <param name="escape">The escape.</param>
    extension(CeresEscape escape)
    {
        /// <summary>The warning the escape writes on: the one the check named, else a fresh one on the installed sheet.</summary>
        internal RoomWarning FixtureWarning => warnings.GetValue(escape, _ => RoomWarningFixtures.Ceres());

        /// <summary>Names the warning the escape writes on, the loaded room's foreground warning for a check in a room.</summary>
        /// <param name="warning">The warning.</param>
        internal void UseWarning(RoomWarning warning) => warnings.AddOrUpdate(escape, Ensure.NotNull(warning));

        /// <summary>Runs Ridley's step of the escape, writing on the check's warning.</summary>
        /// <param name="ridleyFrameCounter">Ridley's frame counter.</param>
        internal void RunRidleysStep(int ridleyFrameCounter) => escape.RunRidleysStep(ridleyFrameCounter, escape.FixtureWarning);
    }
}
