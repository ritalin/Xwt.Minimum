using System;
using Gtk;

namespace Xwt.GtkBackend
{
    public interface IConstraintProvider
    {
        void GetConstraints (Gtk.Widget target, out SizeConstraint width, out SizeConstraint height);
    }
}

