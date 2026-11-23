namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>What one of the Etecoons' routes does beside its run (<see cref="EtecoonRouteDefinitions.Routes"/>).</summary>
/// <param name="CriesOnWaking">Whether its Etecoon cries out as it wakes.</param>
/// <param name="TakesLongRun">Whether, on landing a teaching hop short of the long run's end, its Etecoon waits for Samus before the long run.</param>
/// <param name="RepeatsLessons">Whether its Etecoon hops lesson after lesson once a lesson ends.</param>
internal sealed partial record EtecoonRouteDefinition(bool CriesOnWaking, bool TakesLongRun, bool RepeatsLessons);
