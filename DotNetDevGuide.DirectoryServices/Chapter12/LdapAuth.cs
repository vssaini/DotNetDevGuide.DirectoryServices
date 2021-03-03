using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices.Protocols;
using System.Net;

namespace DotNetDevGuide.DirectoryServices.Chapter12
{
    /// <summary>
    /// Listings 12.3, 12.4, 12.5 full
    /// </summary>
    public class LdapAuth : IDisposable
    {
        LdapConnection _authConnect;
        string _server;
        bool _fastBind;
        bool _useSSL;
        bool _isADAM;

        public LdapAuth(string server, bool useSSL)
        {
            _useSSL = useSSL;
            _fastBind = false;
            _isADAM = false;
            _server = server;

            CheckCapabilities();

            _authConnect = new LdapConnection(
                new LdapDirectoryIdentifier(server),
                null,
                AuthType.Basic
                );

            _authConnect.SessionOptions.SecureSocketLayer = _useSSL;
            _authConnect.SessionOptions.ProtocolVersion = 3;

            if (_fastBind)
            {
                try
                {
                    _authConnect.SessionOptions.FastConcurrentBind();
                }
                catch (PlatformNotSupportedException)
                {
                    //this will happen when server is not W2K3
                    _fastBind = false;
                }
            }

            if (_fastBind && !_useSSL)
            {
                //we are using a non-encrypted channel
                //to pass credentials in cleartext - bad!
                throw new NotSupportedException("We don't support sending credentials in the clear");
            }

            if (!(_fastBind || _useSSL || _isADAM))
            {
                //we did not get a fast bind or
                //SSL so we can try to at least
                //encrypt the credentials
                _authConnect.AuthType = AuthType.Negotiate;

                _authConnect.SessionOptions.Sealing = true;
                _authConnect.SessionOptions.Signing = true;
            }

            if (_isADAM && !(_useSSL || _fastBind))
            {
                //we are using ADAM with no SSL
                //try to use Digest to secure bind
                //Requires Win2k3 R2 ADAM
                _authConnect.AuthType = AuthType.Digest;
            }
        }

        public bool Authenticate(NetworkCredential credentials)
        {
            try
            {
                _authConnect.Bind(credentials);
                return true;
            }
            catch (LdapException ex)
            {
                if (ex.ErrorCode != 49)
                    throw;

                return false;
            }
        }

        /// <summary>
        /// Check the capabilities of our target server using
        /// a RootDSE base search.  Bootstraps the authentication
        /// process.
        /// </summary>
        private void CheckCapabilities()
        {
            string ext = "supportedExtension";
            string cap = "supportedCapabilities";

            LdapConnection connect = new LdapConnection(
                new LdapDirectoryIdentifier(_server),
                null,
                AuthType.Basic
                );

            using (connect)
            {
                connect.Bind();

                connect.SessionOptions.ProtocolVersion = 3;
                connect.SessionOptions.SecureSocketLayer = _useSSL;

                SearchRequest request = new SearchRequest(
                    null, //read the rootDSE
                    "(objectClass=*)",
                    SearchScope.Base,
                    new string[] { ext, cap }
                    );

                //set 120 second timelimit
                request.TimeLimit = TimeSpan.FromSeconds(120);

                SearchResponse response =
                    (SearchResponse)connect.SendRequest(request);

                if (response.ResultCode != ResultCode.Success)
                    throw new Exception(response.ErrorMessage);

                SearchResultEntry entry = response.Entries[0];
                object[] vals = new object[] { };

                if (entry.Attributes.Contains(ext))
                {
                    vals = entry.Attributes[ext].GetValues(typeof(string));

                    foreach (string s in vals)
                    {
                        //OID for Fast Concurrent Bind support
                        if (s == "1.2.840.113556.1.4.1781")
                        {
                            _fastBind = true;
                            break;
                        }
                    }
                }

                //all LDAP servers should support 'supportedCapabilities'
                vals = entry.Attributes[cap].GetValues(typeof(string));

                foreach (string s in vals)
                {
                    //OID for ADAM
                    if (s == "1.2.840.113556.1.4.1851")
                    {
                        _isADAM = true;
                        break;
                    }
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _authConnect.Dispose();
        }
        #endregion
    }
}
