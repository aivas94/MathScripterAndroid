using System;
using System.Collections.Generic;
using System.Linq;
using MathExecutor.Expressions.Arithmetic;
using MathExecutor.Expressions.Equality;
using MathExecutor.Expressions.Sets;
using MathExecutor.Interfaces;
using MathExecutor.Models;

namespace MathExecutor.Rules.FinalRules
{
    public class LinearEquationRule : AbstractRecursiveRule
    {
        protected override InnerRuleResult ApplyRuleInner(IExpression expression)
        {
            var left = expression.Operands[0] as Monomial;
            var right = expression.Operands[1] as Monomial;
            if (Math.Abs(left.Variables.ElementAt(0).Exponent - 1) > 0.001)
            {
                return null;
            }
            var x = new Monomial(1, left.Variables.ElementAt(0).Name[0]);
            if (Math.Abs(left.Coefficient) < 0.001)
            {
                if (Math.Abs(right.Coefficient) < 0.001)
                {
                    var result = new MemberOfExpression(x, new RealSetExpression());
                    return new InnerRuleResult(result);
                }
                else
                {
                    var result = new MemberOfExpression(x, new EmptySetExpression());
                    return new InnerRuleResult(result);
                }
            }
           
            left.Variables = null;
            var division = new DivisionExpression(right, left);
            expression.Operands[0] = x;
            expression.Operands[1] = division;
            x.ParentExpression = expression;
            division.ParentExpression = expression;
            return new InnerRuleResult(expression);
        }

        protected override bool CanBeApplied(IExpression expression)
        {
            if (expression.Operands == null || expression.Operands.Count < 2)
            {
                return false;
            }
            var isEquality = expression.Type == ExpressionType.Equation &&
                            expression.Arity == 2;
            var left = expression.Operands[0] as Monomial;
            var right = expression.Operands[1] as Monomial;
            
            return isEquality &&
                   left != null &&
                   right != null &&
                   right.IsNumeral() &&
                   Math.Abs(left.Coefficient - 1) > 0.001 &&
                   left.Variables != null && left.Variables.Count() == 1;
        }

        public override string Description => "Linear equation: ax=b";
    }
}