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

                    //wb.BackgroundColor = Colors.Red;

                    wb.SetSize (600, 400);

                    IWidgetContainerBackend container = new WidgetContainerBackend ();
                    container.InitializeBackend (null, ctx);
                    container.Initialize (new Xwt.Backends.CanvasEventSink.Default ());
                    container.BackgroundColor = Colors.Blue;
                    container.BoundsRequest = new Rectangle (0, 0, 600, 400); // ContentViewの制約は有効にならない

                    wb.SetChild (container, false);

                    IWidgetContainerBackend box = new WidgetContainerBackend ();
                    box.InitializeBackend (null, ctx);
                    box.Initialize (new Xwt.Backends.WidgetEventSink.Default ());
                    box.BackgroundColor = Colors.Lime;
                    box.BoundsRequest = (new Rectangle (16, 16, 568, 368));
                    container.AddChild (box);

                    // --

                    ComposeItems (ctx, box);
                    PopulateMenu (ctx, wb);

                    wb.EventSink.OnShown.Register (e => {
                        // As Visible property is true, fired.
                        return;
                    });
                    wb.EventSink.OnHidden.Register (e => {
                        // As Visible property is true, fired.
                        return;
                    });
                    wb.EventSink.OnCloseRequested.Register (e => {
                        // As Visible property is true, fired.
                        return;
                    });
                    wb.EventSink.OnClosed.Register (e => {
                        // As Visible property is true, fired.
                        return;
                    });
                    wb.EventSink.OnBoundsChanged.Register (e => {
                        return;
                    });

                    wb.Visible = true;

                    SynchronizationContext.SetSynchronizationContext (new XwtSynchronizationContext (ctx));

                    engine.RunApplication ();
                }
            }
        }

        private static int countedValue;

        private static void ComposeItems (ApplicationContext ctx, IWidgetContainerBackend contaner)
        {
            ILabelBackend label = new LabelBackend ();
            label.InitializeBackend (null, ctx);
            label.Initialize (new Xwt.Backends.WidgetEventSink.Default ());

            label.Text = "Hello minimum xwt";
            label.BackgroundColor = Colors.LightSkyBlue;
            label.BoundsRequest = (new Rectangle (8, 8, 120, 30)); // TODO: AutoResizing
            contaner.AddChild (label);

            ILabelBackend countLabel = new LabelBackend ();
            countLabel.InitializeBackend (null, ctx);
            countLabel.Initialize (new Xwt.Backends.WidgetEventSink.Default ());
            countLabel.Text = "-";
            countLabel.BackgroundColor = Colors.LightSkyBlue;
            countLabel.BoundsRequest = (new Rectangle (8, 42, 120, 30)); // TODO: AutoResizing
            contaner.AddChild (countLabel);

            countedValue = 0;

            // Upボタン
            IButtonBackend up = new ButtonBackend ();
            up.InitializeBackend (null, ctx);
            up.Initialize (new Xwt.Backends.ButtonEventSink.Default ());

            up.Text = "Count up";
            up.BoundsRequest = (new Rectangle (8, 320, 120, 30)); // TODO: AutoResizing
            up.EventSink.OnClicked.Register (
                (obj) => {
                    ++countedValue;
                    countLabel.Text = countedValue.ToString ();
                }
            );
            contaner.AddChild (up);

            // Resetボタン
            IButtonBackend reset = new ButtonBackend ();
            reset.InitializeBackend (null, ctx);
            reset.Initialize (new Xwt.Backends.ButtonEventSink.Default ());

            reset.Text = "Reset";
            reset.BoundsRequest = (new Rectangle (132, 320, 120, 30)); // TODO: AutoResizing
            reset.EventSink.OnClicked.Register (
                (obj) => {
                    countedValue = 0;
                    countLabel.Text = "-";
                }
            );
            contaner.AddChild (reset);
        }

        private static void PopulateMenu (ApplicationContext ctx, IWindowBackend wb)
        {
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
                        mi.Initialize (new Xwt.Backends.MenuItemEventSink.Default ());
                        mi.Label = "Close";
                        m.InsertItem (0, mi);
                        mi.EventSink.OnClicked.Register (delegate {
                            ctx.Engine.ExitApplication ();
                        });
                    }
                }
            }
            mainMenu.InsertItem (0, fileMenu);

            wb.SetMainMenu (mainMenu);
        }
    }
}
