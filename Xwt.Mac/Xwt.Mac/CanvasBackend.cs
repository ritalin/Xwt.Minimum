// 
// CanvasBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Hywel Thomas <hywel.w.thomas@gmail.com>
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
using System.Diagnostics;
using Xwt.Backends;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.CoreGraphics;
using MonoMac.AppKit;
using CGRect = System.Drawing.RectangleF;
using CGSize = System.Drawing.SizeF;
#else
using CoreGraphics;
using AppKit;
#endif

namespace Xwt.Mac
{
    public class CanvasBackend : ViewBackend<NSView, ICanvasEventSink>, ICanvasBackend
    {
        CanvasView viewObject;

        public CanvasBackend ()
        {
        }

        public override void Initialize ()
        {
            viewObject = new CanvasView (EventSink, ApplicationContext);
            this.InitializeViewObject (viewObject);
        }

        public override void LoadView ()
        {
            this.View = viewObject;
        }

        protected override void OnSizeToFit ()
        {
#if false
            var s = EventSink.GetPreferredSize ();
            Widget.SetFrameSize (new CGSize ((nfloat)s.Width, (nfloat)s.Height));
#endif
        }

        public Rectangle Bounds {
            get {
                return new Rectangle (0, 0, this.View.Frame.Width, this.View.Frame.Height);
            }
        }

        public void QueueDraw ()
        {
            this.Widget.NeedsDisplay = true;
        }

        public void QueueDraw (Rectangle rect)
        {
            this.Widget.SetNeedsDisplayInRect (new CGRect ((nfloat)rect.X, (nfloat)rect.Y, (nfloat)rect.Width, (nfloat)rect.Height));
        }

#if false
        public void AddChild (IWidgetBackend widget, Rectangle rect)
#endif
#if false
        public void AddChild (IWidgetBackend child)
        {
            var v = child as IViewObject;
            Debug.Assert (v != null);
            var v = GetWidget (widget);

            this.Widget.AddSubview (v.View);

            // Not using SetWidgetBounds because the view is flipped

            v.View.Frame = new CGRect ((nfloat)rect.X, (nfloat)rect.Y, (nfloat)rect.Width, (nfloat)rect.Height); ;
            v.View.NeedsDisplay = true;
		}
#endif

        public void RemoveChild (IWidgetBackend widget)
        {
            var v = widget as IViewObject;
            Debug.Assert (v != null);
#if false
            var v = GetWidget (widget);
#endif
            v.View.RemoveFromSuperview ();
        }

        public void SetChildBounds (IWidgetBackend widget, Rectangle rect)
        {
            var v = widget as IViewObject;
            Debug.Assert (v != null);
#if false
            var w = GetWidget (widget);
#endif

            // Not using SetWidgetBounds because the view is flipped
            v.View.Frame = new CGRect ((nfloat)rect.X, (nfloat)rect.Y, (nfloat)rect.Width, (nfloat)rect.Height); ;
            v.View.NeedsDisplay = true;
        }

        public override void ReallocateInternal (IViewObject targetView)
        {
            // TODO: 共通化を検討する
            base.ReallocateInternal (targetView);
        }
    }
	
	class CanvasView: WidgetView
	{
		ICanvasEventSink eventSink;
		
		public CanvasView (ICanvasEventSink eventSink, ApplicationContext context): base (eventSink, context)
		{
			this.eventSink = eventSink;
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			context.InvokeUserCode (delegate {
				CGContext ctx = NSGraphicsContext.CurrentContext.GraphicsPort;

				//fill BackgroundColor
                ctx.SetFillColor (this.Backend.BackgroundColor.ToCGColor ());
				ctx.FillRect (Bounds);

				var backend = new CGContextBackend {
					Context = ctx,
					InverseViewTransform = ctx.GetCTM ().Invert ()
				};
				eventSink.OnDraw (backend, new Rectangle (dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height));
			});
		}
	}
}

