using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ZazBoy.Console.InterruptHandler;

namespace ZazBoy.Console.Operations
{
    public class DMATransferOperation : Operation
    {
        private ushort sourceStartAddress;
        private int index;

        public DMATransferOperation(ushort sourceStartAddress) : base(640)
        {
            this.sourceStartAddress = sourceStartAddress;
            index = 0;
        }

        public override void Tick()
        {
            if (executedClocks % 4 == 0)
                TransferByte();

            base.Tick();
        }

        private void TransferByte()
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            ushort sourceAddress = (ushort)(sourceStartAddress + index);
            ushort targetAddress = (ushort)(MemoryMap.OAM_ADDRESS + index);
            if (sourceAddress >= MemoryMap.PROHIBITED_ADDRESS)
                sourceAddress = (ushort)(sourceAddress & ~0x2000); //If the user tries to read from prohibited or beyond, they get converted into OAM compatible locations. Exxx -> Cxxx, Fxxx -> Dxxx.
            byte value = memMap.ReadDirect(sourceAddress);
            memMap.WriteDirect(targetAddress, value);
            System.Console.WriteLine("DMAT: " + sourceAddress + " ("+memMap.ReadDirect(sourceAddress)+") --> " + targetAddress+" ("+memMap.ReadDirect(targetAddress)+")");
            index++;
        }

        protected override void Execute()
        {
            //Nothin'. Operation happens in TransferByte.
        }
    }
}
