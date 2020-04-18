using System;

namespace c8alpha
{
    static class Constants
    {
        public const int VRegCount = 16;
        public const int MemSizeBytes = 4096;
        public const int ScreenWidth = 64;
        public const int ScreenHeight = 32;
        public const int StackSize = 48;
    }
    public class Chip8
    {
        private byte timerDelay = 0;
        private byte timerSound = 0;

        private byte[] regV = new byte[Constants.VRegCount];
        private ushort regI = 0;
        private ushort regPC = 0x200;
        private byte[] mainMem = new byte[Constants.MemSizeBytes];
        private byte[] stack = new byte[Constants.StackSize];

        private bool[,] frameBuffer = new bool[Constants.ScreenWidth,Constants.ScreenHeight];
        private Random r = new Random();

        public void ClockCycle()
        {

        }

        private void Execute() //0NNN
        {

        }

        private void ClearScreen() //00E0
        {
            this.frameBuffer.Initialize();
        }

        private void ReturnFromSub() //00EE
        {

        }

        private void JumpToAddr(ushort addr) //1NNN
        {

        }
        private void ExecuteSub(ushort addr) //2NNN
        {

        }

        private void SkipIfVxEqual(byte x, byte value) //3XNN
        {

        }

        private void SkipIfVxNotEqual(byte x, byte value) //4XNN
        {

        }
        private void SkipIfVxEqualVy(byte x, byte y) //5XY0
        {

        }
        private void StoreInVx(byte x, byte value) //6XNN
        {

        }
        private void AddToVx(byte x, byte value) //7XNN
        {

        }
        private void StoreVxOrVy(byte x, byte y) //8XY1
        {

        }
        private void StoreVxAndVy(byte x, byte y) //8XY2
        {

        }
        private void StoreVxXorVy(byte x, byte y) //8XY3
        {

        }
        private void AddVyToVxWithCarry(byte x, byte y) //8XY4
        {

        }

        private void SubtrackVyFromVx(byte x, byte y) //8XY5
        {

        }

        private void RshVyToVx(byte x, byte y) //8XY6
        {

        }

        private void SubtrackVxFromVy(byte x, byte y) //8XY7
        {

        }
        private void LshVyToVx(byte x, byte y) //8XYE
        {

        }
        private void SkipIfVxNotEqualVy(byte x, byte y) //9XY0
        {

        }

        private void StoreAddrInI(ushort addr) //ANNN
        {

        }

        private void JumpToAddrPlusV0(ushort addr) //BNNN
        {

        }

        private void RandomVx(byte x, byte value) //CXNN
        {
            this.regV[x] = (byte)(r.Next(0, 256) & value);
        }
        private void DrawSprite(byte bytesOfSpriteData) //DXYN
        {

        }
        private void SkipIfKeyPressed(byte x) //EX9E
        {

        }
        private void SkipIfKeyNotPressed(byte x) //EXA1
        {

        }
        private void StoreDelayToVx(byte x) //FX07
        {
            this.regV[x] = this.timerDelay;
        }
        private void WaitForKey(byte x) //FX0A
        {

        }
        private void SetDelayTimer(byte x) //FX15
        {
            this.timerDelay = this.regV[x];
        }
        private void SetSoundTimer(byte x) //FX18
        {
            this.timerSound = this.regV[x];
        }
        private void AddVxToI(byte x) //FX1E
        {
            this.regI += this.regV[x];
        }
        private void SetIToSpriteData(byte x) //FX29
        {

        }
        private void StoreBinaryDecimal(byte x) //FX33
        {

        }
        private void StoreRegistersToRAM(byte x) //FX55
        {
            for(int i = 0; i<=x; i++)
            {
                this.mainMem[this.regI] = this.regV[i];
                this.regI++;
            }
        }
        private void LoadRegistersFromRam(byte x) //FX65
        {
            for (int i = 0; i <= x; i++)
            {
                this.regV[i] = this.mainMem[this.regI];
                this.regI++;
            }
        }
    }
}
