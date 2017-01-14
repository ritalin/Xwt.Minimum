using System;
using Xwt.Backends;

namespace Xwt.GtkBackend
{
    public class BoxBackend : WidgetContainerBackend, IBoxBackend
    {
        public BoxBackend(): base(new Gtk.HBox()) {}
    }
}
