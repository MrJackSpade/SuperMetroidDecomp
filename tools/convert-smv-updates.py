"""Convert an unchanged SMV plus an instrumented native trace to port input updates.

This is deliberately not a duplicate-frame remover. The capture must observe the
pinned J/U main-loop and accepted-NMI controller-read boundaries. WRAM stays private.
"""


def classify_timing(kind, upload_active, scroll_before, scroll_after):
    """Describe observed execution; this classification never drops an input."""
    if kind == "main-loop":
        return "main-loop"
    if kind != "nmi-continuation":
        raise ValueError(f"Unknown input interval kind: {kind}")
    if scroll_before != scroll_after:
        return "door-scroll-continuation"
    if upload_active:
        return "apu-upload-continuation"
    return "other-continuation"


def run():
    import argparse
    import csv
    import gzip
    import hashlib
    import json
    import struct
    from pathlib import Path

    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("movie", type=Path)
    parser.add_argument("trace_directory", type=Path)
    parser.add_argument("--rom", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    movie = args.movie.read_bytes()
    if len(movie) < 32 or movie[:4] != b"SMV\x1a":
        raise ValueError("Not an SMV movie")
    version, = struct.unpack_from("<I", movie, 4)
    frames, = struct.unpack_from("<I", movie, 16)
    offset, = struct.unpack_from("<I", movie, 28)
    if version not in (1, 4, 5) or movie[20] != 1:
        raise ValueError("Only one-controller SMV versions 1, 4 and 5 are supported")
    if offset < 32 or offset + 2 * (frames + 1) > len(movie):
        raise ValueError("Truncated SMV controller stream")
    inputs = struct.unpack_from(f"<{frames + 1}H", movie, offset)
    def digest(path):
        with path.open("rb") as stream:
            return hashlib.file_digest(stream, "sha256").hexdigest().upper()
    rom_hash = digest(args.rom)
    if rom_hash != "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72":
        raise ValueError("Trace addresses require the pinned J/U Super Metroid ROM")
    events_path = args.trace_directory / "input-events.csv"
    boundaries_path = args.trace_directory / "update-boundaries.wram.gz"
    if args.output.resolve() in (args.movie.resolve(), args.rom.resolve(), events_path.resolve(), boundaries_path.resolve()):
        raise ValueError("The output must not overwrite an input")
    # Native addresses are capture-format identities, not guessed timing rules.
    read_enter, read_complete = 0x809459, 0x809496
    main_begin, main_end, wait = 0x828948, 0x82897A, 0x808338
    known = {read_enter, read_complete, main_begin, main_end, wait}
    updates = []
    current = None
    previous_input = inputs[0]
    previous_frame = 0
    with events_path.open(newline="", encoding="utf-8") as source:
        for ordinal, event in enumerate(csv.DictReader(source)):
            frame = int(event["source_frame"])
            pc = int(event["pc"], 16)
            if int(event["event"]) != ordinal or not previous_frame <= frame <= frames or pc not in known:
                raise ValueError(f"Invalid native event {ordinal}")
            previous_frame = frame
            if pc == read_enter:
                if current is not None:
                    if "input" not in current:
                        raise ValueError("Controller read did not complete")
                    updates.append(current)
                current = {"sourceFrame": frame, "mainLoopDispatches": 0}
            elif pc == read_complete:
                if current is None or "input" in current or current["sourceFrame"] != frame:
                    raise ValueError("Ambiguous native controller-read boundary")
                held, pressed = int(event["held"], 16), int(event["pressed"], 16)
                if held != inputs[frame] or pressed != held & ~previous_input:
                    raise ValueError(f"Input/edge mismatch at SMV frame {frame}; cannot collapse this trace to controller masks")
                current.update(input=held, pressed=pressed)
                previous_input = held
            elif pc == main_begin:
                if current is None or "input" not in current:
                    raise ValueError("Main dispatch precedes its input event")
                current["mainLoopDispatches"] += 1
    if current is None or "input" not in current:
        raise ValueError("No completed terminal input step")
    updates.append(current)
    if updates[-1]["sourceFrame"] != frames:
        raise ValueError("Native trace does not cover the original movie's final input")
    for index, update in enumerate(updates):
        if update["mainLoopDispatches"] not in (0, 1):
            raise ValueError("Multiple main-loop dispatches require a richer replay format")
        update["kind"] = "main-loop" if update.pop("mainLoopDispatches") else "nmi-continuation"
        update["update"] = index + 1
        update["expectedRecord"] = index + 1
        update["endSourceFrame"] = updates[index + 1]["sourceFrame"] if index + 1 < len(updates) else frames
    # Validate every private checkpoint, not just the JSON count. The final record
    # is captured after the original movie ends; it is not an early checkpoint.
    # These are the pinned J/U WRAM owners, not inferred durations. IRQ scrolling
    # can continue while the CPU uploads data; neither accepted NMI nor an active
    # APU upload alone proves a gameplay update or a disposable video refresh.
    apu_uploading, door_scroll_counter = 0x0617, 0x0925
    boundary_timing = []
    with gzip.open(boundaries_path, "rb") as source:
        for index in range(len(updates) + 1):
            record = source.read(131080)
            if len(record) != 131080:
                raise ValueError(f"Truncated native checkpoint {index}")
            frame, pc = struct.unpack_from("<II", record)
            expected_frame = updates[index]["sourceFrame"] if index < len(updates) else frames
            boundary_timing.append((
                struct.unpack_from("<H", record, 8 + apu_uploading)[0],
                struct.unpack_from("<H", record, 8 + door_scroll_counter)[0]))
            if frame != expected_frame or pc != (read_enter if index < len(updates) else 0):
                raise ValueError(f"Checkpoint {index} disagrees with its input boundary")
        if source.read(1):
            raise ValueError("Unexpected trailing native checkpoints")
    for index, update in enumerate(updates):
        upload, scroll = boundary_timing[index]
        next_upload, next_scroll = boundary_timing[index + 1]
        update["timingEvidence"] = {
            "apuUploadActiveAtInput": upload != 0,
            "apuUploadActiveAtNextBoundary": next_upload != 0,
            "doorScrollCounterBefore": scroll,
            "doorScrollCounterAfter": next_scroll,
        }
        update["timingClass"] = classify_timing(update["kind"], upload != 0, scroll, next_scroll)
    # Keep every input event until replay can preserve the hardware wait's input
    # latch/counter effects without executing another gameplay frame. Reporting
    # these separately prevents claiming that accepted NMI == gameplay update.
    timing_counts = {name: sum(u["timingClass"] == name for u in updates) for name in (
        "main-loop", "door-scroll-continuation", "apu-upload-continuation", "other-continuation")}
    source_frames = {u["sourceFrame"] for u in updates}
    manifest = {
        "format": "super-metroid-gameplay-updates-v2",
        "movieSha256": hashlib.sha256(movie).hexdigest().upper(),
        "romSha256": rom_hash,
        "nativeCapture": "Snes9x 1.60 913b75d07c6e8d54e966e2c4a79d7c55428007df; instrumented J/U input boundaries",
        "eventsSha256": digest(events_path), "checkpointsSha256": digest(boundaries_path),
        "sourceFrameCount": frames, "initialInput": inputs[0],
        "updateCount": len(updates),
        "timingCounts": timing_counts,
        "hardwareUploadNormalizationComplete": not any(upload for upload, _ in boundary_timing),
        "hardwareLagRefreshesExcluded": frames - len(source_frames),
        "mainLoopUpdates": sum(u["kind"] == "main-loop" for u in updates),
        "continuationUpdates": sum(u["kind"] == "nmi-continuation" for u in updates),
        "updates": updates,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({k: v for k, v in manifest.items() if k != "updates"}, indent=2))
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
