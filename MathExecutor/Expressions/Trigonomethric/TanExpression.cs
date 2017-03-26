using System;
using MathExecutor.Interfaces;
using MathExecutor.Models;

namespace MathExecutor.Expressions.Trigonomethric
{
    public class TanExpression : AbstractUnaryExpression
    {
        public TanExpression(IExpression operand) : base(operand)
        {
        }

        public override IExpression Clone()
        {
            return new TanExpression(Operands[0].Clone());
        }

        public override IExpression InnerExecute()
        {
            var op = Operands[0] as Monomial;
            if (op == null)
            {
                return this;
            }
            return new Monomial(Math.Tan(op.Coefficient));
        }

        public override ExpressionType Type => ExpressionType.Trigonometric;
        public override int Order => 1;

        public override string ToString()
        {
            return $"tan {Operands[0]}";
        }

        public override bool CanBeExecuted()
        {
            var op = Operands[0] as Monomial;
            return op != null && op.IsNumeral();
        }

        public override string Name => "tan";
    }
}