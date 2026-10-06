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


DOOR_LOADER_FUNCTION = 0xE4A9  # DoorTransitionFunction_LoadSpritesBGPLMsAudio_RunDoorRoomASM


def door_loader_enemy_progress(boundary_loading, boundary_enemy_ids):
    """Per boundary: enemy slots whose Initialise_Enemies init AI has completed in $82:E4A9.

    The loader's CPU work spans several NMIs. Load_Enemies clears every slot before
    Initialise_Enemies writes each slot's ID ahead of its init AI, so slot k is known
    complete once slot k+1 has an ID. None outside the loader or before the clear."""
    progress = []
    cleared = False
    for (state, function, _), ids in zip(boundary_loading, boundary_enemy_ids):
        if state != 0x0B or function != DOOR_LOADER_FUNCTION:
            cleared = False
            progress.append(None)
            continue
        if not any(ids):
            cleared = True
        if not cleared:
            progress.append(None)
            continue
        written = next((slot for slot, value in enumerate(ids) if value == 0), len(ids))
        if any(ids[written:]):
            raise ValueError("Initialise_Enemies left a gap in the enemy slots")
        progress.append(max(written - 1, 0))
    return progress


def boot_prelude_length(updates):
    """Count accepted NMIs before the first main-loop dispatch of a power-on movie.

    Native `Boot` displays the logo with NMI enabled, then `CommonBootSection`
    ($80:8482) clears all of bank $7E, including the NMI counter and held-input
    history, before seeding RNG and entering `MainGameLoop`. Nothing read by those
    prelude NMIs survives into gameplay, so they are not port updates.
    """
    for index, update in enumerate(updates):
        if update["kind"] == "main-loop":
            return index
    raise ValueError("Movie never reaches the native main game loop")


def normalize_upload_intervals(updates, initial_input, prelude=0):
    """Collapse the boot prelude and proven post-scroll upload waits, retaining their input audit trail."""
    normalized, excluded = [], []
    for record, original in enumerate(updates):
        update = dict(original)
        evidence = update["timingEvidence"]
        if record < prelude:
            continue
        if update["timingClass"] == "apu-upload-continuation":
            # This contract is deliberately limited to the native door music wait.
            # No CPU gameplay dispatch or moving door IRQ runs in this interval.
            if (not normalized or normalized[-1]["kind"] != "main-loop" or
                not normalized[-1]["timingEvidence"]["apuUploadActiveAtNextBoundary"] or
                evidence["nativeGameState"] != 11 or evidence["nativeDoorFunction"] != 0xe664 or
                not evidence["doorScrollFinished"] or
                evidence["doorScrollCounterBefore"] != evidence["doorScrollCounterAfter"]):
                raise ValueError("Unsupported APU continuation; cannot prove a pure post-scroll hardware wait")
            # The music wait runs no gameplay, but each of its NMIs still reads the
            # controller and replaces the held/new latch. The replay therefore keeps
            # the last such read (`hardwareWaitLatch`) without dispatching an update.
            excluded.append({"sourceFrame": update["sourceFrame"], "input": update["input"],
                             "pressed": update["pressed"], "record": record})
            continue
        if evidence["apuUploadActiveAtInput"]:
            raise ValueError("APU upload overlaps another owner; automatic normalization is unsupported")
        update["inputRecord"] = record
        update["excludedNmiBefore"] = len(excluded)
        waited = excluded and excluded[-1]["record"] == record - 1
        update["hardwareWaitLatch"] = excluded[-1]["input"] if waited else None
        normalized.append(update)
    if not normalized or normalized[-1]["inputRecord"] != len(updates) - 1:
        raise ValueError("Movie ends during hardware upload; completed gameplay boundary is unavailable")
    # A power-on port starts with an empty controller latch, as the cleared native
    # bank $7E did; any held prelude input must therefore not hide a new press.
    previous = 0 if prelude else initial_input
    for index, update in enumerate(normalized):
        if update["hardwareWaitLatch"] is not None:
            previous = update["hardwareWaitLatch"]
        if update["pressed"] != update["input"] & ~previous:
            raise ValueError(f"Removing hardware waits would change a consumed input edge at SMV frame {update['sourceFrame']}")
        previous = update["input"]
        following = normalized[index + 1] if index + 1 < len(normalized) else None
        update["update"] = index + 1
        update["expectedRecord"] = following["inputRecord"] if following else len(updates)
        update["endSourceFrame"] = following["sourceFrame"] if following else update["endSourceFrame"]
        update["excludedNmiAfter"] = following["excludedNmiBefore"] if following else len(excluded)
    return normalized, excluded


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
    # Enemy.ID of each of the 32 enemy slots ($0F78 + $40 * slot).
    enemy_id, enemy_slot_size, enemy_slots = 0x0F78, 0x40, 32
    boundary_timing = []
    boundary_loading = []
    boundary_enemy_ids = []
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
            boundary_loading.append((
                struct.unpack_from("<H", record, 8 + 0x0998)[0],  # GameState
                struct.unpack_from("<H", record, 8 + 0x099c)[0],  # DoorTransitionFunction
                bool(struct.unpack_from("<H", record, 8 + 0x0931)[0] & 0x8000)))
            boundary_enemy_ids.append(tuple(
                struct.unpack_from("<H", record, 8 + enemy_id + enemy_slot_size * slot)[0]
                for slot in range(enemy_slots)))
            if frame != expected_frame or pc != (read_enter if index < len(updates) else 0):
                raise ValueError(f"Checkpoint {index} disagrees with its input boundary")
        if source.read(1):
            raise ValueError("Unexpected trailing native checkpoints")
    loader_progress = door_loader_enemy_progress(boundary_loading, boundary_enemy_ids)
    for index, update in enumerate(updates):
        upload, scroll = boundary_timing[index]
        next_upload, next_scroll = boundary_timing[index + 1]
        update["timingEvidence"] = {
            "apuUploadActiveAtInput": upload != 0,
            "apuUploadActiveAtNextBoundary": next_upload != 0,
            "doorScrollCounterBefore": scroll,
            "doorScrollCounterAfter": next_scroll,
            "nativeGameState": boundary_loading[index][0],
            "nativeDoorFunction": boundary_loading[index][1],
            "doorScrollFinished": boundary_loading[index][2],
            "doorLoaderCompletedEnemySlots": loader_progress[index + 1],
        }
        update["timingClass"] = classify_timing(update["kind"], upload != 0, scroll, next_scroll)
    timing_counts = {name: sum(u["timingClass"] == name for u in updates) for name in (
        "main-loop", "door-scroll-continuation", "apu-upload-continuation", "other-continuation")}
    source_frames = {u["sourceFrame"] for u in updates}
    accepted_input_count = len(updates)
    # SMV v4/v5 option bit 0: the recording starts at power-on (with SRAM) rather
    # than from an embedded snapshot, so the native boot prelude precedes gameplay.
    from_reset = version in (4, 5) and bool(movie[21] & 1)
    prelude = boot_prelude_length(updates) if from_reset else 0
    updates, excluded = normalize_upload_intervals(updates, inputs[0], prelude)
    manifest = {
        "format": "super-metroid-gameplay-updates-v4",
        "startsFromReset": from_reset,
        "initialRecord": prelude,
        "bootPreludeInputsExcluded": prelude,
        "movieSha256": hashlib.sha256(movie).hexdigest().upper(),
        "romSha256": rom_hash,
        "nativeCapture": "Snes9x 1.60 913b75d07c6e8d54e966e2c4a79d7c55428007df; instrumented J/U input boundaries",
        "eventsSha256": digest(events_path), "checkpointsSha256": digest(boundaries_path),
        "sourceFrameCount": frames, "initialInput": inputs[0],
        "updateCount": len(updates),
        "timingCounts": timing_counts,
        "acceptedInputCount": accepted_input_count,
        "hardwareUploadNormalizationComplete": True,
        "hardwareUploadInputsExcluded": len(excluded),
        "excludedUploadInputs": excluded,
        "hardwareLagRefreshesExcluded": frames - len(source_frames),
        "mainLoopUpdates": sum(u["kind"] == "main-loop" for u in updates),
        "continuationUpdates": sum(u["kind"] == "nmi-continuation" for u in updates),
        "updates": updates,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({k: v for k, v in manifest.items() if k not in ("updates", "excludedUploadInputs")}, indent=2))
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
