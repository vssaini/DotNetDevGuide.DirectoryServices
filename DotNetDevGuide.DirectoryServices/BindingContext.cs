using System;
using System.Net;

namespace DotNetDevGuide.DirectoryServices
{
	using DWORD = System.UInt32;
	using HANDLE = System.IntPtr;
	using RPC_AUTH_IDENTITY_HANDLE = System.IntPtr;

    /// <summary>
    /// Used to bind to Active Directory and wraps the unmanaged directory handle
    /// necessary for a number of the Ds* API calls.  This class is also used as
    /// a point of collection of credential information.
    /// 
    /// TODO:: Refactor for ADAM support as well.
    /// </summary>
    public class BindingContext : IDisposable
    {
        #region Private Members

        HANDLE _handle = IntPtr.Zero;
        RPC_AUTH_IDENTITY_HANDLE _authIdentity = IntPtr.Zero;
        NetworkCredential _credential;

        string _domain;

        bool _initiated;
        bool _useCredentials;
        bool _disposed;

        #endregion

        #region Constructors

        internal BindingContext(string username, string password, string domain)
        {
            _useCredentials = (username != null);

            _credential = new NetworkCredential(username, password, domain);
            _domain = domain;

            _initiated = false;
            _disposed = false;
        }
        #endregion

        #region Internal Properties

        internal HANDLE Handle
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("Context already disposed", (Exception)null);

                lock (this)
                {
                    if (!_initiated)
                        InitiateBind();
                }
                return _handle;
            }
        }

        internal NetworkCredential Credential
        {
            get { return _credential; }
        }

        internal string DomainName
        {
            get
            {
                if (_domain != null && _domain != String.Empty)
                    return _domain;

                return null;
            }
        }
        #endregion

        #region Public Static Methods

        public static BindingContext Create()
        {
            return new BindingContext(null, null, null);
        }

        public static BindingContext Create(string username, string password, string domain)
        {
            if (username == null || username == String.Empty)
                throw new ArgumentException("Invalid Username supplied");

            if (domain == null || domain == String.Empty)
                throw new ArgumentException("Invalid Domain Name supplied");

            if (password == null)
                throw new ArgumentNullException("password");

            return new BindingContext(username, password, domain);
        }

        public static BindingContext Create(string username, string password)
        {
            if (username == null || username == String.Empty)
                throw new ArgumentException("Invalid Username supplied");

            if (password == null)
                throw new ArgumentNullException("password");

            return new BindingContext(username, password, null);
        }

        #endregion

        #region Binding Methods

        private void BindWithCredentials()
        {
            //create our credentials
            DWORD rc = NativeMethods.DsMakePasswordCredentials(
                _credential.UserName,
                _credential.Domain,
                _credential.Password,
                out _authIdentity
                );

            if (rc != Win32.ERROR_SUCCESS)
                Win32.ThrowLastError();

            //and now bind with them
            rc = NativeMethods.DsBindWithCred(
                null,
                _domain,
                _authIdentity,
                out _handle
                );

            CheckReturnCode(rc);
        }

        private void Bind()
        {
            //bind using current credentials
            DWORD rc = NativeMethods.DsBind(
                null,
                _domain,
                out _handle
                );

            CheckReturnCode(rc);

        }

        private void CheckReturnCode(DWORD rc)
        {
            if (rc != Win32.ERROR_SUCCESS)
            {
                switch (rc)
                {
                    case 1355: //ERROR_NO_SUCH_DOMAIN
                        throw new InvalidOperationException("No domain found");
                    //throw new DomainNotFoundException();
                    default:
                        Win32.ThrowLastError();
                        break; //unreachable
                }
            }
        }

        internal void InitiateBind()
        {
            lock (this)
            {
                if (!_initiated)
                {
                    if (_useCredentials)
                    {
                        BindWithCredentials();
                    }
                    else
                    {
                        Bind();
                    }
                }
                _initiated = true;
            }
        }

        #endregion

        #region IDisposable Members

        private void UnBind()
        {
            lock (this)
            {
                if (!_disposed)
                {
                    if (!Win32.IsNullHandle(_handle))
                        NativeMethods.DsUnBind(ref _handle);

                    //this must be freed only after the Unbind
                    if (!Win32.IsNullHandle(_authIdentity))
                        NativeMethods.DsFreePasswordCredentials(_authIdentity);

                    _disposed = true;
                }
            }
        }

        private void Dispose(bool disposing)
        {
            UnBind();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BindingContext()
        {
            Dispose(false);
        }

        #endregion
    }
}
