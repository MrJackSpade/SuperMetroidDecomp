using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The state of the Multiviola. Retail recomputes its velocity from the population angle and speed every active frame,
/// with two deliberately imperfect reflection negations.
/// </summary>
public sealed class MultiviolaEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<MultiviolaProgram> Program { get; set; }

    /// <summary>Signed 16.16 velocity in room pixels per update.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The flight heading, from its spawn settings, reflected by exclusive-or at each wall or floor; its velocity is
    /// rebuilt from it every update.
    /// </summary>
    public ByteAngle Heading { get; internal set; }

    /// <summary>The flight speed, from its spawn settings, multiplied with the heading's sine and cosine.</summary>
    public int Speed { get; internal set; }
}
