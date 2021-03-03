using System;
using System.Collections;
using System.Text;
using System.DirectoryServices;
using System.Security.Principal;
using System.Security.AccessControl;

using ActiveDs;

using NUnit.Framework;

namespace DotNetDevGuide.DirectoryServices.Chapter8
{
    class Chapter8
    {
        /// <summary>
        /// Entry point to Listing 8.1 - see authastatuschecker.cs
        /// </summary>
        [Test]
        public void MutualAuth()
        {
            DirectoryEntry entry = TestUtils.GetDefaultPartition();

            using (entry)
            {
                Console.WriteLine(AuthStatusChecker.GetAuthStatus(entry));
                Console.WriteLine(AuthStatusChecker.IsMutuallyAuthenticated(entry));
            }
        }

        /// <summary>
        /// Listing 8.2 Modified.  Also see Listing 5.9 (SecurityDescriptorSearch())
        /// to see the similarities.
        /// </summary>
        [Test]
        public void GetObjectSecurity()
        {
            DirectoryEntry entry = TestUtils.GetDefaultPartition();

            ActiveDirectorySecurity sec = entry.ObjectSecurity;

            PrintSD(sec);

            AuthorizationRuleCollection rules = null;
            rules = sec.GetAccessRules(
                true, true, typeof(NTAccount));

            foreach (ActiveDirectoryAccessRule rule in rules)
            {
                PrintAce(rule);
            }
        }

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

        //repeated from Listing 5.9's helper as well
        private void PrintSD(ActiveDirectorySecurity sd)
        {
            Console.WriteLine("=====Security Descriptor=====");
            Console.Write("    Owner: ");
            //change this to SecurityIdentifier from NTAccount
            //for ADAM to work.
            Console.WriteLine(sd.GetOwner(typeof(NTAccount)));
            Console.Write("    Group: ");
            //same here.
            Console.WriteLine(sd.GetGroup(typeof(NTAccount)));
        }

        /// <summary>
        /// Listing 8.3 Modified
        /// </summary>
        [Test]
        public void UpdateSecurityDescriptor()
        {
            //point this to any object (I chose a user)
            DirectoryEntry entry = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            ActiveDirectorySecurity sec = entry.ObjectSecurity;
            ActiveDirectoryAccessRule rule = new ActiveDirectoryAccessRule(
                new NTAccount("mydomain", "super.user"),
                ActiveDirectoryRights.GenericAll,
                AccessControlType.Allow
                );

            sec.AddAccessRule(rule);
            
            entry.CommitChanges();
        }

        /// <summary>
        /// Listing 8.4 - entry point - see schemaguidconverter.cs
        /// Shows how to use this helper class.
        /// </summary>
        [Test]
        public void SchemaGuidTests()
        {
            string schemaDN;
            string extendedRightsDN = "CN=Extended-Rights,";
            string schemaAtt = "schemaNamingContext";
            string configAtt = "configurationNamingContext";

            Guid samGuid = new Guid("3e0abfd0-126a-11d0-a060-00aa006c33ed");
            Guid cpGuid =  new Guid("ab721a53-1e2f-11d0-9819-00aa0040529b");

            DirectoryEntry rootDse = TestUtils.CreateDirectoryEntry("RootDSE");

            schemaDN = (string)rootDse.Properties[schemaAtt].Value;
            extendedRightsDN += (string)rootDse.Properties[configAtt].Value;

            DirectoryEntry schemaRoot = TestUtils.CreateDirectoryEntry(schemaDN);
            DirectoryEntry extendedRightsRoot = TestUtils.CreateDirectoryEntry(extendedRightsDN);

            using (rootDse)
            using (schemaRoot)
            using (extendedRightsRoot)
            {
                Console.WriteLine(
                    "cn={0}",
                    SchemaGuidConverter.GetSchemaIDGuid("cn", schemaRoot)
                    );

                Console.WriteLine(
                    "Validated-SPN={0}",
                    SchemaGuidConverter.GetRightsGuid("Validated-SPN", extendedRightsRoot)
                    );

                Console.WriteLine(
                    "{0}={1}",
                    samGuid.ToString("B"),
                    SchemaGuidConverter.GetNameForSchemaGuid(
                        samGuid,
                        schemaRoot
                        )
                    );

                Console.WriteLine(
                    "{0}={1}",
                    cpGuid.ToString("B"),
                    SchemaGuidConverter.GetNameForRightsGuid(
                        cpGuid,
                        extendedRightsRoot
                        )
                    );
            }
        }

        /// <summary>
        /// Listing 8.5 Modified.  Uses ActiveDs Interop
        /// </summary>
        [Test]
        public void GetSecurityDescriptorViaInterop()
        {
            DirectoryEntry entry = TestUtils.GetDefaultPartition();

            IADsSecurityDescriptor sd = (IADsSecurityDescriptor)
                entry.Properties["ntSecurityDescriptor"].Value;
            IADsAccessControlList dacl =
                (IADsAccessControlList)sd.DiscretionaryAcl;

            foreach (IADsAccessControlEntry ace in (IEnumerable)dacl)
            {
                Console.WriteLine("Trustee: {0}", ace.Trustee);
                Console.WriteLine("AccessMask: {0}", ace.AccessMask);
                Console.WriteLine("Access Type: {0}", ace.AceType);
                Console.WriteLine("Access Flags: {0}", ace.AceFlags);
            }
        }

        /// <summary>
        /// Listing 8.6 Modified.  Updates DACL using ActiveDs interop.
        /// </summary>
        [Test]
        public void UpdateSecurityDescriptorViaInterop()
        {
            //point this to any object (I chose a user)
            DirectoryEntry entry = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            IADsAccessControlEntry newAce = new AccessControlEntryClass();
            
            IADsSecurityDescriptor sd = (IADsSecurityDescriptor)
                entry.Properties["ntSecurityDescriptor"].Value;
            
            IADsAccessControlList dacl =
                (IADsAccessControlList)sd.DiscretionaryAcl;

            newAce.Trustee = @"mydomain\some user"; //update this to your needs
            newAce.AccessMask = -1;  //all flags
            newAce.AceType = 0;  //access allowed
            dacl.AddAce(newAce);
            sd.DiscretionaryAcl = dacl;
            entry.Properties["ntSecurityDescriptor"].Value = sd;
            entry.CommitChanges();
        }

    }
}
