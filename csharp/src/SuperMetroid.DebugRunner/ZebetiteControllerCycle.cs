using SuperMetroid.Core.Input;

/// <summary>Controller-only return hop and shot timing, relative to the shot's jump base.</summary>
internal readonly record struct ZebetiteControllerCycle(
    int JumpBase, int ReturnStart, int ReturnLeftFrames, int JumpDelay, int LeftFrames)
{
    public SnesButton InputAt(int frame)
    {
        SnesButton input = 0;
        if (frame >= ReturnStart && frame < ReturnStart + 60) input |= SnesButton.A;
        if (frame >= ReturnStart + 1 && frame < ReturnStart + 1 + ReturnLeftFrames) input |= SnesButton.Left;
        if (frame == JumpBase - 50) input |= SnesButton.Right;
        if (frame == JumpBase - 20 || frame == JumpBase - 19) input |= SnesButton.Down;
        if (frame == JumpBase - 14) input |= SnesButton.Left;
        if (frame == JumpBase - 8) input |= SnesButton.X;
        int jump = JumpBase + JumpDelay;
        if (frame >= jump && frame < jump + LeftFrames) input |= SnesButton.Left | SnesButton.A;
        else if (frame >= jump + LeftFrames && frame < jump + LeftFrames + 18) input |= SnesButton.Right | SnesButton.A;
        return input;
    }
}
