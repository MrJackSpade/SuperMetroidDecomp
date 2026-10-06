// Executes original minimap and paused HUD routines on the pinned 65816 core, not
// the translated C routines. ROM is never patched.
#include <windows.h>
#include <stdexcept>
#include <vector>
extern "C" {
#include "../../../upstream-sm/src/snes/cpu.h"
#include "../../../upstream-sm/src/snes/snes.h"
Snes *g_snes = nullptr;
unsigned char g_ram[0x20000];
}
static std::vector<unsigned char> rom;
static unsigned char mdr;
extern "C" uint8_t snes_cpuRead(Snes *, uint32_t address) {
  unsigned bank = address >> 16, offset = address & 0xffff;
  if (bank == 0x7e || bank == 0x7f) return mdr = g_ram[address & 0x1ffff];
  if ((bank & 0x40) == 0 && offset < 0x2000) return mdr = g_ram[offset];
  if (offset >= 0x8000) return mdr = rom.at(((bank & 0x7f) << 15) | (offset & 0x7fff));
  throw std::runtime_error("Unexpected native HUD bus read");
}
extern "C" void snes_cpuWrite(Snes *, uint32_t address, uint8_t value) {
  unsigned bank = address >> 16, offset = address & 0xffff;
  mdr = value;
  if (bank == 0x7e || bank == 0x7f) { g_ram[address & 0x1ffff] = value; return; }
  if ((bank & 0x40) == 0 && offset < 0x2000) { g_ram[offset] = value; return; }
  throw std::runtime_error("Unexpected native HUD bus write");
}
extern "C" int CpuOpcodeHook(uint32_t) { throw std::runtime_error("Unexpected native BRK"); }
extern "C" bool HookedFunctionRts(int) { return false; }
extern "C" __declspec(noreturn) void Die(const char *error) { throw std::runtime_error(error); }
static void word(unsigned address, unsigned value) { g_ram[address] = value; g_ram[address + 1] = value >> 8; }
static unsigned readword(unsigned address) { return g_ram[address] | g_ram[address + 1] << 8; }
static void call(unsigned address) {
  Cpu *cpu = cpu_init(nullptr, 0);
  if (!cpu) throw std::runtime_error("CPU allocation failed");
  cpu->k = address >> 16; cpu->db = address >> 16; cpu->pc = address & 0xffff;
  cpu->sp = 0x1fc; word(0x1fd, 0x7fff); g_ram[0x1ff] = 0x80;
  int steps = 0;
  while (!(cpu->k == 0x80 && cpu->pc == 0x8000) && ++steps < 10000) cpu_runOpcode(cpu);
  cpu_free(cpu);
  if (steps >= 10000) throw std::runtime_error("Native routine did not return");
}
int main(int argc, char **argv) {
  SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX);
  try {
    if (argc != 2) throw std::runtime_error("usage: probe supported-rom.smc");
    FILE *file = fopen(argv[1], "rb");
    if (!file) throw std::runtime_error("Cannot open ROM");
    rom.resize(0x300000);
    size_t count = fread(rom.data(), 1, rom.size(), file); fclose(file);
    if (count != rom.size()) throw std::runtime_error("ROM size mismatch");
    puts("phase,initialCenter,pausedFrame,center");
    for (unsigned phase : {0u, 8u}) {
      memset(g_ram, 0, sizeof(g_ram)); mdr = 0;
      word(0x7a1, 28); word(0x7a3, 1); word(0x7a5, 0x90); word(0x7a7, 0x50);
      word(0xaf6, 0x200); word(0xafa, 0x300); word(0x5b5, phase);
      word(0x9c2, 99); word(0x9c4, 99); word(0xa06, 99); word(0x998, 0x0f);
      call(0x90a91b);
      unsigned initial = readword(0xc680);
      for (unsigned frame = 0; frame < 24; frame++) {
        word(0x5b5, phase + frame + 1);
        call(0x809b44);
        printf("%u,%04X,%u,%04X\n", phase, initial, frame, readword(0xc680));
      }
    }
    return 0;
  } catch (const std::exception &error) { fprintf(stderr, "%s\n", error.what()); return 1; }
    catch (...) { fprintf(stderr, "Unknown native HUD probe failure\n"); return 1; }
}