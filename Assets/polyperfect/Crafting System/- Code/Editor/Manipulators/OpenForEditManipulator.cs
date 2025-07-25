using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Polyperfect.Crafting.Edit
{
    public class OpenForEditManipulator : Manipulator
    {
        readonly int _requiredClickCount;
        public event Action Clicked;
        public MouseButton MouseButton { get; set; } = MouseButton.LeftMouse;

        public OpenForEditManipulator(System.Func<Object> getObject, int requiredClickCount, Action refreshAction = null)
        {
            _requiredClickCount = requiredClickCount;
            Clicked+=()=>ObjectEditWindow.CreateForObject(getObject(), refreshAction);
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(HandleMouseDown);
            target.RegisterCallback<MouseUpEvent>(HandleMouseUp);
            target.RegisterCallback<MouseMoveEvent>(HandleMouseMove);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(HandleMouseDown);
            target.UnregisterCallback<MouseUpEvent>(HandleMouseUp);
            target.UnregisterCallback<MouseMoveEvent>(HandleMouseMove);
        }

        int m_currentClickCount = 0;
        void HandleMouseDown(MouseDownEvent evt)
        {
            if (evt.button != (int)MouseButton)
                return;
            m_currentClickCount++;
        }

        void HandleMouseUp(MouseUpEvent evt)
        {
            if (evt.button != (int)MouseButton)
                return;
            if (m_currentClickCount >= _requiredClickCount)
            {
                Clicked?.Invoke();
                m_currentClickCount = 0;
                evt.StopPropagation();
            }
        }

        void HandleMouseMove(MouseMoveEvent evt)
        {
            m_currentClickCount = 0;
        }
    }
}