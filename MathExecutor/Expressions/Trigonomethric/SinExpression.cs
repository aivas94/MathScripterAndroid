using System;
using MathExecutor.Interfaces;
using MathExecutor.Models;

namespace MathExecutor.Expressions.Trigonomethric
{
    public class SinExpression : AbstractUnaryExpression
    {
        public SinExpression(IExpression operand, string id = null) : base(operand, id)
        {
        }

        public override IExpression Clone(bool changeId)
        {
            return new SinExpression(Operands[0].Clone(changeId), changeId ? null : Id);
        }

        public override IExpression InnerExecute()
        {
            var op = Operands[0] as Monomial;
            if (op == null)
            {
                return this;
            }
            return new Monomial(Math.Sin(op.Coefficient), ParentExpression);
        }

        public override ExpressionType Type => ExpressionType.Trigonometric;
        public override int Order => 1;

        public override string ToString()
        {
            return $"sin {Operands[0]}";
        }

        public override bool CanBeExecuted()
        {
            var op = Operands[0] as Monomial;
            return op != null && op.IsNumeral();
        }

        public override string Name => "sin";
    }
}