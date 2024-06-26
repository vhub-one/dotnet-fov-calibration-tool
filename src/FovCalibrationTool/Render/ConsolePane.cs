﻿namespace FovCalibrationTool.Render
{
    public class ConsolePane : IDisposable
    {
        private readonly int _x;
        private readonly int _y;

        private bool _highlightPane;
        private int _yOffset = 0;

        public ConsolePane(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public void HighlightPane()
        {
            _highlightPane = true;

            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public void DrawHeader(string caption, bool highlightRow = false)
        {
            if (highlightRow)
            {
                if (_highlightPane == false)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
            }

            DrawLine(caption);

            if (highlightRow)
            {
                if (_highlightPane == false)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

        public void DrawLine(string caption = null)
        {
            if (caption == null)
            {
                caption = string.Empty;
            }

            Console.SetCursorPosition(_x, _y + _yOffset);
            Console.Write(caption.PadRight(55));

            MoveNextLine();
        }

        public void DrawLine(string caption, double value)
        {
            Console.SetCursorPosition(_x, _y + _yOffset);
            Console.Write("{0,30}: ", caption);

            if (double.IsFinite(value))
            {
                Console.Write("{0,23:F4}", value);
            }
            else
            {
                Console.Write("{0,23}", $"-.----");
            }

            MoveNextLine();
        }

        public void DrawLine<TValue>(string caption, TValue value)
        {
            Console.SetCursorPosition(_x, _y + _yOffset);
            Console.Write("{0,30}: ", caption);
            Console.Write("{0,23}", value);

            MoveNextLine();
        }

        private void MoveNextLine()
        {
            _yOffset += 1;
        }

        public void Dispose()
        {
            if (_highlightPane)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
