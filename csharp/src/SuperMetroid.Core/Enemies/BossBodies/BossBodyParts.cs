using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss's body as its parts show it (#1515, #1540): the frame each part of the boss last showed, layered in the order its
/// boss draws its parts (<see cref="BossBodyPartDefinitions"/>), and the row above which alone the body shows. Phantoon's
/// body, eye, tentacles and mouth, Draygon's body, eye and dying arms, Crocomire's body and melt images, and Mother
/// Brain's body show their frames here; a part shows its current frame alone, in place of the frame it showed before.
/// The room shows the body placed and hidden as its boss says (<see cref="PlacedBossBody"/>).
/// </summary>
internal sealed partial class BossBodyParts
{
    /// <summary>The boss whose body it is.</summary>
    private readonly BossBodyOwner owner;

    /// <summary>The boss's parts, in draw order.</summary>
    private readonly ImmutableArray<EnemyDefinitionId> parts;

    /// <summary>The frame each part shows, by part; a part that has shown none has none.</summary>
    private readonly Dictionary<EnemyDefinitionId, BossBodyFrame> frames = [];

    /// <summary>
    /// The frame each part's animation last showed on the body, by part; a part whose animation has shown none there has
    /// none (#2573). Clearing the body, or a part showing a picture of its own, leaves it as it was.
    /// </summary>
    private readonly Dictionary<EnemyDefinitionId, EnemyFrameId> selections = [];

    /// <summary>The row, reckoned from the body's placement, above which alone the body shows, or null when the whole body shows.</summary>
    private int? shownAbove;

    /// <summary>The body as it stands, until a part shows another frame or the body is cleared or cut.</summary>
    private BossBodyLayer? published;

    /// <summary>A body of <paramref name="owner"/>'s no part shows a frame on yet, which the room shows from the start.</summary>
    /// <exception cref="InvalidOperationException">The boss draws its own body, as Kraid does.</exception>
    public BossBodyParts(BossBodyOwner owner)
    {
        parts = BossBodyPartDefinitions.DrawOrders.TryGetValue(owner, out ImmutableArray<EnemyDefinitionId> order) ? order
            : throw new InvalidOperationException($"{owner} draws its own body; it shows no parts' frames.");
        this.owner = owner;
        IsShown = true;
    }

    /// <summary>A body of <paramref name="owner"/>'s the room shows only once it is revealed.</summary>
    public static BossBodyParts Hidden(BossBodyOwner owner) => new(owner) { IsShown = false };

    /// <summary>Whether the room shows the body: false only while Mother Brain readies hers before her second phase (#1356).</summary>
    internal bool IsShown { get; set; }

    /// <summary>The room shows the body from now on, as Mother Brain's second phase begins (#1356).</summary>
    public void Reveal() => IsShown = true;

    /// <summary>
    /// Part <paramref name="part"/> of the boss shows <paramref name="frame"/> from now on, in place of the frame it showed.
    /// A frame shown shows the body whole again.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="part"/> is not one of the parts the boss draws on its body.</exception>
    public void Show(EnemyDefinitionId part, BossBodyFrame frame)
    {
        frames[Drawn(part)] = Ensure.NotNull(frame);
        shownAbove = null;
        published = null;
    }

    /// <summary>
    /// Part <paramref name="part"/>'s animation shows <paramref name="selected"/>, whose body frame is
    /// <paramref name="frame"/>, on the body from now on (#2573), as <see cref="Show"/> does.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="part"/> is not one of the parts the boss draws on its body.</exception>
    public void ShowSelected(EnemyDefinitionId part, EnemyFrameId selected, BossBodyFrame frame)
    {
        Show(part, frame);
        selections[Drawn(part)] = selected;
    }

    /// <summary>Whether the frame part <paramref name="part"/>'s animation last showed on the body is <paramref name="selected"/> (#2573).</summary>
    /// <exception cref="InvalidOperationException"><paramref name="part"/> is not one of the parts the boss draws on its body.</exception>
    public bool ShowsSelected(EnemyDefinitionId part, EnemyFrameId selected) =>
        selections.TryGetValue(Drawn(part), out EnemyFrameId shown) && shown == selected;

    /// <summary><paramref name="part"/>, when it is one of the parts the boss draws on its body.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="part"/> is not one of the parts the boss draws on its body.</exception>
    private EnemyDefinitionId Drawn(EnemyDefinitionId part) =>
        parts.Contains(part) ? part : throw new InvalidOperationException($"{part} is not a part {owner} draws on its body.");

    /// <summary>No part shows anything on the body, as a boss's death clears it.</summary>
    public void Clear()
    {
        frames.Clear();
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
    internal BossBodyLayer Publish() => published ??= new BossBodyLayer([.. parts.Where(frames.ContainsKey).Select(part => frames[part])], shownAbove);
}
