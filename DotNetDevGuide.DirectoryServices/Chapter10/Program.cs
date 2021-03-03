using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Principal;

using NUnit.Framework;

namespace DotNetDevGuide.DirectoryServices.Chapter10
{
    class Chapter10
    {
        /// <summary>
        /// Listing 10.1 Modified
        /// </summary>
        [Test]
        public void CreateUserSimple()
        {
            //set this to where you want to put the user
            DirectoryEntry parent = TestUtils.CreateDirectoryEntry(
                "OU=Users," + TestUtils.Settings.DefaultPartition);

            DirectoryEntry user =
                parent.Children.Add("CN=test.user", "user");

            using (user)
            {
                //sAMAccountName is required for W2k AD, we would not use
                //this for ADAM however.
                user.Properties["sAMAccountName"].Value = "test.user";

                //userPrincipalName is not required, but recommended
                //for ADAM.  AD also contains this, so we can use it.
                user.Properties["userPrincipalName"].Value = "test.user";
                user.CommitChanges();
            }
        }

        /// <summary>
        /// Listing 10.3 Modified - Only works with Active Directory as
        /// ADAM does not have userAccountControl attribute.
        /// </summary>
        [Test]
        public void GetUserAccountSettings()
        {
            //point this to a user in the directory
            DirectoryEntry user = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            AdsUserFlags userFlags = (AdsUserFlags)
                user.Properties["userAccountControl"].Value;

            Console.WriteLine(
                "AdsUserFlags for {0}: {1}",
                user.Path,
                userFlags
                );
        }

        /// <summary>
        /// Listing 10.4 Modified - only works with AD again
        /// </summary>
        [Test]
        public void SetUserAccountSettings()
        {
            //point this to a user in the directory
            DirectoryEntry entry = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            using (entry)
            {
                AdsUserFlags newValue = AdsUserFlags.NormalAccount | AdsUserFlags.PasswordDoesNotExpire;

                entry.Properties["userAccountControl"].Value = newValue;
                entry.CommitChanges();
            }
        }

        /// <summary>
        /// Listing 10.5 modified - ADAM and W2k3 AD can use this
        /// new attribute to do the same as 10.4.
        /// </summary>
        [Test]
        public void GetUserAccountSettingsCalculated()
        {
            //point this to a user in the directory
            DirectoryEntry user = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            //this is a pain to type a lot :)
            string msDS = "msDS-User-Account-Control-Computed";

            using (user)
            {
                //this is constructed attribute
                user.RefreshCache(
                    new string[]{msDS}
                    ); 

                AdsUserFlags userFlags = 
                    (AdsUserFlags)user.Properties[msDS].Value;

                Console.WriteLine(
                    "AdsUserFlags for {0}: {1}",
                    user.Path,
                    userFlags
                    );
            }
        }

        /// <summary>
        /// Listing 10.6 modified - ADAM only
        /// </summary>
        [Test]
        public void DisableUserAccount()
        {
             //point this to a user in the directory
            DirectoryEntry user = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            string attrib = "msDS-UserAccountDisabled";

            using (user)
            {
                //disable the account
                user.Properties[attrib].Value = true;
                user.CommitChanges();
            }
        }

        /// <summary>
        /// Sample code to use Listing 10.7 - see domainpolicy.cs
        /// </summary>
        [Test]
        public void GetDomainPolicy()
        {
            DirectoryEntry root = TestUtils.GetDefaultPartition();

            using (root)
            {
                DomainPolicy policy = new DomainPolicy(root);

                Console.WriteLine(policy.DomainDistinguishedName);
                Console.WriteLine(policy.PasswordHistoryLength);
                Console.WriteLine(policy.MaxPasswordAge);
                Console.WriteLine(policy.LockoutDuration);
                Console.WriteLine(policy.MinPasswordLength);
            }
        }

        /// <summary>
        /// Sample method to use Listing 10.8, 10.9, 10.10.  This only works
        /// with AD however because it uses 'userAccountControl'.
        /// </summary>
        [Test]
        public void GetPasswordExpiration()
        {
            DirectoryEntry root = TestUtils.GetDefaultPartition();

             //point this to a user in the directory
            DirectoryEntry user = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            using (root)
            using (user)
            {
                PasswordExpires pe = new PasswordExpires(new DomainPolicy(root));
                
                Console.WriteLine(
                    "{0} expires on {1}",
                    user.Name,
                    pe.GetExpiration(user)
                    );

                Console.WriteLine(
                    "{0} expires {1} days",
                    user.Name,
                    pe.GetTimeLeft(user).Days
                    );
            }
        }

        /// <summary>
        /// Listing 10.11 modified...
        /// </summary>
        [Test]
        public void GetPasswordExpirationBoolean()
        {
            //point this to a user in the directory
            DirectoryEntry user = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            string attrib = "msDS-User-Account-Control-Computed";

            using (user)
            {
                user.RefreshCache(new string[] { attrib });

                int flags = (int)user.Properties[attrib].Value
                    & (int)AdsUserFlags.PasswordExpired;

                if (Convert.ToBoolean(flags))
                {
                    //password has expired
                    Console.WriteLine("Expired");
                }
            }
        }

        /// <summary>
        /// Listing 10.12
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="maxPwdAge"></param>
        /// <returns></returns>
        public string GetExpirationFilter(
            DateTime startDate,
            DateTime endDate,
            TimeSpan maxPwdAge
            )
        {        
            Int64 lowDate;
            Int64 highDate;
            string filterPattern = "(&(sAMAccountType=805306368)(pwdLastSet>={0})(pwdLastSet<={1}))";
            
            lowDate = startDate.Subtract(maxPwdAge).ToFileTime();
            highDate = endDate.Subtract(maxPwdAge).ToFileTime();
                    
            return String.Format( 
                filterPattern, 
                lowDate, 
                highDate 
                );
        }

        /// <summary>
        /// Listing 10.13 modified - AD only sample.
        /// </summary>
        [Test]
        public void GetLastLogon()
        {
            string username = "dunnry"; //update
             
            DirectoryContext context = new DirectoryContext(
                DirectoryContextType.Domain,
                "yourdomain.com" //update this
                );

            DateTime latestLogon = DateTime.MinValue;
            string servername = null;

            DomainControllerCollection dcc = 
                DomainController.FindAll(context);

            foreach (DomainController dc in dcc)
            {
                DirectorySearcher ds;

                using (dc)
                using (ds = dc.GetDirectorySearcher())
                {
                    ds.Filter = String.Format(
                        "(sAMAccountName={0})",
                        username
                        );
                    ds.PropertiesToLoad.Add("lastLogon");
                    ds.SizeLimit = 1;
                    
                    SearchResult sr = ds.FindOne();
                    
                    if (sr != null)
                    {
                        DateTime lastLogon = DateTime.MinValue;
                        if (sr.Properties.Contains("lastLogon"))
                        {
                            lastLogon = DateTime.FromFileTime(
                                (long)sr.Properties["lastLogon"][0]
                                );
                        }
                        
                        if (DateTime.Compare(lastLogon,latestLogon) > 0)
                        {
                            latestLogon = lastLogon;
                            servername = dc.Name;
                        }
                    }
                }
            }

            Console.WriteLine(
                "Last Logon: {0} at {1}",
                servername,
                latestLogon.ToString()
                 );
        }

        /// <summary>
        /// Listing 10.14 modified
        /// </summary>
        [Test]
        public void GetAccountLockoutSimple()
        {
            //point this to a user in the directory
            DirectoryEntry user = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            string attrib = "msDS-User-Account-Control-Computed";

            using (user)
            {
                //this is a constructed attrib
                user.RefreshCache(new string[] { attrib });

                const int UF_LOCKOUT = 0x0010;

                int flags =
                    (int)user.Properties[attrib].Value;

                if (Convert.ToBoolean(flags & UF_LOCKOUT))
                {
                    Console.WriteLine(
                        "{0} is locked out",
                        user.Name
                        );
                }
            }
        }

        /// <summary>
        /// Sample method to show how to use Listing 10.15 - see lockout.cs
        /// </summary>
        [Test]
        public void GetAccountLockout()
        {
            Lockout l = new Lockout("mydomain.com");
            l.FindLockedAccounts();
        }

        /// <summary>
        /// Demonstrates how to use Listing 10.16
        /// </summary>
        [Test]
        public void LdapSetPassword()
        {
            using (LdapPasswordModifier lpm = new LdapPasswordModifier(
                TestUtils.Settings.Server,
                null, //or add an explicit NetworkCredential with alt creds
                false))
            {
                //update this to point to a user
                string userDN = "CN=User1,OU=Users" + TestUtils.Settings.DefaultPartition;

                lpm.SetPassword(userDN, "newPassword.001");
                lpm.ChangePassword(userDN, "newPassword.001", "newPassword.002");
            }
        }

        /// <summary>
        /// Listing 10.17 modified.
        /// </summary>
        [Test]
        public void SetPasswordADAM()
        {
            //.NET 2.0 sample for ADAM password changes
            //point this to a user in the directory
            DirectoryEntry entry = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            using (entry)
            {
                entry.Options.PasswordPort = 389;
                entry.Options.PasswordEncoding =
                    PasswordEncodingMethod.PasswordEncodingClear;

                entry.Invoke(
                    "ChangePassword",
                    new object[] { "UserPassword1", "UserPassword2" }
                    );
            }
        }

        /// <summary>
        /// Listing 10.18 modified.  Use this for v1.x Framework
        /// </summary>
        [Test]
        public void SetPasswordADAMFx1()
        {
            //.NET 1.x sample
            const int ADS_OPTION_PASSWORD_PORTNUMBER = 6;
            const int ADS_OPTION_PASSWORD_METHOD = 7;
            const int ADS_PASSWORD_ENCODE_CLEAR = 1;

            //point this to a user in the directory
            DirectoryEntry entry = TestUtils.FindUserByCN("CN=User1");

            using (entry)
            {
                entry.Invoke(
                    "SetOption",
                    new object[] { ADS_OPTION_PASSWORD_PORTNUMBER, 389 }
                    );
                entry.Invoke(
                    "SetOption",
                    new object[] {
                    ADS_OPTION_PASSWORD_METHOD, 
                    ADS_PASSWORD_ENCODE_CLEAR
                    }
                    );
                entry.Invoke(
                    "ChangePassword",
                    new object[] { "UserPassword1", "UserPassword2" }
                    );
            }
        }

        /// <summary>
        /// Listing 10.19 Modified.  Uses LDAP search to get tokenGroups
        /// </summary>
        [Test]
        public void ExpandTokenGroups()
        {
            //point this to a user in the directory
            DirectoryEntry user = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            using (user)
            {
                StringBuilder sb = new StringBuilder();

                //we are building an '|' clause
                sb.Append("(|");

                foreach (byte[] sid in user.Properties["tokenGroups"])
                {
                    //append each member into the filter
                    sb.AppendFormat(
                        "(objectSid={0})", BuildFilterOctetString(sid));
                }

                //end our initial filter
                sb.Append(")");

                DirectoryEntry searchRoot = TestUtils.GetDefaultPartition();

                using (searchRoot)
                {
                    //we now have our filter, we can just search for the groups
                    DirectorySearcher ds = new DirectorySearcher(
                        searchRoot,
                        sb.ToString(), //our filter
                        null,
                        SearchScope.Subtree
                        );

                    ds.PageSize = 500;

                    using (SearchResultCollection src = ds.FindAll())
                    {
                        foreach (SearchResult sr in src)
                        {
                            //Here is each group now...
                            Console.WriteLine(sr.Path);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Listing 4.2 repeated
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string BuildFilterOctetString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                sb.AppendFormat(
                    "\\{0}",
                    bytes[i].ToString("X2")
                    );
            }
            return sb.ToString();
        }

        /// <summary>
        /// Listing 10.20 Modified.  This sample uses a lot of interop
        /// found in structs.cs, win32.cs, nativemethods.cs, and adutils.cs
        /// </summary>
        [Test]
        public void ExpandTokenGroupsViaCrackNames()
        {
            //point this to a user in the directory
            DirectoryEntry user = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            //you can use another overload to pass credentials
            using (BindingContext ctx = BindingContext.Create())
            using (user)
            {
                //convert to array of string SIDs
                int size = user.Properties["tokenGroups"].Count;
                PropertyValueCollection pvc = user.Properties["tokenGroups"];

                string[] sids = new string[size];

                for (int i = 0; i < size; i++)
                {
                    sids[i] = AdUtils.ConvertSidToString((byte[])pvc[i]);
                }

                //we want to pass in the SID format and retrieve
                //the NT Format names.  This utility class is
                //included in our web site library samples
                //groupNames contains all the converted groups now
                string[] groupNames = AdUtils.DsCrackNamesWrapper(
                    sids,
                    ctx.Handle,
                    DS_NAME_FORMAT.DS_SID_OR_SID_HISTORY_NAME,
                    DS_NAME_FORMAT.DS_NT4_ACCOUNT_NAME
                    );

                foreach (string group in groupNames)
                {
                    Console.WriteLine(group);
                }
            }
        }

        /// <summary>
        /// Listing 10.21 - uses new 2.0 features to accomplish same thing.
        /// Only works with AD
        /// </summary>
        [Test]
        public void ExpandTokenGroupsFx2()
        {
            //point this to a user in the directory
            DirectoryEntry user = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            using (user)
            {
                //we use the collection in order to 
                //batch the request for translation
                IdentityReferenceCollection irc
                    = ExpandTokenGroups(user).Translate(typeof(NTAccount));

                foreach (NTAccount account in irc)
                {
                    Console.WriteLine(account);
                }
            }
        }

        //Sample Helper Function used by Listing 10.21
        private IdentityReferenceCollection ExpandTokenGroups(
            DirectoryEntry user)
        {
            user.RefreshCache(new string[] { "tokenGroups" });

            IdentityReferenceCollection irc =
                new IdentityReferenceCollection();

            foreach (byte[] sidBytes in user.Properties["tokenGroups"])
            {
                irc.Add(new SecurityIdentifier(sidBytes, 0));
            }
            return irc;
        }

    }
}
