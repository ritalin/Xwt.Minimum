using System;
using System.Threading;
using Xwt;
using Xwt.Backends;
using Xwt.GtkBackend;
using Xwt.Drawing;
using Gtk;

namespace GtkTest
{
    class MainClass
    {
        public static void Main (string [] args)
        {
            using (var engine = new GtkEngine ()) {
                engine.Initialize (false);

                ApplicationContext ctx = new ApplicationContext (engine, Thread.CurrentThread);

                using (IWindowBackend wb = new WindowBackend ()) {
                    wb.InitializeBackend (null, ctx);

                    // Connecting to event sink
                    wb.Initialize (new Xwt.Backends.WindowFrameEventSink.Default ());

                    wb.BackgroundColor = Colors.Red;
                    // ウインドウサイズだけは制約ではなく直接指定する必要がある
                    wb.SetSize (600, 400);

                    wb.Visible = true;

                    SynchronizationContext.SetSynchronizationContext (new XwtSynchronizationContext (ctx));

                    engine.RunApplication ();

                    //MainWindow win = new MainWindow ();
                    //win.Show ();
                    //Application.Run ();
                }
            }
        }
    }
}
