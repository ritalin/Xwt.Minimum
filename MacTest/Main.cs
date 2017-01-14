using AppKit;
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
                     
            IWindowBackend wb = new WindowBackend ();
            wb.InitializeBackend (null, ctx);

            // Connecting to event sink
            wb.Initialize (new Xwt.Backends.WindowFrameEventSink.Default ());

            wb.BackgroundColor = Colors.Red;
            // ウインドウサイズだけは制約ではなく直接指定する必要がある
            wb.SetSize (600, 400);

            IWidgetContainerBackend container = new WidgetContainerBackend ();
            container.InitializeBackend (null, ctx);
            container.Initialize (new Xwt.Backends.CanvasEventSink.Default ());
            container.BackgroundColor = Colors.Blue;
            container.BoundsRequest = (new Rectangle (0, 0, 600, 400)); // ContentViewの制約は有効にならない

            wb.SetChild (container, false);

            // TODO: 親の原点と子の矩型領域から、レイアウト制約を決定する

            IWidgetContainerBackend box = new WidgetContainerBackend ();
            box.InitializeBackend (null, ctx);
            box.Initialize (new Xwt.Backends.WidgetEventSink.Default());
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
            wb.EventSink.OnBoundsChanged.Register (e => {
                return;
            });

            wb.Visible = true;

            SynchronizationContext.SetSynchronizationContext (new XwtSynchronizationContext (ctx));

            engine.RunApplication ();

            wb.Dispose ();

            engine.Dispose ();
        }

        private static int countedValue;

        private static void ComposeItems(ApplicationContext ctx, IWidgetContainerBackend contaner) {
            ILabelBackend label = new LabelBackend ();
            label.InitializeBackend (null, ctx);
            label.Initialize (new Xwt.Backends.WidgetEventSink.Default());

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
            up.EventSink.OnClicked.Register(
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
