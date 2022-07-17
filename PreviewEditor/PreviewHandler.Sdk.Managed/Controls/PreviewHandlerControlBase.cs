﻿using PreviewHandler.Sdk.Interop;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PreviewHandler.Sdk.Controls
{
    public abstract class PreviewHandlerControlBase : Form, IPreviewHandlerControl
    {
        /// <summary>
        /// Needed to make the form a child window.
        /// </summary>
        private static int _gwlStyle = -16;
        private static int _wsChild = 0x40000000;

        /// <summary>
        /// Holds the parent window handle.
        /// </summary>
        private IntPtr _parentHwnd;

        public PreviewHandlerControlBase()
        {
            // Gets the handle of the control to create the control on the VI thread. Invoking the Control.Handle get accessor forces the creation of the underlying window for the control.
            // This is important, because the thread that instantiates the preview handler component and calls its constructor is a single-threaded apartment (STA) thread, but the thread that calls into the interface members later on is a multithreaded apartment (MTA) thread. Windows Forms controls are meant to run on STA threads.
            // More details: https://docs.microsoft.com/en-us/archive/msdn-magazine/2007/january/windows-vista-and-office-writing-your-own-preview-handlers.
            var forceCreation = this.Handle;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Visible = false;
        }

        /// <inheritdoc />
        public void QueryFocus(out IntPtr result)
        {
            var getResult = IntPtr.Zero;
            this.InvokeOnControlThread(() =>
            {
                getResult = GetFocus();
            });
            result = getResult;
        }

        /// <inheritdoc />
        public void SetFocus()
        {
            this.InvokeOnControlThread(() =>
            {
                this.Focus();
            });
        }

        /// <inheritdoc />
        public void SetFont(Font font)
        {
            this.InvokeOnControlThread(() =>
            {
                this.Font = font;
            });
        }

        /// <inheritdoc />
        public void SetTextColor(Color color)
        {
            this.InvokeOnControlThread(() =>
            {
                this.ForeColor = color;
            });
        }

        /// <inheritdoc />
        public void SetBackgroundColor(Color argbColor)
        {
            this.InvokeOnControlThread(() =>
            {
                this.BackColor = argbColor;
            });
        }

        /// <inheritdoc />
        public IntPtr GetHandle()
        {
            return this.Handle;
        }

        /// <inheritdoc />
        public virtual void Unload()
        {
            this.InvokeOnControlThread(() =>
            {
                this.Visible = false;
                foreach (Control c in this.Controls)
                {
                    c.Dispose();
                }

                this.Controls.Clear();
            });

            // Call garbage collection at the time of unloading of Preview. This is to mitigate issue with WebBrowser Control not able to dispose properly.
            // Which is preventing prevhost.exe to exit at the time of closing File explorer.
            // Preview Handlers run in a separate process from PowerToys. This will not affect the performance of other modules.
            // Mitigate the following Github issue: https://github.com/microsoft/PowerToys/issues/1468
            GC.Collect();
        }

        /// <inheritdoc />
        public void SetRect(Rectangle windowBounds)
        {
            this.UpdateWindowBounds(windowBounds);
        }

        /// <inheritdoc />
        public void SetWindow(IntPtr hwnd, Rectangle rect)
        {
            this._parentHwnd = hwnd;
            this.UpdateWindowBounds(rect);
        }

        /// <inheritdoc />
        public virtual void DoPreview<T>(T dataSource)
        {
            this.Visible = true;
        }

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying window handle.
        /// </summary>
        /// <param name="func">Delegate to run.</param>
        public void InvokeOnControlThread(MethodInvoker func)
        {
            this.Invoke(func);
        }

        /// <summary>
        /// Changes the parent window of the specified child window.
        /// </summary>
        /// <param name="hWndChild">A handle to the child window.</param>
        /// <param name="hWndNewParent">A handle to the new parent window.</param>
        /// <returns>If the function succeeds, the return value is a handle to the previous parent window and NULL in case of failure.</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        /// <summary>
        /// Retrieves the handle to the window that has the keyboard focus, if the window is attached to the calling thread's message queue.
        /// </summary>
        /// <returns>The return value is the handle to the window with the keyboard focus. If the calling thread's message queue does not have an associated window with the keyboard focus, the return value is NULL.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetFocus();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// Update the Form Control window with the passed rectangle.
        /// </summary>
        /// <param name="windowBounds">An instance of rectangle.</param>
        private void UpdateWindowBounds(Rectangle windowBounds)
        {
            this.InvokeOnControlThread(() =>
            {
                // We must set the WS_CHILD style to change the form to a control within the Explorer preview pane
                int windowStyle = GetWindowLong(this.Handle, _gwlStyle);
                if ((windowStyle & _wsChild) == 0)
                {
                    SetWindowLong(this.Handle, _gwlStyle, windowStyle | _wsChild);
                }

                SetParent(this.Handle, this._parentHwnd);
                this.Bounds = windowBounds;
            });
        }
    }
}
