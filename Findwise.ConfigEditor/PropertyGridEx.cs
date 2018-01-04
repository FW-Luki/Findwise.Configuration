using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Findwise.Connector.ConfigEditor
{
    internal class PropertyGridEx : PropertyGrid, INotifyPropertyChanged
    {
        public PropertyGridEx()
        {
            var gridView = GetPropertyGridView(this);
            var validatedEvent = gridView.GetType().GetEvent(nameof(Invalidated));
            validatedEvent.AddEventHandler(gridView, new InvalidateEventHandler(PropertyGridView_OnInvalidated));
        }

        public int SplitterPosition
        {
            get
            {
                var gridView = GetPropertyGridView(this);
                PropertyInfo propInfo = gridView.GetType().GetProperty("InternalLabelWidth", BindingFlags.NonPublic | BindingFlags.Instance);
                return (int)propInfo.GetValue(gridView, null);
            }
            set
            {
                object gridView = GetPropertyGridView(this);
                MethodInfo methodInfo = gridView.GetType().GetMethod("MoveSplitterTo", BindingFlags.NonPublic | BindingFlags.Instance);
                methodInfo.Invoke(gridView, new object[] { value });
            } 
        }

        private static object GetPropertyGridView(PropertyGrid propertyGrid)
        {
            var methodInfo = typeof(PropertyGrid).GetMethod("GetPropertyGridView", BindingFlags.NonPublic | BindingFlags.Instance);
            return methodInfo.Invoke(propertyGrid, new object[] { });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private int _previousInternalLabelWidthValue = -1;
        private bool _gettingSplitterPositionForInvalidate = false;
        private void PropertyGridView_OnInvalidated(object sender, InvalidateEventArgs e)
        {
            if (!_gettingSplitterPositionForInvalidate)
            {
                _gettingSplitterPositionForInvalidate = true;
                if (SplitterPosition != _previousInternalLabelWidthValue)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SplitterPosition)));
                    _previousInternalLabelWidthValue = SplitterPosition;
                }
                _gettingSplitterPositionForInvalidate = false;
            }
        }

    }
}
