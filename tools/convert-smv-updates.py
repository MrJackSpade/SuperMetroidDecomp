"""Convert an unchanged SMV or lsnes movie plus an instrumented native trace to port input updates.

This is deliberately not a duplicate-frame remover. The capture must observe the
pinned J/U main-loop and accepted-NMI controller-read boundaries. WRAM stays private.
"""


def classify_timing(kind, upload_active, scroll_before, scroll_after, previous_class=None):
    """Describe observed execution; this classification never drops an input."""
    if kind == "main-loop":
        return "main-loop"
    if kind != "nmi-continuation":
        raise ValueError(f"Unknown input interval kind: {kind}")
    if scroll_before != scroll_after:
        return "door-scroll-continuation"
    if upload_active:
        return "apu-upload-continuation"
    if previous_class in ("apu-upload-continuation", "apu-upload-tail-continuation"):
        # SendAPUData returned, but the rest of the same outer dispatch's prologue
        # (HDMA objects, layer blending, RNG) overran into one more accepted NMI. No
        # main-loop dispatch began in between, so this is still that upload's stall.
        return "apu-upload-tail-continuation"
    return "other-continuation"


DOOR_LOADER_FUNCTION = 0xE4A9  # DoorTransitionFunction_LoadSpritesBGPLMsAudio_RunDoorRoomASM


def door_loader_enemy_progress(boundary_loading, boundary_enemy_ids):
    """Per boundary: enemy slots whose Initialise_Enemies init AI has completed in $82:E4A9.

    The loader's CPU work spans several NMIs. Load_Enemies clears every slot before
    Initialise_Enemies writes each slot's ID ahead of its init AI, so slot k is known
    complete once slot k+1 has an ID, and every slot once $0E4E holds the count the
    routine stores on exit (it zeroes $0E4E on entry). None outside the loader or before
    the clear."""
    progress = []
    cleared = False
    for (state, function, _), (ids, initialised) in zip(boundary_loading, boundary_enemy_ids):
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
        progress.append(written if written and initialised == written else max(written - 1, 0))
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
    for index, original in enumerate(updates):
        update = dict(original)
        evidence = update["timingEvidence"]
        if index < prelude:
            continue
        if update["timingClass"] in ("apu-upload-continuation", "apu-upload-tail-continuation"):
            if update["messageBoxStartFrame"] is not None or update["messageBoxEndFrame"] is not None:
                raise ValueError("A message box cannot open or close inside an excluded upload wait")
            # This contract is deliberately limited to the native door music wait.
            # No new CPU gameplay dispatch or moving door IRQ runs in this interval; an
            # upload tail only finishes the dispatch its main-loop update already owns.
            # $82:E664 runs after the upload in the same dispatch and, once the music queue
            # is clear, stores $E6A2 as its only write; a tail NMI may land after that store.
            door_functions = ((0xe664, 0xe6a2) if update["timingClass"] == "apu-upload-tail-continuation"
                              else (0xe664,))
            if (not normalized or normalized[-1]["kind"] != "main-loop" or
                not normalized[-1]["timingEvidence"]["apuUploadActiveAtNextBoundary"] or
                evidence["nativeGameState"] != 11 or evidence["nativeDoorFunction"] not in door_functions or
                not evidence["doorScrollFinished"] or
                evidence["doorScrollCounterBefore"] != evidence["doorScrollCounterAfter"]):
                raise ValueError("Unsupported APU continuation; cannot prove a pure post-scroll hardware wait")
            # The music wait runs no gameplay, but each of its NMIs still reads the
            # controller and replaces the held/new latch. The replay therefore keeps
            # the last such read (`hardwareWaitLatch`) without dispatching an update.
            excluded.append({"sourceFrame": update["sourceFrame"], "input": update["input"],
                             "pressed": update["pressed"], "record": update["inputRecord"], "update": index})
            continue
        if evidence["apuUploadActiveAtInput"]:
            raise ValueError("APU upload overlaps another owner; automatic normalization is unsupported")
        update["excludedNmiBefore"] = len(excluded)
        waited = excluded and excluded[-1]["update"] == index - 1
        update["hardwareWaitLatch"] = excluded[-1]["input"] if waited else None
        normalized.append(update)
    if not normalized or normalized[-1]["update"] != len(updates):
        raise ValueError("Movie ends during hardware upload; completed gameplay boundary is unavailable")
    terminal_record = updates[-1]["expectedRecord"]
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
        update["expectedRecord"] = following["inputRecord"] if following else terminal_record
        update["endSourceFrame"] = following["sourceFrame"] if following else update["endSourceFrame"]
        update["excludedNmiAfter"] = following["excludedNmiBefore"] if following else len(excluded)
    return normalized, excluded


# Native addresses are capture-format identities, not guessed timing rules.
READ_ENTER, READ_COMPLETE = 0x809459, 0x809496
MAIN_BEGIN, MAIN_END, NMI_WAIT = 0x828948, 0x82897A, 0x808338
# MessageBox_Routine entry. Its frame, relative to the dispatch's input read, is how
# many lag frames the dispatch spent before the box's own controller polling began.
MESSAGE_BOX = 0x858080
# MessageBox_Routine's common return. The box's own controller reads start new
# updates, so it can close in a later update than it opened; the dispatch may then
# run on (a save writes SRAM), and those frames are the dispatch's, not the box's.
MESSAGE_BOX_RETURN = 0x8580BA


def collect_input_updates(events, inputs, frames, from_reset):
    """Group the native event stream into port updates, validating every input edge.

    An update normally begins at an accepted NMI's controller read and owns the checkpoint
    record captured there (`inputRecord`). A main-loop dispatch entered straight out of
    CommonBootSection, without an intervening read, starts its own update (`bootCleared`)
    with the cleared latch and no checkpoint of its own.
    """
    read_enter, read_complete = READ_ENTER, READ_COMPLETE
    main_begin, main_end, wait = MAIN_BEGIN, MAIN_END, NMI_WAIT
    message_box, message_box_return = MESSAGE_BOX, MESSAGE_BOX_RETURN
    known = {read_enter, read_complete, main_begin, main_end, wait, message_box, message_box_return}
    updates = []
    current = None
    box_open = False
    previous_input = inputs[0]
    previous_frame = 0
    booted = False
    reads = 0
    for ordinal, event in enumerate(events):
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
            current = {"sourceFrame": frame, "mainLoopDispatches": 0, "inputRecord": reads,
                       "bootCleared": False, "messageBoxStartFrame": None, "messageBoxEndFrame": None}
            reads += 1
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
            # CommonBootSection ($80:8482) clears all of bank $7E, including the NMI
            # counter, game state and held/new/previous controller words, before entering
            # MainGameLoop. Power-on runs it after the logo; quitting from game over and
            # the soft reset run it again mid-movie. Such a dispatch, recognizable by its
            # zero state and NMI counter, consumes an empty latch, whatever the last NMI
            # read, and the next read's edge starts from zero.
            boot_cleared = int(event["state"], 16) == 0 and int(event["nmi"], 16) == 0
            if from_reset and not booted and not boot_cleared:
                raise ValueError(f"First main-loop dispatch at SMV frame {frame} follows no CommonBootSection clear")
            if boot_cleared:
                if int(event["held"], 16) or int(event["pressed"], 16):
                    raise ValueError(f"Main-loop dispatch at SMV frame {frame} did not see the boot-cleared controller latch")
                if current["mainLoopDispatches"]:
                    # The previous dispatch (quitting game over, a soft reset) jumped into
                    # the boot without waiting for NMI: a second dispatch under one read.
                    if box_open:
                        raise ValueError(f"Boot clear at SMV frame {frame} interrupts a message box")
                    updates.append(current)
                    current = {"sourceFrame": frame, "mainLoopDispatches": 0, "inputRecord": None,
                               "bootCleared": True, "messageBoxStartFrame": None, "messageBoxEndFrame": None}
                current.update(input=0, pressed=0)
                previous_input = 0
            booted = True
            current["mainLoopDispatches"] += 1
        elif pc == message_box:
            if current is None or "input" not in current or not current["mainLoopDispatches"]:
                raise ValueError(f"Message box at SMV frame {frame} precedes its dispatch")
            if current["messageBoxStartFrame"] is not None or box_open:
                raise ValueError(f"Two message boxes in the dispatch at SMV frame {current['sourceFrame']}")
            current["messageBoxStartFrame"] = frame
            box_open = True
        elif pc == message_box_return:
            if current is None or not box_open:
                raise ValueError(f"Message box return at SMV frame {frame} has no open box")
            if current["messageBoxEndFrame"] is not None:
                raise ValueError(f"Two message box returns in the update at SMV frame {current['sourceFrame']}")
            current["messageBoxEndFrame"] = frame
            box_open = False
    if current is None or "input" not in current:
        raise ValueError("No completed terminal input step")
    if box_open:
        raise ValueError("The movie ends inside a message box")
    updates.append(current)
    if updates[-1]["sourceFrame"] != frames:
        raise ValueError("Native trace does not cover the original movie's final input")
    return updates, reads


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
    if movie[:4] == b"PK\x03\x04":
        # lsnes movie: one input line per emulated frame, always from power-on. The
        # bsnes v085 adapter labels each read with the line the controller latched, and
        # runs one frame past the movie so the game reads its last line, as Snes9x reads
        # an SMV's trailing word: the line count is the source frame count plus one.
        import sys
        sys.path.insert(0, str(Path(__file__).resolve().parent / "lsmv-native-capture"))
        from lsmv_movie import read_frames
        words = read_frames(args.movie, args.rom.read_bytes())
        frames = len(words) - 1
        inputs = tuple(words)
        from_reset = True
        native_capture = ("bsnes v085 (lsnes compatibility core, debugger option); "
                          "instrumented J/U input boundaries and MessageBox_Routine entry and return")
    else:
        if len(movie) < 32 or movie[:4] != b"SMV\x1a":
            raise ValueError("Not an SMV or lsnes movie")
        version, = struct.unpack_from("<I", movie, 4)
        frames, = struct.unpack_from("<I", movie, 16)
        offset, = struct.unpack_from("<I", movie, 28)
        if version not in (1, 4, 5) or movie[20] != 1:
            raise ValueError("Only one-controller SMV versions 1, 4 and 5 are supported")
        if offset < 32 or offset + 2 * (frames + 1) > len(movie):
            raise ValueError("Truncated SMV controller stream")
        inputs = struct.unpack_from(f"<{frames + 1}H", movie, offset)
        # SMV v4/v5 option bit 0: the recording starts at power-on (with SRAM) rather
        # than from an embedded snapshot, so the native boot prelude precedes gameplay.
        from_reset = version in (4, 5) and bool(movie[21] & 1)
        native_capture = ("Snes9x 1.60 913b75d07c6e8d54e966e2c4a79d7c55428007df; "
                          "instrumented J/U input boundaries and MessageBox_Routine entry and return")
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
    with events_path.open(newline="", encoding="utf-8") as source:
        updates, reads = collect_input_updates(csv.DictReader(source), inputs, frames, from_reset)
    for index, update in enumerate(updates):
        if update["mainLoopDispatches"] not in (0, 1):
            raise ValueError("Multiple main-loop dispatches require a richer replay format")
        update["kind"] = "main-loop" if update.pop("mainLoopDispatches") else "nmi-continuation"
        update["update"] = index + 1
        following = updates[index + 1] if index + 1 < len(updates) else None
        # The checkpoint completing this update is the one captured at the next read. A
        # dispatch that jumped straight into the boot clear has none: no read followed it.
        update["expectedRecord"] = following["inputRecord"] if following else reads
        update["endSourceFrame"] = following["sourceFrame"] if following else frames
    # Validate every private checkpoint, not just the JSON count. The final record
    # is captured after the original movie ends; it is not an early checkpoint.
    # These are the pinned J/U WRAM owners, not inferred durations. IRQ scrolling
    # can continue while the CPU uploads data; neither accepted NMI nor an active
    # APU upload alone proves a gameplay update or a disposable video refresh.
    apu_uploading, door_scroll_counter = 0x0617, 0x0925
    # Enemy.ID of each of the 32 enemy slots ($0F78 + $40 * slot).
    enemy_id, enemy_slot_size, enemy_slots = 0x0F78, 0x40, 32
    # Initialise_Enemies zeroes $0E4E on entry and stores the enemy count on exit.
    initialised_enemy_count = 0x0E4E
    record_timing = []
    record_loading = []
    record_enemy_ids = []
    read_frames = [u["sourceFrame"] for u in updates if u["inputRecord"] is not None]
    with gzip.open(boundaries_path, "rb") as source:
        for index in range(reads + 1):
            record = source.read(131080)
            if len(record) != 131080:
                raise ValueError(f"Truncated native checkpoint {index}")
            frame, pc = struct.unpack_from("<II", record)
            expected_frame = read_frames[index] if index < reads else frames
            record_timing.append((
                struct.unpack_from("<H", record, 8 + apu_uploading)[0],
                struct.unpack_from("<H", record, 8 + door_scroll_counter)[0]))
            record_loading.append((
                struct.unpack_from("<H", record, 8 + 0x0998)[0],  # GameState
                struct.unpack_from("<H", record, 8 + 0x099c)[0],  # DoorTransitionFunction
                bool(struct.unpack_from("<H", record, 8 + 0x0931)[0] & 0x8000)))
            record_enemy_ids.append((tuple(
                struct.unpack_from("<H", record, 8 + enemy_id + enemy_slot_size * slot)[0]
                for slot in range(enemy_slots)),
                struct.unpack_from("<H", record, 8 + initialised_enemy_count)[0]))
            if frame != expected_frame or pc != (READ_ENTER if index < reads else 0):
                raise ValueError(f"Checkpoint {index} disagrees with its input boundary")
        if source.read(1):
            raise ValueError("Unexpected trailing native checkpoints")
    # Every update starts at a boundary: its read's checkpoint or, entering a dispatch
    # straight out of CommonBootSection, the cleared bank $7E those owners then hold.
    def boundary(values, cleared, record_index):
        return cleared if record_index is None else values[record_index]
    starts = [u["inputRecord"] for u in updates] + [reads]
    boundary_timing = [boundary(record_timing, (0, 0), r) for r in starts]
    boundary_loading = [boundary(record_loading, (0, 0, False), r) for r in starts]
    boundary_enemy_ids = [boundary(record_enemy_ids, ((0,) * enemy_slots, 0), r) for r in starts]
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
        update["timingClass"] = classify_timing(
            update["kind"], upload != 0, scroll, next_scroll,
            updates[index - 1]["timingClass"] if index else None)
    timing_counts = {name: sum(u["timingClass"] == name for u in updates) for name in (
        "main-loop", "door-scroll-continuation", "apu-upload-continuation",
        "apu-upload-tail-continuation", "other-continuation")}
    source_frames = {u["sourceFrame"] for u in updates}
    accepted_input_count = len(updates)
    prelude = boot_prelude_length(updates) if from_reset else 0
    updates, excluded = normalize_upload_intervals(updates, inputs[0], prelude)
    manifest = {
        "format": "super-metroid-gameplay-updates-v7",
        "startsFromReset": from_reset,
        "initialRecord": prelude,
        "bootPreludeInputsExcluded": prelude,
        "movieSha256": hashlib.sha256(movie).hexdigest().upper(),
        "romSha256": rom_hash,
        "nativeCapture": native_capture,
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
