﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace mass_pnr_lookup.tests.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("mass_pnr_lookup.tests.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to [
        ///{
        ///  &quot;id&quot;: &quot;0a3f50ad-6bc3-32b8-e044-0003ba298018&quot;,
        ///  &quot;kvhx&quot;: &quot;03161568__14_______&quot;,
        ///  &quot;status&quot;: 1,
        ///  &quot;href&quot;: &quot;http://dawa.aws.dk/adresser/0a3f50ad-6bc3-32b8-e044-0003ba298018&quot;,
        ///  &quot;historik&quot;: {
        ///    &quot;oprettet&quot;: &quot;2000-02-05T21:56:13.000&quot;,
        ///    &quot;ændret&quot;: &quot;2000-02-05T21:56:13.000&quot;
        ///  },
        ///  &quot;etage&quot;: null,
        ///  &quot;dør&quot;: null,
        ///  &quot;adressebetegnelse&quot;: &quot;Studiestræde 14, 4300 Holbæk&quot;,
        ///  &quot;adgangsadresse&quot;: {
        ///    &quot;href&quot;: &quot;http://dawa.aws.dk/adgangsadresser/0a3f5083-4cbf-32b8-e044-0003ba298018&quot;,
        ///    &quot;id&quot;: &quot;0a3f5083-4cbf-32b8-e044 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string AddressSample {
            get {
                return ResourceManager.GetString("AddressSample", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] Eksempel_Liste {
            get {
                object obj = ResourceManager.GetObject("Eksempel_Liste", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Id;EJER_NAVN;EJER_ADR;EJER_POSTADR;EnhedNavn;EnhedAdresse;EnhedPostnr;Initialer;AfsenderNavn;Indeks;LinkBrev;LinkTillBrev
        ///1749;Fornavn Efternavn;Studiestræde 14;1455 København K;Text;Vejnavn 1;1455 København K;init;Navn;3;LinkBrevValue;LinkTillBrevValue
        ///;;;;;;;;;;;
        ///.
        /// </summary>
        internal static string Test_Opslag {
            get {
                return ResourceManager.GetString("Test_Opslag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Id;EJER_NAVN;EJER_ADR;EJER_POSTADR;EnhedNavn;EnhedAdresse;EnhedPostnr;Initialer;AfsenderNavn;Indeks;LinkBrev;LinkTillBrev;PNR;FEJL
        ///1749;Fornavn Efternavn;Studiestræde 14;1455 København K;Text;Vejnavn 1;1455 København K;init;Navn;3;LinkBrevValue;LinkTillBrevValue;2107497164;
        ///;;;;;;;;;;;;;Invalid address
        ///.
        /// </summary>
        internal static string Test_Opslag_output {
            get {
                return ResourceManager.GetString("Test_Opslag_output", resourceCulture);
            }
        }
    }
}
