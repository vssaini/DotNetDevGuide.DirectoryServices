using System;
using System.Configuration;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace DotNetDevGuide.DirectoryServices.Configuration
{
    public class LdapSettings
    {
        string _defaultPartition;
        string _username;
        string _password;
        string _server;

        ConnectionProtection _connectionProtection = ConnectionProtection.Secure;

        /// <summary>
        /// Root application partition or 'defaultNamingContext' - e.g. "DC=mydomain,DC=com"
        /// </summary>
        [XmlAttribute("defaultPartition")]
        public string DefaultPartition
        {
            get { return _defaultPartition; }
            set { _defaultPartition = value; }
        }

        /// <summary>
        /// Server and port to use.  Must be specified if using ADAM.
        /// e.g. adamserver:389
        /// </summary>
        [XmlAttribute("server")]
        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        [XmlAttribute("username")]
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        [XmlAttribute("password")]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        [XmlAttribute("connectionProtection")]
        public ConnectionProtection Protection
        {
            get { return _connectionProtection; }
            set { _connectionProtection = value; }
        }
    }

    /// <summary>
    /// Designate the type of bind to use when connecting to the directory.
    /// </summary>
    public enum ConnectionProtection
    {
        /// <summary>
        /// Simple Bind with no protection.  This is usually the case for
        /// non-SSL ADAM instances.  AD can use Secure Binds always
        /// </summary>
        None,
        /// <summary>
        /// Secure binds using SSPI (Negotiate).  This works for all AD
        /// directories and when using Windows security principals with 
        /// ADAM (pass-thru auth).
        /// </summary>
        Secure,
        /// <summary>
        /// Uses SSL for protection.  Use this with ADAM and SSL connections
        /// </summary>
        SSL
    }

    public class DunnryConfigHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object input, XmlNode node)
        {
            XmlSerializer xs = new XmlSerializer(typeof(LdapSettings));
            XmlNodeReader xnr = new XmlNodeReader(node);
            return (LdapSettings)xs.Deserialize(xnr);
        }
    }
}
