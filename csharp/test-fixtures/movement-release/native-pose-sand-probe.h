#include "native-bounded-cpu.h"

// #463: full pose expansion across surface/submerging sand, including native speed side effects.
int DiagnosticPoseSand(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"gap,parity,kind,damage,x,y,pose,yspeed,gravity\n");
  for(int gap=0;gap<13;gap++) for(int parity=0;parity<2;parity++) for(int kind=0;kind<2;kind++) for(int damage=0;damage<2;damage++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=16; room_size_in_blocks=512;
    interactive_enemy_indexes[0]=0xffff;
    samus_x_pos=136; samus_y_pos=150-gap; samus_y_subpos=0x3456;
    samus_x_radius=5; samus_y_radius=10;
    samus_prev_pose=0x2d; samus_pose=0x29;
    samus_pose_x_dir=samus_prev_pose_x_dir=8;
    samus_movement_type=samus_prev_movement_type2=6;
    samus_x_base_speed=1; samus_x_base_subspeed=0x4000;
    samus_y_dir=2; nmi_frame_counter_word=parity;
    area_index=4; samus_contact_damage_index=damage;
    samus_y_speed=5; samus_y_subspeed=0x4000; samus_y_accel=1; samus_y_subaccel=0x3000;
    level_data[10*16+8]=0x3000; BTS[10*16+8]=kind ? 0x83 : 0x80;
    ProbeRunBounded(0x91f404);
    fprintf(f,"%d,%d,%d,%d,%04X%04X,%04X%04X,%02X,%04X%04X,%04X%04X\n",gap,parity,kind,damage,
      samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,samus_pose,samus_y_speed,samus_y_subspeed,samus_y_accel,samus_y_subaccel);
  }
  fclose(f); return 0;
}
