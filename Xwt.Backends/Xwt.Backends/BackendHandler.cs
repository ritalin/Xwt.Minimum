// 
// IBackendHandler.cs
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
using System.Collections.Generic;

namespace Xwt.Backends
{
	public abstract class BackendHandler
	{
        internal void Initialize (ApplicationContext context)
		{
			ApplicationContext = context;
		}

		protected ApplicationContext ApplicationContext { get; private set; }
	}

    public sealed class BackendEventHub<TArgsType>
    {
        private IList<Action<TArgsType>> actions = new List<Action<TArgsType>> ();

        public Action Enabled { get; set; }
        public Action Disabled { get; set; }

        public void Register (Action<TArgsType> action)
        {
            if ((! this.actions.Skip(1).Any ()) && (this.Enabled != null)) {
                this.Enabled ();
            }

            this.actions.Add (action);
        }

        public void Unregister (Action<TArgsType> action)
        {
            this.actions.Remove (action);

            if ((! this.actions.Any()) && (this.Disabled != null)) {
                this.Disabled ();
            }
        }

        public void Invoke(TArgsType args) {
            foreach (var action in this.actions) {
                action (args);
            }
        }

        public void ForceEnable() {
            if (this.Enabled != null) {
                this.Enabled ();
            }
        }

        public void ForceDisable() {
            if (this.Disabled != null) {
                this.Disabled ();
            }
        }

        public bool Connected {
            get { return this.actions.Any (); }
        }
    }

    public static class BackendEventHub {
        public static Lazy<BackendEventHub<TArgsType>> Factory<TArgsType>()
        {
            return new Lazy<BackendEventHub<TArgsType>> (() => {
                return new BackendEventHub<TArgsType> ();
            });
        }
    }
}

