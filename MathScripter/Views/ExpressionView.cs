using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using MathDrawer;
using MathDrawer.Helpers;
using MathDrawer.Interfaces;
using MathDrawer.Models;
using MathExecutor.Interfaces;
using MathExecutor.Interpreter;
using MathExecutor.Models;
using MathExecutor.RuleBinders;
using MathScripter.Models;

namespace MathScripter.Views
{
    public class ExpressionView : View, GestureDetector.IOnGestureListener
    {

        private readonly IInterpreter _interpreter =
            App.Container.Resolve(typeof(Interpreter), "interpreter") as IInterpreter;
        private readonly IExpressionDrawer _expressionDrawer =
           App.Container.Resolve(typeof(ExpressionDrawer), "expressionDrawer") as IExpressionDrawer;
        private readonly IStepsDrawer _stepsDrawer =
           App.Container.Resolve(typeof(StepsDrawer), "expressionDrawer") as IStepsDrawer;
        private readonly IRecursiveRuleMathcer _ruleMatcher =
            App.Container.Resolve(typeof(RecursiveRuleMatcher), "recursiveRuleMatcher") as IRecursiveRuleMathcer;
        private readonly ITextMeasurer _textMeasurer =
            App.Container.Resolve(typeof(TextMeasurer), "textMeasurer") as ITextMeasurer;

        private Color _expressionColor = Color.Black;
        private readonly Typeface _typeface;

        private string _expression = "";
        private IExpression _expressionModel;
        private IExpression _expressionResult;
        private IEnumerable<Step> _steps = new List<Step>();
        private int _stepHeight;
        private ExpressionViewMode _mode;

        private bool _solve = true;
        public bool EditMode { get; set; }
        private bool _hasError;

        private bool _needsRedraw = true;
        private Bitmap _buffer = Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888);
        private float _currentOffsetY;

        private readonly GestureDetector _gestureDetector;

        private readonly Paint _p;


        public ExpressionView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            var assets = context.Assets;
            _gestureDetector = new GestureDetector(this);
            _mode = ExpressionViewMode.Expression;
            _p = new Paint
            {
                Color = _expressionColor,
                TextSize = 60
            };
            _typeface = Typeface.CreateFromAsset(assets, "Content/Fonts/LinLibertine_R.ttf");
            _p.SetTypeface(_typeface);
        }

        public void DrawStepsToBuffer(Canvas original)
        {
            _buffer.Recycle();
            _buffer = Bitmap.CreateBitmap(original.Width, 3000, Bitmap.Config.Argb8888);
            var canvas = new Canvas(_buffer);
            canvas.DrawColor(Color.White);
            if (string.IsNullOrWhiteSpace(_expression)
                || !_interpreter.CanBeParsed(_expression)) return;
            _stepHeight = _stepsDrawer.DrawSteps(_steps, _p, canvas, 3000, original.Width);
            _needsRedraw = false;
        }

        public void DrawExpression(Canvas canvas)
        {
            canvas.DrawColor(Color.White);
            if (string.IsNullOrWhiteSpace(_expression)
                || !_interpreter.CanBeParsed(_expression)) return;
            var e = _interpreter.GetExpression(_expression);
            _expressionDrawer.Draw(e, _p, canvas, canvas.Width, canvas.Height);
        }

        public void DrawSolution(Canvas canvas)
        {
            canvas.DrawColor(Color.White);
            if (_expressionModel == null ||
                _expressionResult == null) return;
            _expressionDrawer.Draw(_expressionModel, _p, canvas,
                canvas.Width, (int)(0.8 * canvas.Height / 2));
            _expressionDrawer.Draw(_expressionResult, _p, canvas,
                canvas.Width, (int)(0.8 * canvas.Height / 2), canvas.Height / 2);
        }

        public void SetExpressionColor(Color color)
        {
            _expressionColor = color;
            _p.Color = _expressionColor;
            Invalidate();
        }

        public void SetMode(ExpressionViewMode mode)
        {
            _mode = mode;
            _currentOffsetY = 0;
            if (mode != ExpressionViewMode.Expression && (_steps == null || !_steps.Any()))
            {
                ComputeSolution();
            }
            _needsRedraw = true;
            Invalidate();
        }

        private void DrawError(Canvas c)
        {
            var errorText = Resources.GetString(Resource.String.EquationParsingError);
            var p = new Paint { TextSize = 60 };
            p.SetTypeface(_typeface);
            p.Color = Color.Red;
            var v = p.MeasureText(errorText);
            var ratio = Width / v;
            p.TextSize *= ratio * 0.9f;
            var newWidth = p.MeasureText(errorText);
            var nameHeight =
                _textMeasurer.GetGenericTextHeight(new TextParameters { Typeface = _typeface, Size = p.TextSize });
            c.DrawText(errorText, (Width - newWidth) / 2, Height / 2f + nameHeight / 2, p);
        }

        private void ComputeSolution()
        {
            if (string.IsNullOrWhiteSpace(_expression)
                || !_interpreter.CanBeParsed(_expression))
            {
                _hasError = true;
                return;
            }
            _hasError = false;
            _expressionModel = _interpreter.GetExpression(_expression);
            _steps = _ruleMatcher.SolveExpression(_expressionModel.Clone());
            _expressionResult = _steps?.Last().FullExpression;
        }

        public void SetNotSolve()
        {
            _solve = false;
        }

        public void SetExpression(string expression)
        {
            _currentOffsetY = 0;
            _expression = expression;
            _hasError = !_interpreter.CanBeParsed(_expression);
            if (_solve)
            {
                ComputeSolution();
            }
            _needsRedraw = true;
            Invalidate();
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            _gestureDetector.OnTouchEvent(e);
            return true;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            canvas.DrawColor(Color.White);
            if (!EditMode && _hasError && !string.IsNullOrWhiteSpace(_expression))
            {
                DrawError(canvas);
                return;
            }

            switch (_mode)
            {
                case ExpressionViewMode.Expression:
                    DrawExpression(canvas);
                    break;
                case ExpressionViewMode.Solution:
                    DrawSolution(canvas);
                    break;
                case ExpressionViewMode.Steps:
                    if (_needsRedraw)
                    {
                        DrawStepsToBuffer(canvas);
                    }
                    canvas.DrawBitmap(_buffer, 0, _currentOffsetY, null);
                    break;
                case ExpressionViewMode.Animation:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Clear()
        {
            _currentOffsetY = 0;
            _buffer.Recycle();
            _needsRedraw = true;
        }

        public bool OnDown(MotionEvent e)
        {
            return false;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            return false;
        }

        public void OnLongPress(MotionEvent e)
        {
        }

        private bool CanScrool(float distanceY)
        {
            var distDif = _currentOffsetY - distanceY;
            return _mode == ExpressionViewMode.Steps &&
                   distDif > -3000 &&
                   distDif > -(_steps.Count() * _stepHeight) &&
                   distDif < 0;
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            if (CanScrool(distanceY))
            {
                _currentOffsetY -= distanceY;
            }
            Invalidate();
            return true;
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            return false;
        }
    }
}