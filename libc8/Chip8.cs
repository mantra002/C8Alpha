using System;

namespace c8alpha
{
    static class Constants
    {
        public const int VRegCount = 16;
        public const int RamSizeBytes = 4096;
        public const int ScreenWidth = 64;
        public const int ScreenHeight = 32;
        public const int StackSize = 12;
        public const ushort SpriteFontLocation = 0x010;
        public static byte[] FontData = {
            0xF0, 0x90, 0x90, 0x90, 0xF0, //0
            0x20, 0x60, 0x20, 0x20, 0x70, //1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, //2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, //3
            0x90, 0x90, 0xF0, 0x10, 0x10, //4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, //5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, //6
            0xF0, 0x10, 0x20, 0x40, 0x40, //7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, //8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, //9
            0xF0, 0x90, 0xF0, 0x90, 0x90, //A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, //B
            0xF0, 0x80, 0x80, 0x80, 0xF0, //C
            0xE0, 0x90, 0x90, 0x90, 0xE0, //D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, //E
            0xF0, 0x80, 0xF0, 0x80, 0x80  //F;
        };
    }

    public class Chip8
    {
        private byte timerDelay;
        private byte timerSound;

        private byte[] V;
        private ushort I;
        private ushort PC;
        private byte[] RAM;
        private ushort[] stack;
        private byte stackPointer = 0;

        public bool[,] FrameBuffer;
        private Random r;

        public Chip8()
        {
            r = new Random();
            FrameBuffer = new bool[Constants.ScreenWidth, Constants.ScreenHeight];
            RAM = new byte[Constants.RamSizeBytes];
            stack = new ushort[Constants.StackSize];
            V = new byte[Constants.VRegCount];
            I = 0;
            timerDelay = 0;
            timerSound = 0;

            InitSpriteFont(Constants.SpriteFontLocation);

        }
        public void LoadRAM(byte[] ramData, ushort addr=0x200)
        {
            if(ramData.Length + addr > Constants.RamSizeBytes)
            {
                Console.WriteLine("RAM data exceeds memeory bounds, cancelling load.");
                return;
            }
            PC = addr;
            ramData.CopyTo(RAM, addr);
        }
        private void InitSpriteFont(ushort addr)
        {
            Constants.FontData.CopyTo(RAM, addr);
        }

        private void Push(ushort addr)
        {
            this.stack[stackPointer++] = addr;
        }

        private ushort Pop()
        {
            return this.stack[--stackPointer];
        }

        public void ClockCycle()
        {
            ushort addr;
            byte x, y, n, value;
            ushort opcode = (ushort)(RAM[PC++] << 8 | RAM[PC++]);
            byte nibble = (byte)(opcode >> 12);
            addr = (ushort)(opcode & 0x0FFF);
            value = (byte)(opcode & 0x00FF);
            n = (byte)(opcode & 0x000F);
            x = (byte)((opcode & 0x0F00) >> 8);
            y = (byte)((opcode & 0x00F0) >> 4);

            switch (nibble)
            {
                case 0x0:
                    if (n == 0)
                    {
                        ClearScreen();
                    }
                    else
                    {
                        ReturnFromSub();
                    }
                    break;
                case 0x1:
                    JumpToAddr(addr);
                    break;
                case 0x2:
                    ExecuteSub(addr);
                    break;
                case 0x3:
                    SkipIfVxEqual(x, value);
                    break;
                case 0x4:
                    SkipIfVxNotEqual(x, value);
                    break;
                case 0x5:
                    SkipIfVxEqualVy(x, y);
                    break;
                case 0x6:
                    StoreInVx(x, value);
                    break;
                case 0x7:
                    SkipIfVxNotEqualVy(x, y);
                    break;
                case 0x8:
                    switch (n) 
                    {
                        case 0:
                            StoreVyInVx(x, y);
                            break;
                        case 1:
                            StoreVxOrVy(x, y);
                            break;
                        case 2:
                            StoreVxAndVy(x, y);
                            break;
                        case 3:
                            StoreVxXorVy(x, y);
                            break;
                        case 4:
                            AddVyToVxWithCarry(x, y);
                            break;
                        case 5:
                            SubtractVyFromVx(x, y);
                            break;
                        case 6:
                            RshVyToVx(x, y);
                            break;
                        case 7:
                            SubtrackVxFromVy(x, y);
                            break;
                        case 0xE:
                            LshVyToVx(x, y);
                            break;
                        default:
                            throw new Exception("Unknown Opcode!");
                    }
                    
                    break;
                case 0x9:
                    SkipIfVxNotEqualVy(x, y);
                    break;
                case 0xA:
                    StoreAddrInI(addr);
                    break;
                case 0xB:
                    JumpToAddrPlusV0(addr);
                    break;
                case 0xC:
                    RandomVx(x, value);
                    break;
                case 0xD:
                    DrawSprite(x, y, n);
                    break;
                case 0xE:
                    if (n == 0xE) SkipIfKeyPressed(x);
                    else if (n == 1) SkipIfKeyNotPressed(x);
                    break;
                case 0xF:
                    switch(value)
                    {
                        case 0x07:
                            StoreDelayToVx(x);
                            break;
                        case 0x0A:
                            WaitForKey(x);
                            break;
                        case 0x15:
                            SetDelayTimer(x);
                            break;
                        case 0x18:
                            SetSoundTimer(x);
                            break;
                        case 0x1E:
                            AddVxToI(x);
                            break;
                        case 0x29:
                            SetIToSpriteData(x);
                            break;
                        case 0x33:
                            StoreBinaryDecimal(x);
                            break;
                        case 0x55:
                            StoreRegistersToRAM(x);
                            break;
                        case 0x65:
                            LoadRegistersFromRam(x);
                            break;
                        default:
                            throw new Exception("Unknown Opcode!");
                    }
                    break;
                default:
                    throw new Exception("Unknown Opcode!");
            }
        }

        private void Execute() //0NNN
        {

        }

        private void ClearScreen() //00E0
        {
            this.FrameBuffer.Initialize();
        }

        private void ReturnFromSub() //00EE
        {
            this.PC = Pop();
        }

        private void JumpToAddr(ushort addr) //1NNN
        {
            this.PC = addr;
        }
        private void ExecuteSub(ushort addr) //2NNN
        {
            Push(this.PC);
            this.PC = addr;
        }

        private void SkipIfVxEqual(byte x, byte value) //3XNN
        {
            if (V[x] == value) SkipOpcode();
        }

        private void SkipIfVxNotEqual(byte x, byte value) //4XNN
        {
            if (V[x] != value) SkipOpcode();
        }
        private void SkipIfVxEqualVy(byte x, byte y) //5XY0
        {
            if (V[x] == V[y]) SkipOpcode();
        }
        private void StoreInVx(byte x, byte value) //6XNN
        {
            V[x] = value;
        }
        private void AddToVx(byte x, byte value) //7XNN
        {
            V[x] += value;
        }
        private void StoreVyInVx(byte x, byte y) //8XY0
        {
            V[x] = V[y];
        }
        private void StoreVxOrVy(byte x, byte y) //8XY1
        {
            V[x] = (byte)(V[x] | V[y]);
        }
        private void StoreVxAndVy(byte x, byte y) //8XY2
        {
            V[x] = (byte)(V[x] & V[y]);
        }
        private void StoreVxXorVy(byte x, byte y) //8XY3
        {
            V[x] = (byte)(V[x] ^ V[y]);
        }
        private void AddVyToVxWithCarry(byte x, byte y) //8XY4
        {
            int result = V[x] + V[y];
            if (result > 256) V[0xF] = 1;
            V[x] = (byte)result;
        }

        private void SubtractVyFromVx(byte x, byte y) //8XY5
        {
            if (V[x] > V[y]) V[0xF] = 1;
            else V[0xF] = 0;
            V[x] -= V[y];
        }

        private void RshVyToVx(byte x, byte y) //8XY6
        {
            if (V[y] << 7 != 0) V[0xF] = 1;
            else V[0xF] = 0;

            V[x] = (byte)(V[y] >> 1);
        }

        private void SubtrackVxFromVy(byte x, byte y) //8XY7
        {
            if (V[x] < V[y]) V[0xF] = 1;
            else V[0xF] = 0;
            V[x] = (byte)(V[y] - V[x]);
        }
        private void LshVyToVx(byte x, byte y) //8XYE
        {
            if (V[y] >> 7 != 0) V[0xF] = 1;
            else V[0xF] = 0;

            V[x] = (byte)(V[y] << 1);
        }
        private void SkipIfVxNotEqualVy(byte x, byte y) //9XY0
        {
            if (V[x] != V[y]) SkipOpcode();
        }

        private void StoreAddrInI(ushort addr) //ANNN
        {
            I = addr;
        }

        private void JumpToAddrPlusV0(ushort addr) //BNNN
        {
            PC = (byte)(addr + V[0x0]);
        }

        private void RandomVx(byte x, byte value) //CXNN
        {
            V[x] = (byte)(r.Next(0, 256) & value);
        }
        /// <summary>
        /// Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. Each row of 8 pixels is read as bit-coded 
        /// starting from memory location I; I value doesn’t change after the execution of this instruction. As described above, VF is set to 1 if 
        /// any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that doesn’t happen
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="bytesOfSpriteData"></param>
        private void DrawSprite(byte x, byte y, byte bytesOfSpriteData) //DXYN
        {
            bool[] pixelData;
            bool switchedOffPixel = false;
            int clippedX, clippedY;
            for (int i = 0; i < bytesOfSpriteData; i++)
            {
                pixelData = ByteToBools(RAM[I + i]);
                for(int j = 0; j<8; j++)
                {
                    if (switchedOffPixel == false && FrameBuffer[x, y + i] == true && pixelData[j] == true)
                    {
                        switchedOffPixel = true;
                    }
                    FrameBuffer[x%Constants.ScreenWidth, (y + i)%Constants.ScreenHeight] ^= pixelData[j];
                }
            }
            if (switchedOffPixel) V[0xF] = 1;
            else V[0xF] = 0;
        }

        private bool[] ByteToBools(byte b)
        {
            bool[] result = new bool[8];
            for(int i = 0; i<8; i++)
            {
                result[i] = (b & (1 << i)) != 0;
            }
            return result;
        }
        private void SkipIfKeyPressed(byte x) //EX9E
        {

        }
        private void SkipIfKeyNotPressed(byte x) //EXA1
        {

        }
        private void StoreDelayToVx(byte x) //FX07
        {
            this.V[x] = this.timerDelay;
        }
        private void WaitForKey(byte x) //FX0A
        {

        }
        private void SetDelayTimer(byte x) //FX15
        {
            this.timerDelay = this.V[x];
        }
        private void SetSoundTimer(byte x) //FX18
        {
            this.timerSound = this.V[x];
        }
        private void AddVxToI(byte x) //FX1E
        {
            this.I += this.V[x];
        }
        private void SetIToSpriteData(byte x) //FX29
        {
            this.I = (ushort)(Constants.SpriteFontLocation + (x * 5));
        }
        private void StoreBinaryDecimal(byte x) //FX33
        {
            RAM[I] = (byte)((V[x] / 100) % 10);
            RAM[I+1] = (byte)((V[x] / 10) % 10);
            RAM[I+2] = (byte)((V[x]) % 10);
        }
        private void StoreRegistersToRAM(byte x) //FX55
        {
            for(int i = 0; i<=x; i++)
            {
                this.RAM[this.I] = this.V[i];
                this.I++;
            }
        }
        private void LoadRegistersFromRam(byte x) //FX65
        {
            for (int i = 0; i <= x; i++)
            {
                this.V[i] = this.RAM[this.I];
                this.I++;
            }
        }

        private void SkipOpcode()
        {
            this.PC += 2;
        }
    }
}
