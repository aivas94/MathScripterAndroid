using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using MathExecutor.Expressions.Equality;
using MathExecutor.Interfaces;
using MathExecutor.Interpreter;
using MathScripter.Models;
using MathScripter.Views;

namespace MathScripter
{
    [Activity(Label = "Function Graph")]
    public class GraphActivity : Activity
    {

        private readonly IInterpreter _interpreter =
           App.Container.Resolve(typeof(Interpreter), "interpreter") as IInterpreter;


        private const int paddingH = 5;
        private const int paddingV = 2;
        private const float ExpressionWindowSize = 0.17f;

        private int _addingIndex;

        private GraphView _graphView;
        private LinearLayout _rightPanel;

        private ImageButton _zoomInButton;
        private ImageButton _zoomOutButton;
        private ImageButton _resetButton;
        private ImageButton _clearButton;

        private ImageButton _addNewButtonCamera;
        private ImageButton _addNewButtonPencil;

        private readonly IList<string> _expressions = new List<string>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Graph);
            _graphView = FindViewById<GraphView>(Resource.Id.graphView);
            _rightPanel = FindViewById<LinearLayout>(Resource.Id.rightGraphPanel);
            var e = Intent.GetStringExtra("expression") ?? "";
            AddExpression(e);
            RenderRightPanel();
        }

        private void EditExpression(int index, string expression)
        {
            try
            {
                var e = _interpreter.GetExpression(expression);
                _expressions[index] = expression;
                _graphView.ChangeExpressionAt(index, e);
            }
            catch (Exception)
            {
            }
        }

        private void AddExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression) || !_interpreter.CanBeParsed(expression))
            {
                return;
            }
            var e = _interpreter.GetExpression(expression);
            if (e is EqualityExpression)
            {
                _expressions.Add(e.Operands[0].ToString());
                _expressions.Add(e.Operands[1].ToString());
                _graphView.AddExpression(e.Operands[0]);
                _graphView.AddExpression(e.Operands[1]);
            }
            else
            {
                _expressions.Add(expression);
                _graphView.AddExpression(e);
            }
        }

        private void RemoveExpression(int index)
        {
            if (index <= -1 || index >= _expressions.Count) return;
            _expressions.RemoveAt(index);
            _graphView.RemoveExpression(index);
        }

        private View GetAddingPanel()
        {
            var addingPanel = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            var layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
             0)
            { Weight = 0.16f };
            addingPanel.LayoutParameters = layoutParams;
            var buttonLayoutParams = new LinearLayout.LayoutParams(0,
                ViewGroup.LayoutParams.MatchParent)
            { Weight = 0.5f };
            _addNewButtonCamera = new ImageButton(this) { Background = null };
            _addNewButtonPencil = new ImageButton(this) { Background = null };

            _addNewButtonCamera.SetImageResource(Resource.Drawable.cameraplus);
            _addNewButtonCamera.SetScaleType(ImageView.ScaleType.FitCenter);
            _addNewButtonPencil.SetImageResource(Resource.Drawable.editplus);
            _addNewButtonPencil.SetScaleType(ImageView.ScaleType.FitCenter);

            _addNewButtonCamera.Click += AddNewFunctionCam;
            _addNewButtonPencil.Click += AddNewFunctionPencil;

            _addNewButtonCamera.LayoutParameters = buttonLayoutParams;
            _addNewButtonPencil.LayoutParameters = buttonLayoutParams;

            _addNewButtonCamera.SetPadding(paddingH, paddingV, paddingH, paddingV);
            _addNewButtonPencil.SetPadding(paddingH, paddingV, paddingH, paddingV);

            addingPanel.AddView(_addNewButtonCamera);
            addingPanel.AddView(_addNewButtonPencil);

            return addingPanel;
        }

        private View GetZoomPanel()
        {
            var zoomPanel = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            var layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                0)
            { Weight = 0.16f };
            zoomPanel.LayoutParameters = layoutParams;
            var buttonLayoutParams = new LinearLayout.LayoutParams(0,
                ViewGroup.LayoutParams.MatchParent)
            { Weight = 0.25f };
            _zoomInButton = new ImageButton(this) { Background = null };
            _zoomOutButton = new ImageButton(this) { Background = null };
            _resetButton = new ImageButton(this) { Background = null };
            _clearButton = new ImageButton(this) { Background = null };

            _zoomInButton.SetImageResource(Resource.Drawable.plus);
            _zoomInButton.SetScaleType(ImageView.ScaleType.FitCenter);

            _zoomOutButton.SetImageResource(Resource.Drawable.minus);
            _zoomOutButton.SetScaleType(ImageView.ScaleType.FitCenter);

            _resetButton.SetImageResource(Resource.Drawable.zoom);
            _resetButton.SetScaleType(ImageView.ScaleType.FitCenter);

            _clearButton.SetImageResource(Resource.Drawable.trashblue);
            _clearButton.SetScaleType(ImageView.ScaleType.FitCenter);

            _zoomInButton.LayoutParameters = buttonLayoutParams;
            _zoomOutButton.LayoutParameters = buttonLayoutParams;
            _resetButton.LayoutParameters = buttonLayoutParams;
            _clearButton.LayoutParameters = buttonLayoutParams;

            _zoomInButton.Click += ZoomIn;
            _zoomOutButton.Click += ZoomOut;
            _resetButton.Click += Reset;
            _clearButton.Click += Clear;

            _zoomInButton.SetPadding(paddingH, paddingV, paddingH, paddingV);
            _zoomOutButton.SetPadding(paddingH, paddingV, paddingH, paddingV);
            _resetButton.SetPadding(paddingH, paddingV, paddingH, paddingV);
            _clearButton.SetPadding(paddingH, paddingV, paddingH, paddingV);

            zoomPanel.AddView(_zoomInButton);
            zoomPanel.AddView(_zoomOutButton);
            zoomPanel.AddView(_resetButton);
            zoomPanel.AddView(_clearButton);

            return zoomPanel;
        }

        private View GetSingleExpressionPanel(string expression, int i)
        {
            var viewParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0)
            { Weight = 1.0f / _expressions.Count };

            var exprPanel = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal,
                LayoutParameters = viewParams
            };

            var expressionParams = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent)
            { Weight = 0.8f };

            var expressionView = new ExpressionView(this, null) { LayoutParameters = expressionParams };
            expressionView.SetNotSolve();
            expressionView.SetExpressionColor(_graphView.GetColorAt(i));
            expressionView.SetMode(ExpressionViewMode.Expression);
            expressionView.SetExpression(expression);

            var actionsParams = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent)
            { Weight = 0.2f };

            exprPanel.AddView(expressionView);

            var actionPanel = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = actionsParams
            };

            var actionParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0)
            { Weight = 0.5f };

            var removeButton = new ImageButton(this) { LayoutParameters = actionParams, Background = null, Id = i };
            var editButton = new ImageButton(this) { LayoutParameters = actionParams, Background = null, Id = i };

            removeButton.SetPadding(paddingH, paddingV, paddingH, paddingV);
            editButton.SetPadding(paddingH, paddingV, paddingH, paddingV);

            removeButton.SetImageResource(Resource.Drawable.trash);
            removeButton.SetScaleType(ImageView.ScaleType.FitCenter);

            editButton.SetImageResource(Resource.Drawable.edit);
            editButton.SetScaleType(ImageView.ScaleType.FitCenter);

            removeButton.Click += RemoveClick;
            editButton.Click += EditClick;

            actionPanel.AddView(removeButton);
            actionPanel.AddView(editButton);
            exprPanel.AddView(actionPanel);

            return exprPanel;
        }

        private View GetExpressionsView()
        {
            var exprPanel = new LinearLayout(this) { Orientation = Orientation.Vertical };
            var layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                0)
            { Weight = ExpressionWindowSize * _expressions.Count };
            exprPanel.LayoutParameters = layoutParams;
            var i = 0;
            foreach (var expression in _expressions)
            {
                var expressionView = GetSingleExpressionPanel(expression, i);
                exprPanel.AddView(expressionView);
                i++;
            }
            return exprPanel;
        }

        private View GetExpressionsPlaceholder()
        {
            var count = 4 - _expressions.Count;
            var placeholder = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            var layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0)
            { Weight = count * ExpressionWindowSize };
            placeholder.LayoutParameters = layoutParams;
            return placeholder;
        }

        private void RenderRightPanel()
        {
            _rightPanel.RemoveAllViews();
            var zoomPanel = GetZoomPanel();
            var addingPanel = GetAddingPanel();
            var placeholderPanel = GetExpressionsPlaceholder();
            var expressionsView = GetExpressionsView();
            _rightPanel.AddView(zoomPanel);
            _rightPanel.AddView(expressionsView);
            if (_expressions.Count < 4)
            {
                _rightPanel.AddView(addingPanel);
            }
            _rightPanel.AddView(placeholderPanel);
        }

        private void ZoomIn(object sender, EventArgs args)
        {
            _graphView.ZoomToZero();
        }

        private void ZoomOut(object sender, EventArgs args)
        {
            _graphView.ZoomOutToZero();
        }

        private void Reset(object sender, EventArgs args)
        {
            _graphView.Reset();
        }

        private void Clear(object sender, EventArgs args)
        {
            _graphView.Clear();
            _expressions.Clear();
            RenderRightPanel();
        }

        private void AddNewFunctionCam(object sender, EventArgs args)
        {
            _addingIndex = -1;
            StartActivityForResult(typeof(CameraActivity), 0);
        }

        private void AddNewFunctionPencil(object sender, EventArgs args)
        {
            _addingIndex = -1;
            StartActivityForResult(typeof(ExpressionEditActivity), 0);
        }

        private void RemoveClick(object sender, EventArgs args)
        {
            var id = (sender as ImageButton)?.Id ?? 0;
            RemoveExpression(id);
            RenderRightPanel();
        }

        private void EditClick(object sender, EventArgs args)
        {
            var id = (sender as ImageButton)?.Id ?? 0;
            _addingIndex = id;
            var intent = new Intent(this, typeof(ExpressionEditActivity));
            intent.PutExtra("expression", _expressions[id]);
            StartActivityForResult(intent, 0);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode != Result.Ok) return;
            var expression = data.GetStringExtra("expression");
            if (!_interpreter.CanBeParsed(expression))
            {
                return;
            }
            if (_addingIndex < 0)
            {
                AddExpression(expression);
            }
            else
            {
                EditExpression(_addingIndex, expression);
            }
            RenderRightPanel();
        }

        protected override void OnPause()
        {
            base.OnPause();
            _graphView.Clean();
        }
    }
}