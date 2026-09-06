using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyAttractDemoControllerOverride()
    {
        var controller = new ControllerInputState(4, 2);
        controller.Latch((ushort)SnesButton.Start);
        ushort repeat = controller.RepeatTimer;
        // Two consecutive scripted records may deliberately press X anew while still
        // holding it. A second Latch(X) would incorrectly discard that second edge.
        for (int frame = 0; frame < 2; frame++)
        {
            using (controller.UseDemoInput((ushort)SnesButton.X, (ushort)SnesButton.X))
            {
                if (controller.Current != (ushort)SnesButton.X || controller.NewlyPressed != (ushort)SnesButton.X)
                    throw new InvalidDataException("Demo controller did not preserve the explicit scripted edge.");
                if (controller.Previous != (ushort)SnesButton.Start || controller.RepeatTimer != repeat)
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
