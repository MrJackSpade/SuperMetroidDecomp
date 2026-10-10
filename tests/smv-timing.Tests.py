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
    def numbered(updates):
        # As run() leaves them: one checkpoint per controller read, completed by the next.
        for index, update in enumerate(updates):
            update.update(update=index + 1, inputRecord=index, expectedRecord=index + 1)
        return updates
    def interval(frame, upload=False, next_upload=False):
        return {"sourceFrame": frame, "endSourceFrame": frame + 1, "input": 0, "pressed": 0,
                "messageBoxStartFrame": None, "messageBoxEndFrame": None,
                "kind": "nmi-continuation" if upload else "main-loop",
                "timingClass": "apu-upload-continuation" if upload else "main-loop",
                "timingEvidence": {"apuUploadActiveAtInput": upload,
                    "apuUploadActiveAtNextBoundary": next_upload,
                    "nativeGameState": 11, "nativeDoorFunction": 0xe664,
                    "doorScrollFinished": True, "doorScrollCounterBefore": 64,
                    "doorScrollCounterAfter": 64}}
    source = numbered([interval(1, next_upload=True), interval(2, True, True), interval(3, True), interval(4)])
    normalized, skipped = normalize(source, 0)
    assert len(normalized) == 2 and len(skipped) == 2
    assert normalized[0]["expectedRecord"] == 3 and normalized[0]["endSourceFrame"] == 4
    assert normalized[0]["excludedNmiAfter"] == 2 and normalized[1]["excludedNmiBefore"] == 2
    assert normalized[1]["expectedRecord"] == 4
    assert "excludedNmiBefore" not in source[0], "normalization mutated input evidence"
    import copy
    # A button held through the music wait is retained when no edge changes.
    held = copy.deepcopy(source)
    for index in range(4): held[index]["input"] = 0x8000
    held[0]["pressed"] = 0x8000
    assert len(normalize(held, 0)[0]) == 2, "held music-wait input with unchanged edges was refused"
    # A press first read during the wait is latched there, so the following update
    # sees it as held rather than newly pressed, exactly as native did.
    hidden = copy.deepcopy(source)
    hidden[1]["input"] = hidden[1]["pressed"] = 0x8000
    hidden[2]["input"] = hidden[3]["input"] = 0x8000
    latched, _ = normalize(hidden, 0)
    assert latched[1]["hardwareWaitLatch"] == 0x8000 and latched[0]["hardwareWaitLatch"] is None
    # The upload's own dispatch runs $82:E664 after SendAPUData returns, storing $E6A2;
    # a tail NMI landing after that store is still the same dispatch's stall.
    tail = copy.deepcopy(source)
    tail[2]["timingClass"] = "apu-upload-tail-continuation"
    tail[2]["timingEvidence"]["apuUploadActiveAtInput"] = False
    tail[2]["timingEvidence"]["nativeDoorFunction"] = 0xe6a2
    assert len(normalize(tail, 0)[1]) == 2, "post-$E664 upload tail was refused"
    for mutation in ("lost-press", "scroll", "unfinished", "overlap", "advanced-mid-upload"):
        altered = copy.deepcopy(source)
        if mutation == "lost-press":
            # A retained edge that native did not report cannot be reproduced.
            altered[3]["input"] = 0x8000
        if mutation == "scroll": altered[1]["timingEvidence"]["doorScrollCounterAfter"] = 65
        if mutation == "unfinished": altered[1]["timingEvidence"]["doorScrollFinished"] = False
        if mutation == "overlap": altered[1]["timingClass"] = "door-scroll-continuation"
        # While the upload itself runs, $E664 has not executed, so $E6A2 is foreign.
        if mutation == "advanced-mid-upload": altered[1]["timingEvidence"]["nativeDoorFunction"] = 0xe6a2
        try:
            normalize(altered, 0)
        except ValueError:
            pass
        else:
            raise AssertionError(f"Unsafe upload normalization accepted: {mutation}")
    # Power-on prelude: logo NMIs before the first main loop are not port updates.
    boot = numbered([dict(interval(0), kind="nmi-continuation", timingClass="other-continuation"),
            dict(interval(0), kind="nmi-continuation", timingClass="other-continuation", input=0x10)] + copy.deepcopy(source))
    assert converter["boot_prelude_length"](boot) == 2
    booted, _ = normalize(boot, 0, 2)
    assert len(booted) == 2 and booted[0]["inputRecord"] == 2 and booted[0]["excludedNmiBefore"] == 0
    boot[2]["input"] = 0x10
    try:
        normalize(boot, 0, 2)
    except ValueError:
        pass
    else:
        raise AssertionError("Prelude-held input that hides a first new press was accepted")
    # Power-on boot clear (the #1275 "Samus drunk" movie, SMV frames 168-272): the last
    # logo NMI reads A+X, CommonBootSection clears bank $7E, the first dispatch runs on an
    # empty latch, and the next read reports the still-held X as a new press.
    collect = converter["collect_input_updates"]
    def event(ordinal, frame, pc, held, pressed, nmi=0x40, state=8):
        return {"event": str(ordinal), "source_frame": str(frame), "pc": f"{pc:x}",
                "held": f"{held:x}", "pressed": f"{pressed:x}", "nmi": f"{nmi:x}", "state": f"{state:x}"}
    def boot_events(cleared_held=0, cleared_nmi=0):
        return [event(0, 1, 0x809459, 0, 0), event(1, 1, 0x809496, 0xc0, 0xc0),
                event(2, 2, 0x828948, cleared_held, 0, cleared_nmi, 0), event(3, 2, 0x82897a, 0, 0),
                event(4, 3, 0x809459, 0, 0), event(5, 3, 0x809496, 0x40, 0x40),
                event(6, 3, 0x828948, 0x40, 0x40)]
    boot_inputs = [0, 0xc0, 0xc0, 0x40]
    for from_reset in (True, False):
        # Quitting from game over reruns the clear in the middle of any movie.
        (first, second), reads = collect(boot_events(), boot_inputs, 3, from_reset)
        assert reads == 2 and first["inputRecord"] == 0 and second["inputRecord"] == 1
        assert (first["input"], first["pressed"]) == (0, 0), "boot-cleared dispatch did not consume an empty latch"
        assert (second["input"], second["pressed"]) == (0x40, 0x40), "post-boot press edge was not taken from zero"
    # Quitting from game over (the drunk movie's SMV frames 87442-87464): the menu's
    # dispatch jumps into the boot without an NMI wait, so the post-boot dispatch runs
    # under the same read. It becomes its own update with no checkpoint of its own.
    quit_events = [event(0, 1, 0x809459, 0, 0), event(1, 1, 0x809496, 0x8280, 0x8280),
                   event(2, 1, 0x828948, 0x8280, 0x8280, 0x40d8, 0x1a),
                   event(3, 2, 0x828948, 0, 0, 0, 0), event(4, 2, 0x82897a, 0, 0, 1, 1),
                   event(5, 2, 0x808338, 0, 0, 1, 1), event(6, 3, 0x809459, 0, 0, 1, 1),
                   event(7, 3, 0x809496, 0x8200, 0x8200, 1, 1)]
    (menu, boot, after), reads = collect(quit_events, [0, 0x8280, 0x8280, 0x8200], 3, False)
    assert reads == 2 and menu["input"] == 0x8280 and menu["mainLoopDispatches"] == 1
    assert boot["bootCleared"] and boot["inputRecord"] is None and boot["input"] == 0
    assert boot["sourceFrame"] == 2 and boot["mainLoopDispatches"] == 1 and after["inputRecord"] == 1
    for mutation in ("no-clear", "uncleared-latch"):
        try:
            if mutation == "no-clear":
                # Without the clear the held X is not a new press, and a power-on movie
                # cannot reach its first dispatch without one.
                collect(boot_events(cleared_nmi=0x40), boot_inputs, 3, False)
            else:
                collect(boot_events(cleared_held=0xc0), boot_inputs, 3, True)
        except ValueError:
            pass
        else:
            raise AssertionError(f"Boot-clear rule accepted {mutation}")
    try:
        collect(boot_events(cleared_nmi=0x40)[:3], boot_inputs[:2] + [0], 2, True)
    except ValueError:
        pass
    else:
        raise AssertionError("A power-on movie's first dispatch was accepted without a boot clear")
    try:
        normalize(source[:-1], 0)
    except ValueError:
        pass
    else:
        raise AssertionError("Incomplete terminal upload was accepted")
    print("PASS: timing classification, boot prelude and clear, upload-boundary mapping, input retention and unsafe-collapse rejection")
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
