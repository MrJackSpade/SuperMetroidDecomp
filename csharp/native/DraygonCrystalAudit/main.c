/* #431: original-CPU Crystal Flash beta / Draygon gamma shared-counter probe.
   Isolates owner entry and active handlers, not boss movement, drops or rendering. */
#define CAPTURE_DMA_CHANNEL_REGISTERS
#include "../Common/CartridgeCpuFixture.h"
#include "../Common/MovementEntryPoints.h"

enum {
  GrabEntry = 0x90e23b, GrabGamma = 0xe2a1, FlashEntry = 0x90d5a2,
  FlashRaising = 0xd678, FlashMain = 0xd6ce, FlashFinish = 0xd75b,
  Palette = 0x91d6f7
};
static void grab(int right) {
  Cpu *cpu = cpu_init(NULL, 0);
  cpu->pc = GrabEntry; cpu->k = cpu->db = GrabEntry >> 16;
  cpu->sp = cpu->spBreakpoint = 0x1ff0; cpu->a = right;
  returned = false;
  for (int n = 0; n < 1000000 && !returned; n++) cpu_runOpcode(cpu);
  cpu_free(cpu);
  if (!returned) Die("Grab entry exceeded instruction budget");
}
static uint16 input_at(int frame, int mode) {
  if (mode == 0) return 0;
  if (mode == 1) return frame == 16 ? 0x100 : 0;
  if (mode == 2) return frame < 16 ? 0 : frame & 1 ? 0x100 : 0x200;
  return frame >= 16 ? 0x100 : 0;
}
int main(int argc, char **argv) {
  if (argc != 2) return 2;
  FILE *file = fopen(argv[1], "rb");
  if (!file || fread(rom, 1, sizeof(rom), file) != sizeof(rom) || fgetc(file) != EOF)
    Die("Expected unheadered 3 MiB ROM");
  fclose(file);
  printf("order,right,mode,frame,input,pose,y,handler,gamma,counter,previous,index,health,missiles,supers,pbs,shine,palette\n");
  for (int order = 0; order < 2; order++)
  for (int right = 0; right < 2; right++)
  for (int mode = 0; mode < 4; mode++) {
    memset(g_ram, 0, sizeof(g_ram));
    memset(dma_channel_registers, 0, sizeof(dma_channel_registers));
    samus_pose = samus_prev_pose = right ? 1 : 2;
    samus_pose_x_dir = samus_prev_pose_x_dir = right ? 8 : 4;
    samus_x_pos = 256; samus_y_pos = 400;
    samus_health = 49; samus_max_health = 99;
    samus_missiles = samus_super_missiles = samus_power_bombs = 10;
    samus_max_missiles = samus_max_super_missiles = samus_max_power_bombs = 10;
    button_config_shoot_x = 0x40; game_state = 8;
    grapple_beam_function = 0xc4f0;
    run(RefreshRadius);
    if (order == 0) grab(right);
    joypad1_lastkeys = 0x470;
    run(FlashEntry);
    if (samus_movement_handler != FlashRaising) Die("Flash admission failed");
    uint16 previous = 0x470;
    for (int frame = 0; frame < 120; frame++) {
      if (order == 1 && frame == 12) grab(right);
      uint16 input = input_at(frame, mode);
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      nmi_frame_counter_word = frame;
      if (samus_movement_handler == FlashRaising || samus_movement_handler == FlashMain || samus_movement_handler == FlashFinish)
        run(0x900000 | samus_movement_handler);
      if (frame_handler_gamma == GrabGamma) run(0x900000 | GrabGamma);
      run(AnimationPhase); run(Palette);
      if (frame == 96) {
        if (order == 0 && (mode == 1 || mode == 3) && samus_missiles != 0xffff)
          Die("One extra D-pad edge must underflow ten missiles");
        if (mode == 2 && (frame_handler_gamma == GrabGamma || substate != 60))
          Die("Alternating D-pad edges must reach the shared release counter");
        if (order == 1 && (samus_missiles != 10 || samus_super_missiles != 10 || samus_power_bombs != 10))
          Die("Grab during Flash must stop its ammo-drain movement handler");
      }
      printf("%d,%d,%d,%d,%04X,%02X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
        order,right,mode,frame,input,samus_pose,samus_y_pos,samus_movement_handler,frame_handler_gamma,
        substate,suit_pickup_light_beam_pos,which_item_to_pickup,samus_health,samus_missiles,samus_super_missiles,samus_power_bombs,samus_shine_timer,timer_for_shine_timer);
    }
  }
  return 0;
}
