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
static unsigned multiplier, product, dividend, quotient, remainder;
void Die(const char *message) { fprintf(stderr, "%s", message); exit(2); }
int CpuOpcodeHook(uint32_t address) { (void)address; Die("Unexpected CPU opcode hook\n"); return 0; }
bool HookedFunctionRts(int is_long) { (void)is_long; returned = true; return true; }
uint8_t snes_cpuRead(Snes *snes, uint32_t address) {
  (void)snes;
  unsigned bank = address >> 16, offset = address & 0xffff;
  if ((bank & 0x7f) < 0x40) {
    if (offset == 0x4214) return quotient;
    if (offset == 0x4215) return quotient >> 8;
    if (offset == 0x4216) return remainder;
    if (offset == 0x4217) return remainder >> 8;
  }
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
  /* The CPU waits the required cycles in the retail routines; these fixtures only
     observe completed arithmetic results, not intermediate hardware latency. */
  if ((bank & 0x7f) < 0x40) {
    if (offset == 0x4202) { multiplier = value; return; }
    if (offset == 0x4203) { product = multiplier * value; remainder = product; return; }
    if (offset == 0x4204) { dividend = (dividend & 0xff00) | value; return; }
    if (offset == 0x4205) { dividend = (dividend & 0xff) | (value << 8); return; }
    if (offset == 0x4206) {
      quotient = value ? dividend / value : 0xffff;
      remainder = value ? dividend % value : dividend;
      return;
    }
  }
  if (bank == 0x7e || bank == 0x7f) { ram[address - 0x7e0000] = value; return; }
  if ((bank & 0x7f) < 0x40 && offset < 0x2000) { ram[offset] = value; return; }
  fprintf(stderr, "Unmapped write %06X\n", address); exit(2);
}
static void word(unsigned address, unsigned value) { ram[address] = value; ram[address + 1] = value >> 8; }
static unsigned readword(unsigned address) { return ram[address] | ram[address + 1] << 8; }
static void run(unsigned address) {
  Cpu *cpu = cpu_init(NULL, 0);
  cpu->pc = address & 0xffff; cpu->k = cpu->db = address >> 16;
  cpu->sp = cpu->spBreakpoint = 0x1ff;
  returned = false;
  for (int i = 0; i < 100000 && !returned; i++) cpu_runOpcode(cpu);
  cpu_free(cpu);
  if (!returned) Die("Native fixture exceeded instruction limit\n");
}
int main(int argc, char **argv) {
  if (argc != 2) { fprintf(stderr, "Usage: audit <unheadered-rom>\n"); return 2; }
  FILE *file = fopen(argv[1], "rb");
  if (!file || fread(rom, 1, sizeof(rom), file) != sizeof(rom)) return 2;
  fclose(file);
  for (int left = 0; left < 2; left++) {
    memset(ram, 0, sizeof(ram));
    word(Pose, left ? 0x14 : 0x13); ram[MovementType] = NormalJumping;
    word(Shot, 0x40); word(PreviousDrawNewInput, 0x40);
    word(GrappleFunction, GrappleInactive);
    /* Current NMI new keys remain zero: only the post-draw snapshot can fire. */
    run(NativeGrappleInactive);
    printf("Native retained Fire edge, facing %s: function=%04X\n", left ? "left" : "right", readword(GrappleFunction));
    if (readword(GrappleFunction) != GrappleFiring) return 1;
  }
  /* State 0's ledge geometry: the body's center is over air, but its left
     boundary intersects a solid column. Execute the original ROM routine with
     the swing radius, not a fabricated upward nudge or a center-only probe. */
  memset(ram, 0, sizeof(ram));
  word(RoomWidth, 96); word(SamusX, 0x542); word(SamusY, 0x4a1);
  word(SamusRadiusX, 5); word(SamusRadiusY, 17);
  word(LevelWords + 2 * (74 * 96 + 82), 0x1311);
  ram[BlockBts + 74 * 96 + 82] = 0x53;
  word(LevelWords + 2 * (74 * 96 + 83), 0x8311);
  for (int row = 75; row <= 77; row++) {
    word(LevelWords + 2 * (row * 96 + 82), row == 75 ? 0x82f3 : 0x0313);
    if (row == 75) word(LevelWords + 2 * (row * 96 + 83), 0x8311);
  }
  Cpu *collision = cpu_init(NULL, 0);
  collision->pc = NativePostGrappleCollision & 0xffff;
  collision->k = NativePostGrappleCollision >> 16;
  collision->db = 0x90;
  collision->sp = collision->spBreakpoint = 0x1ff;
  returned = false;
  for (int i = 0; i < 100000 && !returned; i++) cpu_runOpcode(collision);
  unsigned ejectedY = ram[SamusY] | ram[SamusY + 1] << 8;
  printf("Native post-grapple ledge ejection: Y=%04X (start 04A1, radius 17)\n", ejectedY);
  cpu_free(collision);
  if (!returned || ejectedY != 0x48f) return 1;
  word(Pose, 0xb2); word(PreviousPose, 0xb2); word(PreviousDirection, 0x1608);
  word(SuperSpecialPose, ReleasedLeft); word(SuperSpecialCommand, 7);
  run(NativeUpdatePose);
  printf("Native queued release pose: Y=%04X pose=%04X radius=%u\n", readword(SamusY), readword(Pose), readword(SamusRadiusY));
  run(NativeSetRadius);
  word(DisplacementFraction, 0x1c00); word(DisplacementWhole, 0);
  run(NativeMoveVertical);
  word(SuperSpecialPose, 0xffff); word(SpecialPose, 0xffff);
  word(ProspectivePose, 0xa5); word(PoseCommand, 5);
  run(NativeUpdatePose);
  run(NativeSetRadius);
  printf("Native landing pose: Y=%04X pose=%04X radius=%u\n", readword(SamusY), readword(Pose), readword(SamusRadiusY));
  if (readword(SamusY) != 0x48d || readword(Pose) != 0xa5 || readword(SamusRadiusY) != 21) return 1;
  FILE *vectors = fopen("csharp/test-fixtures/issue-350-grounded-grapple-floor-clip/post-grapple-vectors.csv", "wb");
  if (!vectors) Die("Cannot create post-grapple reference vectors\n");
  fprintf(vectors, "type,bts,x,y,radius,ceiling,resultY\n");
  const unsigned low[] = { 0, 7, 8, 15 };
  for (unsigned type = 0; type < 16; type++)
  for (unsigned bts = 0; bts < (type == 1 ? 256 : 1); bts++) {
    if (bts & 0x20) continue; /* Unused bit does not select a new slope. */
    for (unsigned xi = 0; xi < 4; xi++)
    for (unsigned yi = 0; yi < 4; yi++)
    for (unsigned ceiling = 0; ceiling < 2; ceiling++) {
      memset(ram, 0, sizeof(ram));
      unsigned x = 80 + low[xi], y = 64 + low[yi];
      word(RoomWidth, 12); word(SamusX, x); word(SamusY, y);
      word(SamusRadiusX, 5); word(SamusRadiusY, 17);
      for (unsigned row = 2; row <= 5; row++)
      for (unsigned col = 3; col <= 7; col++) {
        if (row < 4 && !ceiling) continue;
        unsigned index = row * 12 + col;
        word(LevelWords + 2 * index, type << 12); ram[BlockBts + index] = bts;
      }
      run(NativePostGrappleCollision);
      fprintf(vectors, "%u,%u,%u,%u,17,%u,%u\n", type, bts, x, y, ceiling, readword(SamusY));
    }
  }
  fclose(vectors);
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
