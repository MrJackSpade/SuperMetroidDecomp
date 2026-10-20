namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The entrance statue's state and the unlock effects the statue sequence drives.</summary>
internal interface ITourianStatueActors
{
    /// <summary>Which statues are animating, whether a release is in progress, and whether all four have released.</summary>
    TourianStatueAnimationState AnimationState { get; set; }

    /// <summary>The statue's vertical offset as it sinks.</summary>
    short VerticalOffset { get; set; }

    /// <summary>Whether the sequence has finished.</summary>
    bool Finished { get; set; }

    /// <summary>The water surface the unlock effects follow; only its Y axis is meaningful.</summary>
    Position16<RoomPixels> WaterSurface { get; set; }

    /// <summary>Spawns a statue eye's glow.</summary>
    void SpawnEyeGlow(ushort parameter);

    /// <summary>Spawns a statue's released soul.</summary>
    void SpawnSoul(ushort parameter);

    /// <summary>Spawns the descent's dust.</summary>
    void SpawnDescentDust();
}
