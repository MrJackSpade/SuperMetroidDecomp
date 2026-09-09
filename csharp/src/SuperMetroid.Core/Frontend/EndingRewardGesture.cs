using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Runs E342's reward gesture actors until their native jumping-actor handoff.</summary>
internal sealed class EndingRewardGesture
{
    private readonly ISnesAddressSpace bus;
    private readonly List<IntroDiscoverySprite> actors = [];
    public bool JumpRequested { get; private set; }
    public bool SuitlessJumpRequested { get; private set; }

    public EndingRewardGesture(ISnesAddressSpace bus, EndingReward reward)
    {
        this.bus = bus;
        if (reward == EndingReward.Suitless)
        {
            Spawn(EndingRewardActorDefinitions.SuitlessUpper);
            Spawn(EndingRewardActorDefinitions.SuitlessLower);
        }
        else
        {
            // Keep E342's allocation order: head first, then body, then arm.
            Spawn(reward == EndingReward.Helmetless ? EndingRewardActorDefinitions.HelmetlessHead
                : EndingRewardActorDefinitions.HelmetedHead);
            Spawn(EndingRewardActorDefinitions.SuitedBody);
            Spawn(EndingRewardActorDefinitions.SuitedArm);
        }
    }

    public void Step()
    {
        if (JumpRequested) throw new InvalidOperationException("Reward gesture handoff must be consumed before advancing again.");
        foreach (IntroDiscoverySprite actor in actors)
            actor.Step(bus, HandleInstruction);
        actors.RemoveAll(actor => !actor.IsActive);
    }

    public OamBuffer Draw()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        foreach (IntroDiscoverySprite actor in actors) actor.Draw(bus, oam);
        oam.FinalizeFrame();
        return oam;
    }

    private ushort? HandleInstruction(ushort instruction, ushort cursor)
    {
        switch (instruction)
        {
            case EndingRewardActorDefinitions.SpawnSuitlessJump:
                SuitlessJumpRequested = true;
                JumpRequested = true;
                return cursor;
            case EndingRewardActorDefinitions.SpawnSuitedJump:
                JumpRequested = true;
                return cursor;
            default:
                return null; // The shared interpreter reports the exact unsupported opcode.
        }
    }

    private void Spawn(ushort definition)
    {
        ushort initialization = RomDataReader.ReadWordFixedBank(bus, 0x8b0000 | definition);
        ushort list = RomDataReader.ReadWordFixedBank(bus, 0x8b0000 | (definition + 4));
        var (x, y, palette) = EndingRewardActorDefinitions.GetInitialization(initialization);
        actors.Add(new IntroDiscoverySprite((ushort)x, (ushort)y, (ushort)palette, list));
    }
}
