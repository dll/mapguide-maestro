﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4959
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LocalConfigure.Properties {
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LocalConfigure.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to # *****************************************************************************
        ///# MapGuide Server Configuration File
        ///#
        ///# The following configuration is based on a single CPU with a single core.
        ///#
        ///# WARNING: BE VERY CAREFUL WHEN MODIFYING THIS FILE AS IT COULD
        ///#          ADVERSLY IMPACT SERVER PERFORMANCE
        ///#
        ///# When saving this file use a UTF-8 encoding.
        ///#
        ///# *****************************************************************************
        ///# COMMON VALIDATION INFORMATION
        ///#
        ///# (Unless otherwise noted und [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Platform {
            get {
                return ResourceManager.GetString("Platform", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;ConnectionProvider&gt;
        ///    &lt;Name&gt;Maestro.Local&lt;/Name&gt;
        ///    &lt;Description&gt;Connection that wraps the mg-desktop library&lt;/Description&gt;
        ///    &lt;Assembly&gt;%ADDINDIR%\OSGeo.MapGuide.MaestroAPI.Local.dll&lt;/Assembly&gt;
        ///    &lt;Type&gt;OSGeo.MapGuide.MaestroAPI.Local.LocalConnection&lt;/Type&gt;
        ///&lt;/ConnectionProvider&gt;.
        /// </summary>
        internal static string ProviderEntry {
            get {
                return ResourceManager.GetString("ProviderEntry", resourceCulture);
            }
        }
    }
}