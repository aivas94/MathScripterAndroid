using Android.Graphics;
using MathExecutor.Interfaces;

namespace MathDrawer.Interfaces
{
    public interface IExpressionDrawer
    {
        void Draw(IExpression expression , Paint p, Canvas c, int width, int  height, int yOffset = 0);
    }
}