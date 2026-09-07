#include <stdio.h>
#include <stdint.h>
#include <stdbool.h>
#include <stdlib.h>
#include <string.h>
#include "../../../upstream-sm/src/snes/cpu.h"
#include "../../../upstream-sm/src/snes/snes.h"
#include "fixture.h"

/* Execute ROM bytes, not translated gameplay. Unknown accesses fail on stderr. */
static uint8_t rom[0x300000], ram[0x20000];
static bool returned;
void Die(const char *message) { fprintf(stderr, "%s", message); exit(2); }
int CpuOpcodeHook(uint32_t address) { (void)address; Die("Unexpected CPU opcode hook\n"); return 0; }
bool HookedFunctionRts(int is_long) { (void)is_long; returned = true; return true; }
uint8_t snes_cpuRead(Snes *snes, uint32_t address) {
  (void)snes;
  unsigned bank = address >> 16, offset = address & 0xffff;
  if (bank == 0x7e || bank == 0x7f) return ram[address - 0x7e0000];
  if ((bank & 0x7f) < 0x40 && offset < 0x2000) return ram[offset];
  if (offset >= 0x8000) {
    unsigned index = ((bank & 0x7f) << 15) | (offset & 0x7fff);
    if (index < sizeof(rom)) return rom[index];
  }
  fprintf(stderr, "Unmapped read %06X\n", address); exit(2);
}
void snes_cpuWrite(Snes *snes, uint32_t address, uint8_t value) {
  (void)snes;
  unsigned bank = address >> 16, offset = address & 0xffff;
  if (bank == 0x7e || bank == 0x7f) { ram[address - 0x7e0000] = value; return; }
  if ((bank & 0x7f) < 0x40 && offset < 0x2000) { ram[offset] = value; return; }
  fprintf(stderr, "Unmapped write %06X\n", address); exit(2);
}
static void word(unsigned address, unsigned value) { ram[address] = value; ram[address + 1] = value >> 8; }
int main(int argc, char **argv) {
  if (argc != 2) { fprintf(stderr, "Usage: audit <unheadered-rom>\n"); return 2; }
  FILE *file = fopen(argv[1], "rb");
  if (!file || fread(rom, 1, sizeof(rom), file) != sizeof(rom)) return 2;
  fclose(file);
  for (int left = 0; left < 2; left++) {
    memset(ram, 0, sizeof(ram));
    word(Shot, 0x40); word(Jump, 0x80); word(Dash, 0x8000); word(Cancel, 0x4000);
    word(Select, 0x2000); word(AimDown, 0x20); word(AimUp, 0x10);
    word(Pose, left ? ReleasedLeft : ReleasedRight);
    ram[MovementType] = NormalJumping;
    word(HeldInput, left ? 0x210 : 0x110);
    word(ProspectivePose, 0xffff);
    Cpu *cpu = cpu_init(NULL, 0);
    cpu->pc = NativePoseInput & 0xffff; cpu->k = cpu->db = NativePoseInput >> 16;
    cpu->sp = cpu->spBreakpoint = 0x1ff;
    returned = false;
    for (int i = 0; i < 10000 && !returned; i++) cpu_runOpcode(cpu);
    unsigned result = ram[ProspectivePose] | ram[ProspectivePose + 1] << 8;
    printf("Native released-%s held aim: prospective pose=%04X\n", left ? "left" : "right", result);
    if (!returned || result != (left ? AimLeft : AimRight)) return 1;
    cpu_free(cpu);
  }
  return 0;
}
