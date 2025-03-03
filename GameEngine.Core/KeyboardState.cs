using System;
using System.Collections.Generic;

namespace GameEngine.Core
{
    /// <summary>
    /// 键盘按键枚举
    /// </summary>
    public enum Keys
    {
        None = 0,

        // 字母键
        A = 65, B = 66, C = 67, D = 68, E = 69, F = 70, G = 71, H = 72, I = 73, J = 74,
        K = 75, L = 76, M = 77, N = 78, O = 79, P = 80, Q = 81, R = 82, S = 83, T = 84,
        U = 85, V = 86, W = 87, X = 88, Y = 89, Z = 90,

        // 数字键
        D0 = 48, D1 = 49, D2 = 50, D3 = 51, D4 = 52, D5 = 53, D6 = 54, D7 = 55, D8 = 56, D9 = 57,

        // 小键盘数字键
        NumPad0 = 96, NumPad1 = 97, NumPad2 = 98, NumPad3 = 99, NumPad4 = 100,
        NumPad5 = 101, NumPad6 = 102, NumPad7 = 103, NumPad8 = 104, NumPad9 = 105,

        // 功能键
        F1 = 112, F2 = 113, F3 = 114, F4 = 115, F5 = 116, F6 = 117,
        F7 = 118, F8 = 119, F9 = 120, F10 = 121, F11 = 122, F12 = 123,

        // 箭头键
        Left = 37, Up = 38, Right = 39, Down = 40,

        // 特殊键
        Escape = 27, Space = 32, Enter = 13, Tab = 9, Backspace = 8,
        Insert = 45, Delete = 46, Home = 36, End = 35, PageUp = 33, PageDown = 34,
        LeftShift = 16, RightShift = 16, LeftControl = 17, RightControl = 17,
        LeftAlt = 18, RightAlt = 18, LeftWindows = 91, RightWindows = 92,
        CapsLock = 20, NumLock = 144, ScrollLock = 145,

        // 符号键
        Add = 107, Subtract = 109, Multiply = 106, Divide = 111, Decimal = 110,
        OemSemicolon = 186, OemPlus = 187, OemComma = 188, OemMinus = 189, OemPeriod = 190,
        OemQuestion = 191, OemTilde = 192, OemOpenBrackets = 219, OemPipe = 220,
        OemCloseBrackets = 221, OemQuotes = 222
    }

    /// <summary>
    /// 表示键盘的状态
    /// </summary>
    public class KeyboardState
    {
        private readonly HashSet<Keys> pressedKeys = new HashSet<Keys>();

        /// <summary>
        /// 获取按下的键的集合
        /// </summary>
        public IReadOnlyCollection<Keys> PressedKeys => pressedKeys;

        /// <summary>
        /// 构造函数
        /// </summary>
        public KeyboardState()
        {
        }

        /// <summary>
        /// 构造函数（指定按下的键）
        /// </summary>
        /// <param name="keys">按下的键</param>
        public KeyboardState(params Keys[] keys)
        {
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    pressedKeys.Add(key);
                }
            }
        }

        /// <summary>
        /// 检查指定的键是否被按下
        /// </summary>
        /// <param name="key">要检查的键</param>
        /// <returns>如果键被按下则返回true，否则返回false</returns>
        public bool IsKeyDown(Keys key)
        {
            return pressedKeys.Contains(key);
        }

        /// <summary>
        /// 检查指定的键是否未被按下
        /// </summary>
        /// <param name="key">要检查的键</param>
        /// <returns>如果键未被按下则返回true，否则返回false</returns>
        public bool IsKeyUp(Keys key)
        {
            return !pressedKeys.Contains(key);
        }

        /// <summary>
        /// 设置键为按下状态
        /// </summary>
        /// <param name="key">要设置的键</param>
        public void SetKeyDown(Keys key)
        {
            pressedKeys.Add(key);
        }

        /// <summary>
        /// 设置键为释放状态
        /// </summary>
        /// <param name="key">要设置的键</param>
        public void SetKeyUp(Keys key)
        {
            pressedKeys.Remove(key);
        }

        /// <summary>
        /// 设置多个键为按下状态
        /// </summary>
        /// <param name="keys">要设置的键</param>
        public void SetKeysDown(IEnumerable<Keys> keys)
        {
            if (keys == null)
                return;

            foreach (var key in keys)
            {
                pressedKeys.Add(key);
            }
        }

        /// <summary>
        /// 设置多个键为释放状态
        /// </summary>
        /// <param name="keys">要设置的键</param>
        public void SetKeysUp(IEnumerable<Keys> keys)
        {
            if (keys == null)
                return;

            foreach (var key in keys)
            {
                pressedKeys.Remove(key);
            }
        }

        /// <summary>
        /// 清除所有按键状态
        /// </summary>
        public void ClearKeys()
        {
            pressedKeys.Clear();
        }

        /// <summary>
        /// 创建当前状态的拷贝
        /// </summary>
        /// <returns>新的键盘状态实例</returns>
        public KeyboardState Clone()
        {
            KeyboardState state = new KeyboardState();
            state.SetKeysDown(pressedKeys);
            return state;
        }
    }
}