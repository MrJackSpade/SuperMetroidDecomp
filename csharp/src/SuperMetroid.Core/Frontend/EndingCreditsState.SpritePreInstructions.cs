namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private void StepEndingSpritePreInstruction(EndingSprite wrapper)
    {
        IntroDiscoverySprite sprite = wrapper.Sprite;
        switch (wrapper.Role)
        {
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
                if (sprite.PreInstructionPointer == EndingSpritePreInstructions.WaitForFlyaway
                    && Phase == EndingCreditsPhase.PlanetEscapeFast)
                {
                    sprite.PreInstructionPointerForDiscovery(EndingSpritePreInstructions.MoveFlyawayStars);
                    sprite.YSubPosition = 0x4000;
                    sprite.GeneralTimer = 0;
                }
                else if (sprite.PreInstructionPointer == EndingSpritePreInstructions.MoveFlyawayStars)
                {
                    int velocity = unchecked((int)(((uint)sprite.GeneralTimer << 16) | sprite.YSubPosition)) - 32;
                    sprite.GeneralTimer = unchecked((ushort)(velocity >> 16));
                    sprite.YSubPosition = unchecked((ushort)velocity);
                    uint position = unchecked(((uint)sprite.XPosition << 16) | sprite.XSubPosition);
                    position = unchecked(position + (uint)velocity);
                    sprite.XPosition = (ushort)(position >> 16);
                    sprite.XSubPosition = (ushort)position;
                }
                if (Phase == EndingCreditsPhase.OperationSuccessfulText)
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionAfterglow:
                // F39B deletes the afterglow at the same native null-function handoff.
                if (Phase == EndingCreditsPhase.OperationSuccessfulText)
                    sprite.Delete();
                break;
        }
    }
}
