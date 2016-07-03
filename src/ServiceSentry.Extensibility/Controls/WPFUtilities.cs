// -----------------------------------------------------------------------
//  <copyright file="WPFUtilities.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Generic;
//using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace ServiceSentry.Extensibility.Controls
{
    public abstract class WPFUtilities
    {
        public static WPFUtilities GetInstance()
        {
            return GetInstance(UtilityHelper.GetHelper());
        }

        internal static WPFUtilities GetInstance(UtilityHelper helper)
        {
            return new WPFUtilitiesImplementation(helper);
        }

        public abstract IEnumerable<Window> Windows(object sender);
        public abstract IEnumerable<TabItem> TabItems(object sender);
        public abstract List<T> GetLogicalChildCollection<T>(object parent) where T : DependencyObject;


        internal abstract class UtilityHelper
        {
            internal static UtilityHelper GetHelper()
            {
                return new UtilityHelperImplementation();
            }

            internal abstract IEnumerable<string> WindowNames(object sender);
            internal abstract Window WindowByName(string windowName);

            internal abstract void GetLogicalChildCollection<T>(DependencyObject parent,
                                                                ICollection<T> logicalCollection)
                where T : DependencyObject;

            private sealed class UtilityHelperImplementation : UtilityHelper
            {
                internal override IEnumerable<string> WindowNames(object sender)
                {
                    //Contract.Requires(sender != null);
                    var asm = Assembly.GetAssembly(sender.GetType());

                    var ieWindowNames = from types in asm.GetTypes()
                                        where types.BaseType != null && types.BaseType.Name == "Window"
                                        orderby types.Name
                                        select types.Name;
                    return ieWindowNames;
                }

                internal override Window WindowByName(string windowName)
                {
                    if (string.IsNullOrEmpty(windowName)) return null;
                    var asm = Assembly.GetCallingAssembly();

                    var fullyQualifiedName = asm.GetName().Name + "." + windowName;
                    var obj = asm.CreateInstance(fullyQualifiedName);

                    return (obj as Window);
                }

                internal override void GetLogicalChildCollection<T>(DependencyObject parent,
                                                                    ICollection<T> logicalCollection)
                {
                    var children = LogicalTreeHelper.GetChildren(parent);
                    foreach (var child in children)
                    {
                        if (!(child is DependencyObject)) continue;

                        var depChild = child as DependencyObject;
                        if (child is T)
                        {
                            logicalCollection.Add(child as T);
                        }
                        GetLogicalChildCollection(depChild, logicalCollection);
                    }
                }
            }
        }

        private sealed class WPFUtilitiesImplementation : WPFUtilities
        {
            private readonly UtilityHelper _helper;

            internal WPFUtilitiesImplementation(UtilityHelper helper)
            {
                _helper = helper;
            }

            public override IEnumerable<Window> Windows(object sender)
            {
                //Contract.Requires(sender != null);

                var windowList = new List<Window>();
                foreach (var windowName in _helper.WindowNames(sender))
                {
                    var win = _helper.WindowByName(windowName);
                    if (win == null) continue;
                    windowList.Add(win);
                }
                return windowList.ToArray();
            }

            public override IEnumerable<TabItem> TabItems(object sender)
            {
                //Contract.Requires(sender != null);

                var tabList = new List<TabItem>();
                foreach (var window in Windows(sender))
                {
                    tabList.AddRange(GetLogicalChildCollection<TabItem>(window));
                }

                return tabList;
            }

            public override List<T> GetLogicalChildCollection<T>(object parent)
            {
                var logicalCollection = new List<T>();
                _helper.GetLogicalChildCollection(parent as DependencyObject, logicalCollection);
                return logicalCollection;
            }
        }
    }
}