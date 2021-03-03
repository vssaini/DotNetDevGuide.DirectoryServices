using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.Security.Principal;

using NUnit.Framework;

namespace DotNetDevGuide.DirectoryServices.Chapter5
{
    class Chapter5
    {
        /// <summary>
        /// Listing 5.1 Modified.  This will need to run in AD (not ADAM)
        /// and should be in a domain context to work correctly.
        /// </summary>
        [Test]
        public void GlobalCatalogSearch()
        {
            //we will find this lastname
            //change this to suit your needs
            string lastName = "Dunn";

            DirectoryEntry gc = new DirectoryEntry("GC:");

            DirectoryEntry _root = null;

            using (gc)
            {
                //there is only 1 child under "GC:"
                foreach (DirectoryEntry root in gc.Children)
                {
                    _root = root;
                    break;
                }
            }

            StringBuilder sb = new StringBuilder();

            //note the filter must be searching
            //  for a GC replicated attribute!
            string filter = String.Format(
                "(sn={0}*)",
                lastName
                );

            DirectorySearcher ds = new DirectorySearcher(
                _root,
                filter,
                null,
                SearchScope.Subtree
                );

            using (SearchResultCollection src = ds.FindAll())
            {
                foreach (SearchResult sr in src)
                {
                    sb.AppendFormat("{0}\n", sr.Path);
                }
            }

            Console.WriteLine(sb.ToString());
        }

        /// <summary>
        /// Listing 5.2 Modified
        /// </summary>
        [Test]
        public void VirtualListViewByOffset()
        {
            //Explicitly create our SearchRoot
            DirectoryEntry searchRoot = TestUtils.GetDefaultPartition();

            using (searchRoot)
            {
                DirectorySearcher ds = new DirectorySearcher(
                    searchRoot,
                    "(objectCategory=person)"
                    );

                //sorting must be turned on
                ds.Sort = new SortOption(
                    "sn",
                    SortDirection.Descending
                    );

                //grab first 50 users starting at 1st index
                ds.VirtualListView =
                    new DirectoryVirtualListView(0, 50, 1);

                using (SearchResultCollection src = ds.FindAll())
                {
                    Console.WriteLine("Found {0}", src.Count);
                    foreach (SearchResult sr in src)
                    {
                        Console.WriteLine(sr.Path);
                    }
                }

                Console.WriteLine(
                    "Approx: {0} Found",
                    ds.VirtualListView.ApproximateTotal
                    );

                int offset = 50;
                
                //this is how we would search again using the same VLV
                while (offset < ds.VirtualListView.ApproximateTotal)
                {
                    //update our offset and continue to search 
                    ds.VirtualListView.Offset = offset;

                    using (SearchResultCollection src = ds.FindAll())
                    {
                        Console.WriteLine("Found {0}", src.Count);
                        foreach (SearchResult sr in src)
                        {
                            Console.WriteLine(sr.Path);
                        }
                    }
                    //increment our offset
                    offset += 50;
                }
            }
        }

        /// <summary>
        /// Listing 5.3 Modified.
        /// </summary>
        [Test]
        public void VirtualListViewByTarget()
        {
            //Explicitly create our SearchRoot
            DirectoryEntry searchRoot = TestUtils.GetDefaultPartition();

            using (searchRoot)
            {
                DirectorySearcher ds = new DirectorySearcher(
                    searchRoot,
                    "(objectCategory=person)"
                    );

                //sorting must be turned on,
                //sort by last name
                ds.Sort = new SortOption(
                    "sn",
                    SortDirection.Descending
                    );

                //grab 50 users starting with sn="D*"
                ds.VirtualListView =
                    new DirectoryVirtualListView(0, 50, "D");

                using (SearchResultCollection src = ds.FindAll())
                {
                    Console.WriteLine(
                        "Returning {0}",
                        src.Count
                        );

                    foreach (SearchResult sr in src)
                    {
                        Console.WriteLine(sr.Path);
                    }
                }
                Console.WriteLine(
                    "Approx: {0} Found",
                    ds.VirtualListView.ApproximateTotal
                    );
            }
        }

        /// <summary>
        /// Listing 5.4 Modified...
        /// </summary>
        [Test]
        public void TombstoneSearch()
        {
            //Explicitly create our SearchRoot
            //notice we are using Wellknown GUID
            DirectoryEntry searchRoot = TestUtils.CreateDirectoryEntry(
                String.Format("<WKGUID=18e2ea80684f11d2b9aa00c04f79f805,{0}>", TestUtils.Settings.DefaultPartition));

            //we must add the fastbind option
            searchRoot.AuthenticationType |= AuthenticationTypes.FastBind;

            using (searchRoot)
            {
                DirectorySearcher ds = new DirectorySearcher(
                    searchRoot,
                    "(isDeleted=TRUE)" //all deleted objects
                    );

                ds.SearchScope = SearchScope.OneLevel;
                ds.Tombstone = true;

                using (SearchResultCollection src = ds.FindAll())
                {
                    Console.WriteLine("Returning {0}", src.Count);

                    foreach (SearchResult sr in src)
                    {
                        Console.WriteLine(sr.Path);
                    }
                }
            }
        }

        /// <summary>
        /// Listing 5.5 & 5.6.  This uses the included DirSync class
        /// in this same folder.  Update as necessary.
        /// </summary>
        [Test]
        public void DirSyncSearch()
        {
            //Create our DirSync class
            DirSync ds = new DirSync(
                TestUtils.Settings.Username,
                TestUtils.Settings.Password,
                TestUtils.BuildAdsPath(TestUtils.Settings.DefaultPartition)
                );

            //we must initialize our search to create a cookie
            ds.InitializeCookie("(objectClass=user)");

            //now do something that changes directory
            UpdateDirectory();

            //we are not saving the updates to the cookie here so
            //we can run it again and again without updating each time...
            ds.GetSynchedChanges("(objectClass=user)", false);
        }

        //helper method for DirSyncSearch
        private void UpdateDirectory()
        {
            //add a user
            new Chapter3().CreateUserSimple();
        }


        /// <summary>
        /// Listing 5.7 Modified.
        /// </summary>
        [Test]
        public void AttributeScopedQuerySearch()
        {
            //point this to some group in your directory
            string adsPath = "CN=Readers,CN=Roles," + TestUtils.Settings.DefaultPartition;

            //Explicitly create our SearchRoot
            DirectoryEntry searchRoot = TestUtils.CreateDirectoryEntry(adsPath);

            using (searchRoot) //we are responsible for Disposing
            {
                string[] attribs = new string[]{
                    "distinguishedName",
                    "sAMAccountName",
                    "name",
                    "mail"
                     };

                DirectorySearcher ds = new DirectorySearcher(
                    searchRoot,
                    "(&(objectClass=user)(objectCategory=person))",
                    attribs
                    );

                //must be SearchScope.Base
                ds.SearchScope = SearchScope.Base;

                //we choose any DN-type attribute
                ds.AttributeScopeQuery = "member";

                using (SearchResultCollection src = ds.FindAll())
                {
                    Console.WriteLine("Returning {0}", src.Count);

                    foreach (SearchResult sr in src)
                    {
                        foreach (string s in attribs)
                        {
                            if (sr.Properties.Contains(s))
                            {
                                Console.WriteLine(
                                    "{0}: {1}",
                                    s,
                                    sr.Properties[s][0]
                                    );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Listing 5.8 Modified - Only Windows 2003 clients support this
        /// right now (not sure about Vista).  InvalidOperationException
        /// will occur when using XP or 2000 as client.  Server must also
        /// be either ADAM or Windows 2003 AD.
        /// </summary>
        [Test]
        public void ExtendedDnSearch()
        {
            //Create our SearchRoot
            DirectoryEntry entry = TestUtils.GetDefaultPartition();
            using (entry)
            {
                //Create our searcher
                DirectorySearcher ds = new DirectorySearcher(
                    entry,
                    "(sAMAccountName=User1)", //find 'User1'
                    new string[] { "distinguishedName" }
                    );

                //Specify the Standard Syntax
                ds.ExtendedDN = ExtendedDN.Standard;

                SearchResult sr = ds.FindOne();

                string dn =
                    sr.Properties["distinguishedName"][0].ToString();

                //ExtendedDN is in
                //"<GUID=XXX>;<SID=XXX>;distinguishedName" format
                string[] parts = dn.Split(new char[] { ';' });

                //Output each piece of the extended DN
                foreach (string part in parts)
                {
                    Console.WriteLine(part);
                }
            }
        }

        /// <summary>
        /// Listing 5.9 Modified
        /// </summary>
        [Test]
        public void SecurityDescriptorSearch()
        {            
            //explicitly create our searchroot
            DirectoryEntry searchRoot = TestUtils.GetDefaultPartition();

            using (searchRoot) //we are responsible for disposing
            {
                DirectorySearcher ds = new DirectorySearcher(
                    searchRoot,
                    "(cn=User1)",
                    new string[]{"ntSecurityDescriptor"}
                    );

                //Get the Security for this object
                ds.SecurityMasks = SecurityMasks.Dacl
                    | SecurityMasks.Group
                    | SecurityMasks.Owner;

                SearchResult sr = ds.FindOne();

                if (sr == null)
                    throw new Exception("No user found");

                byte[] descriptorBytes =
                    (byte[])sr.Properties["ntSecurityDescriptor"][0];


                ActiveDirectorySecurity ads = new ActiveDirectorySecurity();

                ads.SetSecurityDescriptorBinaryForm(
                    descriptorBytes,
                    AccessControlSections.All
                    );

                //helper function
                PrintSD(ads);

                AuthorizationRuleCollection rules = ads.GetAccessRules(
                    true,
                    true,
                    typeof(NTAccount)
                    );

                foreach (ActiveDirectoryAccessRule rule in rules)
                {
                    //helper function
                    PrintAce(rule);
                }
            }

            //Sample output:
            //
            //=====Security Descriptor=====
            //    Owner: S-1-450115865-2557621802-512
            //    Group: S-1-450115865-2557621802-512
            //=====ACE=====
            //    Identity: NT AUTHORITY\SELF
            //    AccessControlType: Allow
            //    ActiveDirectoryRights: ExtendedRight
            //    InheritanceType: None
            //    ObjectType: ab721a53-1e2f-11d0-9819-00aa0040529b
            //    InheritedObjectType: <null>
            //    ObjectFlags: ObjectAceTypePresent
        }

        //helper in SecurityDescriptorSearch()
        private void PrintAce(ActiveDirectoryAccessRule rule)
        {
            Console.WriteLine("=====ACE=====");
            Console.Write("    Identity: ");
            Console.WriteLine(rule.IdentityReference.ToString());
            Console.Write("    AccessControlType: ");
            Console.WriteLine(rule.AccessControlType.ToString());
            Console.Write("    ActiveDirectoryRights: ");
            Console.WriteLine(
                rule.ActiveDirectoryRights.ToString());
            Console.Write("    InheritanceType: ");
            Console.WriteLine(rule.InheritanceType.ToString());
            Console.Write("    ObjectType: ");
            if (rule.ObjectType == Guid.Empty)
                Console.WriteLine("<null>");
            else
                Console.WriteLine(rule.ObjectType.ToString());

            Console.Write("    InheritedObjectType: ");
            if (rule.InheritedObjectType == Guid.Empty)
                Console.WriteLine("<null>");
            else
                Console.WriteLine(
                    rule.InheritedObjectType.ToString());
            Console.Write("    ObjectFlags: ");
            Console.WriteLine(rule.ObjectFlags.ToString());
        }

        //helper in SecurityDescriptorSearch()
        private void PrintSD(ActiveDirectorySecurity sd)
        {
            Console.WriteLine("=====Security Descriptor=====");
            Console.Write("    Owner: ");
            Console.WriteLine(sd.GetOwner(typeof(SecurityIdentifier))); //can use NTAccount for AD
            Console.Write("    Group: ");
            Console.WriteLine(sd.GetGroup(typeof(SecurityIdentifier)));
        }

        /// <summary>
        /// Listing 5.10 entry point - see async.cs
        /// </summary>
        [Test]
        public void AsynchSearch()
        {
            Async a = new Async();
            a.Search();
        }

        /// <summary>
        /// Listing 5.11 entry point - see partialresults.cs
        /// </summary>
        [Test]
        public void AsynchPartialResultsSearch()
        {
            PartialResults p = new PartialResults();
            p.Search();
        }

    }
}
