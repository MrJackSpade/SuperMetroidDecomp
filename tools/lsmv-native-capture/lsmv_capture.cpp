// Native lsnes-movie capture on bsnes v085 (lsnes's patched compatibility core, debugger
// option). Powers on like lsnes's load_rom with its default settings, then runs one
// snes_run per movie frame with that frame's controller word answering every poll.
// Records the same input events, update-boundary WRAM and terminal state as the Snes9x
// adapter in ../smv-native-capture, so tools/convert-smv-updates.py can consume them.
// The movie start time a different lsnes movie records must replace movieStartTime.
#include <snes/snes.hpp>
#include <ui-libsnes/libsnes.hpp>
#include <zlib.h>
#include <cstdio>
#include <cstring>
#include <exception>
#include <fstream>
#include <stdexcept>
#include <string>
#include <vector>

static std::vector<uint16_t> frames;
static unsigned currentFrame = 0;
// Movie line most recently latched by the controller hardware. bsnes's run() returns at
// vblank start right after the auto-joypad latch, so the NMI routine reading $4218 runs in
// the next run(); events carry the line the game is consuming, as the converter requires.
static unsigned latchedFrame = 0;
// The movie's last line is latched at the end of its final frame, so the game reads it in one
// further emulated frame, as Snes9x consumes an SMV's trailing word. That frame's own latch
// polls past the movie; the value is never read, provided the final read came first.
static bool finalLineRead = false;
static bool latchedPastMovieBeforeFinalRead = false;
static gzFile updateTrace = nullptr;
static gzFile instructionTrace = nullptr;
static FILE* eventCsv = nullptr;
static unsigned eventCount = 0;
static bool traceActive = false;
static unsigned traceStart = 0, traceEnd = 0;

// Capture-format boundary identities shared with the Snes9x adapter.
static bool isBoundary(unsigned pc) {
  switch(pc) {
  case 0x828948: case 0x82897a:            // main-loop entry / completed outer update
  case 0x809459: case 0x809496:            // controller-read entry / return
  case 0x808338:                           // explicit NMI wait
  case 0x858080: case 0x8580ba:            // MessageBox_Routine entry / return
    return true;
  }
  return false;
}

static unsigned word(unsigned address) {
  return SNES::cpu.wram[address] | SNES::cpu.wram[address + 1] << 8;
}

static void observeBoundary(unsigned pc) {
  if(pc == 0x809496 && latchedFrame + 1 == frames.size()) finalLineRead = true;
  std::fprintf(eventCsv, "%u,%u,%x,%x,%x,%x,%x,%x\n", eventCount++, latchedFrame, pc,
    word(0x8b), word(0x8f), word(0x5b6), word(0x998), word(0x99c));
  if(pc != 0x809459) return;
  unsigned frame = latchedFrame;
  if(gzwrite(updateTrace, &frame, 4) != 4 || gzwrite(updateTrace, &pc, 4) != 4 ||
     gzwrite(updateTrace, SNES::cpu.wram, 131072) != 131072)
    throw std::runtime_error("Cannot write native update boundary");
}

static bool stepEvent() {
  unsigned pc = SNES::cpu.regs.pc;
  if(traceActive) {
    unsigned frame = currentFrame;
    if(gzwrite(instructionTrace, &frame, 4) != 4 || gzwrite(instructionTrace, &pc, 4) != 4)
      throw std::runtime_error("Cannot write instruction trace");
  }
  if(updateTrace && isBoundary(pc)) observeBoundary(pc);
  return false;  // never break: the hook observes without altering emulation
}

// lsnes installs its own SNES::Interface; bsnes never assigns one itself. Its clock and
// random seed come from the movie start time, which a power-on lsnes movie records as
// starttime.second. Super Metroid has no RTC chip, and with config.random off the seeded
// generator returns its argument unchanged, but both are reproduced exactly regardless.
static const time_t movieStartTime = 1000000000;

struct CaptureInterface : SNES::Interface {
  void videoRefresh(const uint32_t*, bool, bool, bool) override {}
  void audioSample(int16_t, int16_t) override {}
  int16_t inputPoll(bool port, SNES::Input::Device device, unsigned index, unsigned id) override {
    // lsnes default: port 1 gamepad, port 2 none. Button id n is $4218 bit 15 - n.
    if(port || device != SNES::Input::Device::Joypad || index != 0 || id > 11) return 0;
    if(currentFrame >= frames.size()) {
      // Exceptions cannot cross bsnes's cothreads; main() fails after the run instead.
      if(!finalLineRead) latchedPastMovieBeforeFinalRead = true;
      return 0;
    }
    return (frames[currentFrame] >> (15 - id)) & 1;
  }
  nall::string path(SNES::Cartridge::Slot, const nall::string& hint) override { return hint; }
  time_t currentTime() override { return movieStartTime; }
  time_t randomSeed() override { return movieStartTime; }
  // lsnes patch 0011 reports every controller latch, automatic or through $4016.
  void notifyLatched() override { latchedFrame = currentFrame; }
};
static CaptureInterface captureInterface;

int main(int argc, char** argv) {
  SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX | SEM_NOOPENFILEERRORBOX);
  try {
    bool traceMode = argc == 7 && std::string(argv[4]) == "--trace";
    if(argc != 4 && !traceMode)
      throw std::runtime_error("usage: lsmv_capture ROM FRAMES OUT | lsmv_capture ROM FRAMES OUT --trace START END");
    std::string output = argv[3];
    CreateDirectoryA(output.c_str(), nullptr);

    std::ifstream frameFile(argv[2], std::ios::binary);
    if(!frameFile) throw std::runtime_error("Frame file open failed");
    std::vector<char> frameBytes((std::istreambuf_iterator<char>(frameFile)), {});
    if(frameBytes.empty() || frameBytes.size() % 2) throw std::runtime_error("Frame file is not whole words");
    frames.resize(frameBytes.size() / 2);
    std::memcpy(frames.data(), frameBytes.data(), frameBytes.size());

    std::ifstream romFile(argv[1], std::ios::binary);
    if(!romFile) throw std::runtime_error("ROM open failed");
    std::vector<uint8_t> rom((std::istreambuf_iterator<char>(romFile)), {});

    // lsnes load_rom with default settings: no random initial state, stock poll timings,
    // no bus fixes, stock clock table, then load (which powers on) and connect ports.
    SNES::interface = &captureInterface;
    snes_init();
    snes_term();
    snes_unload_cartridge();
    SNES::config.random = false;
    SNES::config.expansion_port = SNES::System::ExpansionPortDevice::None;
    SNES::config.cpu.alt_poll_timings = false;
    SNES::config.cpu.bus_fixes = false;
    SNES::config.cpu.ntsc_frequency = 21477272;
    SNES::config.cpu.pal_frequency = 21281370;
    SNES::config.smp.ntsc_frequency = 24607104;
    SNES::config.smp.pal_frequency = 24607104;
    if(!snes_load_cartridge_normal(nullptr, rom.data(), rom.size()))
      throw std::runtime_error("ROM load failed");
    snes_set_controller_port_device(false, SNES_DEVICE_JOYPAD);
    snes_set_controller_port_device(true, SNES_DEVICE_NONE);
    SNES::cpu.step_event = stepEvent;

    unsigned length = frames.size();
    if(traceMode) {
      traceStart = std::stoul(argv[5]);
      traceEnd = std::stoul(argv[6]);
      if(traceStart > traceEnd || traceEnd >= length) throw std::runtime_error("Trace window must lie within the movie");
      instructionTrace = gzopen((output + "/instructions.bin.gz").c_str(), "wb1");
      if(!instructionTrace) throw std::runtime_error("Cannot open instruction trace");
      for(currentFrame = 0; currentFrame <= traceEnd; currentFrame++) {
        traceActive = currentFrame >= traceStart;
        snes_run();
      }
      traceActive = false;
      if(gzclose(instructionTrace) != Z_OK) throw std::runtime_error("Cannot finalize instruction trace");
      std::fprintf(stderr, "Traced source frames %u..%u\n", traceStart, traceEnd);
      return 0;
    }

    updateTrace = gzopen((output + "/update-boundaries.wram.gz").c_str(), "wb1");
    if(!updateTrace) throw std::runtime_error("Cannot open update trace");
    eventCsv = std::fopen((output + "/input-events.csv").c_str(), "w");
    if(!eventCsv) throw std::runtime_error("Cannot open event stream");
    std::fprintf(eventCsv, "event,source_frame,pc,held,pressed,nmi,state,door_function\n");
    for(currentFrame = 0; currentFrame < length; currentFrame++) {
      snes_run();
      if(currentFrame % 10000 == 0)
        std::fprintf(stderr, "Frame %u/%u, room %04X\n", currentFrame, length, word(0x79b));
    }
    currentFrame = length;
    snes_run();
    if(!finalLineRead || latchedPastMovieBeforeFinalRead)
      throw std::runtime_error("The frame after the movie did not read its final latched line first");
    // Terminal record after the read of the last line. As with an SMV's frames+1 words,
    // the line count is the source frame count plus one.
    unsigned sourceFrames = length - 1;
    unsigned terminalPc = 0;
    if(gzwrite(updateTrace, &sourceFrames, 4) != 4 || gzwrite(updateTrace, &terminalPc, 4) != 4 ||
       gzwrite(updateTrace, SNES::cpu.wram, 131072) != 131072)
      throw std::runtime_error("Cannot write terminal update state");
    if(std::fclose(eventCsv) != 0) throw std::runtime_error("Cannot finalize event stream");
    eventCsv = nullptr;
    if(gzclose(updateTrace) != Z_OK) throw std::runtime_error("Cannot finalize update trace");
    updateTrace = nullptr;
    std::fprintf(stderr, "Completed original movie: %u lines, %u source frames, %u events\n", length, sourceFrames, eventCount);
    return 0;
  } catch(const std::exception& e) {
    std::fprintf(stderr, "%s\n", e.what());
    return 1;
  } catch(...) {
    std::fprintf(stderr, "Unknown native capture exception\n");
    return 2;
  }
}
