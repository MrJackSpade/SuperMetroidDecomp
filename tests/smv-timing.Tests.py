"""Confirm that native APU waits are not mislabeled as authored gameplay waits."""


def run():
    import runpy
    from pathlib import Path
    classify = runpy.run_path(str(Path(__file__).resolve().parents[1] / "tools" / "convert-smv-updates.py"))["classify_timing"]
    cases = [
        # Native Ridley source frames 294..334: completed scroll, CPU in APU upload.
        (("nmi-continuation", True, 64, 64), "apu-upload-continuation"),
        # Source frame 214: IRQ advances scrolling without another main dispatch.
        (("nmi-continuation", False, 2, 3), "door-scroll-continuation"),
        # Uploads must never hide simultaneously advancing IRQ gameplay.
        (("nmi-continuation", True, 2, 3), "door-scroll-continuation"),
        # Final scroll continuation has no counter change; retain it as unresolved.
        (("nmi-continuation", False, 64, 64), "other-continuation"),
        (("main-loop", True, 64, 64), "main-loop"),
    ]
    for args, expected in cases:
        actual = classify(*args)
        if actual != expected:
            raise AssertionError(f"{args}: expected {expected}, got {actual}")
    try:
        classify("unknown", False, 0, 0)
    except ValueError:
        pass
    else:
        raise AssertionError("Unknown execution kind was accepted")
    print("PASS: native hardware-upload/IRQ timing classification (6 cases)")
    return 0


if __name__ == "__main__":
    import sys
    try:
        if sys.platform == "win32":
            import ctypes
            ctypes.windll.kernel32.SetErrorMode(0x0001 | 0x0002 | 0x8000)
        raise SystemExit(run())
    except Exception:
        import traceback
        traceback.print_exc(file=sys.stderr)
        raise SystemExit(1)
