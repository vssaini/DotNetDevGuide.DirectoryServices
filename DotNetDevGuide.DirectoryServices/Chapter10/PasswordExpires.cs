using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace DotNetDevGuide.DirectoryServices.Chapter10
{
    /// <summary>
    /// Listing 10.8, 10.9, & 10.10 in full.
    /// </summary>
    public class PasswordExpires
    {
        DomainPolicy _policy;

        const int UF_DONT_EXPIRE_PASSWD = 0x10000;

        public PasswordExpires()
        {
            DirectoryContext ctx = new DirectoryContext(
                    DirectoryContextType.Domain
                    );

            //get our current domain policy
            Domain domain = Domain.GetDomain(ctx);
            DirectoryEntry root = domain.GetDirectoryEntry();

            using (domain)
            using (root)
            {
                _policy = new DomainPolicy(root);
            }
        }

        public PasswordExpires(DomainPolicy policy)
        {
            _policy = policy;
        }

        /// <summary>
        /// Gets the Password Expiration 
        /// Date for a domain user
        /// Returns MaxValue if never expiring
        /// Returns MinValue if user must
        /// change password at next logon
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public DateTime GetExpiration(DirectoryEntry user)
        {
            int flags =
                (int)user.Properties["userAccountControl"][0];

            //check to see if passwords expire
            if (Convert.ToBoolean(flags & UF_DONT_EXPIRE_PASSWD))
            {
                //the user's password will never expire
                return DateTime.MaxValue;
            }

            long ticks = GetInt64(user, "pwdLastSet");

            //user must change password at next logon
            if (ticks == 0)
                return DateTime.MinValue;

            //password has never been set
            if (ticks == -1)
            {
                throw new InvalidOperationException(
                    "User does not have a password"
                    );
            }

            //get when the user last set their password;
            DateTime pwdLastSet = DateTime.FromFileTime(
                ticks
                );

            //use our policy class to determine when
            //it will expire
            return pwdLastSet.Add(
                _policy.MaxPasswordAge
                );
        }

        /// <summary>
        /// Returns a TimeSpan representing how long
        /// until a user's password expires.
        /// 
        /// MinValue means already expired
        /// MaxValue means never expires
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public TimeSpan GetTimeLeft(DirectoryEntry user)
        {
            DateTime willExpire = GetExpiration(user);

            if (willExpire == DateTime.MaxValue)
                return TimeSpan.MaxValue;

            if (willExpire == DateTime.MinValue)
                return TimeSpan.MinValue;

            if (willExpire.CompareTo(DateTime.Now) > 0)
            {
                //the password has not expired
                //(pwdLast + MaxPwdAge)- Now = Time Left
                return willExpire.Subtract(DateTime.Now);
            }

            //the password has already expired
            return TimeSpan.MinValue;
        }

        private Int64 GetInt64(DirectoryEntry entry, string attr)
        {
            //we will use the marshaling behavior of
            //the searcher
            DirectorySearcher ds = new DirectorySearcher(
                entry,
                String.Format("({0}=*)", attr),
                new string[] { attr },
                SearchScope.Base
                );

            SearchResult sr = ds.FindOne();

            if (sr != null)
            {
                if (sr.Properties.Contains(attr))
                {
                    return (Int64)sr.Properties[attr][0];
                }
            }
            return -1;
        }
    }
}
