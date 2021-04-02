using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZazBoy.Console
{
    /// <summary>
    /// The Game Boy timer.
    /// </summary>
    public class Timer
    {
        public const ushort DividerRegister = 0xFF04;
        public const ushort TimerCounter = 0xFF05;
        public const ushort TimerModulo = 0xFF06;
        public const ushort TimerControl = 0xFF07;
        
        public bool timerEnable
        {
            get
            {
                byte timerControl = GameBoy.Instance().MemoryMap.Read(TimerControl);
                byte flagBit = (1 << 2);
                return ((timerControl & flagBit) != 0);
            }
            set
            {
                bool isTimerEnabled = timerEnable;
                if (isTimerEnabled != value)
                {
                    byte timerControl = GameBoy.Instance().MemoryMap.Read(TimerControl);
                    byte flagBit = (1 << 2);
                    timerControl = (byte)(timerControl ^ flagBit);
                    GameBoy.Instance().MemoryMap.WriteDirect(TimerControl, timerControl);
                }
            }
        }
        public ushort divider
        { 
            get
            {
                return _divider;
            }
            private set
            {
                bool dividerTimerBitSet = IsDividerTimerFrequencyBitSet();
                _divider = value;
                if (dividerTimerBitSet && !IsDividerTimerFrequencyBitSet()) //Timer increments on a falling edge, even when the divider is being reset.
                    IncrementTimerCounter();
            }
        }
        public bool isOverflowCycle { get => timerOverflowDelayClocks > 0; }
        public bool isTIMAModifiedDuringOverflow { get; set; } //If the timer is written to, the interrupt is not triggered, and it is not reset to timer modulo

        private int timerOverflowDelayClocks; //There is a 4 clock delay when the timer overflows
        private ushort _divider;

        public Timer()
        {
            _divider = 0;
            timerOverflowDelayClocks = -1;
        }

        /// <summary>
        /// Progresses the divider register by 1 clock. Updates Timer as necassary.
        /// </summary>
        public void Tick()
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            timerOverflowDelayClocks--;
            if (timerOverflowDelayClocks == 0 && !isTIMAModifiedDuringOverflow)
            {
                byte timerModulo = memMap.Read(TimerModulo);
                memMap.WriteDirect(TimerCounter, timerModulo);
                GameBoy.Instance().InterruptHandler.SetInterruptRequested(InterruptHandler.InterruptType.Timer, true);
            }
            divider++;
            memMap.WriteDirect(DividerRegister, (byte)(divider / 0x100));
        }

        /// <summary>
        /// Resets the divider to 0. Updates Timer as necassary.
        /// </summary>
        public void ResetDivider()
        {
            divider = 0;
        }

        /// <summary>
        /// Increments the timer by 1. If an overflow occurs, the interrupt is set and timer's value set to Timer Modulo.
        /// </summary>
        public void IncrementTimerCounter()
        {
            if (timerEnable && timerOverflowDelayClocks < 0)
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte timerValue = memMap.Read(TimerCounter);
                timerValue++;
                if (timerValue == 0)
                {
                    timerOverflowDelayClocks = 4;
                    isTIMAModifiedDuringOverflow = false;
                }
                memMap.WriteDirect(TimerCounter, timerValue);
            }
        }

        /// <summary>
        /// Direct method for setting the timer frequency in 0xFF07. <br></br>
        /// It basically translates the number of clocks into the binary value, and writes that to memory. Thus, it will still replicate the timer bugs from changing frequency.
        /// </summary>
        /// <param name="clocks">The clock frequency for the timer (16, 64, 256, 1024)</param>
        /// <exception cref="ArgumentOutOfRangeException">Provided clocks is not within the range.</exception>
        public void SetTimerFrequency(int clocks)
        {
            byte clocksByte;
            switch (clocks)
            {
                case 16:
                    clocksByte = 1;
                    break;
                case 64:
                    clocksByte = 2;
                    break;
                case 256:
                    clocksByte = 3;
                    break;
                case 1024:
                    clocksByte = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Invalid timer frequency: " + clocks);
            }
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            byte timerControl = memMap.Read(TimerControl);
            byte cleanControl = (byte)(timerControl & (1 << 2)); //Sets all bits except #2 to 0.
            byte newControl = (byte)(cleanControl | clocksByte); //Add frequency bits.
            memMap.Write(TimerControl, newControl);
        }

        /// <summary>
        /// Direct method for getting the current frequency of the timer, in clocks.
        /// </summary>
        /// <returns>The clock frequency (16, 64, 256, 1024)</returns>
        /// <exception cref="InvalidOperationException">Timer has invalid frequency.</exception>
        public int GetTimerClocksFrequency()
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            byte timerControl = memMap.Read(TimerControl);
            byte frequencyByte = (byte)(timerControl & 0x03); //0000 0011

            switch (frequencyByte)
            {
                case 1:
                    return 16;
                case 2:
                    return 64;
                case 3:
                    return 256;
                case 0:
                    return 1024;
                default:
                    throw new InvalidOperationException("Timer has invalid frequency selector: " + frequencyByte);
            }
        }

        /// <summary>
        /// Method name is a bit crap, admittedly.<br></br>
        /// This method gets the current state of the bit the timer is following to detect a falling edge (change to 0), which indicates it should increment.<br></br>
        /// An example use case is getting the state of the bit, altering the divider, and then comparing to the new state of the bit. If the old state was 1 and the new state is 0, the timer should increment.
        /// </summary>
        /// <returns>True/false on the bit the timer is currently monitoring for a falling edge</returns>
        public bool IsDividerTimerFrequencyBitSet()
        {
            int clockFrequency = GetTimerClocksFrequency();
            ushort dividerBitPosition = 0;
            switch (clockFrequency)
            {
                case 16:
                    dividerBitPosition = 3;
                    break;
                case 64:
                    dividerBitPosition = 5;
                    break;
                case 256:
                    dividerBitPosition = 7;
                    break;
                case 1024:
                    dividerBitPosition = 9;
                    break;
            }
            ushort incrementBit = (ushort)(1 << dividerBitPosition);
            bool tickBitSet = ((divider & incrementBit) != 0);
            return tickBitSet;
        }
    }
}
