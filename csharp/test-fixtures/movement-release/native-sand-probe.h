#include "native-bounded-cpu.h"

int DiagnosticSandProbe(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"probe,up,ydir,damage,collision,amount,sand\n");
  for(int probe=0;probe<2;probe++) for(int up=0;up<2;up++)
  for(int ydir=0;ydir<4;ydir++) for(int damage=0;damage<2;damage++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=16; room_size_in_blocks=512;
    area_index=4; samus_x_pos=136; samus_y_pos=up ? 185 : 149;
    samus_x_radius=5; samus_y_radius=12; samus_y_dir=ydir;
    samus_contact_damage_index=damage; samus_collision_direction=up ? 2 : 3;
    level_data[10*16+8]=0x3000; BTS[10*16+8]=0x80;
    *(uint16 *)(g_ram+0x12)=up ? -7 : 7; *(uint16 *)(g_ram+0x14)=0;
    ProbeRunBounded(probe ? 0x9496ab : 0x949763);
    int32 amount=((uint32)*(uint16 *)(g_ram+0x12)<<16)|*(uint16 *)(g_ram+0x14);
    fprintf(f,"%d,%d,%d,%d,%d,%08X,%d\n",probe,up,ydir,damage,samus_collision_flag,
      amount<0 ? -amount : amount,flag_samus_in_quicksand);
  }
  fclose(f);return 0;
}
