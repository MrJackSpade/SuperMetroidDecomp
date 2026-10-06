// Executes the original $91:D9B2 handler on the pinned 65816 core, not sm_91.c's
// patched C translation. Only its WRAM, ROM and undriven expansion reads exist.
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
  if (bank == 0x9b && offset >= 0x6000 && offset < 0x8000) return mdr;
  throw std::runtime_error("Unexpected native palette bus read");
}
extern "C" void snes_cpuWrite(Snes *, uint32_t address, uint8_t value) {
  unsigned bank = address >> 16, offset = address & 0xffff;
  mdr = value;
  if (bank == 0x7e || bank == 0x7f) { g_ram[address & 0x1ffff] = value; return; }
  if ((bank & 0x40) == 0 && offset < 0x2000) { g_ram[offset] = value; return; }
  throw std::runtime_error("Unexpected native palette bus write");
}
extern "C" int CpuOpcodeHook(uint32_t) { throw std::runtime_error("Unexpected native BRK"); }
extern "C" bool HookedFunctionRts(int) { return false; }
extern "C" __declspec(noreturn) void Die(const char *error) { throw std::runtime_error(error); }
static void word(unsigned address, unsigned value) { g_ram[address] = value; g_ram[address + 1] = value >> 8; }
static unsigned readword(unsigned address) { return g_ram[address] | g_ram[address + 1] << 8; }
int main(int argc, char **argv) {
  SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX);
  try {
    if (argc != 2) throw std::runtime_error("usage: probe supported-rom.smc");
    FILE *file = fopen(argv[1], "rb");
    if (!file) throw std::runtime_error("Cannot open ROM");
    rom.resize(0x300000);
    size_t count = fread(rom.data(), 1, rom.size(), file); fclose(file);
    if (count != rom.size()) throw std::runtime_error("ROM size mismatch");
    puts("phase,nextPhase,timer,colors");
    for (unsigned phase : {8u, 10u}) {
      memset(g_ram, 0, sizeof(g_ram)); mdr = 0x75;
      word(0xa74, 4); word(0xb3e, 0x400); word(0xace, phase); word(0xad0, 1);
      g_ram[0xa1f] = 2;
      Cpu *cpu = cpu_init(nullptr, 0);
      if (!cpu) throw std::runtime_error("CPU allocation failed");
      cpu->k = 0x91; cpu->db = 0x91; cpu->pc = 0xd9b2; cpu->sp = 0x1fd;
      word(0x1fe, 0x7fff);
      int steps = 0;
      while (cpu->pc != 0x8000 && ++steps < 500) cpu_runOpcode(cpu);
      cpu_free(cpu);
      if (steps >= 500) throw std::runtime_error("Native routine did not return");
      printf("%u,%u,%u,", phase, readword(0xace), readword(0xad0));
      for (unsigned color = 0; color < 16; color++) printf("%04X", readword(0xc180 + color * 2) & 0x7fff);
      puts("");
    }
    return 0;
  } catch (const std::exception &error) { fprintf(stderr, "%s\n", error.what()); return 1; }
    catch (...) { fprintf(stderr, "Unknown native palette probe failure\n"); return 1; }
}
