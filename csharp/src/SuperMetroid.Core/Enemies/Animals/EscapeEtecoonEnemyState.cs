using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The state of the escape Etecoon.</summary>
public sealed class EscapeEtecoonEnemyState
{
    /// <summary>The actor's typed program cursor.</summary>
    internal AnimationProgramCursor<EscapeEtecoonProgram> Program { get; set; }


    /// <summary>
    /// The signed 8.8 walking velocity in pixels per update consumed by the walking behaviour; only its X is meaningful. A
    /// wall reverses it by negating it.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; set; }

    /// <summary>The installed pre-instruction.</summary>
    public EscapeEtecoonFrameBehavior? FrameBehavior { get; set; }
}
