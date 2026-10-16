namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The entrance statue's words and the unlock effects the statue sequence drives.</summary>
internal interface ITourianStatueActors
{
    /// <summary>The statue animation's busy and release bits.</summary>
    ushort AnimationState { get; set; }

    /// <summary>The statue's vertical offset as it sinks.</summary>
    short VerticalOffset { get; set; }

    /// <summary>Whether the sequence has finished.</summary>
    bool Finished { get; set; }

    /// <summary>The water surface the unlock effects follow.</summary>
    ushort WaterY { get; set; }

    /// <summary>Spawns an eye glow or, with <paramref name="soul"/>, a released soul.</summary>
    void SpawnUnlockEffect(ushort parameter, bool soul);

    /// <summary>Spawns the descent's dust.</summary>
    void SpawnDescentDust();
}
