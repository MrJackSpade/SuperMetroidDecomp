namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// A run of a room effect actor's animation (#2275, #2369): the run each actor starts with, named by the actor itself, and
/// the cycles a Metroid's electricity and shell loop after their first pass. Its beats are
/// <see cref="RoomEffectActorAnimations"/>.
/// </summary>
internal readonly record struct RoomEffectActorSegment
{
    /// <summary>The run <paramref name="actor"/> starts with.</summary>
    private RoomEffectActorSegment(RoomEffectActorKind actor) => Actor = actor;

    /// <summary>The actor whose animation the run belongs to.</summary>
    internal RoomEffectActorKind Actor { get; }

    /// <summary>Whether the run is the cycle the actor loops after its first pass, rather than the run it starts with.</summary>
    internal bool IsCycle { get; private init; }

    /// <summary>A Metroid's electricity's cycle, which loops after its first pass.</summary>
    internal static RoomEffectActorSegment MetroidElectricityCycle { get; } = new(RoomEffectActorKind.MetroidElectricity) { IsCycle = true };

    /// <summary>A Metroid's shell's cycle, which loops after its first pass.</summary>
    internal static RoomEffectActorSegment MetroidShellCycle { get; } = new(RoomEffectActorKind.MetroidShell) { IsCycle = true };

    /// <summary>Every room effect actor in the order its runs and its sheets' pictures are listed (#2567), written out so reordering the kinds' members reorders neither.</summary>
    internal static IReadOnlyList<RoomEffectActorKind> Kinds { get; } =
    [
        RoomEffectActorKind.SmallExplosion,
        RoomEffectActorKind.Smoke,
        RoomEffectActorKind.DudShot,
        RoomEffectActorKind.RisingDustPuff,
        RoomEffectActorKind.SmokePuff,
        RoomEffectActorKind.BigExplosion,
        RoomEffectActorKind.ShortBigDustCloud,
        RoomEffectActorKind.BigDustCloud,
        RoomEffectActorKind.DraygonBreathBubbles,
        RoomEffectActorKind.NuclearWaffleBody,
        RoomEffectActorKind.NuclearWaffleRightExplosion,
        RoomEffectActorKind.NuclearWaffleLeftExplosion,
        RoomEffectActorKind.NuclearWaffleSplash,
        RoomEffectActorKind.FallingSparkTrail,
        RoomEffectActorKind.MetroidElectricity,
        RoomEffectActorKind.MetroidShell,
        RoomEffectActorKind.YappingMawBaseFacingDown,
        RoomEffectActorKind.YappingMawBaseFacingUp,
        RoomEffectActorKind.EvirFacingLeft,
        RoomEffectActorKind.EvirFacingRight,
        RoomEffectActorKind.DraygonFoam,
        RoomEffectActorKind.HitBurst,
    ];

    /// <summary>Every run: each actor's own run and the two cycles.</summary>
    internal static IEnumerable<RoomEffectActorSegment> All =>
        [.. Kinds.Select(Of), MetroidElectricityCycle, MetroidShellCycle];

    /// <summary>The run <paramref name="actor"/> starts with: its whole animation, or a Metroid's electricity's or shell's first pass.</summary>
    internal static RoomEffectActorSegment Of(RoomEffectActorKind actor) => new(actor);
}
