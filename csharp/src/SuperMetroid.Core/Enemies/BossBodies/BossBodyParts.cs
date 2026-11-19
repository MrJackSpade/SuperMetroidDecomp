using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss's body as its parts show it (#1515, #1540): the frame each part of the boss last showed, layered in the order its
/// boss draws its parts (<see cref="BossBodyPartDefinitions"/>), and the row above which alone the body shows. Phantoon's
/// body, eye, tentacles and mouth, Draygon's body, eye and dying arms, Crocomire's body and melt images, and Mother
/// Brain's body show their frames here; a part shows its current frame alone, in place of the frame it showed before.
/// </summary>
internal sealed class BossBodyParts : IBossBodyDisplay
{
    /// <summary>Where the boss places the body and whether it shows nothing, or none for a body no boss places (#2194).</summary>
    private readonly IBossBodyShowing? showing;

    /// <summary>The boss whose body it is.</summary>
    private readonly BossBodyOwner owner;

    /// <summary>The boss's parts, in draw order.</summary>
    private readonly ImmutableArray<EnemyDefinitionId> parts;

    /// <summary>The frame each part shows, by its place in <see cref="parts"/>, or null before it shows one.</summary>
    private readonly BossBodyFrame?[] frames;

    /// <summary>The row, reckoned from the body's placement, above which alone the body shows, or null when the whole body shows.</summary>
    private int? shownAbove;

    /// <summary>The body as it stands, until a part shows another frame or the body is cleared or cut.</summary>
    private BossBodyLayer? published;

    /// <summary>A body of <paramref name="owner"/>'s no part shows a frame on yet, which the room shows from the start.</summary>
    /// <exception cref="InvalidOperationException">The boss draws its own body, as Kraid does.</exception>
    public BossBodyParts(BossBodyOwner owner)
        : this(owner, null)
    {
    }

    /// <summary>
    /// A body of <paramref name="owner"/>'s no part shows a frame on yet, which the room shows from the start, placed and
    /// hidden as <paramref name="showing"/> says (#2194).
    /// </summary>
    /// <exception cref="InvalidOperationException">The boss draws its own body, as Kraid does.</exception>
    public BossBodyParts(BossBodyOwner owner, IBossBodyShowing? showing)
    {
        this.showing = showing;
        parts = BossBodyPartDefinitions.DrawOrders.TryGetValue(owner, out ImmutableArray<EnemyDefinitionId> order) ? order
            : throw new InvalidOperationException($"{owner} draws its own body; it shows no parts' frames.");
        frames = new BossBodyFrame?[parts.Length];
        this.owner = owner;
        IsShown = true;
    }

    /// <summary>
    /// A body of <paramref name="owner"/>'s the room shows only once it is revealed, placed and hidden as
    /// <paramref name="showing"/> says (#2194).
    /// </summary>
    public static BossBodyParts Hidden(BossBodyOwner owner, IBossBodyShowing? showing) => new(owner, showing) { IsShown = false };

    /// <summary>Whether the room shows the body: false only while Mother Brain readies hers before her second phase (#1356).</summary>
    public bool IsShown { get; private set; }

    /// <summary>The room shows the body from now on, as Mother Brain's second phase begins (#1356).</summary>
    public void Reveal() => IsShown = true;

    /// <summary>
    /// Part <paramref name="part"/> of the boss shows <paramref name="frame"/> from now on, in place of the frame it showed.
    /// A frame shown shows the body whole again.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="part"/> is not one of the parts the boss draws on its body.</exception>
    public void Show(EnemyDefinitionId part, BossBodyFrame frame)
    {
        int place = parts.IndexOf(part);
        if (place < 0)
            throw new InvalidOperationException($"{part} is not a part {owner} draws on its body.");
        frames[place] = Ensure.NotNull(frame);
        shownAbove = null;
        published = null;
    }

    /// <summary>No part shows anything on the body, as a boss's death clears it.</summary>
    public void Clear()
    {
        Array.Clear(frames);
        shownAbove = null;
        published = null;
    }

    /// <summary>
    /// Shows the body only above row <paramref name="top"/>, reckoned from its placement, until a part next shows a frame, each part's picture cut
    /// there, as the acid hides Crocomire's sinking body; a lower row shows no more than shows already.
    /// </summary>
    public void ShowAbove(int top)
    {
        if (shownAbove is { } shown && shown <= top)
            return;
        shownAbove = top;
        published = null;
    }

    /// <summary>The body as it stands, shared until it changes.</summary>
    public BossBodyLayer Publish() => published ??= new BossBodyLayer([.. frames.OfType<BossBodyFrame>()], shownAbove);

    /// <inheritdoc />
    BossBodyLayer? IBossBodyDisplay.Layer => IsShown ? Publish() : null;

    /// <inheritdoc />
    Position16<RoomPixels>? IBossBodyDisplay.Placement => showing?.BodyPlacement;

    /// <inheritdoc />
    bool IBossBodyDisplay.Hidden => showing?.BodyHidden == true;

    /// <inheritdoc/>
    bool IBossBodyDisplay.HidesBackgroundLayer => showing?.HidesBackgroundLayer == true;
}
