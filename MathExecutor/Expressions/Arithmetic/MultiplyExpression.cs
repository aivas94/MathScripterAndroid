using System.Collections.Generic;
using System.Linq;
using MathExecutor.Interfaces;
using MathExecutor.Models;

namespace MathExecutor.Expressions.Arithmetic
{
    public class MultiplyExpression : AbstractBinaryExpression
    {
        public MultiplyExpression(IExpression leftOperand, IExpression rightOperand, string id = null) 
            : base(leftOperand, rightOperand, id)
        {
        }

        public override IExpression Clone(bool changeId)
        {
            return new MultiplyExpression(Operands[0].Clone(changeId), Operands[1].Clone(changeId), changeId ? null : Id);
        }

        public override IExpression InnerExecute()
        {
            var left = Operands[0] as Monomial;
            var right = Operands[1] as Monomial;
            var variables = new List<IVariable>();
            var rightVariables = right?.Variables?.ToList() ?? new List<IVariable>();
            foreach (var variable in left?.Variables ?? new List<IVariable>())
            {
                var rightVar = rightVariables.FirstOrDefault(v => v.Name == variable.Name);
                var rightExponent = rightVar?.Exponent ?? 0;
                if (rightVar != null)
                {
                    rightVariables.Remove(rightVar);
                }
                variables.Add(new Variable {Name = variable.Name, Exponent = variable.Exponent + rightExponent });
            }
            variables.AddRange(rightVariables);
            return new Monomial(left.Coefficient * right.Coefficient, variables, ParentExpression);
        }

        public override string ToString()
        {
            return $"{Operands[0]} * {Operands[1]}";
        }

        public override int Order => 2;
        public override bool CanBeExecuted() => true;
        public override ExpressionType Type => ExpressionType.Arithmetic;
        public override string Name => "�";
    }
}