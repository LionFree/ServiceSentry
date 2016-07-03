// -----------------------------------------------------------------------
//  <copyright file="PrintWrapper.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Drawing;
using System.Drawing.Printing;

#endregion

namespace ServiceSentry.UnitTests.Common.Printing
{
    public abstract class PrintWrapper : PrintDocument
    {
        public abstract string TextToPrint { get; set; }
        public abstract Font PrinterFont { get; set; }

        public static PrintWrapper GetInstance()
        {
            return new Implementation(string.Empty);
        }

        public static PrintWrapper GetInstance(string textToPrint)
        {
            return new Implementation(textToPrint);
        }


        private sealed class Implementation : PrintWrapper
        {
            private static int _curChar;

            public Implementation(string textToPrint)
            {
                TextToPrint = textToPrint;
            }

            public override string TextToPrint { get; set; }
            public override Font PrinterFont { get; set; }

            /// <summary>
            ///     Override the default OnBeginPrint method of the PrintDocument Object
            /// </summary>
            /// <param name="e"></param>
            /// <remarks></remarks>
            protected override void OnBeginPrint(PrintEventArgs e)
            {
                // Run base code
                base.OnBeginPrint(e);

                //Check to see if the user provided a font
                //if they didn't then we default to Times New Roman
                if (PrinterFont == null)
                {
                    //Create the font we need
                    PrinterFont = new Font("Times New Roman", 10);
                }
            }

            /// <summary>
            ///     Override the default OnPrintPage method of the PrintDocument
            /// </summary>
            /// <param name="e"></param>
            /// <remarks>This provides the print logic for our document</remarks>
            protected override void OnPrintPage(PrintPageEventArgs e)
            {
                // Run base code
                base.OnPrintPage(e);

                //Declare local variables needed

                int printHeight;
                int printWidth;
                int leftMargin;
                int rightMargin;
                Int32 lines;
                Int32 chars;

                //Set print area size and margins
                {
                    printHeight = DefaultPageSettings.PaperSize.Height - DefaultPageSettings.Margins.Top -
                                  DefaultPageSettings.Margins.Bottom;
                    printWidth = DefaultPageSettings.PaperSize.Width - DefaultPageSettings.Margins.Left -
                                 DefaultPageSettings.Margins.Right;
                    leftMargin = DefaultPageSettings.Margins.Left; //X
                    rightMargin = DefaultPageSettings.Margins.Top; //Y
                }

                //Check if the user selected to print in Landscape mode
                //if they did then we need to swap height/width parameters
                if (DefaultPageSettings.Landscape)
                {
                    var tmp = printHeight;
                    printHeight = printWidth;
                    printWidth = tmp;
                }

                //Now we need to determine the total number of lines
                //we're going to be printing
                //var numLines = (int) printHeight/PrinterFont.Height;

                //Create a rectangle printing are for our document
                var printArea = new RectangleF(leftMargin, rightMargin, printWidth, printHeight);

                //Use the StringFormat class for the text layout of our document
                var format = new StringFormat(StringFormatFlags.LineLimit);

                //Fit as many characters as we can into the print area      

                e.Graphics.MeasureString(TextToPrint.Substring(RemoveZeros(_curChar)), PrinterFont,
                                         new SizeF(printWidth, printHeight), format, out chars, out lines);

                //Print the page
                e.Graphics.DrawString(TextToPrint.Substring(RemoveZeros(_curChar)), PrinterFont, Brushes.Black,
                                      printArea,
                                      format);

                //Increase current char count
                _curChar += chars;

                //Detemine if there is more text to print, if
                //there is the tell the printer there is more coming
                if (_curChar < TextToPrint.Length)
                {
                    e.HasMorePages = true;
                }
                else
                {
                    e.HasMorePages = false;
                    _curChar = 0;
                }
            }

            /// <summary>
            ///     Function to replace any zeros in the size to a 1
            ///     Zero's will mess up the printing area
            /// </summary>
            /// <param name="value">Value to check</param>
            /// <returns></returns>
            /// <remarks></remarks>
            private int RemoveZeros(int value)
            {
                //Check the value passed into the function,
                //if the value is a 0 (zero) then return a 1,
                //otherwise return the value passed in
                switch (value)
                {
                    case 0:
                        return 1;
                    default:
                        return value;
                }
            }
        }
    }
}