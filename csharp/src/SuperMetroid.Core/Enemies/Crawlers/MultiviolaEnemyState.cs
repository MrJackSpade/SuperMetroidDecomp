using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The state of the Multiviola. Retail recomputes its velocity from the population angle and speed every active
/// frame, with two deliberately imperfect reflection negations. The velocity is a value the cartridge keeps in the
/// enemy's common record, so a kill empties it with that record (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class MultiviolaEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<MultiviolaProgram> Program { get; set; }

    /// <summary>Signed 16.16 velocity in room pixels per update.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Empties the value the cartridge keeps in the enemy's common record: no velocity.</summary>
    internal void ClearWithRecord() => Velocity = FixedVector<RoomPixels>.Zero;
}
