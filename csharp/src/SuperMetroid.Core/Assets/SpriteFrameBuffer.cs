using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// One frame's sprite description: the whole-frame draws of every actor, in emission order, frontmost first. Gameplay
/// builds one per frame and the frame boundary latches it; backends draw it directly.
/// </summary>
public sealed partial class SpriteFrameBuffer
{
    /// <summary>Draws appended since <see cref="BeginFrame"/>.</summary>
    private readonly List<SpriteDraw> pending = [];

    /// <summary>The finalized draws; never mutated once published, so <see cref="Draws"/> can share it.</summary>
    private SpriteDraw[] finalized = [];

    /// <summary>The draws of the most recently finalized (or latched) frame, in emission order.</summary>
    public ImmutableArray<SpriteDraw> Draws => ImmutableCollectionsMarshal.AsImmutableArray(finalized);

    /// <summary>Starts building a frame: clears the pending draws.</summary>
    public void BeginFrame() => pending.Clear();

    /// <summary>Appends a whole-frame draw after every draw emitted so far this frame.</summary>
    public void Add(SpriteDraw draw) => pending.Add(Ensure.NotNull(draw));

    /// <summary>What <c>Add</c> does for each <see cref="ActorLayering"/> member, each member's own arm.</summary>
    private IdentityTable<ActorLayering, Action<SpriteDraw>> AddForActorLayering => field ??= new(new Dictionary<ActorLayering, Action<SpriteDraw>>
    {
        [ActorLayering.InDrawOrder] = draw => Add(draw),
        [ActorLayering.InFrontOfActors] = draw => pending.Insert(0, Ensure.NotNull(draw)),
    });

    /// <summary>Adds a whole-frame draw as <paramref name="layering"/> places it among the draws emitted so far this frame (#2573).</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="layering"/> is undefined.</exception>
    public void Add(SpriteDraw draw, ActorLayering layering)
    {
        AddForActorLayering[layering](draw);
    }

    /// <summary>
    /// The place after every draw emitted so far this frame (#2618), where draws emitted later can be inserted with
    /// <see cref="InsertAt"/>: in front of everything emitted after it.
    /// </summary>
    public SpriteDrawPlace NextPlace() => new(pending.Count == 0 ? null : pending[^1]);

    /// <summary>
    /// Inserts <paramref name="drawn"/>'s draws, in their order, at <paramref name="place"/> (#2618): behind the draws
    /// emitted before the place was taken and in front of every draw emitted after it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The draw the place follows is no longer among this frame's draws.</exception>
    public void InsertAt(SpriteDrawPlace place, SpriteFrameBuffer drawn)
    {
        Ensure.NotNull(drawn);
        int index = 0;
        if (place.After is { } after)
        {
            index = pending.FindIndex(draw => ReferenceEquals(draw, after)) + 1;
            if (index == 0)
                throw new InvalidOperationException("The draw a sprite place follows is not among this frame's draws.");
        }
        pending.InsertRange(index, drawn.pending);
    }

    /// <summary>Finishes the frame: publishes the pending draws as <see cref="Draws"/>.</summary>
    public void FinalizeFrame()
    {
        finalized = [.. pending];
        pending.Clear();
    }

    /// <summary>Makes a finalized frame the displayed one, as the frame boundary does.</summary>
    /// <param name="built">The buffer the update built and finalized.</param>
    public void LatchFrom(SpriteFrameBuffer built) => finalized = Ensure.NotNull(built).finalized;
}
