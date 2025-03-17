using System.Collections.Generic;

namespace UB.UI
{
    public class UINavigation
    {
        private Stack<UIView> _viewStack = new Stack<UIView>();

        public void PushView(UIView view)
        {
            if (_viewStack.Count > 0)
            {
                UIView topView = _viewStack.Peek();
                topView.Hide();
            }

            _viewStack.Push(view);
            view.Show();
        }

        public void PopView()
        {
            if (_viewStack.Count == 0)
            {
                return;
            }

            UIView topView = _viewStack.Pop();
            topView.Hide();

            if (_viewStack.Count > 0)
            {
                UIView nextView = _viewStack.Peek();
                nextView.Show();
            }
        }

        public void Clear()
        {
            foreach (UIView view in _viewStack)
            {
                view.Hide();
            }
            _viewStack.Clear();
        }

        public void PopToRoot()
        {
            while (_viewStack.Count > 1)
            {
                UIView topView = _viewStack.Pop();
                topView.Hide();
            }

            UIView rootView = _viewStack.Peek();
            rootView.Show();
        }

        public int Count()
        {
            return _viewStack.Count;
        }

        public UIView Peek()
        {
            return _viewStack.Peek();
        }
    }
}