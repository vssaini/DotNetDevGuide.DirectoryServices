using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

using NUnit.Framework;

namespace DotNetDevGuide.DirectoryServices
{
    class Chapter3
    {
        /// <summary>
        /// Taken from Listing 3.3 and modified slightly to work with configuration
        /// </summary>
        [Test]
        public void GuidBindingTest()
        {            
            Guid objectGuid;
            Guid guidProperty;
            Guid convertedGuid;
            string nativeGuidString;
            string octetGuidProperty;

            using (DirectoryEntry domainRoot = TestUtils.GetDefaultPartition())
            {
                guidProperty = domainRoot.Guid;
                nativeGuidString = domainRoot.NativeGuid;
                objectGuid = new Guid((byte[])
                domainRoot.Properties["objectGuid"].Value);
            }

            octetGuidProperty = BitConverter.ToString(guidProperty.ToByteArray()).Replace("-", "").ToLower();

            byte[] nativeGuidBytes = new byte[16];
            
            for (int i = 0; i < 16; i++)
            {
                nativeGuidBytes[i] = Byte.Parse(
                    nativeGuidString.Substring(i * 2, 2),
                    System.Globalization.NumberStyles.HexNumber
                    );
            }

            convertedGuid = new Guid(nativeGuidBytes);
            
            Console.WriteLine("Guid property (COM syntax): {0}", guidProperty.ToString("D"));
            Console.WriteLine("NativeGuid (octet style): {0}", nativeGuidString);
            Console.WriteLine("Guid property (dashes removed): {0}", guidProperty.ToString("N"));
            Console.WriteLine("objectGuid (COM syntax): {0}", objectGuid.ToString("D"));
            Console.WriteLine("Guid Property as octet string: {0}", octetGuidProperty);
            Console.WriteLine("NativeGuid converted to Guid: {0}", convertedGuid.ToString("D"));
            
            string comBindingSyntax = String.Format("<GUID={0}>", guidProperty.ToString("D"));
            string nativeBindingSyntax = String.Format("<GUID={0}>", nativeGuidString);
            string invalidBindingSyntax = String.Format("<GUID={0}>", guidProperty.ToString("N"));

            using (DirectoryEntry entry1 = TestUtils.CreateDirectoryEntry(comBindingSyntax))
            {
                entry1.RefreshCache(); //force bind
                Console.WriteLine(
                "{0} worked as expected.", comBindingSyntax);
            }

            using (DirectoryEntry entry2 = TestUtils.CreateDirectoryEntry(nativeBindingSyntax))
            {
                entry2.RefreshCache(); //force bind
                Console.WriteLine(
                "{0} worked as expected.", nativeBindingSyntax);
            }

            using (DirectoryEntry entry3 = TestUtils.CreateDirectoryEntry(invalidBindingSyntax))
            {
                try
                {
                    entry3.RefreshCache(); //force bind
                }
                //this should fail unless there just happens
                //to be another object with the other GUID.
                //This is extremely unlikely!
                catch
                {
                    Console.WriteLine(
                    "{0} failed as expected.",
                    invalidBindingSyntax);
                }
            }

        }

        /// <summary>
        /// Listing 3.4 modified for config
        /// </summary>
        [Test]
        public void WellKnownGuidBind()
        {
            using (DirectoryEntry domainDNS = TestUtils.GetDefaultPartition())
            {
                if (!domainDNS.Properties.Contains("domainDNS"))
                    Assert.Fail("Well-Known GUID Binding only works with AD");

                string path = String.Format(
                    "<WKGUID=a9d1ca15768811d1aded00c04fd8d5cd,{0)>",
                    domainDNS.Properties["distinguishedName"][0]
                    );

                using (DirectoryEntry de = TestUtils.CreateDirectoryEntry(path))
                {
                    Console.WriteLine("Successfully Bound To: {0}",
                        de.Properties["distinguishedName"].Value);
                }
            }
        }

        /// <summary>
        /// Useful utility to format binary strings - from 3.5
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string BuildOctetString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
            }
            return sb.ToString();
        } 

        /// <summary>
        /// From 3.6 Modified
        /// </summary>
        [Test]
        public void SidBind()
        {
            //You will need to update the "User1" portion to find a real user by CN
            using (DirectoryEntry user = TestUtils.FindUserByCN("User1"))
            {
                Assert.IsNotNull(user, "Did not find the user");

                //retrieve the SID
                byte[] sidBytes =
                    user.Properties["objectSid"].Value as byte[];

                //format the bytes using the BuildOctetString function
                string adsPath = String.Format(
                    TestUtils.BuildAdsPath("<SID={0}>"),
                    BuildOctetString(sidBytes)
                    );

                //Now Bind using SID Format
                DirectoryEntry sidBind = TestUtils.CreateDirectoryEntry(adsPath);

                using (sidBind)
                {
                    //force the bind
                    object native = sidBind.NativeObject;
                    Assert.IsNotNull(native);
                    Console.WriteLine("Bound to {0}", sidBind.Path);
                }
            }
        }

        /// <summary>
        /// Taken From 3.11 and modified
        /// </summary>
        [Test]
        public void GetDefaultNamingContext()
        {
            using (DirectoryEntry rootDSE = TestUtils.CreateDirectoryEntry("RootDSE"))
            {
                //we might not have a defaultNamingContext defined when using ADAM
                Assert.IsNotNull(rootDSE.Properties["defaultNamingContext"].Value, "Did not find a defaultNamingContext");

                Console.WriteLine("Default Naming Context: {0}", rootDSE.Properties["defaultNamingContext"].Value);
            }
        }

        /// <summary>
        /// Listing 3.13 modified
        /// </summary>
        [Test]
        public void CreateUserSimple()
        {
            //update this to point to a valid container in your domain
            string parentOU = "OU=Users," + TestUtils.Settings.DefaultPartition;
            
            using (DirectoryEntry entry = TestUtils.CreateDirectoryEntry(parentOU))
            {
                using (DirectoryEntry newUser =
                     entry.Children.Add("CN=John Doe", "user"))
                {
                    //add mandatory attribs
                    //newUser.Properties["sAMAccountName"].Add("jdoe1"); //will fail for ADAM

                    //add optional ones
                    newUser.Properties["sn"].Add("Doe");
                    //Last Name
                    newUser.Properties["givenName"].Add("John");
                    //First Name
                    newUser.Properties["description"].Add("Average Guy");
                    //description
                    newUser.Properties["telephoneNumber"].Add("555-1212");
                    //phone number
                    newUser.Properties["displayName"].Add("Doe, John");
                    //this is how it will be presented to users

                    //update the directory
                    newUser.CommitChanges();
                }
            }
        }

        /// <summary>
        /// Listing 3.14 modified
        /// </summary>
        [Test]
        public void DeleteObjectSimple()
        {
            //set this to a container that holds the object in CreateUserSimple()
            string parentPath = "OU=ParentContainer,DC=domain,DC=com";

            DirectoryEntry parent = TestUtils.CreateDirectoryEntry(parentPath);

            using (parent)
            {
                //find the child to remove
                DirectoryEntry child = parent.Children.Find("CN=John Doe");

                using (child)
                {
                    //immediately delete
                    parent.Children.Remove(child);
                }
            }
        }

        /// <summary>
        /// Listing 3.15 modified
        /// </summary>
        [Test]
        public void DeleteObjectSimpleAlt()
        {
            string adsPath = "CN=ObjectToBeDeleted,OU=ParentContainer,DC=domain,DC=com";

            DirectoryEntry entry = TestUtils.CreateDirectoryEntry(adsPath);

            using (entry)
            using (DirectoryEntry parent = entry.Parent)
            {
                parent.Children.Remove(entry);
            }
        }

        /// <summary>
        /// Listing 3.16 modified - be careful here not to erase the whole OU or something
        /// on accident.
        /// </summary>
        [Test]
        public void DeleteTree()
        {
            //update this path - BE CAREFUL!!!!!!
            string objectPath = "CN=ObjectToBeDeleted,OU=ParentContainer,DC=domain,DC=com";

            using (DirectoryEntry entry = TestUtils.CreateDirectoryEntry(objectPath))
            {
                entry.DeleteTree(); //remove object and all children
            }
        }

        /// <summary>
        /// Listing 3.17 revised
        /// </summary>
        [Test]
        public void MoveObject()
        {
            string objectPath = "CN=ObjectToBeMoved,OU=ParentContainer,DC=domain,DC=com";

            using (DirectoryEntry entry = TestUtils.CreateDirectoryEntry(objectPath))
            {
                string parentPath = "OU=NewParentContainer,DC=domain,DC=com";
                using (DirectoryEntry newParent = TestUtils.CreateDirectoryEntry(parentPath))
                {
                    //choose one or the other
                    //entry.MoveTo(newParent); //move         
                    entry.MoveTo(newParent, "CN=NewObjectName");  //move and rename
                }
            }
        }

        /// <summary>
        /// Listing 3.18 Revised for config
        /// </summary>
        [Test]
        public void RenameObject()
        {
            string adsPath = "CN=ObjectToBeRenamed,OU=ParentContainer,DC=domain,DC=com";

            DirectoryEntry entry = TestUtils.CreateDirectoryEntry(adsPath);

            using (entry)
            {
                entry.Rename("CN=NewObjectName"); //rename only
            }
        }

        /// <summary>
        /// Listing 3.19 revised.  This shows how to use the MoveTo for
        /// Renaming purposes without moving.  Intended to show why Rename
        /// method exists... don't do this in production.
        /// </summary>
        [Test]
        public void RenameObjectAlt()
        {
            string adsPath = "CN=ObjectToBeRenamed,OU=ParentContainer,DC=domain,DC=com";

            DirectoryEntry entry = TestUtils.CreateDirectoryEntry(adsPath);

            using (entry)
            using (DirectoryEntry parent = entry.Parent)
            {
                //Equivalent to .Rename, but more confusing...
                entry.MoveTo(parent, "CN=NewObjectName"); //rename only
            }
        }
    }
}
