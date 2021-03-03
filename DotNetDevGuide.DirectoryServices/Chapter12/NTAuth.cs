using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Principal;
using System.Security.Authentication;
using System.Threading;

namespace DotNetDevGuide.DirectoryServices.Chapter12
{
    /// <summary>
    /// Listing 12.6
    /// </summary>
    class NTAuth
    {
        TcpListener listener;
        int port;

        public NTAuth(int port)
        {
            this.port = port;
            this.listener = new TcpListener(
                IPAddress.Loopback, this.port);
            this.listener.Start();
        }

        private void CreateServer(object state)
        {
            try
            {
                NegotiateStream nsServer = new NegotiateStream(
                    this.listener.AcceptTcpClient().GetStream()
                    );
        
                nsServer.AuthenticateAsServer(
                    CredentialCache.DefaultNetworkCredentials,
                    ProtectionLevel.None,
                    TokenImpersonationLevel.Impersonation
                    );
            }
            catch (AuthenticationException) {}
        }

        public bool Authenticate(NetworkCredential creds)
        {
            TcpClient client = new TcpClient(
                "localhost",
                 this.port
                 );

            ThreadPool.QueueUserWorkItem(
                new WaitCallback(CreateServer)
                );

            NegotiateStream nsClient = new NegotiateStream(
                client.GetStream(),
                true
                );

            using (nsClient)
            {
                try
                {
                    nsClient.AuthenticateAsClient(
                        creds,
                        creds.Domain + @"\" + creds.UserName, 
                        ProtectionLevel.None,
                        TokenImpersonationLevel.Impersonation
                        );
                    return nsClient.IsAuthenticated;
                }
                catch (AuthenticationException)
                {
                    return false;
                }
            }
        }
    }
}




