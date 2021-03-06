// 
// ButtonBackend.cs
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
using Xwt.Backends;
using Xwt.Drawing;
using System.Diagnostics;
#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using CGRect = System.Drawing.RectangleF;
#else
using AppKit;
using CoreGraphics;
#endif

namespace Xwt.Mac
{
	public class ButtonBackend: ViewBackend<NSButton,IButtonEventSink>, IButtonBackend
	{
		ButtonType currentType;
		ButtonStyle currentStyle;

        MacButton buttonView;

		public ButtonBackend ()
		{
		}

		#region IButtonBackend implementation
		public override void Initialize ()
		{
			buttonView = new MacButton ();
            this.InitializeViewObject (buttonView);

			Widget.SetButtonType (NSButtonType.MomentaryPushIn);
		}

        protected override void InitializeEventInternal ()
        {
            this.RegisterOnClick (delegate (object sender, EventArgs args) {
                this.ApplicationContext.InvokeUserCode (delegate {
                    this.EventSink.OnClicked.Invoke (args);
                });
                buttonView.OnActivatedInternal ();
            });
        }

        public void Initialize (IRadioButtonEventSink eventSink)
        {
            base.Initialize (eventSink);

            this.RegisterOnClick (delegate (object sender, EventArgs args) {
                buttonView.OnActivatedInternal ();
            });

        }

        void RegisterOnClick (EventHandler handler)
        {
            this.EventSink.OnClicked.Enabled = () => {
                buttonView.Activated += handler;
            };
            this.EventSink.OnClicked.Disabled = () => {
                buttonView.Activated -= handler;
            };
        }

#if false
        public void EnableEvent (Xwt.Backends.ButtonEvent ev)
        {
            ((MacButton)Widget).EnableEvent (ev);
        }

        public void DisableEvent (Xwt.Backends.ButtonEvent ev)
        {
            ((MacButton)Widget).DisableEvent (ev);
        }
		#endif

        public ButtonType ButtonType { 
            get {
                return currentType;
            } 
            set {
                this.SetButtonType (value);
            } 
        }

        string IButtonBackend.Text { 
            get { return this.Widget.Title; } 
            set { this.SetContent (value, true, this.ButtonType); } 
        }

#if false
        public void SetContent (string label, bool useMnemonic, ImageDescription image, ContentPosition imagePosition, ButtonType type)
#endif
        public void SetContent (string label, bool useMnemonic, ButtonType type)
		{
            switch (type) {
            case ButtonType.Help:
            case ButtonType.Disclosure:
                return;
            }

			if (useMnemonic)
				label = label.RemoveMnemonic ();
			Widget.Title = label ?? "";


#if false
            if (string.IsNullOrEmpty (label))
                imagePosition = ContentPosition.Center;
            if (!image.IsNull) {
                var img = image.ToNSImage ();
                Widget.Image = (NSImage)img;
                Widget.Cell.ImageScale = NSImageScale.None;
                switch (imagePosition) {
                case ContentPosition.Bottom: Widget.ImagePosition = NSCellImagePosition.ImageBelow; break;
                case ContentPosition.Left: Widget.ImagePosition = NSCellImagePosition.ImageLeft; break;
                case ContentPosition.Right: Widget.ImagePosition = NSCellImagePosition.ImageRight; break;
                case ContentPosition.Top: Widget.ImagePosition = NSCellImagePosition.ImageAbove; break;
                case ContentPosition.Center: Widget.ImagePosition = string.IsNullOrEmpty (label) ? NSCellImagePosition.ImageOnly : NSCellImagePosition.ImageOverlaps; break;
                }
            }
			#endif

			SetButtonStyle (currentStyle);
			ResetFittingSize ();
		}
		
		public virtual void SetButtonStyle (ButtonStyle style)
		{
			currentStyle = style;
			if (currentType == ButtonType.Normal)
			{
				switch (style) {
				case ButtonStyle.Normal:
					if (Widget.Image != null
#if false
                        || Frontend.MinHeight > 0
                        || Frontend.HeightRequest > 0
#endif
						|| Widget.Title.Contains (Environment.NewLine))
						Widget.BezelStyle = NSBezelStyle.RegularSquare;
					else
						Widget.BezelStyle = NSBezelStyle.Rounded;
#if MONOMAC
					Messaging.void_objc_msgSend_bool (Widget.Handle, selSetShowsBorderOnlyWhileMouseInside.Handle, false);
#else
					Widget.ShowsBorderOnlyWhileMouseInside = false;
#endif
					break;
				case ButtonStyle.Borderless:
				case ButtonStyle.Flat:
					Widget.BezelStyle = NSBezelStyle.ShadowlessSquare;
#if MONOMAC
					Messaging.void_objc_msgSend_bool (Widget.Handle, selSetShowsBorderOnlyWhileMouseInside.Handle, true);
#else
					Widget.ShowsBorderOnlyWhileMouseInside = true;
#endif
					break;
				}
			}
		}
		
#if MONOMAC
		protected static Selector selSetShowsBorderOnlyWhileMouseInside = new Selector ("setShowsBorderOnlyWhileMouseInside:");
#endif

		public void SetButtonType (ButtonType type)
		{
			currentType = type;
			switch (type) {
			case ButtonType.Disclosure:
				Widget.BezelStyle = NSBezelStyle.Disclosure;
				Widget.Title = "";
				break;
			case ButtonType.Help:
				Widget.BezelStyle = NSBezelStyle.HelpButton;
				Widget.Title = "";
				break;
			default:
					SetButtonStyle (currentStyle);
				break;
			}
		}
		
		#endregion

		public override Color BackgroundColor {
			get { return ((MacButton)Widget).BackgroundColor; }
			set { ((MacButton)Widget).BackgroundColor = value; }
		}
	}

    class MacButton : NSButton, IViewObject
    {
        //
        // This is necessary since the Activated event for NSControl in AppKit does 
        // not take a list of handlers, instead it supports only one handler.
        //
        // This event is used by the RadioButton backend to implement radio groups
        //
        internal event Action<MacButton> ActivatedInternal;

        public MacButton (IntPtr p) : base (p)
        {
        }

        public MacButton ()
        {
            Cell = new ColoredButtonCell ();
            BezelStyle = NSBezelStyle.Rounded;
        }

        ViewBackend IViewObject.Backend { get; set; }

        NSView IViewObject.View { get { return this; } }

#if false
        public void EnableEvent (ButtonEvent ev)
        {
        }

        public void DisableEvent (ButtonEvent ev)
        {
        }
#endif

        internal void OnActivatedInternal ()
		{
			if (ActivatedInternal == null)
				return;

			ActivatedInternal (this);
		}

		public override void ResetCursorRects ()
		{
			base.ResetCursorRects ();
            this.ResetCursorRectsInternal (this);
		}

        void ResetCursorRectsInternal (IViewObject v)
        {
            if (v.Backend.Cursor != null) {
                AddCursorRect (Bounds, v.Backend.Cursor);
            }
        }

		public Color BackgroundColor {
			get {
				return ((ColoredButtonCell)Cell).Color.GetValueOrDefault ();
			}
			set {
				((ColoredButtonCell)Cell).Color = value;
			}
		}

		class ColoredButtonCell : NSButtonCell
		{
			public Color? Color { get; set; }

			public override void DrawBezelWithFrame (CGRect frame, NSView controlView)
			{
				controlView.DrawWithColorTransform(Color, delegate { base.DrawBezelWithFrame (frame, controlView); });
			}
		}
	}
}

