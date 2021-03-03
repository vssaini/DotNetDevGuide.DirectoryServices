using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

using NUnit.Framework;

namespace DotNetDevGuide.DirectoryServices.Chapter11
{
    class Chapter11
    {
        /// <summary>
        /// Listing 11.3 modified.
        /// </summary>
        [Test]
        public void CreateSecurityGroup()
        {
            string groupName = "Group1";

            //this is where the group will be created
            DirectoryEntry parent = TestUtils.CreateDirectoryEntry(
                "CN=Roles," + TestUtils.Settings.DefaultPartition);

            using (parent)
            {
                DirectoryEntry group = parent.Children.Add(
                    String.Format("CN={0}", groupName),
                    "group"
                    );

                using (group)
                {
                    //this is the default if not specified
                    GroupType type = GroupType.GlobalSecurity;

                    //sAMAccountName is not used by ADAM
                    group.Properties["sAMAccountName"].Add(groupName);
                    group.Properties["groupType"].Add((int)type);
                    group.CommitChanges();
                }
            }
        }

        /// <summary>
        /// Demonstrates how to use Listing 11.4 & Listing 11.5 - see group.cs
        /// </summary>
        [Test]
        public void UpdateGroupMembership()
        {
            DirectoryEntry entry = TestUtils.CreateDirectoryEntry(
                "CN=Group1,CN=Roles," + TestUtils.Settings.DefaultPartition);

            DirectoryEntry user = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            Group group = new Group(entry);
            
            using (group)
            using (user)
            {
                group.Add(user);
                group.Remove(user);
                Console.WriteLine("User {0} a member", group.IsMember(user) ? "is" : "is not");
            }
        }

        /// <summary>
        /// Demonstrates how to use Listing 11.6 - see GroupExpander2.cs
        /// </summary>
        [Test]
        public void ExpandGroupMembership()
        {
            DirectoryEntry entry = TestUtils.CreateDirectoryEntry(
                "CN=Group1,CN=Roles," + TestUtils.Settings.DefaultPartition);

            using (entry)
            {
                GroupExpander2 expander = new GroupExpander2(entry);

                foreach (string member in expander.Members)
                {
                    Console.WriteLine(member);
                }
            }
        }

        /// <summary>
        /// Demonstrates how to use Listing 11.7 - see GroupExpander.cs
        /// This is what we can use in v1.x.
        /// </summary>
        [Test]
        public void ExpandGroupMembershipAlt()
        {
            DirectoryEntry entry = TestUtils.CreateDirectoryEntry(
                "CN=Group1,CN=Roles," + TestUtils.Settings.DefaultPartition);

            DirectoryEntry root = TestUtils.GetDefaultPartition();

            using (root)
            using (entry)
            {
                GroupExpander expander = new GroupExpander(entry, root);

                foreach (string member in expander.Members)
                {
                    Console.WriteLine(member);
                }
            }
        }
    }
}
