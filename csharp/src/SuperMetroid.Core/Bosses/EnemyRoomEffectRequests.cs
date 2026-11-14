using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The fixed room objects and room writes enemy AI requested during one pass, in their producers' publication order.
/// The runtime applies them right after the enemies' update, and the room loader after enemy initialization.
/// </summary>
/// <param name="BotwoonWall">Botwoon's wall room object: the defeated-room clear or the live crumble.</param>
/// <param name="SporeSpawnCeiling">Spore Spawn's ceiling room object: the defeated-room clear or the death crumble.</param>
/// <param name="CrocomireObjects">Crocomire's arena mutations.</param>
/// <param name="KraidObjects">Kraid's ceiling and platform mutations.</param>
/// <param name="MotherBrainDeathStep">Mother Brain's death-sequence step and its palette, timer, defeated-boss and event requests.</param>
/// <param name="MotherBrainAcidChange">The change Mother Brain's fight makes to her room's acid (<see cref="MotherBrainRoomFxDefinitions"/>), or null for none.</param>
/// <param name="MotherBrainObjects">Mother Brain's room mutations.</param>
/// <param name="ShitroidObjects">The Shitroid's wall mutations.</param>
/// <param name="TorizoBellyGlow">The belly glow a Torizo's fight start asks the palette-FX handler for, with the Torizo it lights, or null.</param>
/// <param name="PhantoonDoor">Phantoon's door change: the fight's door or the restored door, or null for none.</param>
public readonly record struct EnemyRoomEffectRequests(
    BotwoonWallMutation? BotwoonWall,
    SporeSpawnCeilingMutation? SporeSpawnCeiling,
    IReadOnlyList<RoomObjectSpawnRequest<CrocomireArenaMutation>> CrocomireObjects,
    IReadOnlyList<RoomObjectSpawnRequest<KraidRoomMutation>> KraidObjects,
    MotherBrainRainbowBeamAttackStepResult? MotherBrainDeathStep,
    RoomEffectDefinition? MotherBrainAcidChange,
    IReadOnlyList<RoomObjectSpawnRequest<MotherBrainRoomMutation>> MotherBrainObjects,
    IReadOnlyList<RoomObjectSpawnRequest<ShitroidWallMutation>> ShitroidObjects,
    TorizoBellyGlowRequest? TorizoBellyGlow,
    PhantoonDoorChange? PhantoonDoor)
{

    /// <summary>Two request sets are equal when every value and every list's contents, in order, are equal.</summary>
    public bool Equals(EnemyRoomEffectRequests other) =>
        BotwoonWall == other.BotwoonWall && SporeSpawnCeiling == other.SporeSpawnCeiling &&
        CrocomireObjects.SequenceEqual(other.CrocomireObjects) && KraidObjects.SequenceEqual(other.KraidObjects) &&
        MotherBrainDeathStep == other.MotherBrainDeathStep && MotherBrainAcidChange == other.MotherBrainAcidChange &&
        MotherBrainObjects.SequenceEqual(other.MotherBrainObjects) && ShitroidObjects.SequenceEqual(other.ShitroidObjects) &&
        TorizoBellyGlow == other.TorizoBellyGlow && PhantoonDoor == other.PhantoonDoor;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(BotwoonWall, SporeSpawnCeiling, CrocomireObjects.Count, KraidObjects.Count,
        HashCode.Combine(MotherBrainDeathStep, MotherBrainAcidChange, MotherBrainObjects.Count, ShitroidObjects.Count, TorizoBellyGlow, PhantoonDoor));
}
