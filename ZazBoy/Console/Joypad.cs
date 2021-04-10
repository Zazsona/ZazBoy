using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console
{
    /// <summary>
    /// A data class representing the current status of the Game Boy's I/O.
    /// </summary>
    public class Joypad
    {
        public const ushort JoypadRegister = 0xFF00;

        public bool ActionButtonsSelected { get; set; }
        public bool DirectionalPadSelected { get; set; }

        private bool DPadUp { get; set; }
        private bool DPadDown { get; set; }
        private bool DPadLeft { get; set; }
        private bool DPadRight { get; set; }

        private bool BtnA { get; set; }
        private bool BtnB { get; set; }
        private bool BtnSelect { get; set; }
        private bool BtnStart { get; set; }

        private InterruptHandler interruptHandler;

        public Joypad(InterruptHandler interruptHandler)
        {
            this.interruptHandler = interruptHandler;
        }

        /// <summary>
        /// Sets the state of the selected button.
        /// </summary>
        /// <param name="buttonType">The button to set</param>
        /// <param name="state">The state (true == pressed)</param>
        public void SetButton(ButtonType buttonType, bool state)
        {
            bool oldButtonState = GetButton(buttonType);
            switch (buttonType)
            {
                case ButtonType.DPadDown:
                    DPadDown = state;
                    break;
                case ButtonType.DPadUp:
                    DPadUp = state;
                    break;
                case ButtonType.DPadLeft:
                    DPadLeft = state;
                    break;
                case ButtonType.DPadRight:
                    DPadRight = state;
                    break;

                case ButtonType.BtnStart:
                    BtnStart = state;
                    break;
                case ButtonType.BtnSelect:
                    BtnSelect = state;
                    break;
                case ButtonType.BtnB:
                    BtnB = state;
                    break;
                case ButtonType.BtnA:
                    BtnA = state;
                    break;
            }
            if (!oldButtonState && state)
                RequestInterrupt(buttonType);
        }

        /// <summary>
        /// Gets the state of the selected button (ignoring whether the button mode is selected)
        /// </summary>
        /// <param name="buttonType">The button to get</param>
        /// <returns>The state of the button (true == pressed)</returns>
        public bool GetButton(ButtonType buttonType)
        {
            switch (buttonType)
            {
                case ButtonType.DPadDown:
                    return DPadDown;
                case ButtonType.DPadUp:
                    return DPadUp;
                case ButtonType.DPadLeft:
                    return DPadLeft;
                case ButtonType.DPadRight:
                    return DPadRight;

                case ButtonType.BtnStart:
                    return BtnStart;
                case ButtonType.BtnSelect:
                    return BtnSelect;
                case ButtonType.BtnB:
                    return BtnB;
                case ButtonType.BtnA:
                    return BtnA;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the controls byte corresponding to the one selected in FF00
        /// </summary>
        /// <returns>The respective control byte, or FF if the selection is invalid.</returns>
        public byte GetControlsByte()
        {
            if (ActionButtonsSelected && !DirectionalPadSelected)
                return GetActionButtonByte();
            else if (DirectionalPadSelected && !ActionButtonsSelected)
                return GetDirectionalPadByte();
            else
                return 0xFF;
        }

        /// <summary>
        /// Gets the binary representation of the flags that is returned when accessing memory address FF00 for the action buttons
        /// </summary>
        /// <returns>The byte representing button state, where 0 == activated. The mode bit (5) is always set.</returns>
        public byte GetActionButtonByte()
        {
            byte modeBit = 0x10; //Set DPad mode bit to high to signify it's an action button check (0 == enabled, 1 == disabled)
            byte startBit = (byte)((BtnStart) ? 0x00 : 0x08); //1000
            byte selectBit = (byte)((BtnSelect) ? 0x00 : 0x04); //0100
            byte bBit = (byte)((BtnB) ? 0x00 : 0x02); //0010                //For some reason, bits are unset when the button is activated, and set when not.
            byte aBit = (byte)((BtnA) ? 0x00 : 0x01); //0010

            byte actionByte = (byte)(modeBit | startBit | selectBit | bBit | aBit);
            return actionByte;
        }

        /// <summary>
        /// Gets the binary representation of the flags that is returned when accessing memory address FF00 for the DPad
        /// </summary>
        /// <returns>The byte representing button state, where 0 == activated. The mode bit (4) is always set.</returns>
        public byte GetDirectionalPadByte()
        {
            byte modeBit = 0x20; //Set action button mode bit to high to signify it's a DPad check (0 == enabled, 1 == disabled)
            byte startBit = (byte)((DPadDown) ? 0x00 : 0x08); //1000
            byte selectBit = (byte)((DPadUp) ? 0x00 : 0x04); //0100
            byte bBit = (byte)((DPadLeft) ? 0x00 : 0x02); //0010                //For some reason, bits are unset when the button is activated, and set when not.
            byte aBit = (byte)((DPadRight) ? 0x00 : 0x01); //0010

            byte directionByte = (byte)(modeBit | startBit | selectBit | bBit | aBit);
            return directionByte;
        }

        /// <summary>
        /// Requests an interrupt. Request is denied if the button is not from the current mode.
        /// </summary>
        /// <param name="buttonType"></param>
        private void RequestInterrupt(ButtonType buttonType)
        {
            //TODO: Might need to specify the other mode must be disabled?
            bool interruptAllowed = (ActionButtonsSelected && (buttonType == ButtonType.BtnStart || buttonType == ButtonType.BtnSelect || buttonType == ButtonType.BtnB || buttonType == ButtonType.BtnA))
                || (DirectionalPadSelected && (buttonType == ButtonType.DPadDown || buttonType == ButtonType.DPadUp || buttonType == ButtonType.DPadLeft || buttonType == ButtonType.DPadRight));
            
            if (interruptAllowed)
                interruptHandler.SetInterruptRequested(InterruptHandler.InterruptType.Joypad, true);
        }

        public enum ButtonType
        {
            DPadDown,
            DPadUp,
            DPadLeft,
            DPadRight,
            BtnStart,
            BtnSelect,
            BtnB,
            BtnA
        }
    }
}
