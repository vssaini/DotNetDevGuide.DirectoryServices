using System;
using System.Collections;
using System.Text;
using System.DirectoryServices;

namespace DotNetDevGuide.DirectoryServices.Chapter11
{
    /// <summary>
    /// Listing 11.7 full listing - compatible with v1.x
    /// </summary>
    public class GroupExpander
    {
        DirectoryEntry searchRoot;
        ArrayList members;
        Hashtable processed;

        const string DN_ATTRIB = "distinguishedName";

        public GroupExpander(
            DirectoryEntry group,
            DirectoryEntry searchRoot)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            //a null searchRoot can lead to unexpected
            //behavior, especially with ADAM
            if (searchRoot == null)
                throw new ArgumentNullException("group");

            this.searchRoot = searchRoot;
            this.processed = new Hashtable();

            this.members = Expand(group);
        }

        public ArrayList Members
        {
            get { return this.members; }
        }

        public static ArrayList RangeExpansion(
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


        private ArrayList Expand(DirectoryEntry group)
        {
            ArrayList al = new ArrayList(5000);
            string dn = group.Properties[DN_ATTRIB][0].ToString();

            if (!this.processed.ContainsKey(dn))
            {
                this.processed.Add(dn, null);
            }

            //first we find all members of nested
            //groups, then the direct members
            string filter = String.Format(
                "(&(objectClass=group)(memberOf={0}))",
                dn
                );

            DirectorySearcher ds = new DirectorySearcher(
                this.searchRoot,
                filter
                );

            using (SearchResultCollection src = ds.FindAll())
            {
                string srDN = null;
                foreach (SearchResult sr in src)
                {
                    srDN = (string)sr.Properties[DN_ATTRIB][0];

                    if (!this.processed.ContainsKey(srDN))
                    {
                        using (DirectoryEntry grp =
                            sr.GetDirectoryEntry())
                        {
                            al.AddRange(Expand(grp));
                        }
                    }
                }
            }

            foreach (string member in RangeExpansion(group, "member"))
            {
                //in case our nested groups contained the
                //same members, we need to check for uniqueness
                if (!this.processed.ContainsKey(member))
                {
                    this.processed.Add(member, null);
                    al.Add(member);
                }
            }

            return al;
        }
    }
}
