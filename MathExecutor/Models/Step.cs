using MathExecutor.Interfaces;

namespace MathExecutor.Models
{
    public class Step
    {
        public IExpression FullExpression { get; set; }
        public IExpression ComputedExpression { get; set; }
        public IExpression ExpressionResult { get; set; }
        public string TextExpressionResult { get; set; }
        public string RuleDescription { get; set; }

        public override string ToString()
        {
            return 
                string.IsNullOrWhiteSpace(TextExpressionResult)
                ? FullExpression.ToString()
                : TextExpressionResult;
        }
    }
}