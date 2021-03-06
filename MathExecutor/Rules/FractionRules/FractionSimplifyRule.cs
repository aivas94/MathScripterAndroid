using System.Collections.Generic;
using System.Linq;
using MathExecutor.Expressions.Arithmetic;
using MathExecutor.Interfaces;
using MathExecutor.Models;

namespace MathExecutor.Rules.FractionRules
{
    public class FractionSimplifyRule : AbstractRecursiveRule
    {
        private readonly IExpressionFlatener _expressionFlatener;
        private readonly IElementsChanger _elementsChanger;
        private readonly IParentChecker _parentChecker;

        public FractionSimplifyRule(IExpressionFlatener expressionFlatener, 
            IElementsChanger elementsChanger,
            IParentChecker parentChecker)
        {
            _expressionFlatener = expressionFlatener;
            _elementsChanger = elementsChanger;
            _parentChecker = parentChecker;
        }

        private static bool HasPlusOrMinus(IExpression e)
        {
            return e is SumExpression || e is SubtractExpression;
        }

        private static bool IsNumericExponent(IExpression e)
        {
            return e is ExponentExpression &&
                   e.Operands[1] is Monomial &&
                   ((Monomial) e.Operands[1]).IsNumeral();
        }

        private bool ExpressionExists(IExpression expression, IEnumerable<IExpression>  expressions)
        {
            return expressions.Any(e => e.IsEqualTo(expression));
        }

        private bool IsMultiplyElement(IExpression e)
        {
            return e.ParentExpression is MultiplyExpression ||
                   e.ParentExpression is DivisionExpression ||
                   e.ParentExpression is ParenthesisExpression;
        }

        protected override InnerRuleResult ApplyRuleInner(IExpression expression)
        {
            var top = _parentChecker.GetUnderParenthesis(expression.Operands[0]);
            var bot = _parentChecker.GetUnderParenthesis(expression.Operands[1]);

            if (top is SumExpression || top is SubtractExpression)
            {
                var p = new ParenthesisExpression(top);
                top = p;
                p.ParentExpression = expression;
                expression.Operands[0] = p;
            }

            if (bot is SumExpression || bot is SubtractExpression)
            {
                var p = new ParenthesisExpression(bot);
                bot = p;
                p.ParentExpression = expression;
                expression.Operands[1] = p;
            }

            var topElements = _expressionFlatener.FlattenExpression(top, true, true, true)
                .Select(e => e.Expression)
                .ToList();
            var botElements = _expressionFlatener.FlattenExpression(bot, true, true, true)
                .Select(e => e.Expression)
                .ToList();

            topElements = topElements.Where(IsMultiplyElement).ToList();
            botElements = botElements.Where(IsMultiplyElement).ToList();

            if (topElements.Any(HasPlusOrMinus) || botElements.Any(HasPlusOrMinus))
            {
                return null;
            }

            var sameExpression = topElements.FirstOrDefault(e => ExpressionExists(e, botElements));
            if (sameExpression != null)
            {
                var sameBot = botElements.First(e => e.IsEqualTo(sameExpression));
                _elementsChanger.ChangeElement(sameExpression, new Monomial(1));
                _elementsChanger.ChangeElement(sameBot, new Monomial(1));
                return new InnerRuleResult(expression);
            }

            var topExp = topElements.Where(IsNumericExponent).ToList();
            var botExp = botElements.Where(IsNumericExponent).ToList();
            if (topExp.Count <= 0 || botExp.Count <= 0) return null;
            {
                var topOperands = topExp.Select(e => e.Operands[0]);
                var botOperands = topExp.Select(e => e.Operands[0]);
                var equalTop = topOperands.FirstOrDefault(e => ExpressionExists(e, botOperands));
                if (equalTop == null) return null;
                var equalBot = botOperands.First(e => e.IsEqualTo(equalTop));
                var op1 = equalTop.Operands[1] as Monomial;
                var op2 = equalBot.Operands[1] as Monomial;
                op1.Coefficient -= 1;
                op2.Coefficient -= 1;
                return null;
            }
        }

        protected override bool CanBeApplied(IExpression expression)
        {
            return expression is DivisionExpression;
        }

        public override string Description => "Simplifying fraction";
    }
}