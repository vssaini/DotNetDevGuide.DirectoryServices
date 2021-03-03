using System;
using System.Collections;
using System.Text;
using System.DirectoryServices;
using System.Security;
using System.Runtime.InteropServices;

namespace DotNetDevGuide.DirectoryServices.Chapter11
{
    /// <summary>
    /// Listing 11.6 in full - for 2.0 only
    /// </summary>
    public class GroupExpander2
    {
        DirectoryEntry group;
        ArrayList members;
        Hashtable processed;

        public GroupExpander2(DirectoryEntry group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            this.group = group;
            this.processed = new Hashtable();
            this.processed.Add(
                this.group.Properties[
                    "distinguishedName"][0].ToString(),
                null
                );

            this.members = Expand(this.group);
        }

        public ArrayList Members
        {
            get { return this.members; }
        }

        private ArrayList Expand(DirectoryEntry group)
        {
            ArrayList al = new ArrayList(5000);
            string oc = "objectClass";

            DirectorySearcher ds = new DirectorySearcher(
                group,
                "(objectClass=*)",
                new string[] {
                "member",
                "distinguishedName",
                "objectClass" },
                SearchScope.Base
                );

            ds.AttributeScopeQuery = "member";
            ds.PageSize = 1000;

            using (SearchResultCollection src = ds.FindAll())
            {
                string dn = null;
                foreach (SearchResult sr in src)
                {
                    dn = (string)
                        sr.Properties["distinguishedName"][0];

                    if (!this.processed.ContainsKey(dn))
                    {
                        this.processed.Add(dn, null);

                        //oc == "objectClass", we had to 
                        //truncate to fit in book.
                        //if it is a group, do this recursively
                        if (sr.Properties[oc].Contains("group"))
                        {
                            SetNewPath(this.group, dn);
                            al.AddRange(Expand(this.group));
                        }
                        else
                            al.Add(dn);
                    }
                }
            }
            return al;
        }

        //we will use IADsPathName utility function instead
        //of parsing string values.  This particular function
        //allows us to replace only the DN portion of a path
        //and leave the server and port information intact
        private void SetNewPath(DirectoryEntry entry, string dn)
        {
            IAdsPathname pathCracker = (IAdsPathname)new Pathname();

            pathCracker.Set(entry.Path, 1);
            pathCracker.Set(dn, 4);

            entry.Path = pathCracker.Retrieve(5);
        }
    }

    [ComImport, Guid("D592AED4-F420-11D0-A36E-00C04FB950DC"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface IAdsPathname
    {
        [SuppressUnmanagedCodeSecurity]
        int Set([In, MarshalAs(UnmanagedType.BStr)] string bstrADsPath, [In, MarshalAs(UnmanagedType.U4)] int lnSetType);
        int SetDisplayType([In, MarshalAs(UnmanagedType.U4)] int lnDisplayType);
        [return: MarshalAs(UnmanagedType.BStr)]
        [SuppressUnmanagedCodeSecurity]
        string Retrieve([In, MarshalAs(UnmanagedType.U4)] int lnFormatType);
        [return: MarshalAs(UnmanagedType.U4)]
        int GetNumElements();
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetElement([In, MarshalAs(UnmanagedType.U4)] int lnElementIndex);
        void AddLeafElement([In, MarshalAs(UnmanagedType.BStr)] string bstrLeafElement);
        void RemoveLeafElement();
        [return: MarshalAs(UnmanagedType.Interface)]
        object CopyPath();
        [return: MarshalAs(UnmanagedType.BStr)]
        [SuppressUnmanagedCodeSecurity]
        string GetEscapedElement([In, MarshalAs(UnmanagedType.U4)] int lnReserved, [In, MarshalAs(UnmanagedType.BStr)] string bstrInStr);
        int EscapedMode { get; [SuppressUnmanagedCodeSecurity] set; }
    }

    [ComImport, Guid("080d0d78-f421-11d0-a36e-00c04fb950dc")]
    internal class Pathname
    {
    }

}
