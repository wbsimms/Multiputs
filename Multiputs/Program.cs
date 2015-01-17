#region copyright
// Copyright (c) 2015 Wm. Barrett Simms wbsimms.com
//
// Permission is hereby granted, free of charge, to any person 
// obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, 
// distribute, sublicense, and/or sell copies of the Software, 
// and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Collections;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.SocketInterfaces;
using GHI.Pins;
using Microsoft.SPOT.Hardware;
using Button = Gadgeteer.Modules.GHIElectronics.Button;
using Gadgeteer.Modules.GHIElectronics;

namespace Multiputs
{
    public partial class Program
    {
        private InterruptInput interruptInput;
        private DigitalOutput latch, clock, data;

        private int allOn = 0xFF;
        private Font font;
        private DisplayHelper displayHelper;

        void ProgramStarted()
        {
            Debug.Print("Program Started");
            interruptInput = breadBoardX1.CreateInterruptInput(GT.Socket.Pin.Three, GlitchFilterMode.Off, ResistorMode.Disabled, InterruptMode.RisingEdge);
            latch = breadBoardX1.CreateDigitalOutput(GT.Socket.Pin.Six, false);
            clock = breadBoardX1.CreateDigitalOutput(GT.Socket.Pin.Seven, false);
            data = breadBoardX1.CreateDigitalOutput(GT.Socket.Pin.Five, false);
            interruptInput.Interrupt += interruptInput_Interrupt;
            button.ButtonPressed += button_ButtonPressed;
            font = Resources.GetFont(Resources.FontResources.NinaB);
            displayHelper = new DisplayHelper(displayT43,font);
            displayHelper.StartDisplay();
        }

        private int onPins = 0;
        ArrayList onPinsArray = new ArrayList();
        void interruptInput_Interrupt(InterruptInput sender, bool value)
        {
            int check = 1;
            for (int i = 0; i <= 7; i++)
            {
                WriteBits(check);
                if (interruptInput.Read())
                {
                    onPins = onPins + 1;
                    onPinsArray.Add(i);
                }
                onPins = onPins << 1;
                check = check << 1;
            }
//            displayHelper.Update(onPins);
            displayHelper.Update(onPinsArray);
            onPins = 0;
            onPinsArray.Clear();
            WriteBits(allOn);
        }

        private bool isEnabled;
        void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            if (!isEnabled)
            {
                isEnabled = !isEnabled;
                WriteBits(allOn);
            }
            else
            {
                isEnabled = !isEnabled;
                WriteBits(0x0);
            }
        }

        private void WriteBits(int bits)
        {
            latch.Write(false);
            int copy = bits;
            for (int i = 0; i <= 7; i++)
            {
                int b = copy & 1;
                if (b == 1)
                {
                    data.Write(true);
                    Clock();
                }
                else
                {
                    data.Write(false);
                    Clock();
                }
                copy = copy >> 1;
            }
            Latch();
        }

        void Latch()
        {
            latch.Write(true);
            latch.Write(false);
        }
        void Clock()
        {
            clock.Write(true);
            clock.Write(false);
        }
    }

    public class DisplayHelper
    {
        private DisplayT43 display;
        Font font;
        public DisplayHelper(DisplayT43 display, Font font)
        {
            this.display = display;
            this.font = font;
        }

        public void StartDisplay()
        {
            int rowOne = 30;
            //display.SimpleGraphics.BackgroundColor = Color.White;
            display.SimpleGraphics.DisplayText("One", font, Color.White, 40, rowOne);
            display.SimpleGraphics.DisplayText("Two", font, Color.White, 90, rowOne);
            display.SimpleGraphics.DisplayText("Three", font, Color.White, 140, rowOne);
            display.SimpleGraphics.DisplayText("Four", font, Color.White, 190, rowOne);
            display.SimpleGraphics.DisplayText("Five", font, Color.White, 240, rowOne);
            display.SimpleGraphics.DisplayText("Six", font, Color.White, 290, rowOne);
            display.SimpleGraphics.DisplayText("Seven", font, Color.White, 340, rowOne);
            display.SimpleGraphics.DisplayText("Eight", font, Color.White, 390, rowOne);

            Update(0x00, true);

        }

        private int lastBit = 0;
        public void Update(int bits, bool force = false)
        {
            if (bits == lastBit && !force) return;
            Debug.Print("Bits = " + bits);
            int rowTwo = 60;
            int radius = 10;
            int thickness = 1;
            int offset = 50;
            int copy = bits;
            for (int i = 0; i <= 7; i++)
            {
                Debug.Print("Copy = "+copy);
                Color fill = Color.Black;
                int bit = copy & 1;
                if (bit == 1) fill = GT.Color.Yellow;
                display.SimpleGraphics.DisplayEllipse(GT.Color.White, thickness, fill, offset, rowTwo, radius, radius);
                offset += 50;
                copy = copy >> 1;
            }
            lastBit = bits;
            Debug.Print("===============");
        }

        public void Update(ArrayList onPinsArray)
        {
            int rowTwo = 60;
            int radius = 10;
            int thickness = 1;
            int offset = 400;
            for (int i = 0; i <= 7; i++)
            {
                Color fill = Color.Black;
                if (onPinsArray.Contains(i)) fill = GT.Color.Yellow;
                display.SimpleGraphics.DisplayEllipse(GT.Color.White, thickness, fill, offset, rowTwo, radius, radius);
                offset -= 50;
            }
        }
    }
}
