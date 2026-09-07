#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <stdbool.h>
#include <string.h>
#include "../../../upstream-sm/src/snes/cpu.h"
#include "../../../upstream-sm/src/snes/snes.h"

/* Runs original ROM instructions, not a reimplementation of the projectile logic.
 * The fixture mirrors recording frame 31487: owner crosses a one-block pillar,
 * while its ten-pixel supplemental sample lands inside that pillar. */
static uint8_t rom[0x300000], ram[0x20000];
static unsigned multiplicand, product;
static bool returned;
void Die(const char *error) { fprintf(stderr, "%s", error); exit(2); }
int CpuOpcodeHook(uint32_t address) { fprintf(stderr, "Unexpected opcode hook %06X\n", address); exit(2); }
bool HookedFunctionRts(int is_long) { (void)is_long; returned = true; return true; }
uint8_t snes_cpuRead(Snes *snes, uint32_t address) {
  (void)snes;
  unsigned bank = address >> 16, offset = address & 0xffff;
  if (bank == 0x7e || bank == 0x7f) return ram[address - 0x7e0000];
  if ((bank & 0x7f) < 0x40 && offset < 0x2000) return ram[offset];
  if ((bank & 0x7f) < 0x40 && offset == 0x4216) return product;
  if ((bank & 0x7f) < 0x40 && offset == 0x4217) return product >> 8;
  if (offset >= 0x8000) {
    unsigned index = ((bank & 0x7f) << 15) | (offset & 0x7fff);
    if (index < sizeof(rom)) return rom[index];
  }
  fprintf(stderr, "Unmapped native read %06X\n", address); exit(2);
}
void snes_cpuWrite(Snes *snes, uint32_t address, uint8_t value) {
  (void)snes;
  unsigned bank = address >> 16, offset = address & 0xffff;
  if (bank == 0x7e || bank == 0x7f) { ram[address - 0x7e0000] = value; return; }
  if ((bank & 0x7f) < 0x40 && offset < 0x2000) { ram[offset] = value; return; }
  if ((bank & 0x7f) < 0x40 && offset == 0x4202) { multiplicand = value; return; }
  if ((bank & 0x7f) < 0x40 && offset == 0x4203) { product = multiplicand * value; return; }
  fprintf(stderr, "Unmapped native write %06X\n", address); exit(2);
}
static void word(unsigned address, unsigned value) { ram[address] = value; ram[address + 1] = value >> 8; }
static unsigned readword(unsigned address) { return ram[address] | ram[address + 1] << 8; }
static void expect(unsigned actual, unsigned expected, const char *property) {
  if (actual != expected) {
    fprintf(stderr, "%s: expected %04X, got %04X\n", property, expected, actual);
    exit(1);
  }
}
static void run_routine(Cpu *cpu, unsigned address) {
  cpu->pc = address; cpu->k = address >> 16; cpu->db = cpu->k;
  cpu->sp = cpu->spBreakpoint = 0x1ff; returned = false;
  for (int step = 0; step < 10000 && !returned; step++) {
    if (cpu->k == 0x94 && cpu->pc == 0xa4c8)
      printf("Native collision block-byte=%04X word=%04X helperX=%04X\n", cpu->x, readword(0x10002 + cpu->x), readword(0xb66));
    cpu_runOpcode(cpu);
  }
  if (!returned) { fprintf(stderr, "Native routine did not return\n"); exit(2); }
}
int main(int argc, char **argv) {
  if (argc != 2) return 2;
  FILE *file = fopen(argv[1], "rb");
  if (!file || fread(rom, 1, sizeof(rom), file) != sizeof(rom)) return 2;
  fclose(file);
  Cpu *cpu = cpu_init(NULL, 0);
  word(0x7a5, 64); word(0x7a9, 4);
  word(0x7b9, 64 * 16 * 2);
  word(0x10002 + 851 * 2, 0x8119);
  word(0x10002 + 852 * 2, 0x3115);
  ram[0x16402 + 852] = 0x82;
  word(0xc18, 0x8200); word(0xc1a, 0x8200);
  word(0xc7c, 0x0102); word(0xb64, 0x0141); word(0xb66, 0x0128);
  word(0xb78, 0x00d1); word(0xb7a, 0x00d1);
  word(0xbdc, 0x1200); word(0xbde, 0);
  run_routine(cpu, 0x90b406);
  printf("Native impact: owner=%04X helper=%04X helperX=%04X\n", readword(0xc18), readword(0xc1a), readword(0xb66));
  printf("Native index=%04X helperVelocity=%04X ownerX=%04X\n", readword(0xdde), readword(0xbde), readword(0xb64));
  word(0xb64, 0x0154); word(0xbdc, 0x1300);
  run_routine(cpu, 0x90b406);
  printf("Native next follow: owner=%04X helper=%04X helperX=%04X\n", readword(0xc18), readword(0xc1a), readword(0xb66));
  expect(readword(0xc1a), 0x8800, "native helper remains an explosion over air");
  expect(readword(0xb66), 0x014b, "native exploded helper follows owner");
  /* Repeat through the complete descending-slot handler, including native animation. */
  memset(ram, 0, 0x2000);
  word(0x7a5, 64); word(0x7a9, 4); word(0x7b9, 64 * 16 * 2);
  word(0xc7c, 0x0102); word(0xb78, 0x00d1); word(0xb7a, 0x00d1);
  word(0xc18, 0x8200); word(0xc1a, 0x8200);
  word(0xb64, 0x012f); word(0xb66, 0x0128); word(0xbdc, 0x1100);
  word(0xc04, 2); word(0xc06, 2);
  word(0xc40, 0x9f3b); word(0xc42, 0x9f83);
  word(0xc54, 1); word(0xc56, 1);
  word(0xc68, 0xafe5); word(0xc6a, 0xb075);
  word(0xc2c, 300); word(0xc2e, 300);
  word(0x911, 0x0100);
  for (int frame = 0; frame < 3; frame++) {
    run_routine(cpu, 0x90aece);
    printf("Native full frame %d: owner=%04X x=%04X helper=%04X x=%04X sprite=%04X\n",
      frame, readword(0xc18), readword(0xb64), readword(0xc1a), readword(0xb66), readword(0xcba));
    expect(readword(0xc18), 0x8200, "owner continues flying");
    expect(readword(0xc1a), 0x8800, "helper remains exploded");
    expect(readword(0xb66), frame == 0 ? 0x139 : frame == 1 ? 0x14b : 0x15e,
      "helper position through full native handler");
    expect(readword(0xcba), frame == 0 ? 0xa117 : 0xaa84,
      "native helper switches from blank to visible explosion");
  }
  /* Separate impact semantics from the cartridge's moving-helper quirk. */
  word(0xc1a, 0x8200); word(0xb66, 0x139); word(0xbb6, 8);
  cpu->x = 2;
  run_routine(cpu, 0x90ae06);
  expect(readword(0xb66), 0x139, "missile impact does not add beam radius");
  expect(readword(0xc1a), 0x8800, "first impact starts explosion");
  cpu->x = 2;
  run_routine(cpu, 0x90ae06);
  expect(readword(0xc1a), 0, "second impact clears an existing explosion");
  puts("Native projectile-link assertions passed.");
  cpu_free(cpu);
  return 0;
}
