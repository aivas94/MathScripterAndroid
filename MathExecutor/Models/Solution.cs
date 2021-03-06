using System.Collections.Generic;
using MathExecutor.Interfaces;

namespace MathExecutor.Models
{
    public class Solution
    {
        public IList<Step> Steps { get; set; }
        public IExpression Result { get; set; }
        public bool HasNumericResult { get; set; }
        public double NumericResult { get; set; }

        public Solution()
        {
            Steps = new List<Step>();
        }

        public void AddStep(Step step)
        {
            Steps.Add(step);
        }

    }
}