using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship attic PLM's typed program (#1296).</summary>
internal sealed class WreckedShipAtticPlmAnimation()
    : PlmProgramAnimation<WreckedShipAtticProgram, PlmNoDraw, WreckedShipAtticCue>(WreckedShipAtticProgram.Attic)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<WreckedShipAtticProgram, PlmNoDraw, WreckedShipAtticCue> Programs =>
        WreckedShipAtticPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(PlmNoDraw frame) =>
        throw new InvalidOperationException($"The attic program never draws; got {frame}.");

    /// <inheritdoc/>
    private protected override WreckedShipAtticProgram? Publish(WreckedShipAtticCue cue, PlmSlot slot, PlmStepContext context)
    {
        if (cue != WreckedShipAtticCue.InstallInertPreInstruction)
            throw new InvalidOperationException($"Undefined {nameof(WreckedShipAtticCue)} {cue}.");
        slot.PreInstruction = WreckedShipAtticPlmRomData.NoOpCallback;
        return null;
    }
}
