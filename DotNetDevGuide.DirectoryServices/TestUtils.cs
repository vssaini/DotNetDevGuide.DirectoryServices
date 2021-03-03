using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

using DotNetDevGuide.DirectoryServices.Configuration;


namespace DotNetDevGuide.DirectoryServices
{
    class TestUtils
    {
        static LdapSettings settings;

        static TestUtils()
        {
            settings = System.Configuration.ConfigurationSettings.GetConfig("LdapSettings") as LdapSettings;

            if (settings == null)
                throw new InvalidOperationException("Invalid Configuration Specified");
        }

        public static LdapSettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Retrieves a DirectoryEntry using configuration data
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DirectoryEntry CreateDirectoryEntry(string path)
        {
            //I am intentionally not checking a bunch of stuff for input validation
            //because this is just test harness and I don't care if you screw it up too much
            
            AuthenticationTypes bindingAuth = AuthenticationTypes.Secure;

            switch (settings.Protection)
            {
                //typical for non-SSL ADAM - so assuming server specified
                case ConnectionProtection.None:
                    bindingAuth = AuthenticationTypes.None;
                    break;

                //typical for AD
                case ConnectionProtection.Secure:
                    bindingAuth = AuthenticationTypes.Secure | AuthenticationTypes.Sealing | AuthenticationTypes.Signing;
                    break;

                //typical for SSL-ADAM (assuming server specified as well)
                case ConnectionProtection.SSL:
                    bindingAuth = AuthenticationTypes.SecureSocketsLayer;
                    break;
            }

            if (!String.IsNullOrEmpty(settings.Server))
                bindingAuth = bindingAuth | AuthenticationTypes.ServerBind;

            return new DirectoryEntry(
                BuildAdsPath(path),
                settings.Username,
                settings.Password,
                bindingAuth
                );
        }

        /// <summary>
        /// Creates a DirectoryEntry from the DefaultPartition defined in config
        /// </summary>
        /// <returns></returns>
        public static DirectoryEntry GetDefaultPartition()
        {
            return CreateDirectoryEntry(settings.DefaultPartition);
        }

        /// <summary>
        /// Simple method to find and return a user using the CN name and searching the
        /// defaultNamingContext defined in config.
        /// </summary>
        /// <param name="userRDN"></param>
        /// <returns></returns>
        public static DirectoryEntry FindUserByCN(string userRDN)
        {
            using (DirectoryEntry searchRoot = GetDefaultPartition())
            {
                DirectorySearcher ds = new DirectorySearcher(
                    searchRoot,
                    String.Format("(cn={0})", userRDN),
                    new string[] { "cn" },
                    SearchScope.Subtree
                    );

                SearchResult sr = ds.FindOne();

                return (sr != null) ? sr.GetDirectoryEntry() : null;
            }
        }

        /// <summary>
        /// We use this take into account whether we have a server or not
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string BuildAdsPath(string path)
        {
            return String.Format("LDAP://{0}{1}", String.IsNullOrEmpty(settings.Server) ? String.Empty : settings.Server + "/", path);
        }
    }
}
