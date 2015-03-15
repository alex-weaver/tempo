using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Tempo;
using Tempo.Util;

namespace Tempo.Wpf
{
    /// <summary>
    /// Provides helpers for observing dependency properties.
    /// </summary>
    public static class PropertyAdapter
    {
        /// <summary>
        /// Observe a dependency property as a read-only value cell. When the calling teporal scope ends, the tracking stops and resources are released.
        /// </summary>
        /// <typeparam name="T">The type of values of the dependency property.</typeparam>
        /// <param name="obj">The dependency object exposing the observed property.</param>
        /// <param name="property">The property to observe.</param>
        /// <returns>A read-only value cell which always contains the current value of the property.</returns>
        public static ICellRead<T> Read<T>(DependencyObject obj, DependencyProperty property)
        {
            var state = new MemoryCell<T>(GetPropertyValue<T>(obj, property));
            Listen<T>(obj, property, state.Set);
            return state;
        }

        /// <summary>
        /// Create a write-only memory cell for modifying a dependency property.
        /// </summary>
        /// <typeparam name="T">The type of values of the dependency property.</typeparam>
        /// <param name="obj">The dependency object exposing the property to write.</param>
        /// <param name="property">The property to write.</param>
        /// <returns>A write-only memory cell which modifies the dependency property when modified.</returns>
        public static ICellWrite<T> Write<T>(DependencyObject obj, DependencyProperty property)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(property, obj.GetType());
            if (descriptor == null)
            {
                throw new InvalidOperationException("Cannot get DependencyProperty " + property + " on object " + obj);
            }

            return new AnonymousCellWrite<T>(value =>
            {
                descriptor.SetValue(obj, value);
            });
        }

        private static T GetPropertyValue<T>(DependencyObject obj, DependencyProperty property)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(property, obj.GetType());
            if (descriptor == null)
            {
                throw new InvalidOperationException("Cannot get DependencyProperty " + property + " on object " + obj);
            }
            
            return (T)descriptor.GetValue(obj);
        }

        private static void Listen<T>(DependencyObject obj, DependencyProperty property, Action<T> handler)
        {
            var internalHandler = new EventHandler((s, e) => handler((T)obj.GetValue(property)));

            var descriptor = DependencyPropertyDescriptor.FromProperty(property, obj.GetType());
            if (descriptor != null)
            {
                descriptor.AddValueChanged(obj, internalHandler);

                Events.WhenEnded(() => descriptor.RemoveValueChanged(obj, internalHandler));
            }
        }
    }
}
