// 
// BoxBackend.cs
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
using System.Linq;
using Xwt.Backends;
using System.Collections.Generic;
using System.Diagnostics;
using Xwt;
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
    using Gtk = global::Gtk;

    public class WidgetContainerBackend : WidgetBackend, IWidgetContainerBackend
	{
        IMovableContainer container;
        Dictionary<Gtk.Widget, IWidgetBackend> childrenLookup = new Dictionary<Gtk.Widget, IWidgetBackend> ();

        public WidgetContainerBackend (): this(new MovableContainerWrapper(new Gtk.Fixed())) 
        { 
        }

        protected WidgetContainerBackend (Gtk.Container c): this(new ManagedContainerWrapper(c))
        {
		}

        WidgetContainerBackend (IMovableContainer c)
        {
            container = c;
            container.Widget.Show ();
            base.Widget = container.Widget;

            Widget.Show ();
        }

  //      CustomContainer Container {
		//	get { return (CustomContainer)base.Widget; }
		//	set { base.Widget = value; }
		//}
		
		public void AddChild (IWidgetBackend widget)
		{

            if (widget is ILabelBackend) {
                //widget.Visible = true;
            }

            var w = GetWidget (widget);
            //this.Container.AddChild (GetWidget (widget), widget);
            container.AddChild (w, widget.BoundsRequest);

            childrenLookup.Add (w, widget);
        }

		public void RemoveChild (IWidgetBackend widget)
		{
            var w = GetWidget (widget);
            //this.Container.RemoveChild (GetWidget (widget), widget);
            container.Widget.Remove (GetWidget (widget));

            childrenLookup.Remove (w);
		}

        public IEnumerable<IWidgetBackend> Children {
            get {
                return container.Children.Select (w => childrenLookup [w]);
            }
        }

#if false
        public void SetAllocation (IWidgetBackend [] widgets, Rectangle [] rects)
        {
            bool changed = false;
            for (int n = 0; n < widgets.Length; n++) {
                var w = GetWidget (widgets [n]);
                if (Widget.SetAllocation (w, rects [n]))
                    changed = true;
            }
            if (changed)
                Widget.QueueResizeIfRequired ();
        }
		#endif
	}
	
    partial class CustomContainer: Gtk.Bin, IGtkContainer
	{
#if false
        public BoxBackend Backend;
#endif
        public bool IsReallocating;

        IMovableContainer realContainer;
        Dictionary<Gtk.Widget, WidgetData> childrenLookup = new Dictionary<Gtk.Widget, WidgetData> ();

        struct WidgetData
		{
			public Rectangle Rect;
#if false
            public Widget Widget;
#endif
            public IWidgetBackend Backend;
        }
		
		public CustomContainer (IMovableContainer container)
        {
            Debug.Assert (container != null);

            realContainer = container;
            this.Child = container.Widget;
            this.Child.SizeAllocated += HandleSizeAllocated;
            this.Child.Show();

            container.Widget.FixContainerLeak ();
            container.Widget.SetHasWindow (false);
		}
		
        public IEnumerable<IWidgetBackend> GetChildren()
        {
            return realContainer.Widget.Children
                                .Select (w => childrenLookup [w].Backend);
        }

        public void AddChild(Gtk.Widget w, IWidgetBackend childBackend)
        {
            var ww = new Gtk.Fixed ();
            ((Gtk.Fixed)realContainer.Widget).Put (ww, 32, 24);
            ww.Show ();
            ww.SetSizeRequest (50, 50);
            ww.ModifyBg (Gtk.StateType.Normal, Colors.Green.ToGtkValue ());

            ww.Realize ();
            var sty = ww.Style;
            var bg = sty.Backgrounds;
            var sz = ww.SizeRequest ();

            var ch = ((Gtk.Fixed)realContainer.Widget) [ww];
            ww.Parent.Parent.Parent.Parent.Parent.ShowAll ();

            //ww.Parent.Realize ();
            var p_bg = ww.Parent.Style.Backgrounds;
            var p_sz = ww.Parent.SizeRequest ();

            ww.Parent.Parent.SetSizeRequest (100, 100);
            //ww.Parent.Parent.Realize ();
            var pp_bg = ww.Parent.Parent.Style.Backgrounds;
            var pp_sz = ww.Parent.Parent.SizeRequest ();

            ww.Parent.Parent.Parent.SetSizeRequest (pp_sz.Width, pp_sz.Height);
            //ww.Parent.Parent.Parent.Realize ();
            var ppp_bg = ww.Parent.Parent.Parent.Style.Backgrounds;
            var ppp_sz = ww.Parent.Parent.Parent.SizeRequest ();
            //realContainer.AddChild (w, childBackend.BoundsRequest);
            //childrenLookup.Add (w, new WidgetData { Backend = childBackend });
            //w.Parent = realContainer.Widget;

            //ww.Parent.Parent.Parent.Parent.Realize ();
            var pppp_sz = ww.Parent.Parent.Parent.Parent.SizeRequest ();

            var ppppp_sz = ww.Parent.Parent.Parent.Parent.Parent.SizeRequest ();

        }

        public void RemoveChild (Gtk.Widget w, IWidgetBackend widget)
        {
            if (w.Parent != realContainer) {
                throw new InvalidOperationException ("Widget is not a child of this container");
            }

            childrenLookup.Remove (w);
            realContainer.Widget.Remove (w);
            w.Unparent ();
            realContainer.Widget.QueueResize ();
        }

        public void ReplaceChild (Gtk.Widget oldWidget, Gtk.Widget newWidget)
		{
			var r = childrenLookup [oldWidget];
            realContainer.Widget.Remove (oldWidget);
            realContainer.Widget.Add (newWidget);
			childrenLookup [newWidget] = r;
		}

        public bool SetAllocation (Gtk.Widget w, Rectangle rect)
        {
            WidgetData r;
            childrenLookup.TryGetValue (w, out r);
            if (r.Rect != rect) {
                r.Rect = rect;
                childrenLookup [w] = r;
                return true;
            } else
                return false;
        }

        public void RearrangeChild(Gtk.Widget w, Rectangle bounds)
        {
            realContainer.RearrangeChild (w, bounds);     
        }

		protected void OnReallocate ()
		{
#if false
            ((IWidgetSurface)Backend.Frontend).Reallocate ();
#endif
            throw new NotSupportedException ("This is unsupported feature temporally.");
        }

		protected Gtk.Requisition OnGetRequisition (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
#if false
            var size = Backend.Frontend.Surface.GetPreferredSize (widthConstraint, heightConstraint, true);
            return size.ToGtkRequisition ();
#endif
            throw new NotSupportedException ("This is unsupported feature temporally.");
        }

		void HandleSizeAllocated (object o, Gtk.SizeAllocatedArgs args)
		{
			try {
				IsReallocating = true;
				OnReallocate ();
			} catch {
				IsReallocating = false;
			}
			foreach (var cr in childrenLookup) {
				var r = cr.Value.Rect;
				cr.Key.SizeAllocate (new Gdk.Rectangle (args.Allocation.X + (int)r.X, args.Allocation.Y + (int)r.Y, (int)r.Width, (int)r.Height));
			}
		}
		
		protected override void ForAll (bool includeInternals, Gtk.Callback callback)
		{
			base.ForAll (includeInternals, callback);
#if false
            foreach (var c in childrenLookup.Keys.ToArray ())
                callback (c);
			#endif
		}
	}

    interface IMovableContainer {
        Gtk.Container Widget { get; }
        Gtk.Widget [] Children { get; }
        void AddChild (Gtk.Widget w, Rectangle bounds);
        void RearrangeChild (Gtk.Widget w, Rectangle bounds);
    }

    class MovableContainerWrapper: IMovableContainer 
    {
        Gtk.Fixed container;

        public MovableContainerWrapper(Gtk.Fixed c)
        {
            container = c;
        }

        Gtk.Container IMovableContainer.Widget { get { return container; } }

        Gtk.Widget [] IMovableContainer.Children { get { return container.Children; } }

        void IMovableContainer.AddChild (Gtk.Widget w, Rectangle bounds)
        {
            container.Put (w, (int)bounds.X, (int)bounds.Y);

            //w.WidthRequest = (int)bounds.Width;
            //w.HeightRequest = (int)bounds.Height;

            //var size = w.SizeRequest ();

            //var descent = ((Gtk.Container)w).Children [0];
            //descent.WidthRequest = w.WidthRequest;
            //descent.HeightRequest = w.HeightRequest;

            //var size2 = descent.SizeRequest ();

            //var descent2 = ((Gtk.Container)descent).Children [0];
            //descent2.WidthRequest = w.WidthRequest;
            //descent2.HeightRequest = w.HeightRequest;

            //var size3 = descent2.SizeRequest ();


        }

        void IMovableContainer.RearrangeChild (Gtk.Widget w, Rectangle bounds)
        {
            container.Move (w, (int)bounds.X, (int)bounds.Y);

            w.WidthRequest = (int)bounds.Width;
            w.HeightRequest = (int)bounds.Height;

            var descent = ((Gtk.Container)w).Children[0];
            descent.WidthRequest = w.WidthRequest;
            descent.HeightRequest = w.HeightRequest;

        }
    }

    class ManagedContainerWrapper : IMovableContainer
    {
        Gtk.Container container;

        public ManagedContainerWrapper (Gtk.Container c)
        {
            container = c;
        }

        Gtk.Container IMovableContainer.Widget { get { return container; } }
        Gtk.Widget [] IMovableContainer.Children { get { return container.Children; } }

        void IMovableContainer.AddChild (Gtk.Widget w, Rectangle bounds)
        {
            container.Add (w);
        }

        void IMovableContainer.RearrangeChild (Gtk.Widget w, Rectangle bounds)
        {
        }
    }
}

