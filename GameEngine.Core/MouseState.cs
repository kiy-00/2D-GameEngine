using System;
using System.Collections.Generic;
using GameEngine.Common;

namespace GameEngine.Core
{
    /// <summary>
    /// 鼠标按钮枚举
    /// </summary>
    public enum MouseButton
    {
        /// <summary>
        /// 左键
        /// </summary>
        Left,

        /// <summary>
        /// 右键
        /// </summary>
        Right,

        /// <summary>
        /// 中键
        /// </summary>
        Middle,

        /// <summary>
        /// 侧键1
        /// </summary>
        XButton1,

        /// <summary>
        /// 侧键2
        /// </summary>
        XButton2
    }

    /// <summary>
    /// 表示鼠标的状态
    /// </summary>
    public class MouseState
    {
        private readonly HashSet<MouseButton> pressedButtons = new HashSet<MouseButton>();
        private Vector2 position;
        private float scrollWheelValue;
        private float scrollWheelDelta;

        /// <summary>
        /// 获取按下的按钮集合
        /// </summary>
        public IReadOnlyCollection<MouseButton> PressedButtons => pressedButtons;

        /// <summary>
        /// 获取或设置鼠标位置
        /// </summary>
        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        /// <summary>
        /// 获取或设置滚轮值
        /// </summary>
        public float ScrollWheelValue
        {
            get => scrollWheelValue;
            set
            {
                scrollWheelDelta = value - scrollWheelValue;
                scrollWheelValue = value;
            }
        }

        /// <summary>
        /// 获取滚轮增量
        /// </summary>
        public float ScrollWheelDelta => scrollWheelDelta;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MouseState()
        {
            position = Vector2.Zero;
            scrollWheelValue = 0;
            scrollWheelDelta = 0;
        }

        /// <summary>
        /// 构造函数（指定位置）
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public MouseState(int x, int y) : this()
        {
            position = new Vector2(x, y);
        }

        /// <summary>
        /// 构造函数（指定位置和按钮）
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="scrollWheelValue">滚轮值</param>
        /// <param name="buttons">按下的按钮</param>
        public MouseState(int x, int y, float scrollWheelValue, params MouseButton[] buttons) : this(x, y)
        {
            this.scrollWheelValue = scrollWheelValue;

            if (buttons != null)
            {
                foreach (var button in buttons)
                {
                    pressedButtons.Add(button);
                }
            }
        }

        /// <summary>
        /// 检查指定的按钮是否被按下
        /// </summary>
        /// <param name="button">要检查的按钮</param>
        /// <returns>如果按钮被按下则返回true，否则返回false</returns>
        public bool IsButtonDown(MouseButton button)
        {
            return pressedButtons.Contains(button);
        }

        /// <summary>
        /// 检查指定的按钮是否未被按下
        /// </summary>
        /// <param name="button">要检查的按钮</param>
        /// <returns>如果按钮未被按下则返回true，否则返回false</returns>
        public bool IsButtonUp(MouseButton button)
        {
            return !pressedButtons.Contains(button);
        }

        /// <summary>
        /// 设置按钮为按下状态
        /// </summary>
        /// <param name="button">要设置的按钮</param>
        public void SetButtonDown(MouseButton button)
        {
            pressedButtons.Add(button);
        }

        /// <summary>
        /// 设置按钮为释放状态
        /// </summary>
        /// <param name="button">要设置的按钮</param>
        public void SetButtonUp(MouseButton button)
        {
            pressedButtons.Remove(button);
        }

        /// <summary>
        /// 设置多个按钮为按下状态
        /// </summary>
        /// <param name="buttons">要设置的按钮</param>
        public void SetButtonsDown(IEnumerable<MouseButton> buttons)
        {
            if (buttons == null)
                return;

            foreach (var button in buttons)
            {
                pressedButtons.Add(button);
            }
        }

        /// <summary>
        /// 设置多个按钮为释放状态
        /// </summary>
        /// <param name="buttons">要设置的按钮</param>
        public void SetButtonsUp(IEnumerable<MouseButton> buttons)
        {
            if (buttons == null)
                return;

            foreach (var button in buttons)
            {
                pressedButtons.Remove(button);
            }
        }

        /// <summary>
        /// 清除所有按钮状态
        /// </summary>
        public void ClearButtons()
        {
            pressedButtons.Clear();
        }

        /// <summary>
        /// 创建当前状态的拷贝
        /// </summary>
        /// <returns>新的鼠标状态实例</returns>
        public MouseState Clone()
        {
            MouseState state = new MouseState((int)position.X, (int)position.Y, scrollWheelValue);
            state.SetButtonsDown(pressedButtons);
            state.scrollWheelDelta = scrollWheelDelta;
            return state;
        }

        /// <summary>
        /// 获取位置的X坐标
        /// </summary>
        public int X => (int)position.X;

        /// <summary>
        /// 获取位置的Y坐标
        /// </summary>
        public int Y => (int)position.Y;

        /// <summary>
        /// 检查左键是否按下
        /// </summary>
        public bool LeftButton => IsButtonDown(MouseButton.Left);

        /// <summary>
        /// 检查右键是否按下
        /// </summary>
        public bool RightButton => IsButtonDown(MouseButton.Right);

        /// <summary>
        /// 检查中键是否按下
        /// </summary>
        public bool MiddleButton => IsButtonDown(MouseButton.Middle);

        /// <summary>
        /// 检查侧键1是否按下
        /// </summary>
        public bool XButton1 => IsButtonDown(MouseButton.XButton1);

        /// <summary>
        /// 检查侧键2是否按下
        /// </summary>
        public bool XButton2 => IsButtonDown(MouseButton.XButton2);
    }
}