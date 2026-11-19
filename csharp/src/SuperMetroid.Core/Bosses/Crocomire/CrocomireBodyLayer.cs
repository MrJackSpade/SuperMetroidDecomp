using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's body: what it shows, the rows of it the acid hides, the melt's stretch of it, the tongue placed with it and the acid smoke drawn with it.</summary>
[RoomLifetime]
internal sealed class CrocomireBodyLayer
{
    /// <summary>Allocates arena debris and cinematic effect actors.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>Draws Crocomire's body, skeleton and melt, and holds the colours and background pictures they show.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Crocomire's state lookups, body animation start, room objects, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;
    /// <summary>The room's effect, whose acid surface his acid smoke rises from.</summary>
    private readonly EnemyRoomFx _roomFx;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireBodyLayer(RoomEffectActors effectActors, EnemyPresentation presentation, CrocomireEncounterAccess access, EnemyRoomFx roomFx)
    {
        _effectActors = effectActors;
        _presentation = presentation;
        _access = access;
        _roomFx = roomFx;
    }

    /// <summary>Starts the tongue's <paramref name="segment"/>, its first step playing on the next update.</summary>
    internal static void StartCrocomireTongueAnimation(CrocomireEnemyState state, CrocomireTongueSegment segment)
    {
        state.TonguePlayback = new AnimationPlayback<CrocomireTongueSegment>(CrocomireAnimations.TongueStartingWith(segment), AnimationPlace.StartOfAnimation);
        state.Tongue.AnimationClock.Restart();
    }

    /// <summary>
    /// Hides the tongue as the bridge collapses under him (#2463): it plays its hidden animation, which shows nothing, and
    /// its clock stops, so the frame it last showed stays until his first melt starts its melting animation.
    /// </summary>
    internal static void HideCrocomireTongue(CrocomireEnemyState state)
    {
        state.TonguePlayback = new AnimationPlayback<CrocomireTongueSegment>(CrocomireAnimations.TongueStartingWith(CrocomireTongueSegment.Hidden), AnimationPlace.StartOfAnimation);
        state.Tongue.AnimationClock.Stop();
        state.Tongue.Presence = state.Tongue.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>
    /// His body shows <paramref name="frame"/> from now on in place of whatever it showed (#1355, #1515, #1540), as a melt
    /// image replaces it: his body is the one part he draws on it.
    /// </summary>
    internal void ShowCrocomireBodyFrame(BossBodyFrame frame) =>
        _presentation.RequireBossBody().Show(EnemyDefinitionId.Crocomire, frame);

    /// <summary>His body shows nothing from now on, until a frame is shown on it again (#1515).</summary>
    internal void ClearCrocomireBody() => _presentation.RequireBossBody().Clear();

    /// <summary>
    /// Hides his body from room row <paramref name="roomRow"/> down, until a frame is shown on it again (#1515, #2031): his
    /// body's picture shows only above that row, reckoned from where he lies.
    /// </summary>
    internal void HideCrocomireBodyBelow(CrocomireEnemyState state, int roomRow) =>
        _presentation.RequireBossBody().ShowAbove(roomRow - state.Body.Position.Y.Whole);

    /// <summary>Levels the melt's stretch: every line shows his body as it lies now (#1468, #1802, #2031).</summary>
    internal static void LevelCrocomireMeltDistortion(CrocomireEnemyState state) =>
        state.Death.MeltDistortion = CrocomireMeltDistortion.Level(state.Body.Position.Y.Whole);

    /// <summary>
    /// Places the tongue at the body: a normal second enemy, it lies at its body-relative X offset, and every placement of
    /// his body during the fight and his death moves it there, even while the body's bulk shows as his body's pictures.
    /// The tongue's picture is drawn where the tongue lies until his steps move his body later in the update (#2031).
    /// </summary>
    internal static void PlaceCrocomireTongue(CrocomireEnemyState state)
    {
        RoomEnemyPlace body = state.Body;
        RoomEnemyPlace tongue = state.Tongue;
        tongue.Position = tongue.Position.WithWhole(new(body.Position.X.Whole + CrocomireFightDefinitions.TongueBodyXOffset, body.Position.Y.Whole));
        tongue.DrawOffset = Offset16<RoomPixels>.Zero;
    }

    /// <summary>
    /// Emits smoke at the acid surface during Crocomire's death each time its wait runs out, every
    /// <see cref="CrocomireBridgeCollapseDefinitions.AcidSmokeInterval"/> updates.
    /// </summary>
    internal void SpawnCrocomireAcidSmoke(CrocomireEnemyState state)
    {
        CrocomireDeathState death = state.Death;
        if (death.AcidSmokeWait.StepKeepsRunning())
            return;
        death.AcidSmokeWait.LastFor(CrocomireBridgeCollapseDefinitions.AcidSmokeInterval);

        int random = _access.ReadCrocomireRandom();
        // Smoke on the left rises one pixel further from the body than its mirror on the right.
        int xOffset = CrocomireRandomDefinitions.AcidSmokeXOffset.Of(random);
        if (!CrocomireRandomDefinitions.AcidSmokeRisesOnRight.IsTakenBy(random))
            xOffset = -(xOffset + CrocomireRandomDefinitions.AcidSmokeLeftExtraPixels);
        // Crocomire's room always has its acid surface, which its tide only moves; a surface above the room's top is none.
        RoomFxOverlayState roomFx = _roomFx.FxOverlay;
        int acidY = roomFx.Type is RoomFxType.Lava or RoomFxType.Acid && roomFx.Surface is int surface and >= 0
            ? surface
            : throw new InvalidOperationException("Crocomire's acid smoke rises from his room's acid surface.");
        _effectActors.SpawnRoomEffectActor(
            new(state.Body.Position.X.Whole + xOffset, acidY + CrocomireBridgeCollapseDefinitions.AcidSmokeDepth - CrocomireRandomDefinitions.AcidSmokeRise.Of(random)),
            RoomEffectActorKind.BigDustCloud,
            graphics: default);
    }
}
