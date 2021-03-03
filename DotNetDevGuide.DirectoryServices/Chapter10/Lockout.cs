using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace DotNetDevGuide.DirectoryServices.Chapter10
{
    /// <summary>
    /// Listing 10.15
    /// </summary>
    public class Lockout : IDisposable
    {
        DirectoryContext context;
        DirectoryEntry root;
        DomainPolicy policy;

        public Lockout(string domainName)
        {
            this.context = new DirectoryContext(
                DirectoryContextType.Domain,
                domainName
                );

            //get our current domain policy
            Domain domain = Domain.GetDomain(this.context);

            this.root = domain.GetDirectoryEntry();
            this.policy = new DomainPolicy(this.root);
        }

        public bool IsAccountLocked(DirectoryEntry entry)
        {
            //default for when accounts stay locked indefinitely
            string qry = "(lockoutTime>=1)";
        
            TimeSpan duration = this.policy.LockoutDuration;
        
            if (duration != TimeSpan.MaxValue)
            {
                DateTime lockoutThreshold =
                    DateTime.Now.Subtract(duration);
        
                qry = String.Format(
                    "(lockoutTime>={0})",
                    lockoutThreshold.ToFileTime()
                    );
            }
        
            DirectorySearcher ds = new DirectorySearcher(
                entry,
                qry,
                null,
                SearchScope.Base
                );
                
            //if we find a result, it means it was locked out!
            SearchResult sr = ds.FindOne();
            
            return (sr != null);
        }
        
        public void FindLockedAccounts()
        {
            //default for when accounts stay locked indefinitely
            string qry = "(lockoutTime>=1)";

            TimeSpan duration = this.policy.LockoutDuration;

            if (duration != TimeSpan.MaxValue)
            {
                DateTime lockoutThreshold =
                    DateTime.Now.Subtract(duration);

                qry = String.Format(
                    "(lockoutTime>={0})",
                    lockoutThreshold.ToFileTime()
                    );
            }

            DirectorySearcher ds = new DirectorySearcher(
                this.root,
                qry
                );

            using (SearchResultCollection src = ds.FindAll())
            {
                foreach (SearchResult sr in src)
                {
                    long ticks =
                        (long)sr.Properties["lockoutTime"][0];

                    Console.WriteLine(
                        "{0} locked out at {1}",
                        sr.Properties["name"][0],
                        DateTime.FromFileTime(ticks)
                        );
                }
            }
        }

        public void Dispose()
        {
            if (this.root != null)
            {
                this.root.Dispose();
            }
        }
    }
}
