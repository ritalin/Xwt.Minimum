using System;
using System.Diagnostics;
using System.Threading;

namespace Xwt.Backends
{
    public class ApplicationContext
    {
        public delegate void AppExceptionHandler (Exception ex);
        event AppExceptionHandler AppException;

        int inUserCode;

        public ApplicationContext (ToolkitEngineBackend engine, Thread mainThread)
        {
            this.Engine = engine;
            this.UIThread = mainThread;
        }

        public ToolkitEngineBackend Engine { get; private set; }
        public Thread UIThread { get; private set; }
        /// <summary>
        /// Invokes the user handler on the main GUI thread.
        /// </summary>
        /// <returns><c>true</c>, if user code was invoked successfully, <c>false</c> otherwise.</returns>
        /// <param name="a">The action to invoke as user code.</param>
        /// <remarks>
        /// The return value indicates whether the user code was executed without exceptions (<c>true</c>).
        /// The user can handle the exceptions by subscribing the <see cref="Xwt.Application.UnhandledException"/> event.
        /// </remarks>
        public bool InvokeUserCode (Action a)
        {
            Debug.Assert (a != null);

            try {
                EnterUserCode ();
                a ();
                ExitUserCode ();
                return true;
            } 
            catch (Exception ex) {
                FailUserCode (ex);
                return false;
            }
        }

        public void InvokeAsync(Action a) 
        {
            Debug.Assert (a != null);

            this.Engine.InvokeAsync (delegate {
                this.InvokeUserCode (a);
            });    
        }

        /// <summary>
        /// Enters the user code.
        /// </summary>
        /// <remarks>EnterUserCode must be called before executing any user code.</remarks>
        internal void EnterUserCode ()
        {
            inUserCode++;
        }

        /// <summary>
        /// Exits the user code.
        /// </summary>
        public void ExitUserCode () {
#if false
            if (inUserCode == 1 && !exitCallbackRegistered) {
                while (exitActions.Count > 0) {
                    try {
                        exitActions.Dequeue () ();
                    } catch (Exception ex) {
                        Invoke (delegate {
                            Application.NotifyException (ex);
                        });
                    }
                }
            }
#endif
            inUserCode--;
        }

        internal void FailUserCode (Exception error)
        {
            if (this.AppException != null) {
                this.InvokeUserCode (delegate {
                    this.AppException (error);
                });
            }
        }
    }
}

