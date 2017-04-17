using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.Graphics;
using MathDrawer.Interfaces;
using MathDrawer.Models;
using MathExecutor.Interfaces;

namespace MathDrawer.Functions
{
    public class GraphDrawer : IGraphDrawer
    {
        private const float DefaultStartX = -10;
        private const float DefaultStartY = 10;
        private const int DefaultSize = 1;
        private const float DefaultPixels = 100;
        private const int Accuracy = 100;

        private float _pixelsPerUnit;
        private float _baseX = 0;
        private float _baseY = 0;
        private Typeface _typeface;

        private readonly IFunctionManager _functionManager;

        private GraphNetString _scaleString;

        private readonly IEnumerable<GraphNetString> _netStrings = new List<GraphNetString>
        {
            new GraphNetString {Width = 1, Frequency = 1, MaxSize = 5f},
            new GraphNetString {Width = 2, Frequency = 4, MaxSize = 0.25f},
            new GraphNetString {Width = 3, Frequency = 20, MaxSize = 0.05f},
            new GraphNetString {Width = 4, Frequency = 100, MaxSize = 0.01f}
        };

        private IList<Color> _functionColors = new List<Color>();

        public GraphDrawer(IFunctionManager functionManager)
        {
            _functionManager = functionManager;
            Reset();
        }

        public void Init(int sizeX, int sizeY, Color netColor, Typeface typeface)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            NetColor = netColor;
            _typeface = typeface;
        }

        private void DrawFunctions(Canvas c, Paint p)
        {

        }

        private void DrawVerticalLine(float x, int width, Canvas c, Paint p)
        {
            p.StrokeWidth = width;
            p.Color = NetColor;
            c.DrawLine(x, 0, x, SizeY, p);
        }

        private void DrawHorizontalLine(float y, int width, Canvas c, Paint p)
        {
            p.StrokeWidth = width;
            p.Color = NetColor;
            c.DrawLine(0, y, SizeX, y, p);
        }

        private void DrawVerticalNet(float startX,   float startUnit, Canvas c, Paint p)
        {
            var currentPoint = startUnit;
            var currentPixels = startX;
            var i = -1;
            while (currentPixels < SizeX)
            {
                i++;
                if (Math.Abs(currentPoint) < 0.01)
                {
                    DrawVerticalLine(currentPixels, 6, c, p);
                }
                if (i % _scaleString.Frequency == 0)
                {
                    DrawLabel(currentPixels, _baseY, currentPoint, c, p);
                }
                foreach (var netString in _netStrings.Reverse())
                {
                    if (i < 1 || !(netString.MaxSize < Scale) || i % netString.Frequency != 0) continue;
                    DrawVerticalLine(currentPixels, netString.Width, c, p);
                    break;
                }
                currentPixels += _pixelsPerUnit / 4;
                currentPoint += 0.25f;
            }
        }

        private void DrawHorizontalNet(float startY, float startUnit, Canvas c, Paint p)
        {
            var currentPoint = startUnit;
            var currentPixels = startY;
            var i = -1;
            while (currentPixels < SizeX)
            {
                i++;
                if (Math.Abs(currentPoint) < 0.01)
                {
                    DrawHorizontalLine(currentPixels, 6, c, p);
                }
                if (i % _scaleString.Frequency == 0)
                {
                    DrawLabel(_baseX, currentPixels, currentPoint, c, p);
                }
                foreach (var netString in _netStrings.Reverse())
                {
                    if (i < 1 || !(netString.MaxSize < Scale) || i % netString.Frequency != 0) continue;
                    DrawHorizontalLine(currentPixels, netString.Width, c, p);
                    break;
                }
                currentPixels += _pixelsPerUnit / 4;
                currentPoint -= 0.25f;
            }
        }

        private void DrawNet(Canvas c, Paint p)
        {
            var offsetX = (float)(Convert.ToInt32(StartX * 100) % 25) / 100;
            var offsetY = (float)(Convert.ToInt32(StartY * 100) % 25) / 100;
            DrawHorizontalNet(offsetY * _pixelsPerUnit, StartY + offsetY, c, p);
            DrawVerticalNet(offsetX * _pixelsPerUnit, StartX + offsetX, c, p);
        }

        public float StartX { get; set; }
        public float StartY { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }
        public float Scale { get; set; }
        public Color NetColor { get; set; }

        public void DrawGraph(Canvas c, Paint p)
        {
            DrawNet(c, p);
            DrawFunctions(c, p);
        }

        public bool AddFunction(IExpression expression, Color color)
        {
            var result = _functionManager.AddFunction(expression);
            if (result)
            {
                _functionColors.Add(color);
            }
            return result;
        }

        public void RemoveFunction(int index)
        {
            var result = _functionManager.RemoveFunction(index);
            if (result)
            {
                _functionColors.RemoveAt(index);
            }
        }

        private void DrawLabel(float positionX, float positionY, float value,
            Canvas c, Paint p)
        {
            p.SetTypeface(_typeface);
            p.Color = Color.Black;
            p.TextSize = 32;
            var text = Math.Round(value, 2).ToString(CultureInfo.InvariantCulture);
            c.DrawText(text, positionX, positionY, p);
        }

        public void ClearGraph()
        {
            _functionColors.Clear();
            _functionManager.ClearAll();
        }

        public void Reset()
        {
            _pixelsPerUnit = DefaultPixels;
            StartX = DefaultStartX;
            StartY = DefaultStartY;
            SetScale(DefaultSize);
            _scaleString = _netStrings.First(n => n.MaxSize < Scale);
        }

        public void SetScale(float scale)
        {
            Scale = scale;
            _pixelsPerUnit = DefaultPixels * scale;
            _baseX = Math.Abs(StartX) * _pixelsPerUnit;
            _baseY = Math.Abs(StartY) * _pixelsPerUnit;
        }
    }
}