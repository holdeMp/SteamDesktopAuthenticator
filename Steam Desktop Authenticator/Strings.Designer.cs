﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Steam_Desktop_Authenticator {
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
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Steam_Desktop_Authenticator.Strings", typeof(Strings).Assembly);
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
        ///   Looks up a localized string similar to Adding authenticator aborted..
        /// </summary>
        internal static string Aborted {
            get {
                return ResourceManager.GetString("Aborted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This account already has an authenticator linked. You must remove that authenticator to add SDA as your authenticator..
        /// </summary>
        internal static string AlreadyLinkedAuthenticator {
            get {
                return ResourceManager.GetString("AlreadyLinkedAuthenticator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Another instance of the app is already running..
        /// </summary>
        internal static string AnotherInstance {
            get {
                return ResourceManager.GetString("AnotherInstance", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please check your email, and click the link Steam sent you before continuing..
        /// </summary>
        internal static string CheckEmail {
            get {
                return ResourceManager.GetString("CheckEmail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error adding your authenticator..
        /// </summary>
        internal static string ErrorAddingAuthenticator {
            get {
                return ResourceManager.GetString("ErrorAddingAuthenticator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error adding your authenticator: .
        /// </summary>
        internal static string ErrorAddingYourAuthenticator {
            get {
                return ResourceManager.GetString("ErrorAddingYourAuthenticator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to add your phone number. Please try again or use a different phone number..
        /// </summary>
        internal static string FailedAddingPhoneNumber {
            get {
                return ResourceManager.GetString("FailedAddingPhoneNumber", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to That passkey is invalid. Please enter the same passkey you used for your other accounts..
        /// </summary>
        internal static string InvalidPasskey {
            get {
                return ResourceManager.GetString("InvalidPasskey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Revocation code incorrect; the authenticator has not been linked..
        /// </summary>
        internal static string InvalidRevocationCode {
            get {
                return ResourceManager.GetString("InvalidRevocationCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Steam account login succeeded. Press OK to continue adding SDA as your authenticator..
        /// </summary>
        internal static string LoginSucceded {
            get {
                return ResourceManager.GetString("LoginSucceded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Mobile Authenticator has not yet been linked. Before finalizing the authenticator, please write down your revocation code: .
        /// </summary>
        internal static string MobileAuthenticatorNotLinked {
            get {
                return ResourceManager.GetString("MobileAuthenticatorNotLinked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your session was refreshed..
        /// </summary>
        internal static string SessionRefreshed {
            get {
                return ResourceManager.GetString("SessionRefreshed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Steam Login.
        /// </summary>
        internal static string SteamLogin {
            get {
                return ResourceManager.GetString("SteamLogin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Steam Login Error.
        /// </summary>
        internal static string SteamLoginError {
            get {
                return ResourceManager.GetString("SteamLoginError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mobile authenticator successfully linked. Please write down your revocation code: .
        /// </summary>
        internal static string SuccessLink {
            get {
                return ResourceManager.GetString("SuccessLink", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to finalize this authenticator. The authenticator should not have been linked. In the off-chance it was, please write down your revocation code, as this is the last chance to see it: .
        /// </summary>
        internal static string UnableFinalizeAuthenticator {
            get {
                return ResourceManager.GetString("UnableFinalizeAuthenticator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to generate the proper codes to finalize this authenticator. The authenticator should not have been linked. In the off-chance it was, please write down your revocation code, as this is the last chance to see it: .
        /// </summary>
        internal static string UnableGenerateCode {
            get {
                return ResourceManager.GetString("UnableGenerateCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to save mobile authenticator file. The mobile authenticator has not been linked..
        /// </summary>
        internal static string UnableToSaveMobile {
            get {
                return ResourceManager.GetString("UnableToSaveMobile", resourceCulture);
            }
        }
    }
}
