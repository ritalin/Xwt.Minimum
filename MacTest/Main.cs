﻿using AppKit;
using Xwt.Mac;
using Xwt.Backends;
using System.Threading;
using Xwt;
using Xwt.Drawing;

namespace Project1
{
    static class MainClass
    {
        static void Main (string [] args)
        {
            var engine = new MacEngine ();
            engine.Initialize (false);

            ApplicationContext ctx = new ApplicationContext (engine, Thread.CurrentThread);

            ICanvasBackend container = new CanvasBackend ();
            container.InitializeBackend (null, ctx);
            container.Initialize (null);
            container.BackgroundColor = Colors.Blue;

            IWindowBackend wb = new WindowBackend (container as IViewObject);
            wb.InitializeBackend (null, ctx);

            // Connecting to event sink
            wb.Initialize (new Xwt.Backends.WindowFrameEventSink.Default ());

            wb.BackgroundColor = Colors.Red;
            wb.SetSize (600, 400);

            // TODO: 親の原点と子の矩型領域から、レイアウト制約を決定する

            ICanvasBackend canvas = new CanvasBackend ();
            canvas.InitializeBackend (null, ctx);
            canvas.Initialize (null);
            canvas.BackgroundColor = Colors.Lime;
            canvas.SetBoundsRequest (new Rectangle (16, 16, 568, 368));
            container.AddChild (canvas);

            // --

            ComposeLabel (ctx, canvas);
            PopulateMenu (ctx, wb);

            wb.EventSink.OnShown.Register (e => {
                // As Visible property is true, fired.
                return;
            });
            wb.EventSink.OnBoundsChanged.Register (e => {
                return;
            });

            wb.Visible = true;

            SynchronizationContext.SetSynchronizationContext (new XwtSynchronizationContext (ctx));

            engine.RunApplication ();

            wb.Dispose ();

            engine.Dispose ();
        }

        private static void ComposeLabel(ApplicationContext ctx, IWidgetBackend contaner) {
            ILabelBackend label = new LabelBackend ();
            label.InitializeBackend (null, ctx);
            label.Initialize (new Xwt.Backends.WidgetEventSink.Default());

            label.Text = "Hello minimum xwt";
            label.BackgroundColor = Colors.LightSkyBlue;
            label.SetBoundsRequest (new Rectangle (8, 8, 400, 30));
            contaner.AddChild (label);
        }

        private static void PopulateMenu(ApplicationContext ctx, IWindowBackend wb) {
            var mainMenu = new MenuBackend ();
            mainMenu.InitializeBackend (null, ctx);

            var fileMenu = new MenuItemBackend ();
            fileMenu.InitializeBackend (null, ctx);
            {
                fileMenu.Label = "File";
                {
                    var m = new MenuBackend ();
                    m.InitializeBackend (null, ctx);
                    fileMenu.SetSubmenu (m);
                    {
                        var mi = new MenuItemBackend ();
                        mi.InitializeBackend (null, ctx);
                        mi.Initialize (new Xwt.Backends.MenuItemEventSink.Default());
                        mi.Label = "Close";
                        m.InsertItem (0, mi);
                        mi.EventSink.OnClicked.Register (delegate {
                            ctx.Engine.ExitApplication();
                        });
                    }
                }
            }
            mainMenu.InsertItem (0, fileMenu);

            wb.SetMainMenu (mainMenu);
        }
    }
}
