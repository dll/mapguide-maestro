﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OSGeo.MapGuide.Maestro.Strings {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class FormExpression {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal FormExpression() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("OSGeo.MapGuide.Maestro.Strings.FormExpression", typeof(FormExpression).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occured while reading column values: {0}.
        /// </summary>
        internal static string ColumnValueError {
            get {
                return ResourceManager.GetString("ColumnValueError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Function: {1}({2})
        ///Description: {3}
        ///Returns: {0}.
        /// </summary>
        internal static string FunctionTooltip {
            get {
                return ResourceManager.GetString("FunctionTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No values found in selected column.
        /// </summary>
        internal static string NoColumnValuesError {
            get {
                return ResourceManager.GetString("NoColumnValuesError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Property: {0}
        ///Type: {1}.
        /// </summary>
        internal static string PropertyTooltip {
            get {
                return ResourceManager.GetString("PropertyTooltip", resourceCulture);
            }
        }
    }
}