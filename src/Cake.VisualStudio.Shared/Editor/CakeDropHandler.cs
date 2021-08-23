using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor.DragDrop;

namespace Cake.VisualStudio.Editor
{
    internal abstract class CakeDropHandler : IDropHandler
    {
        public DragDropPointerEffects HandleDragStarted(DragDropInfo dragDropInfo)
        {
            return DragDropPointerEffects.All;
        }

        public DragDropPointerEffects HandleDraggingOver(DragDropInfo dragDropInfo)
        {
            return DragDropPointerEffects.All;
        }

        public abstract DragDropPointerEffects HandleDataDropped(DragDropInfo dragDropInfo);

        public abstract bool IsDropEnabled(DragDropInfo dragDropInfo);

        public void HandleDragCanceled()
        {
            
        }
    }
}
