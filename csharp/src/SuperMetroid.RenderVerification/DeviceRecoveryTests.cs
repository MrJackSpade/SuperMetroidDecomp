using System.ComponentModel;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class SwapchainTests
{
    private static void VerifyRecoveryFailure(D3D11DeviceKind kind, bool repeatDeviceLoss)
    {
        nint window = CreateWindowExW(0, "STATIC", "Hidden recovery failure verification", 0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        D3D11RenderWorker? worker = null;
        int attempts = 0;
        try
        {
            worker = new(window, 640, 480, 1, kind, () =>
            {
                Interlocked.Increment(ref attempts);
                if (repeatDeviceLoss) new SharpGen.Runtime.Result(D3D11RecoveryPolicy.DeviceRemoved).CheckError();
                throw new InvalidDataException("Injected coverage failure must not be retried.");
            });
            PumpUntil(() => worker.Ready.IsCompleted);
            worker.Ready.GetAwaiter().GetResult();
            worker.Publish(new(new(1, 1, 0), new Rgba32(1, 2, 3)));
            PumpUntil(() => worker.Completion.IsCompleted);
            if (!worker.Completion.IsFaulted) throw new InvalidOperationException("Renderer failure was suppressed.");
            int expected = repeatDeviceLoss ? D3D11RecoveryPolicy.MaximumConsecutiveRecreations + 1 : 1;
            if (attempts != expected || worker.DeviceRecoveries != expected - 1)
                throw new InvalidOperationException($"Recovery limit mismatch: {attempts} attempts, {worker.DeviceRecoveries} recreations.");
            // Observe the original fault through the same health API used by the desktop.
            Exception? observed = null;
            try { worker.ThrowIfFaulted(); } catch (Exception error) { observed = error; }
            if (repeatDeviceLoss ? observed?.HResult != D3D11RecoveryPolicy.DeviceRemoved : observed is not InvalidDataException)
                throw new InvalidOperationException("Renderer fault lost its original type/HRESULT.");
            Console.WriteLine($"{kind}: repeated loss={repeatDeviceLoss}; {attempts} attempts then original failure surfaced.");
        }
        finally
        {
            if (worker is not null)
            {
                Task stop = worker.StopAsync(); PumpUntil(() => stop.IsCompleted);
                _ = stop.Exception; // Expected terminal fault has already been asserted above.
            }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
