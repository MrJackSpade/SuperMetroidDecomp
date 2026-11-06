using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Enemy definitions that share frame tables, with the frames each one's own programs and states can show (#1513): a
/// sheet of one of them may hold a frame of <paramref name="Tables"/> only when that definition can show it.
/// </summary>
/// <param name="Name">The family's name in failure messages.</param>
/// <param name="Tables">Every frame of the tables the definitions share.</param>
/// <param name="Own">The frames of <paramref name="Tables"/> each definition's own programs and states reach.</param>
internal sealed record EnemySheetOwnFrameFamily(
    string Name, IReadOnlySet<EnemyFrameId> Tables, IReadOnlyDictionary<EnemyDefinitionId, IReadOnlySet<EnemyFrameId>> Own);
