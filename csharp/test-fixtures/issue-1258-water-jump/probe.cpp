// #1258: bounded original-CPU jump in the reported room's exported collision data.
#include <windows.h>
#include <stdexcept>
#include <vector>
extern "C" {
#include "../../../upstream-sm/src/snes/cpu.h"
#include "../../../upstream-sm/src/snes/snes.h"
#include "../../../upstream-sm/src/variables.h"
Snes *g_snes = nullptr;
unsigned char g_ram[0x20000];
}
static std::vector<unsigned char> rom;
static unsigned char mathregs[0x18];
static bool returned;
static void bad(const char *operation, unsigned address) {
  char message[128]; sprintf(message, "Unexpected native %s at %06X", operation, address);
  throw std::runtime_error(message);
}
extern "C" uint8_t snes_cpuRead(Snes *, uint32_t address) {
  unsigned bank = address >> 16, offset = address & 0xffff;
  if (bank == 0x7e || bank == 0x7f) return g_ram[address & 0x1ffff];
  if ((bank & 0x40) == 0 && offset < 0x2000) return g_ram[offset];
  if (offset >= 0x8000) return rom.at(((bank & 0x7f) << 15) | (offset & 0x7fff));
  if ((bank & 0x40) == 0 && offset >= 0x4214 && offset <= 0x4217) return mathregs[offset - 0x4200];
  bad("read", address); return 0;
}
extern "C" void snes_cpuWrite(Snes *, uint32_t address, uint8_t value) {
  unsigned bank = address >> 16, offset = address & 0xffff;
  if (bank == 0x7e || bank == 0x7f) { g_ram[address & 0x1ffff] = value; return; }
  if ((bank & 0x40) == 0 && offset < 0x2000) { g_ram[offset] = value; return; }
  if ((bank & 0x40) == 0 && offset >= 0x4202 && offset <= 0x4206) {
    mathregs[offset - 0x4200] = value;
    if (offset == 0x4203) {
      unsigned product = mathregs[2] * value;
      mathregs[0x16] = product; mathregs[0x17] = product >> 8;
    }
    if (offset == 0x4206) {
      unsigned dividend = mathregs[4] | mathregs[5] << 8;
      unsigned quotient = value ? dividend / value : 0xffff;
      unsigned remainder = value ? dividend % value : dividend;
      mathregs[0x14] = quotient; mathregs[0x15] = quotient >> 8;
      mathregs[0x16] = remainder; mathregs[0x17] = remainder >> 8;
    }
    return;
  }
  bad("write", address);
}
extern "C" int CpuOpcodeHook(uint32_t address) { bad("BRK", address); return 0; }
extern "C" bool HookedFunctionRts(int) { returned = true; return true; }
extern "C" __declspec(noreturn) void Die(const char *error) { throw std::runtime_error(error); }
static void run(Cpu *cpu, unsigned address) {
  cpu->db = cpu->k = address >> 16; cpu->pc = address;
  cpu->a = cpu->x = cpu->y = 0; cpu->mf = cpu->xf = cpu->e = false;
  cpu->sp = cpu->spBreakpoint = 0x1ff0; returned = false;
  for (int budget = 200000; !returned; --budget) {
    if (!budget) bad("instruction limit", (cpu->k << 16) | cpu->pc);
    cpu_runOpcode(cpu);
  }
}
static FILE *open(const char *path, const char *mode) {
  FILE *file = fopen(path, mode); if (!file) throw std::runtime_error("Cannot open probe input"); return file;
}
int main(int argc, char **argv) {
  SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX);
  try {
    if (argc != 4) throw std::runtime_error("usage: probe ROM room.bin seed.txt");
    FILE *file = open(argv[1], "rb"); rom.resize(0x300000);
    size_t count = fread(rom.data(), 1, rom.size(), file); fclose(file);
    if (count != rom.size()) throw std::runtime_error("ROM size mismatch");
    file = open(argv[2], "rb"); unsigned short width, height;
    if (fread(&width, 2, 1, file) != 1 || fread(&height, 2, 1, file) != 1 || width != 32 || height != 32)
      throw std::runtime_error("Unexpected retail room dimensions");
    if (fread(level_data, 2, width * height, file) != width * height || fread(BTS, 1, width * height, file) != width * height)
      throw std::runtime_error("Incomplete retail collision data");
    fclose(file);
    unsigned seed[11]; file = open(argv[3], "r");
    for (unsigned &value : seed) if (fscanf(file, "%x", &value) != 1) throw std::runtime_error("Incomplete seed");
    fclose(file);
    room_width_in_blocks = width; room_height_in_blocks = height; room_size_in_blocks = width * height * 2;
    interactive_enemy_indexes[0] = 0xffff;
    samus_x_pos = samus_prev_x_pos = seed[0]; samus_x_subpos = seed[1];
    samus_y_pos = samus_prev_y_pos = seed[2]; samus_y_subpos = seed[3];
    samus_pose = samus_prev_pose = seed[4]; samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_anim_frame = seed[5]; samus_anim_frame_timer = seed[6]; samus_anim_frame_buffer = seed[7];
    fx_y_pos = seed[8]; lava_acid_y_pos = 0xffff; fx_liquid_options = seed[9]; fx_type = 6; liquid_physics_type = seed[10];
    equipped_items = 0; samus_health = samus_max_health = 99;
    samus_x_speed_table_pointer = 0x9f55; samus_input_handler = 0xe913;
    grapple_beam_function = 0xc4f0; samus_y_dir = 2;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    Cpu *cpu = cpu_init(nullptr, 0); if (!cpu) throw std::runtime_error("CPU allocation failed");
    puts("frame,pose,y,yspeed,ydir,radius,medium");
    for (int frame = 0; frame < 60; frame++) {
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      joypad1_lastkeys = 0x80; joypad1_newkeys = frame == 0 ? 0x80 : 0;
      run(cpu, 0x90ec22); run(cpu, 0x90e90f); run(cpu, 0x909c5b);
      run(cpu, 0x90a337); run(cpu, 0x908000); run(cpu, 0x91e8b6); run(cpu, 0x91eb88); run(cpu, 0x90eab3);
      printf("%d,%02X,%04X%04X,%04X%04X,%u,%u,%u\n", frame, samus_pose, samus_y_pos, samus_y_subpos,
        samus_y_speed, samus_y_subspeed, samus_y_dir, samus_y_radius, liquid_physics_type);
    }
    cpu_free(cpu); return 0;
  } catch (const std::exception &error) { fprintf(stderr, "%s\n", error.what()); return 1; }
    catch (...) { fprintf(stderr, "Unknown native shallow-water probe failure\n"); return 1; }
}