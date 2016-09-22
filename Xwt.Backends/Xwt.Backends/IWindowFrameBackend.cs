// 
// IWindowFrameBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
#if false
using Xwt.Drawing;
#endif

namespace Xwt.Backends
{
    public interface IWindowFrameBackend : IBackend
    {
        void Initialize (IWindowFrameEventSink eventSink);
        void Dispose ();

        /// <summary>
        /// Gets or sets the name of the window.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Size and position of the window content in screen coordinates
        /// </summary>
        Rectangle Bounds { get; set; }
        void Move (double x, double y);

        /// <summary>
        /// Sets the size of the window
        /// </summary>
        /// <param name='width'>
        /// New width, or -1 if the width doesn't have to be changed
        /// </param>
        /// <param name='height'>
        /// New height, or -1 if the height doesn't have to be changed
        /// </param>
        /// <remarks>
        /// </remarks>
        void SetSize (double width, double height);

        bool Visible { get; set; }
        bool Sensitive { get; set; }
        string Title { get; set; }
        bool Decorated { get; set; }
        bool ShowInTaskbar { get; set; }
        void SetTransientFor (IWindowFrameBackend window);
        bool Resizable { get; set; }
        double Opacity { get; set; }

#if false
        void SetIcon (ImageDescription image);
#endif

        /// <summary>
        /// Presents a window to the user. This may mean raising the window in the stacking order,
        /// deiconifying it, moving it to the current desktop, and/or giving it the keyboard focus
        /// </summary>
        void Present ();

        /// <summary>
        /// Closes the window
        /// </summary>
        /// <returns><c>true</c> if the window could be closed</returns>
        /// <remarks>
        /// Closes the window like if the user clicked on the close window button.
        /// The CloseRequested event is fired and subscribers can cancel the closing,
        /// so there is no guarantee that the window will actually close.
        /// This method doesn't dispose the window. The Dispose method has to be called.
        /// </remarks>
        bool Close ();

        /// <summary>
        /// Gets or sets a value indicating whether this window is in full screen mode
        /// </summary>
        /// <value><c>true</c> if the window is in full screen mode; otherwise, <c>false</c>.</value>
        bool FullScreen { get; set; }

        /// <summary>
        /// Gets the screen on which most of the area of this window is placed
        /// </summary>
        /// <value>The screen.</value>
        object Screen { get; }

        /// <summary>
        /// Gets the reference to the native window.
        /// </summary>
        /// <value>The native window.</value>
        object Window { get; }

        /// <summary>
        /// Gets the system handle of the native Window.
        /// </summary>
        /// <value>The native handle.</value>
        /// <remarks>
        /// The native handle is the platform specific (Cocoa, X, Win32, etc.) window handle,
        /// which is not necessarily the handle of the toolkit window.
        /// </remarks>
        IntPtr NativeHandle { get; }

        IWindowFrameEventSink EventSink { get; }
    }

    public class CloseRequestEventArgs : EventArgs
    {
        public CloseRequestEventArgs (bool accepted) { this.Accepted = accepted; }

        public bool Accepted { get; set; }
    }

	public interface IWindowFrameEventSink
	{
		//void OnBoundsChanged (Rectangle bounds);
        BackendEventHub<Rectangle> OnBoundsChanged { get; }
		//void OnShown ();
        BackendEventHub<EventArgs> OnShown { get; }
        //void OnHidden ();
        BackendEventHub<EventArgs> OnHidden { get; }
        //bool OnCloseRequested ();
        BackendEventHub<CloseRequestEventArgs> OnCloseRequested { get; }
		//void OnClosed ();
        BackendEventHub<EventArgs> OnClosed { get; }
	}

    namespace WindowFrameEventSink 
    {
        public class Default : IWindowFrameEventSink
        {
            private Lazy<BackendEventHub<Rectangle>> onBoundsChangeImpl =
                BackendEventHub.Factory<Rectangle> ();

            private Lazy<BackendEventHub<EventArgs>> onShownImpl =
                BackendEventHub.Factory<EventArgs> ();


            private Lazy<BackendEventHub<EventArgs>> onHiddenImpl =
                BackendEventHub.Factory<EventArgs> ();


            private Lazy<BackendEventHub<CloseRequestEventArgs>> onCloseRequestedImpl =
                BackendEventHub.Factory<CloseRequestEventArgs> ();

            private Lazy<BackendEventHub<EventArgs>> onClosedImpl =
                BackendEventHub.Factory<EventArgs> ();


            public BackendEventHub<Rectangle> OnBoundsChanged 
            {
                get { return this.onBoundsChangeImpl.Value; }
            }

            public BackendEventHub<EventArgs> OnShown 
            {
                get { return this.onShownImpl.Value; }
            }

            public BackendEventHub<EventArgs> OnHidden 
            {
                get { return this.onHiddenImpl.Value; }
            }

            public BackendEventHub<CloseRequestEventArgs> OnCloseRequested {
                get { return this.onCloseRequestedImpl.Value; }
            }

            public BackendEventHub<EventArgs> OnClosed {
                get { return this.onClosedImpl.Value; }
            }
        }
    }

	[Flags]
	public enum WindowFrameEvent
	{
		BoundsChanged = 1,
		Shown = 2,
		Hidden = 4,
		CloseRequested = 8,
		Closed = 16
	}
}

