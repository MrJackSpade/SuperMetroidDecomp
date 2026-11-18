namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// How far forward a Torizo's body moves, in whole pixels, after each pose of a step on one leg (#1891, #2411): each walking
/// cue after a pose moves it by that much. Both Torizos and the faceless Bomb Torizo step alike on each leg.
/// </summary>
/// <param name="AfterLifting">After the leg lifts.</param>
/// <param name="AfterSwinging">After the leg swings.</param>
/// <param name="AfterReaching">After the leg reaches.</param>
/// <param name="AfterPlanting">After the leg plants.</param>
/// <param name="AfterPlanted">After the leg stands planted, before the next step lifts the other.</param>
internal sealed record TorizoStridePaces(int AfterLifting, int AfterSwinging, int AfterReaching, int AfterPlanting, int AfterPlanted);
