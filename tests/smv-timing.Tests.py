"""Confirm that native APU waits are not mislabeled as authored gameplay waits."""


def run():
    import runpy
    from pathlib import Path
    converter = runpy.run_path(str(Path(__file__).resolve().parents[1] / "tools" / "convert-smv-updates.py"))
    classify = converter["classify_timing"]
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
    normalize = converter["normalize_upload_intervals"]
    def interval(frame, upload=False, next_upload=False):
        return {"sourceFrame": frame, "endSourceFrame": frame + 1, "input": 0, "pressed": 0,
                "kind": "nmi-continuation" if upload else "main-loop",
                "timingClass": "apu-upload-continuation" if upload else "main-loop",
                "timingEvidence": {"apuUploadActiveAtInput": upload,
                    "apuUploadActiveAtNextBoundary": next_upload,
                    "nativeGameState": 11, "nativeDoorFunction": 0xe664,
                    "doorScrollFinished": True, "doorScrollCounterBefore": 64,
                    "doorScrollCounterAfter": 64}}
    source = [interval(1, next_upload=True), interval(2, True, True), interval(3, True), interval(4)]
    normalized, skipped = normalize(source, 0)
    assert len(normalized) == 2 and len(skipped) == 2
    assert normalized[0]["expectedRecord"] == 3 and normalized[0]["endSourceFrame"] == 4
    assert normalized[0]["excludedNmiAfter"] == 2 and normalized[1]["excludedNmiBefore"] == 2
    assert normalized[1]["expectedRecord"] == 4
    assert "inputRecord" not in source[0], "normalization mutated input evidence"
    import copy
    for mutation in ("button", "scroll", "unfinished", "overlap"):
        altered = copy.deepcopy(source)
        if mutation == "button": altered[1]["input"] = 0x8000
        if mutation == "scroll": altered[1]["timingEvidence"]["doorScrollCounterAfter"] = 65
        if mutation == "unfinished": altered[1]["timingEvidence"]["doorScrollFinished"] = False
        if mutation == "overlap": altered[1]["timingClass"] = "door-scroll-continuation"
        try:
            normalize(altered, 0)
        except ValueError:
            pass
        else:
            raise AssertionError(f"Unsafe upload normalization accepted: {mutation}")
    try:
        normalize(source[:-1], 0)
    except ValueError:
        pass
    else:
        raise AssertionError("Incomplete terminal upload was accepted")
    print("PASS: timing classification, upload-boundary mapping, input retention and unsafe-collapse rejection")
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
