using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The hardcoded PLMs and room writes enemy AI requested during one pass, in their producers' publication order. The
/// runtime applies them right after EnemyMain, and the room loader after enemy initialization.
/// </summary>
/// <param name="BotwoonWall">Botwoon's wall PLM: the defeated-room clear or the live crumble.</param>
/// <param name="SporeSpawnCeiling">Spore Spawn's ceiling PLM: the defeated-room clear or the death crumble.</param>
/// <param name="CrocomireObjects">Crocomire's arena mutations.</param>
/// <param name="KraidObjects">Kraid's ceiling and platform mutations.</param>
/// <param name="MotherBrainDeathStep">Mother Brain's death-sequence step and its palette, timer, boss-bit and event requests.</param>
/// <param name="MotherBrainFxEntry">Mother Brain's pending room FX entry index (one or two), or zero for none.</param>
/// <param name="MotherBrainObjects">Mother Brain's room mutations.</param>
/// <param name="ShitroidObjects">The Shitroid's wall mutations.</param>
/// <param name="TorizoBellyGlow">The belly glow a Torizo's fight start asks the palette-FX handler for, or null.</param>
public readonly record struct EnemyRoomEffectRequests(
    BotwoonWallMutation? BotwoonWall,
    SporeSpawnCeilingMutation? SporeSpawnCeiling,
    IReadOnlyList<RoomObjectSpawnRequest<CrocomireArenaMutation>> CrocomireObjects,
    IReadOnlyList<RoomObjectSpawnRequest<KraidRoomMutation>> KraidObjects,
    MotherBrainRainbowBeamAttackStepResult? MotherBrainDeathStep,
    int MotherBrainFxEntry,
    IReadOnlyList<RoomObjectSpawnRequest<MotherBrainRoomMutation>> MotherBrainObjects,
    IReadOnlyList<RoomObjectSpawnRequest<ShitroidWallMutation>> ShitroidObjects,
    PaletteFxObject? TorizoBellyGlow)
{

    /// <summary>Two request sets are equal when every value and every list's contents, in order, are equal.</summary>
    public bool Equals(EnemyRoomEffectRequests other) =>
        BotwoonWall == other.BotwoonWall && SporeSpawnCeiling == other.SporeSpawnCeiling &&
        CrocomireObjects.SequenceEqual(other.CrocomireObjects) && KraidObjects.SequenceEqual(other.KraidObjects) &&
        MotherBrainDeathStep == other.MotherBrainDeathStep && MotherBrainFxEntry == other.MotherBrainFxEntry &&
        MotherBrainObjects.SequenceEqual(other.MotherBrainObjects) && ShitroidObjects.SequenceEqual(other.ShitroidObjects) &&
        TorizoBellyGlow == other.TorizoBellyGlow;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(BotwoonWall, SporeSpawnCeiling, CrocomireObjects.Count, KraidObjects.Count,
        HashCode.Combine(MotherBrainDeathStep, MotherBrainFxEntry, MotherBrainObjects.Count, ShitroidObjects.Count, TorizoBellyGlow));
}
