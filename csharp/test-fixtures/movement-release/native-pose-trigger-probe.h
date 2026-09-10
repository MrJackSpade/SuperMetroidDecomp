#include "native-bounded-cpu.h"

// #463: positive door/item side effects from full original pose-expansion dispatch.
int DiagnosticPoseTrigger(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"item,above,gap,parity,x,y,pose,door,trigger,base\n");
  for(int item=0;item<2;item++) for(int above=0;above<2;above++)
  for(int gap=0;gap<13;gap++) for(int parity=0;parity<2;parity++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=16; room_size_in_blocks=512;
    interactive_enemy_indexes[0]=0xffff;
    samus_x_pos=136; samus_y_pos=above ? 185+gap : 150-gap; samus_y_subpos=0x3456;
    samus_x_radius=5; samus_y_radius=10;
    samus_prev_pose=0x2d; samus_pose=0x29;
    samus_pose_x_dir=samus_prev_pose_x_dir=8;
    samus_movement_type=samus_prev_movement_type2=6;
    samus_x_base_speed=1; samus_x_base_subspeed=0x4000;
    samus_y_dir=2; nmi_frame_counter_word=parity;
    door_list_pointer=0x927b;
    level_data[10*16+8]=item ? 0xb000 : 0x9000;
    if(item) {
      BTS[10*16+8]=0x45;
      plm_header_ptr[39]=0xeedb; plm_block_indices[39]=2*(10*16+8);
    }
    ProbeRunBounded(0x91f404);
    fprintf(f,"%d,%d,%d,%d,%04X%04X,%04X%04X,%02X,%04X,%04X,%04X%04X\n",item,above,gap,parity,
      samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,samus_pose,door_def_ptr,plm_timers[39],samus_x_base_speed,samus_x_base_subspeed);
  }
  fclose(f); return 0;
}
