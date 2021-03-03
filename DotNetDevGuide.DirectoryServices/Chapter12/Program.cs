using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

using NUnit.Framework;

namespace DotNetDevGuide.DirectoryServices.Chapter12
{
    class Chapter12
    {
        const int ERROR_LOGON_FAILURE = -2147023570;

        /// <summary>
        /// Shows how to use Listing 12.1
        /// </summary>
        [Test]
        public void AuthenticateAD()
        {
            //notice the domain name(s)
            Assert.IsTrue(
                AuthenticateUser(@"corp\dunnry", "secret", "corp.domain.org")
            );
        }

        /// <summary>
        /// Shows how to use Listing 12.1
        /// </summary>
        [Test]
        public void AuthenticateADAM()
        {
            //Authenticate via simple bind using CN or UPN
            Assert.IsTrue(
                AuthenticateUser2(@"CN=Ryan,O=Dunnry,C=US", "secret", "localhost:389")
            );
        }

        /// <summary>
        /// Shows how to use Listing 12.3, .4, .5 - see LdapAuth.cs
        /// If you are using ADAM - make sure you have SSL or Digest working
        /// </summary>
        [Test]
        public void LdapAuthenticate()
        {
            LdapAuth auth = new LdapAuth(TestUtils.Settings.Server, false);

            using (auth)
            {
                bool truth = auth.Authenticate(
                    new System.Net.NetworkCredential("username", "password")
                );

                Assert.IsTrue(truth);
            }
        }

        /// <summary>
        /// Shows how to use Listing 12.6 - see NTAuth.cs
        /// </summary>
        [Test]
        public void SSPIAuthenticate()
        {
            NTAuth auth = new NTAuth(5555);
            bool truth = auth.Authenticate(
                new System.Net.NetworkCredential("dunnry", "secret", "netbiosdomain")
            );

            Assert.IsTrue(truth);
        }

        /// <summary>
        /// Listing 12.1 - used for AD.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        private bool AuthenticateUser(
            string username,
            string password,
            string domain)
        {
            //optionally add the domain
            string adsPath = String.Format(
                "LDAP://{0}rootDSE",
                (domain != null && domain.Length > 0) ? domain + "/" : String.Empty
                );

            DirectoryEntry root = new DirectoryEntry(
                adsPath,
                username,
                password,
                AuthenticationTypes.Secure
                | AuthenticationTypes.FastBind
                );

            using (root)
            {
                try
                {
                    //force the bind
                    object tmp = root.NativeObject;
                    return true;
                }
                catch (System.Runtime.InteropServices.COMException ex)
                {
                    //some other error happened, so rethrow it
                    if (ex.ErrorCode != ERROR_LOGON_FAILURE)
                        throw;

                    return false;
                }
            }
        }

        /// <summary>
        /// Listing 12.2 - ADAM Auth
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        private bool AuthenticateUser2(
            string username,
            string password,
            string server)
        {
            //optionally add the domain
            string adsPath = String.Format(
                "LDAP://{0}/rootDSE",
                server
                );

            DirectoryEntry root = new DirectoryEntry(
                adsPath,
                username,
                password,
                AuthenticationTypes.SecureSocketsLayer
                | AuthenticationTypes.FastBind
                );

            using (root)
            {
                try
                {
                    //force the bind
                    object tmp = root.NativeObject;
                    return true;
                }
                catch (System.Runtime.InteropServices.COMException ex)
                {
                    //some other error happened, so rethrow it
                    if (ex.ErrorCode != ERROR_LOGON_FAILURE)
                        throw;

                    return false;
                }
            }
        }
    }
}
