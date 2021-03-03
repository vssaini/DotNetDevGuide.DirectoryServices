using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices.Protocols;
using System.Threading;

namespace DotNetDevGuide.DirectoryServices.Chapter5
{
    /// <summary>
    /// Listing 5.10 Modified...
    /// </summary>
    class Async
    {
        Mutex mutex = new Mutex();
        LdapConnection connect;

        public void Search()
        {
            this.connect = new LdapConnection(
                new LdapDirectoryIdentifier(TestUtils.Settings.Server),
                new System.Net.NetworkCredential(
                    TestUtils.Settings.Username,
                    TestUtils.Settings.Password
                ),
                AuthType.Basic //SecureSocketsLayer
                );

            this.connect.Bind();

            this.connect.SessionOptions.ProtocolVersion = 3;
            this.connect.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

            SearchRequest request = new SearchRequest(
                TestUtils.Settings.DefaultPartition,
                "(cn=user9*)",
                SearchScope.Subtree,
                null
                );

            request.SizeLimit = 500;

            Console.WriteLine(
                "Main Execution on thread #{0}",
                Thread.CurrentThread.ManagedThreadId);

            IAsyncResult result = this.connect.BeginSendRequest(
                request,
                PartialResultProcessing.NoPartialResultSupport,
                new AsyncCallback(InternalCallback),
                null
                );

            //let the async thread signal
            result.AsyncWaitHandle.WaitOne();

            //wait until the other thread finishes output
            this.mutex.WaitOne();

            Console.WriteLine(
                "Finished...Press Enter to Continue"
                );

            this.mutex.ReleaseMutex();
        }

        private void InternalCallback(IAsyncResult result)
        {
            this.mutex.WaitOne();

            Console.WriteLine(
                "Callback on thread #{0}",
                Thread.CurrentThread.ManagedThreadId);

            SearchResponse response =
                this.connect.EndSendRequest(result)
                    as SearchResponse;

            foreach (SearchResultEntry entry in response.Entries)
            {
                Console.WriteLine(entry.DistinguishedName);
            }

            this.mutex.ReleaseMutex();
        }
    }
}
