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

    /// <summary>Starts the tongue's <paramref name="segment"/> from its first step after <paramref name="updates"/> updates.</summary>
    internal static void StartCrocomireTongueAnimation(CrocomireEnemyState state, CrocomireTongueSegment segment, int updates)
    {
        RoomEnemyPlace tongue = state.Tongue ??
            throw new InvalidOperationException("Crocomire's tongue animation started without a tongue.");
        state.TonguePlayback = new AnimationPlayback<CrocomireTongueSegment>(CrocomireAnimations.TongueStartingWith(segment), AnimationPlace.StartOfAnimation);
        tongue.AnimationClock.RestartAfter(updates);
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
        if (state.Tongue is { } tongue)
        {
            int offset = (tongue.SpeciesState<CrocomireTongueState>()
                ?? throw new InvalidOperationException("Crocomire's tongue has no state.")).BodyXOffset;
            tongue.Position = tongue.Position.WithWhole(new(body.Position.X.Whole + offset, body.Position.Y.Whole));
            tongue.DrawOffset = Offset16<RoomPixels>.Zero;
        }
    }

    /// <summary>Emits smoke at the acid surface during Crocomire's death presentation.</summary>
    internal void SpawnCrocomireAcidSmoke(CrocomireEnemyState state)
    {
        CrocomireDeathState death = state.Death;
        if (--death.AcidSmokeTimer != 0)
            return;
        death.AcidSmokeTimer = CrocomireBridgeCollapseDefinitions.AcidSmokeInterval;

        int random = _access.ReadCrocomireRandom();
        // Smoke on the left rises one pixel further from the body than its mirror on the right; the smoke's X keeps
        // its wrap around the room where the offset is added.
        int xOffset = CrocomireRandomDefinitions.AcidSmokeXOffset.Of(random);
        if (!CrocomireRandomDefinitions.AcidSmokeOnRight.IsSetIn(random))
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
