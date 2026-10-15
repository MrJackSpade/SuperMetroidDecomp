namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One of the Bomb Torizo hand's cues (#1296): its kind and, for a fragment, which one.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Fragment">The fragment <see cref="BombTorizoHandCueKind.SpawnFragment"/> spawns.</param>
internal readonly record struct BombTorizoHandCue(BombTorizoHandCueKind Kind, int Fragment = 0)
{
    /// <summary>The fragment's bank-$86 spawn parameter: two bytes per fragment.</summary>
    internal ushort FragmentParameter => checked((ushort)(Fragment * sizeof(ushort)));
}
