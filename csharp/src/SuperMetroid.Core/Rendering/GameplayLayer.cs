namespace SuperMetroid.Core.Rendering;

/// <summary>
/// One of the layers the gameplay scene below the HUD is drawn as, named for what it shows (#1514). Which of the room's
/// tiles are its back tiles and which its front tiles is level design: an actor behind the front tiles passes behind a
/// pillar or a door frame and in front of the wall behind it. <see cref="GameplaySceneLayer.Order"/> gives their order from
/// back to front; the backdrop lies behind them all, and the effect overlay, the glows and the HUD are drawn over them by
/// the scene's later layers.
/// </summary>
public enum GameplayLayer
{
    /// <summary>
    /// The actors drawn behind the whole room, at both depths behind it (<see cref="Assets.ActorDepth.BehindRoom"/> and
    /// <see cref="Assets.ActorDepth.BehindRoomInFrontOfFloor"/>), which only a turning floor tells apart (#2583).
    /// </summary>
    ActorsBehindRoom,

    /// <summary>The room's background, its back tiles.</summary>
    BackgroundBackPart,

    /// <summary>A boss's body's parts drawn behind the actors (#2573).</summary>
    BossBodyBehindActors,

    /// <summary>The room's foreground, its back tiles.</summary>
    ForegroundBackPart,

    /// <summary>The actors drawn behind the room's front tiles (<see cref="Assets.ActorDepth.BehindFrontParts"/>).</summary>
    ActorsBehindFrontParts,

    /// <summary>The room's background, its front tiles.</summary>
    BackgroundFrontPart,

    /// <summary>A boss's body's parts drawn in front of the actors (#2573).</summary>
    BossBodyInFrontOfActors,

    /// <summary>The room's foreground, its front tiles.</summary>
    ForegroundFrontPart,

    /// <summary>The actors drawn in front of the whole room (<see cref="Assets.ActorDepth.InFront"/>).</summary>
    ActorsInFront,
}
