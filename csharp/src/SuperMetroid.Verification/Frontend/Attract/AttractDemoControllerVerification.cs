using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Frontend.Attract;

/// <summary>Checks scripted demo input edges against the preserved physical controller latch.</summary>
internal static class AttractDemoControllerVerification
{
    /// <summary>
    /// Checks that consecutive scripted X presses retain their explicit new-press edges
    /// while the Start latch history and repeat timer survive each temporary override.
    /// A later real Start latch must see no edge leaked from the demo.
    /// </summary>
    internal static void VerifyAttractDemoControllerOverride()
    {
        var controller = new ControllerInputState(4, 2);
        controller.Latch((ushort)SnesButton.Start);
        ushort repeat = controller.RepeatTimer.RawValue;
        for (int frame = 0; frame < 2; frame++)
        {
            using (controller.UseDemoInput((ushort)SnesButton.X, (ushort)SnesButton.X))
            {
                if (controller.Current != (ushort)SnesButton.X || controller.NewlyPressed != (ushort)SnesButton.X)
                    throw new InvalidDataException("Demo controller did not preserve the explicit scripted edge.");
                if (controller.Previous != (ushort)SnesButton.Start || controller.RepeatTimer.RawValue != repeat)
                    throw new InvalidDataException("Demo override modified the hardware latch history.");
            }
            if (controller.Current != (ushort)SnesButton.Start || controller.NewlyPressed != (ushort)SnesButton.Start)
                throw new InvalidDataException("Demo beta failed to restore the player cancellation input.");
        }
        controller.Latch((ushort)SnesButton.Start);
        if (controller.NewlyPressed != 0)
            throw new InvalidDataException("Demo input leaked into the next real controller edge calculation.");
        Console.WriteLine("  Attract controller: explicit script edges and restored hardware history agree.");
    }
}
