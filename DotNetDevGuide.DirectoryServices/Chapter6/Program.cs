using System;
using System.Collections;
using System.Text;
using System.Reflection;
using System.DirectoryServices;
using ActiveDs;

using NUnit.Framework;

namespace DotNetDevGuide.DirectoryServices.Chapter6
{
    class Chapter6
    {
        /// <summary>
        /// Listing 6.1 & 6.3 modified and combined to show
        /// all three behaviors.
        /// </summary>
        [Test]
        public void ReadLargeInteger()
        {
            DirectoryEntry entry = TestUtils.GetDefaultPartition();

            using (entry)
            {
                //the book originally had 'lockoutDuration', but we are
                //using this attribute so it will work with ADAM as well
                object val = entry.Properties["usnChanged"].Value;
                    
                Console.WriteLine(
                    "Using ActiveDs Interop: {0}",
                     GetInt64(val)
                    );

                Console.WriteLine(
                    "Using Reflection: {0}",
                    LongFromLargeInteger(val)
                    );

                Console.WriteLine("Now Demonstrating SearchResult Marshalling from 6.3");

                //use a base-scoped search
                DirectorySearcher ds = new DirectorySearcher(
                    entry,  //this is our targeted entry
                    "(objectClass=*)", //base level filter
                    null,
                    SearchScope.Base
                    );

                //we essentially convert our DirectoryEntry
                //to a SearchResult to get the desired behavior
                SearchResult result = ds.FindOne();

                //now we get the different marshaling
                //behavior we want (no IADsLargeInteger)
                 long usnChanged = 
                     (long) result.Properties["usnChanged"][0];

                 Console.WriteLine("SearchResult Marshalling: {0}",
                     usnChanged
                 );
            }
        }
        
        //using the RCW interop
        private Int64 GetInt64(object largeIntVal)
        {
            if (largeIntVal == null)
                throw new ArgumentNullException("longInt");

            IADsLargeInteger largeInt;
            largeInt = (IADsLargeInteger) largeIntVal;   
            return (long)largeInt.HighPart << 32 | 
                (uint)largeInt.LowPart;
        }

        //decodes IADsLargeInteger objects into Int64 format (long)
        //using Reflection instead of Interop
        private long LongFromLargeInteger(object largeInteger)
        {
            System.Type type = largeInteger.GetType();

            int highPart = (int)type.InvokeMember(
                "HighPart",
                BindingFlags.GetProperty,
                null,
                largeInteger,
                null
                );

            int lowPart = (int)type.InvokeMember(
                "LowPart",
                BindingFlags.GetProperty,
                null,
                largeInteger,
                null
                );

            return (long)highPart << 32 | (uint)lowPart;
        }

        /// <summary>
        /// Listing 6.4 modified.  Uses DirectoryEntry and Reflection to decode
        /// DN-With-Binary syntax.
        /// </summary>
        [Test]
        public void ReadDnWithBinary()
        {
            DirectoryEntry root = TestUtils.GetDefaultPartition();

            using (root)
            {
                if (root.Properties.Contains("wellKnownObjects"))
                {
                    foreach (object o in root.Properties["wellKnownObjects"])
                    {
                        byte[] guidBytes;
                        string dn;

                        //use our helper function
                        DecodeDnWithBinary(
                            o,
                            out guidBytes,
                            out dn
                            );

                        Console.WriteLine(
                            "Guid: {0}",
                            new Guid(guidBytes).ToString("p")
                            );

                        Console.WriteLine(
                            "DN: {0}",
                            dn
                            );
                    }
                }
            }
        }

        //This is our reflection-based helper for
        //decoding DN-With-Binary attributes
        private void DecodeDnWithBinary(
            object dnWithBinary,
            out byte[] binaryPart,
            out string dnString)
        {
            System.Type type = dnWithBinary.GetType();

            binaryPart = (byte[])type.InvokeMember(
                "BinaryValue",
                BindingFlags.GetProperty,
                null,
                dnWithBinary,
                null
                );

            dnString = (string)type.InvokeMember(
                "DNString",
                BindingFlags.GetProperty,
                null,
                dnWithBinary,
                null
                );
        }

        /// <summary>
        /// Listing 6.5 modified.  Uses SearchResult for marshaling.
        /// Notice it has same output as ReadDnWithBinary()
        /// </summary>
        [Test]
        public void ReadDnWithBinaryViaSearchResult()
        {
            DirectoryEntry root = TestUtils.GetDefaultPartition();

            using (root)
            {
                DirectorySearcher ds = new DirectorySearcher(
                    root,
                    "(objectClass=*)",
                    new string[] { "wellKnownObjects" },
                    SearchScope.Base
                    );

                SearchResult sr = ds.FindOne();

                if (sr != null)
                {
                    foreach (string s in sr.Properties["wellKnownObjects"])
                    {
                        string[] parts = s.Split(new char[] { ':' });

                        Console.WriteLine(
                            "Guid: {0}",
                            new Guid(parts[2]).ToString("b")
                            );

                        Console.WriteLine(
                            "DN: {0}",
                            parts[3]
                            );
                    }
                }
            }
        }

        /// <summary>
        /// Listing 6.7 Modified
        /// </summary>
        [Test]
        public void ReadSecurityDescriptorWithInterop()
        {
           DirectoryEntry entry = TestUtils.GetDefaultPartition();

            using (entry)
            {
                IADsSecurityDescriptor sd = 
                    (IADsSecurityDescriptor)entry.Properties["ntSecurityDescriptor"].Value;

                Console.WriteLine("Owner= {0}", sd.Owner);
            }
        }

        /// <summary>
        /// Shows how to use Listing 6.8
        /// </summary>
        [Test]
        public void EnumerateLargeAttributes()
        {
            //point this to a big group in your directory.
            DirectoryEntry bigGroup = TestUtils.CreateDirectoryEntry(
                "CN=Ranged Group,CN=Roles," + TestUtils.Settings.DefaultPartition
                );

            using (bigGroup)
            {
                foreach (string s in RangeExpansion(bigGroup, "member"))
                {
                    Console.WriteLine(s);
                }
            }
        }

        /// <summary>
        /// Listing 6.8 - Range Expansion method used for
        /// large attributes.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public ArrayList RangeExpansion(
             DirectoryEntry entry,
             string attribute)
        {
            ArrayList al = new ArrayList(5000);
            int idx = 0;

            //zero based index, so less 1
            int step = entry.Properties[attribute].Count - 1;

            string range = String.Format(
                "{0};range={{0}}-{{1}}",
                attribute
                );

            string currentRange = String.Format(range, idx, step);

            DirectorySearcher ds = new DirectorySearcher(
                entry,
                String.Format("({0}=*)", attribute),
                new string[] { currentRange },
                System.DirectoryServices.SearchScope.Base
                );

            bool lastSearch = false;
            SearchResult sr = null;

            while (true)
            {
                if (!lastSearch)
                {
                    ds.PropertiesToLoad.Clear();
                    ds.PropertiesToLoad.Add(currentRange);

                    sr = ds.FindOne();
                }

                if (sr != null)
                {
                    if (sr.Properties.Contains(currentRange))
                    {
                        foreach (object dn in sr.Properties[currentRange])
                        {
                            al.Add(dn);
                            idx++;
                        }

                        //our exit condition
                        if (lastSearch)
                            break;

                        currentRange = String.Format(
                            range,
                            idx,
                            (idx + step)
                            );
                    }
                    else
                    {
                        //one more search
                        lastSearch = true;
                        currentRange = String.Format(range, idx, "*");
                    }
                }
                else
                    break;
            }
            return al;
        }

        /// <summary>
        /// Listing 6.9 - this is used by Listing 6.10 as sample
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public IADsLargeInteger GetLargeInteger(long val)
        {
            IADsLargeInteger largeInt = new LargeInteger();
            largeInt.HighPart = (int)(val >> 32);
            largeInt.LowPart = (int)(val & 0xFFFFFFFF);
            return largeInt;
        }

        /// <summary>
        /// Listing 6.10 modified.  Sets an account's expiration.
        /// </summary>
        [Test]
        public void SetLargeInteger()
        {
            //point this to your own user
            DirectoryEntry entry = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            using (entry)
            {
                //Get the Int64 for 2 years from now
                Int64 expireTicks = DateTime.Now.AddYears(2).ToFileTime();

                //set it on the account
                entry.Properties["accountExpires"].Value = GetLargeInteger(expireTicks);

                //save it
                entry.CommitChanges();
            }
        }

        /// <summary>
        /// Listing 6.12 Modified
        /// </summary>
        [Test]
        public void SetDnWithBinary()
        {
            IADsDNWithBinary dnb = new DNWithBinaryClass();

            DirectoryEntry entry = TestUtils.GetDefaultPartition();

            //we would probably never use this example in actual practice...
            dnb.BinaryValue = Guid.NewGuid().ToByteArray();

            //we are setting to same name (e.g."DC=foo,DC=blah") here
            dnb.DNString = TestUtils.Settings.DefaultPartition;

            using (entry)
            {
                entry.Properties["otherWellKnownObjects"].Value = dnb;
                entry.CommitChanges();
            }
        }

        /// <summary>
        /// Listing 6.13 Modified.  Unfortunately, it is hard to demo this one,
        /// so this gives the general idea here.  You need to add your own 
        /// modifications to any descriptors.
        /// </summary>
        [Test]
        public void SetSecurityDescriptor()
        {
            IADsSecurityDescriptor sd;

            DirectoryEntry entry = TestUtils.GetDefaultPartition();

            using (entry)
            {
                sd = (IADsSecurityDescriptor) 
                    entry.Properties["ntSecurityDescriptor"].Value;

                //modify the security descriptor here...
                //omitted code for modifications... you get the idea.

                //then set it back.
                entry.Properties["ntSecurityDescriptor"].Value = sd;
                entry.CommitChanges();
            }
        }

    }
}
