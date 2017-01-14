// 
// WindowBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Konrad Kruczyński <kkruczynski@antmicro.com>
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

namespace Xwt.GtkBackend
{
	public class WindowBackend: WindowFrameBackend, IWindowBackend, IConstraintProvider
	{
		Gtk.Alignment alignment;
		Gtk.MenuBar mainMenu;
		Gtk.VBox mainBox;
		
		public override void Initialize ()
		{
            Window = new Gtk.Window ("");
            Window.Add (CreateMainLayout ());
            Window.Realize ();
		}
		
		protected virtual Gtk.Widget CreateMainLayout ()
		{
			mainBox = new Gtk.VBox ();
			mainBox.Show ();
			alignment = new RootWindowAlignment (this);
			mainBox.PackStart (alignment, true, true, 0);
			alignment.Show ();
			return mainBox;
		}
		
		public Gtk.VBox MainBox {
			get { return mainBox; }
		}

		public override void GetMetrics (out Size minSize, out Size decorationSize)
		{
			if (mainMenu != null) {
				var ms = mainMenu.SizeRequest ();
				minSize = new Size (ms.Width, 0);
				decorationSize = new Size (0, ms.Height);
			}
			else {
				minSize = decorationSize = Size.Zero;
			}
		}

		public void SetChild (IWidgetBackend child, bool needAlignment)
		{
#if false
            if (alignment.Child != null) {
                WidgetBackend.RemoveChildPlacement (alignment.Child);
                alignment.Remove (alignment.Child);
            }
#endif
            alignment.Child = WidgetBackend.GetWidgetWithPlacement (child, needAlignment);
		}
		
		public virtual void UpdateChildPlacement (IWidgetBackend childBackend, bool needAlignment)
		{
#if false
            WidgetBackend.SetChildPlacement (childBackend, needAlignment);
#endif
        }

		public void SetMainMenu (IMenuBackend menu)
		{
            if (mainMenu != null)
                mainBox.Remove (mainMenu);

            if (menu != null) {
                MenuBackend m = (MenuBackend)menu;
                mainMenu = m.MenuBar;
                mainBox.PackStart (mainMenu, false, false, 0);
                ((Gtk.Box.BoxChild)mainBox [mainMenu]).Position = 0;
            } else
                mainMenu = null;
		}

		public void SetPadding (double left, double top, double right, double bottom)
		{
			alignment.LeftPadding = (uint) left;
			alignment.RightPadding = (uint) right;
			alignment.TopPadding = (uint) top;
			alignment.BottomPadding = (uint) bottom;
		}

		public void SetMinSize (Size s)
		{
			// Not required
		}

		public override void SetSize (double width, double height)
		{
			base.SetSize (width, height);
			if (alignment.Child != null)
				alignment.Child.QueueResize ();
		}
		
		public void GetConstraints (Gtk.Widget target, out SizeConstraint width, out SizeConstraint height)
		{
			width = RequestedSize.Width;
			height = RequestedSize.Height;
		}

        Color IWindowBackend.BackgroundColor {
            get {
                return Window.Style.Background (Gtk.StateType.Normal).ToXwtValue ();
            }
            set {
                this.Window.ModifyBg (Gtk.StateType.Normal, value.ToGtkValue ());
            }
        }

        IWidgetBackend IWindowBackend.Child {
            get {
                return null;
            }
        }
	}

	class RootWindowAlignment: Gtk.Alignment, IConstraintProvider
	{
		WindowBackend backend;

		public RootWindowAlignment (WindowBackend backend): base (0, 0, 1, 1)
		{
			this.backend = backend;
		}

		public void GetConstraints (Gtk.Widget target, out SizeConstraint width, out SizeConstraint height)
		{
			backend.GetConstraints (this, out width, out height);
		}
	}
}

