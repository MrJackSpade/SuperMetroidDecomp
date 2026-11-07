using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A PLM animation whose programs only draw, loop and delete: <see cref="RoomObjectNoCue"/> has no cue to publish.</summary>
/// <typeparam name="TProgram">The family's program identity.</typeparam>
/// <typeparam name="TFrame">The family's draw identity.</typeparam>
/// <param name="program">The program to start.</param>
internal abstract class RoomObjectCuelessProgramAnimation<TProgram, TFrame>(TProgram program)
    : RoomObjectProgramAnimation<TProgram, TFrame, RoomObjectNoCue>(program)
    where TProgram : struct, Enum
    where TFrame : struct
{
    /// <summary>Rejects a cue: <see cref="RoomObjectNoCue"/> defines none, so a cueless program set cannot hold a cue step.</summary>
    [SuppressMessage("Design", "SME6426", Justification =
        "The cue contract passes the slot and step context every cue-publishing PLM family reads; RoomObjectNoCue has no members, so this override is the one place a cueless family states that no cue exists.")]
    private protected sealed override TProgram? Publish(RoomObjectNoCue cue, RoomObjectSlot slot, RoomObjectStepContext context) =>
        throw new InvalidOperationException($"{GetType().Name}'s programs publish no cues; got {cue}.");
}
