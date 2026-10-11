namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private void StepEndingSpritePreInstruction(EndingSprite wrapper)
    {
        IntroDiscoverySprite sprite = wrapper.Sprite;
        switch (wrapper.Role)
        {
            case EndingSpriteRole.AnimalEscape:
                uint y = ((uint)sprite.YPosition << 16) | sprite.YSubPosition;
                y += EndingAnimalEscapeDefinitions.YFractionVelocity;
                sprite.YPosition = (ushort)(y >> 16);
                sprite.YSubPosition = (ushort)y;
                sprite.XPosition += EndingAnimalEscapeDefinitions.XVelocity;
                if (sprite.XPosition >= EndingAnimalEscapeDefinitions.DeleteX)
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionGlow:
            case EndingSpriteRole.ExplosionStars:
            case EndingSpriteRole.ExplosionLava:
                // F2A5 observes the original planet actor's instruction pointer, not a
                // cinematic timer. These subordinate layers disappear with that actor.
                if (!sprites.Any(actor => actor.Role == EndingSpriteRole.ExplodingZebes && actor.Sprite.IsActive))
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionStarsRight:
            case EndingSpriteRole.ExplosionStarsLeft:
                // The right starfield runs no flyaway pre-instruction until its list installs $F35A.
                if (Enum.IsDefined((EndingSpritePreInstruction)sprite.PreInstructionPointer))
                    StepFlyawayStars(sprite, (EndingSpritePreInstruction)sprite.PreInstructionPointer);
                if (Phase == EndingCreditsPhase.OperationSuccessfulText)
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionAfterglow:
                // F39B deletes the afterglow at the same native null-function handoff.
                if (Phase == EndingCreditsPhase.OperationSuccessfulText)
                    sprite.Delete();
                break;
            // These actors run no pre-instruction.
            case EndingSpriteRole.CloudRightA:
            case EndingSpriteRole.CloudLeftA:
            case EndingSpriteRole.CloudRightB:
            case EndingSpriteRole.CloudLeftB:
            case EndingSpriteRole.CloudTopA:
            case EndingSpriteRole.CloudTopB:
            case EndingSpriteRole.CloudBottomA:
            case EndingSpriteRole.CloudBottomB:
            case EndingSpriteRole.ExplodingZebes:
            case EndingSpriteRole.ExplosionSilhouette:
            case EndingSpriteRole.OperationWasText:
            case EndingSpriteRole.CompletedSuccessfullyText:
            case EndingSpriteRole.ClearTimeText:
            case EndingSpriteRole.ClearTimeDigit:
            case EndingSpriteRole.RewardSamus:
                break;
            default:
                throw new InvalidOperationException($"Undefined ending sprite role {wrapper.Role}.");
        }
    }

    private void StepFlyawayStars(IntroDiscoverySprite sprite, EndingSpritePreInstruction preInstruction)
    {
        switch (preInstruction)
        {
            case EndingSpritePreInstruction.WaitForFlyaway:
                if (Phase == EndingCreditsPhase.PlanetEscapeFast)
                {
                    sprite.PreInstructionPointerForDiscovery((ushort)EndingSpritePreInstruction.MoveFlyawayStars);
                    sprite.YSubPosition = 0x4000;
                    sprite.GeneralTimer = 0;
                }
                break;
            case EndingSpritePreInstruction.MoveFlyawayStars:
                int velocity = unchecked((int)(((uint)sprite.GeneralTimer << 16) | sprite.YSubPosition)) - 32;
                sprite.GeneralTimer = unchecked((ushort)(velocity >> 16));
                sprite.YSubPosition = unchecked((ushort)velocity);
                uint position = unchecked(((uint)sprite.XPosition << 16) | sprite.XSubPosition);
                position = unchecked(position + (uint)velocity);
                sprite.XPosition = (ushort)(position >> 16);
                sprite.XSubPosition = (ushort)position;
                break;
            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(EndingSpritePreInstruction)} {(int)preInstruction}.");
        }
    }
}
