using System;
using System.Collections.Generic;

namespace Xwt.Backends
{
    public interface IWidgetContainerBackend: IWidgetBackend
    {
        void AddChild (IWidgetBackend widget);
        void RemoveChild (IWidgetBackend widget);

        IEnumerable<IWidgetBackend> Children { get; }
    }
}

