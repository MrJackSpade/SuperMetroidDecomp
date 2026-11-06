using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The hardcoded PLMs and room writes enemy AI requested during one pass, in their producers' publication order. The
/// runtime applies them right after EnemyMain, and the room loader after enemy initialization.
/// </summary>
/// <param name="BotwoonWall">Botwoon's wall PLM: the defeated-room clear or the live crumble.</param>
/// <param name="SporeSpawnCeiling">Spore Spawn's ceiling PLM: the defeated-room clear or the death crumble.</param>
/// <param name="CrocomirePlms">Crocomire's arena mutations.</param>
/// <param name="KraidPlms">Kraid's ceiling and platform mutations.</param>
/// <param name="MotherBrainDeathStep">Mother Brain's death-sequence step and its palette, timer, boss-bit and event requests.</param>
/// <param name="MotherBrainFxEntry">Mother Brain's pending room FX entry index (one or two), or zero for none.</param>
/// <param name="MotherBrainPlms">Mother Brain's room mutations.</param>
/// <param name="ShitroidPlms">The Shitroid's wall mutations.</param>
/// <param name="TorizoBellyGlow">The belly glow a Torizo's fight start asks the palette-FX handler for, or null.</param>
public readonly record struct EnemyRoomEffectRequests(
    BotwoonWallMutation? BotwoonWall,
    SporeSpawnCeilingMutation? SporeSpawnCeiling,
    IReadOnlyList<PlmSpawnRequest<CrocomireArenaMutation>> CrocomirePlms,
    IReadOnlyList<PlmSpawnRequest<KraidRoomMutation>> KraidPlms,
    MotherBrainRainbowBeamAttackStepResult? MotherBrainDeathStep,
    int MotherBrainFxEntry,
    IReadOnlyList<PlmSpawnRequest<MotherBrainRoomMutation>> MotherBrainPlms,
    IReadOnlyList<PlmSpawnRequest<ShitroidWallMutation>> ShitroidPlms,
    PaletteFxObject? TorizoBellyGlow)
{

    /// <summary>Two request sets are equal when every value and every list's contents, in order, are equal.</summary>
    public bool Equals(EnemyRoomEffectRequests other) =>
        BotwoonWall == other.BotwoonWall && SporeSpawnCeiling == other.SporeSpawnCeiling &&
        CrocomirePlms.SequenceEqual(other.CrocomirePlms) && KraidPlms.SequenceEqual(other.KraidPlms) &&
        MotherBrainDeathStep == other.MotherBrainDeathStep && MotherBrainFxEntry == other.MotherBrainFxEntry &&
        MotherBrainPlms.SequenceEqual(other.MotherBrainPlms) && ShitroidPlms.SequenceEqual(other.ShitroidPlms) &&
        TorizoBellyGlow == other.TorizoBellyGlow;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(BotwoonWall, SporeSpawnCeiling, CrocomirePlms.Count, KraidPlms.Count,
        HashCode.Combine(MotherBrainDeathStep, MotherBrainFxEntry, MotherBrainPlms.Count, ShitroidPlms.Count, TorizoBellyGlow));
}
