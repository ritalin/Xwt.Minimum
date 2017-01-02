// 
// WindowBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Andres G. Aragoneses <andres.aragoneses@7digital.com>
// 
// Copyright (c) 2011 Xamarin Inc
// Copyright (c) 2012 7Digital Media Ltd
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
using System.Collections.Generic;
using System.Diagnostics;
using Xwt.Backends;
using Xwt.Drawing;

#if false
using System.Drawing;
#endif

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using CGRect = System.Drawing.RectangleF;
#else
using Foundation;
using AppKit;
using ObjCRuntime;
using CoreGraphics;
#endif

namespace Xwt.Mac
{
    public class WindowBackend: NSWindow, IWindowBackend
	{
		WindowBackendController controller;
		IWindowFrameEventSink eventSink;
        IViewObject realContainer;
#if DEPLECATED
        NSView childView;
#endif
        bool sensitive = true;

        AppDelegate app;

		public WindowBackend (IntPtr ptr): base (ptr)
		{
		}
		
		public WindowBackend (IViewObject container)
		{
            Debug.Assert (container != null);

            this.controller = new WindowBackendController ();
			this.controller.Window = this;
            this.controller.ContentViewController = container.Backend;

			this.StyleMask |= NSWindowStyle.Resizable | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable;
			AutorecalculatesKeyViewLoop = true;

			//ContentView.AutoresizesSubviews = true;
			ContentView.Hidden = true;

            this.realContainer = container;

            //ContentView.AddSubview (this.realContainer.View);


            //this.ContentView.AddConstraint(this.NewEdgeConstraint (NSLayoutAttribute.Width, this.ContentView, v.View, 50f));
            //this.ContentView.AddConstraint (this.NewEdgeConstraint (NSLayoutAttribute.Top, this.ContentView, v.View, 10f));

            //this.ContentView.AddConstraint (this.NewEdgeConstraint (NSLayoutAttribute.Leading, this.ContentView, v.View, 20f));
            //this.ContentView.AddConstraint(this.NewEdgeConstraint (NSLayoutAttribute.Top, this.ContentView, v.View, 0f));
            //this.ContentView.AddConstraint (this.NewEdgeConstraint (NSLayoutAttribute.Trailing, this.ContentView, v.View, -20f));
            //this.ContentView.AddConstraint(this.NewEdgeConstraint (NSLayoutAttribute.Bottom, this.ContentView, v.View, 0f));

            // TODO: do it only if mouse move events are enabled in a widget
            AcceptsMouseMovedEvents = true;

			this.Center ();
		}

		object IWindowFrameBackend.Window 
        {
			get { return this; }
		}

		public IntPtr NativeHandle 
        {
			get { return Handle; }
		}

		public IWindowFrameEventSink EventSink 
        { 
			get { return eventSink; }
		}

		public virtual void InitializeBackend (object frontend, ApplicationContext context)
		{
			this.ApplicationContext = context;
            this.app = ((MacEngine)context.Engine).App;
		}

        private void RegisterWillClose(EventHandler handler) 
        {
            this.eventSink.OnClosed.Enabled = () => {
                this.WillClose += handler;
            };
            this.eventSink.OnClosed.Disabled = () => {
                this.WillClose -= handler;
            };
        }

        public void Initialize (IWindowFrameEventSink eventSink)
        {
            Debug.Assert (eventSink != null);

            this.eventSink = eventSink;

            this.eventSink.OnBoundsChanged.Enabled = () => {
                this.DidResize += this.HandleDidResize;
#if MONOMAC
                DidMoved += HandleDidResize;
#else
                this.DidMove += this.HandleDidResize;
#endif
            };
            this.eventSink.OnBoundsChanged.Disabled = () => {
                this.DidResize -= this.HandleDidResize;
#if MONOMAC
                DidMoved -= HandleDidResize;
#else
                this.DidMove -= this.HandleDidResize;
#endif
            };

            this.eventSink.OnShown.Enabled = () => {
                this.EnableVisibilityEvent ();
            };
            this.eventSink.OnShown.Disabled = () => {
                this.DisableVisibilityEvent ();
            };

            this.eventSink.OnHidden.Enabled = () => {
                this.WillClose += this.OnWillClose;
                this.EnableVisibilityEvent ();
            };
            this.eventSink.OnHidden.Disabled = () => {
                this.DisableVisibilityEvent ();
                this.WillClose -= this.OnWillClose;
            };

            this.eventSink.OnCloseRequested.Enabled = () => {
                this.WindowShouldClose = this.OnShouldClose;
            };
            this.eventSink.OnCloseRequested.Enabled = () => {
                this.WindowShouldClose = null;
            };

            this.RegisterWillClose (delegate (object sender, EventArgs args) {
                this.OnClosed (args);
            });
        }
		
		public ApplicationContext ApplicationContext {
			get;
			private set;
		}
		
		public object NativeWidget {
			get {
				return this;
			}
		}

		public string Name { get; set; }
		
		internal void InternalShow ()
		{
			MakeKeyAndOrderFront (this.app);
		}
		
		public void Present ()
		{
			MakeKeyAndOrderFront (this.app);
		}

		public bool Visible {
			get {
				return !ContentView.Hidden;
			}
			set {
				if (value)
					this.app.ShowWindow (this);
				ContentView.Hidden = !value;
			}
		}

		public double Opacity {
			get { return AlphaValue; }
			set { AlphaValue = (float)value; }
		}

		public bool Sensitive {
			get {
				return sensitive;
			}
			set {
				sensitive = value;
#if false
                if (child != null)
                    child.UpdateSensitiveStatus (child.Widget, sensitive);
				#endif
			}
		}
		
        Color IWindowFrameBackend.BackgroundColor { 
            get {
                return this.BackgroundColor.ToXwtColor ();
            } 
            set {
                this.BackgroundColor = value.ToNSColor ();
            }
        }

		public virtual bool CanGetFocus {
			get { return true; }
		}
		
		public virtual bool HasFocus {
			get { return false; }
		}
		
		public void SetFocus ()
		{
		}

		public bool FullScreen {
			get {
				if (MacSystemInformation.OsVersion < MacSystemInformation.Lion)
					return false;

				return (StyleMask & NSWindowStyle.FullScreenWindow) != 0;

			}
			set {
				if (MacSystemInformation.OsVersion < MacSystemInformation.Lion)
					return;

				if (value != ((StyleMask & NSWindowStyle.FullScreenWindow) != 0))
					ToggleFullScreen (null);
			}
		}

		object IWindowFrameBackend.Screen {
			get {
				return Screen;
			}
		}

		#region IWindowBackend implementation
		void IBackend.EnableEvent (object eventId)
		{
#if DEPLECATED
			if (eventId is WindowFrameEvent) {
				var @event = (WindowFrameEvent)eventId;
				switch (@event) {
				case WindowFrameEvent.BoundsChanged:
					DidResize += HandleDidResize;
#if MONOMAC
					DidMoved += HandleDidResize;
#else
					DidMove += HandleDidResize;
#endif
                    break;


                case WindowFrameEvent.Hidden:
                    EnableVisibilityEvent (@event);
                    this.WillClose += OnWillClose;
                    break;
                case WindowFrameEvent.Shown:
                    EnableVisibilityEvent (@event);
                    break;
                case WindowFrameEvent.CloseRequested:
                    WindowShouldClose = OnShouldClose;
                    break;
            }
        }
#endif
        }
		
		void OnWillClose (object sender, EventArgs args) {
			OnHidden (args);
		}

		bool OnShouldClose (NSObject ob)
		{
			return closePerformed = RequestClose ();
		}

		internal bool RequestClose ()
		{
            var args = new CloseRequestEventArgs (true);
            ApplicationContext.InvokeUserCode (() => {
                eventSink.OnCloseRequested.Invoke (args);
            });

            return args.Accepted;
		}

		protected virtual void OnClosed (EventArgs args)
		{
            if (!disposing) {
                ApplicationContext.InvokeUserCode (() => {
                    eventSink.OnClosed.Invoke (args);
                });
            }
		}

		bool closePerformed;

		bool IWindowFrameBackend.Close ()
		{
			closePerformed = true;
			PerformClose (this);
			return closePerformed;
		}
		
		bool VisibilityEventsEnabled ()
		{
            return this.eventSink.OnShown.Connected || this.eventSink.OnHidden.Connected;
		}

		NSString HiddenProperty {
			get { return new NSString ("hidden"); }
		}
		
		void EnableVisibilityEvent ()
		{
			if (!VisibilityEventsEnabled ()) {
				ContentView.AddObserver (this, this.HiddenProperty, NSKeyValueObservingOptions.New, IntPtr.Zero);
			}
		}

		public override void ObserveValue (NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			if (keyPath.ToString () == HiddenProperty.ToString () && ofObject.Equals (ContentView)) {
				if (ContentView.Hidden) {
                    if (this.eventSink.OnHidden.Connected) {
                        this.OnHidden (EventArgs.Empty);
					}
				} else {
                    if (this.eventSink.OnShown.Connected) {
                        this.OnShown (EventArgs.Empty);
					}
				}
			}
		}

		void OnHidden (EventArgs args) {
			ApplicationContext.InvokeUserCode (delegate ()
			{
                eventSink.OnHidden.Invoke (args);
			});
		}

		void OnShown (EventArgs args) {
			ApplicationContext.InvokeUserCode (delegate ()
			{
                eventSink.OnShown.Invoke (args);
			});
		}

		void DisableVisibilityEvent ()
		{
            if (!VisibilityEventsEnabled ()) {
                ContentView.RemoveObserver (this, this.HiddenProperty);
            }
		}

		void IBackend.DisableEvent (object eventId)
		{
#if DEPLECATED
			if (eventId is WindowFrameEvent) {
				var @event = (WindowFrameEvent)eventId;
				switch (@event) {
					case WindowFrameEvent.BoundsChanged:
						DidResize -= HandleDidResize;
#if MONOMAC
					DidMoved -= HandleDidResize;
#else
					DidMove -= HandleDidResize;
#endif
						break;
                case WindowFrameEvent.Hidden:
                    this.WillClose -= OnWillClose;
                    DisableVisibilityEvent (@event);
                    break;
                case WindowFrameEvent.Shown:
                    DisableVisibilityEvent (@event);
                    break;
        }
			}
#endif
        }

		void HandleDidResize (object sender, EventArgs e)
		{
			OnBoundsChanged (((IWindowBackend)this).Bounds);
		}

		protected virtual void OnBoundsChanged (Rectangle bounds)
		{
			LayoutWindow ();
			ApplicationContext.InvokeUserCode (delegate {
                eventSink.OnBoundsChanged.Invoke (bounds);
			});
		}

        private bool TryGetChiledView (out NSView outView)
        {
            if (this.ContentView.Subviews.Length == 0) {
                outView = null;
                return false;
            } else {
                outView = this.ContentView.Subviews [0];
                return true;
            }
        }

		void IWindowBackend.SetChild (IWidgetBackend child)
		{
            var v = child as IViewObject;
            Debug.Assert (child != null);
            this.realContainer= v;

            this.LayoutWindow ();
            //if (child != null) {
            //    var obj = child as IViewObject;
            //    Debug.Assert (obj != null);

            //    this.ContentView.AddSubview (obj.View);
            //    this.LayoutWindow ();
            //    obj.View.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
            //}


#if DEPLECATED
            if (child != null) {
                ViewBackend.RemoveChildPlacement (this.child.Widget);
                child.Widget.RemoveFromSuperview ();
                this.childView = null;
            }
            this.child = (ViewBackend)child;
            if (child != null) {
                childView = ViewBackend.GetWidgetWithPlacement (child);
                ContentView.AddSubview (childView);
                LayoutWindow ();
                childView.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
            }
#endif


        }
		
		public virtual void UpdateChildPlacement (IWidgetBackend childBackend)
		{
            #if false
            var w = ViewBackend.SetChildPlacement (childBackend);
            LayoutWindow ();
            w.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
			#endif
		}

		bool IWindowFrameBackend.Decorated {
			get {
				return (StyleMask & NSWindowStyle.Titled) != 0;
			}
			set {
				if (value)
					StyleMask |= NSWindowStyle.Titled;
				else
					StyleMask &= ~(NSWindowStyle.Titled | NSWindowStyle.Borderless);
			}
		}
		
		bool IWindowFrameBackend.ShowInTaskbar {
			get {
				return false;
			}
			set {
			}
		}

		void IWindowFrameBackend.SetTransientFor (IWindowFrameBackend window)
		{
			// Generally, TransientFor is used to implement dialog, we reproduce the assumption here
			Level = window == null ? NSWindowLevel.Normal : NSWindowLevel.ModalPanel;
		}

		bool IWindowFrameBackend.Resizable {
			get {
				return (StyleMask & NSWindowStyle.Resizable) != 0;
			}
			set {
				if (value)
					StyleMask |= NSWindowStyle.Resizable;
				else
					StyleMask &= ~NSWindowStyle.Resizable;
			}
		}
		
		public void SetPadding (double left, double top, double right, double bottom)
		{
            this.LayoutContent (this.ContentView.Frame, new WidgetSpacing(left, top, right, bottom));
		}

		void IWindowFrameBackend.Move (double x, double y)
		{
			var r = FrameRectFor (new CGRect ((nfloat)x, (nfloat)y, Frame.Width, Frame.Height));
			SetFrame (r, true);
		}
		
		void IWindowFrameBackend.SetSize (double width, double height)
		{
			var cr = ContentRectFor (Frame);
			if (width == -1)
				width = cr.Width;
			if (height == -1)
				height = cr.Height;
			var r = FrameRectFor (new CGRect ((nfloat)cr.X, (nfloat)cr.Y, (nfloat)width, (nfloat)height));
			this.SetFrame (r, true);

            var v = ((IViewObject)realContainer);
            //v.View.SetFrameSize (r.Size);
         
            LayoutWindow ();
		}
		
		Rectangle IWindowFrameBackend.Bounds {
			get {
				var b = ContentRectFor (Frame);
				var r = MacDesktopBackend.ToDesktopRect (b);
				return new Rectangle ((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
			}
			set {
				var r = MacDesktopBackend.FromDesktopRect (value);
				var fr = FrameRectFor (r);
				SetFrame (fr, true);
			}
		}

        public void SetMainMenu (IMenuBackend menu)
        {
            var m = (MenuBackend)menu;
            m.SetMainMenuMode ();
            NSApplication.SharedApplication.Menu = m;

//			base.Menu = m;
        }
		
		#endregion

		static Selector closeSel = new Selector ("close");

		bool disposing;

		void IWindowFrameBackend.Dispose ()
		{
			disposing = true;
			try {
				Messaging.void_objc_msgSend (this.Handle, closeSel.Handle);
			} finally {
				disposing = false;
			}
		}

        #if false
        public void DragStart (TransferDataSource data, DragDropAction dragAction, object dragImage, double xhot, double yhot)
        {
            throw new NotImplementedException ();
        }

        public void SetDragSource (string [] types, DragDropAction dragAction)
        {
        }

        public void SetDragTarget (string [] types, DragDropAction dragAction)
        {
        }
		#endif
		
		public virtual void SetMinSize (Size s)
		{
			var b = ((IWindowBackend)this).Bounds;
			if (b.Size.Width < s.Width)
				b.Width = s.Width;
			if (b.Size.Height < s.Height)
				b.Height = s.Height;

			if (b != ((IWindowBackend)this).Bounds)
				((IWindowBackend)this).Bounds = b;

			var r = FrameRectFor (new CGRect (0, 0, (nfloat)s.Width, (nfloat)s.Height));
			MinSize = r.Size;
		}

        #if false
        public void SetIcon (ImageDescription icon)
        {
        }
		#endif
		
		public virtual void GetMetrics (out Size minSize, out Size decorationSize)
		{
			minSize = decorationSize = Size.Zero;
		}

		public virtual void LayoutWindow ()
		{
			LayoutContent (ContentView.Frame, 0);
		}
		
        public void LayoutContent (CGRect frame, WidgetSpacing padding)
		{

            NSView subView;
            //if (this.TryGetChiledView(out subView)) 
            {
                frame.X += (nfloat)padding.Left;
                frame.Width -= (nfloat)(padding.HorizontalSpacing);
                frame.Y += (nfloat)padding.Top;
                frame.Height -= (nfloat)(padding.VerticalSpacing);

                //(subView as IWidgetBackend)?.Reallocate (frame.ToXwtRect());
#if DEPLECATED
                childView.Frame = frame;
#endif
            }
		}
	}
	
	public partial class WindowBackendController : NSWindowController
	{
		public WindowBackendController ()
		{
		}
	}
}

