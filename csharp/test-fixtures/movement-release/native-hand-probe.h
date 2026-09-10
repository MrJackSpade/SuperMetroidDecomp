#include "native-bounded-cpu.h"

int DiagnosticHandProbe(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"ship,probe,poseIndex,eligible,collision,parameter,block,event\n");
  const uint16 poses[]={0x1d,0x41,0x79,0x7a};
  for(int ship=0;ship<2;ship++) for(int probe=0;probe<2;probe++)
  for(int pose=0;pose<4;pose++) for(int eligible=0;eligible<2;eligible++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=128; room_height_in_blocks=80; room_size_in_blocks=128*80*2;
    area_index=ship ? 3 : 2; boss_bits_for_area[3]=eligible;
    collected_items=eligible ? 0x200 : 0;
    samus_x_pos=136; samus_y_pos=149; samus_x_radius=5; samus_y_radius=7;
    samus_pose=poses[pose]; samus_collision_direction=3;
    int block=10*128+8;
    level_data[block]=0xb123; BTS[block]=ship ? 0x80 : 0x83;
    enemy_data[0].parameter_2=ship ? 0 : 1;
    *(uint16 *)(g_ram+0x12)=7; *(uint16 *)(g_ram+0x14)=0;
    ProbeRunBounded(probe ? 0x9496ab : 0x949763);
    fprintf(f,"%d,%d,%d,%d,%d,%04X,%04X,%d\n",ship,probe,pose,eligible,
      samus_collision_flag,enemy_data[0].parameter_1,level_data[block],(events_that_happened[1]>>4)&1);
  }
  fclose(f);return 0;
}
