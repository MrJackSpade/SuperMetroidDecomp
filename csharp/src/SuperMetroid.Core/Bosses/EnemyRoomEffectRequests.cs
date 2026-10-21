using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The hardcoded PLMs and room writes enemy AI requested during one pass, in their producers' publication order. The
/// runtime applies them right after EnemyMain, and the room loader after enemy initialization.
/// </summary>
/// <param name="BotwoonWall">Botwoon's wall PLM: the defeated-room clear or the live crumble.</param>
/// <param name="SporeSpawnCeiling">Spore Spawn's ceiling PLM.</param>
/// <param name="CrocomirePlms">Crocomire's arena mutations.</param>
/// <param name="KraidPlms">Kraid's ceiling and platform mutations.</param>
/// <param name="MotherBrainDeathStep">Mother Brain's death-sequence step and its palette, timer, boss-bit and event requests.</param>
/// <param name="MotherBrainFxEntry">Mother Brain's pending room FX entry index (one or two), or zero for none.</param>
/// <param name="MotherBrainPlms">Mother Brain's room mutations.</param>
/// <param name="ShitroidPlms">The Shitroid's wall mutations.</param>
public readonly record struct EnemyRoomEffectRequests(
    PlmHeaderId? BotwoonWall,
    PlmSpawnRequest? SporeSpawnCeiling,
    IReadOnlyList<PlmSpawnRequest> CrocomirePlms,
    IReadOnlyList<PlmSpawnRequest> KraidPlms,
    MotherBrainRainbowBeamAttackStepResult? MotherBrainDeathStep,
    int MotherBrainFxEntry,
    IReadOnlyList<PlmSpawnRequest> MotherBrainPlms,
    IReadOnlyList<PlmSpawnRequest> ShitroidPlms)
{

    /// <summary>Two request sets are equal when every value and every list's contents, in order, are equal.</summary>
    public bool Equals(EnemyRoomEffectRequests other) =>
        BotwoonWall == other.BotwoonWall && SporeSpawnCeiling == other.SporeSpawnCeiling &&
        CrocomirePlms.SequenceEqual(other.CrocomirePlms) && KraidPlms.SequenceEqual(other.KraidPlms) &&
        MotherBrainDeathStep == other.MotherBrainDeathStep && MotherBrainFxEntry == other.MotherBrainFxEntry &&
        MotherBrainPlms.SequenceEqual(other.MotherBrainPlms) && ShitroidPlms.SequenceEqual(other.ShitroidPlms);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(BotwoonWall, SporeSpawnCeiling, CrocomirePlms.Count, KraidPlms.Count,
        MotherBrainDeathStep, MotherBrainFxEntry, MotherBrainPlms.Count, ShitroidPlms.Count);
}
