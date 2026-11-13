using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the ninja Space Pirates' animations, with its typed operands.</summary>
internal abstract record NinjaSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private NinjaSpacePirateCue()
    {
    }

    /// <summary>Plays the operand's enemy-library sound.</summary>
    /// <param name="Sound">For a sound cue: the enemy-library sound it plays.</param>
    internal sealed record PlaySound(SoundEffectId? Sound) : NinjaSpacePirateCue;

    /// <summary>Sets the function main AI runs next.</summary>
    /// <param name="Function">The cue's Function operand.</param>
    internal sealed record SetFunction(NinjaSpacePirateFunction Function) : NinjaSpacePirateCue;

    /// <summary>Sets the look the Pirate draws in.</summary>
    /// <param name="Shading">The shading the Pirate is drawn in from then on (#1512).</param>
    internal sealed record SetShading(PictureShading Shading) : NinjaSpacePirateCue;

    /// <summary>Throws a claw in the operand's direction from the given offset.</summary>
    /// <param name="Throw">For a claw cue: the way the claw flies.</param>
    /// <param name="Offset">The cue's signed pixel offset, horizontal and vertical.</param>
    internal sealed record ThrowClaw(HorizontalDirection Throw, Offset16<RoomPixels> Offset) : NinjaSpacePirateCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(NinjaSpacePirateCueKind Kind) : NinjaSpacePirateCue;
}
