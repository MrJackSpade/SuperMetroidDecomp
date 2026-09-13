// #617: original-CPU health/stun decisions, including RNG consumption and link writes.
#include "native-bounded-cpu.h"
int DiagnosticGoldenDistanceDecisions(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output,"wx"); if (!f) return 4;
  const uint16 distances[]={3,4,31,32,39,40,95,96};
  const uint16 poses[]={1,0x1d,0x79,0x7c};
  const uint16 seeds[]={0,0x31,0x100,0x1234};
  const uint32 routines[]={0xaad3ea,0xaad445};
  fprintf(f,"routine,samusX,facing,pose,seed,cursor,random,link,counter\n");
  for(int r=0;r<2;r++) for(int d=0;d<8;d++) for(int side=0;side<2;side++)
  for(int facing=0;facing<2;facing++) for(int p=0;p<4;p++) for(int s=0;s<4;s++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    Enemy_Torizo *enemy=Get_Torizo(0);
    enemy->base.x_pos=256; enemy->toriz_parameter_1=facing?0x8000:0;
    enemy->toriz_var_00=0x1234; enemy->toriz_var_09=9;
    samus_x_pos=256+(side?distances[d]:-distances[d]); samus_pose=poses[p];
    random_number=seeds[s];
    ProbeRunBoundedRegisters(routines[r],0,0,0xd000);
    fprintf(f,"%04X,%u,%u,%u,%u,%u,%u,%u,%u\n",routines[r]&0xffff,
      samus_x_pos,enemy->toriz_parameter_1,poses[p],seeds[s],g_snes->cpu->y,
      random_number,enemy->toriz_var_00,enemy->toriz_var_09);
  }
  fclose(f); return 0;
}
int DiagnosticGoldenDecisions(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  const uint16 healths[] = {1927,1928,1929,10799,10800,10801};
  const uint16 seeds[] = {0,1,2,3,0x31,0x61,0xff,0x100,0x1234,0xffff};
  const uint32 routines[] = {0xaad474,0xaad49b};
  fprintf(f,"routine,health,flags,seed,cursor,random,link,counter\n");
  for (int r=0;r<2;r++) for(int h=0;h<6;h++)
  for(int flag=0;flag<2;flag++) for(int s=0;s<10;s++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    Enemy_Torizo *enemy = Get_Torizo(0);
    enemy->base.health=healths[h]; enemy->toriz_parameter_2=flag ? 0x2000 : 0;
    enemy->toriz_var_00=0x1234; enemy->toriz_var_09=9; random_number=seeds[s];
    ProbeRunBoundedRegisters(routines[r],0,0,0xd000);
    fprintf(f,"%04X,%u,%u,%u,%u,%u,%u,%u\n",routines[r]&0xffff,healths[h],
      flag?0x2000:0,seeds[s],g_snes->cpu->y,random_number,enemy->toriz_var_00,enemy->toriz_var_09);
  }
  fclose(f); return 0;
}
