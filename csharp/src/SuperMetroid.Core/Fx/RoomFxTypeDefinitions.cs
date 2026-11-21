namespace SuperMetroid.Core.Fx;

/// <summary>Each kind of room effect (<see cref="RoomFxType"/>).</summary>
internal static class RoomFxTypeDefinitions
{
    /// <summary>
    /// Each kind: lava and acid, water and the Tourian entrance statue's water (whose second motion moves the statue rather
    /// than the background's wave), rain, spores and fog show on the overlay; lava alone bubbles; the scrolling sky's effect spawns its drifting bands; the Firefleas' effect
    /// starts their darkness; the rest show nothing there.
    /// </summary>
    internal static readonly IdentityTable<RoomFxType, RoomFxTypeDefinition> Types = new(new Dictionary<RoomFxType, RoomFxTypeDefinition>
    {
        [RoomFxType.None] = new(RoomFxOverlayMotion.None),
        [RoomFxType.Lava] = new(RoomFxOverlayMotion.LavaOrAcid, BubblesAmbiently: true),
        [RoomFxType.Acid] = new(RoomFxOverlayMotion.LavaOrAcid),
        [RoomFxType.Water] = new(RoomFxOverlayMotion.Water, WavesBackground: true),
        [RoomFxType.Spores] = new(RoomFxOverlayMotion.Spores),
        [RoomFxType.Rain] = new(RoomFxOverlayMotion.Rain),
        [RoomFxType.Fog] = new(RoomFxOverlayMotion.Fog),
        [RoomFxType.ScrollingSky] = new(RoomFxOverlayMotion.None, SpawnsSkyBands: true),
        [RoomFxType.Fireflea] = new(RoomFxOverlayMotion.None, DarkensWithFirefleas: true),
        [RoomFxType.TourianEntranceStatue] = new(RoomFxOverlayMotion.Water),
        [RoomFxType.CeresRidley] = new(RoomFxOverlayMotion.None),
        [RoomFxType.CeresElevator] = new(RoomFxOverlayMotion.None),
        [RoomFxType.CeresHaze] = new(RoomFxOverlayMotion.None),
    });
}
