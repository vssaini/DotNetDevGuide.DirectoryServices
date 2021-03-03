using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

namespace DotNetDevGuide.DirectoryServices.Chapter11
{
    /// <summary>
    /// Listing 11.4 & 11.5 modified - a stripped down version of a
    /// strongly typed Group object
    /// </summary>
    public class Group : DirectoryEntry
    {
        const int ERR_DS_ATTRIBUTE_OR_VALUE_EXISTS = -2147016691;
        const int ERR_DS_NO_ATTRIBUTE_OR_VALUE = -2147016694;

        public Group(DirectoryEntry entry)
            : base(entry.NativeObject)
        {
        }

        public bool Remove(DirectoryEntry obj)
        {
            if (IsMember(obj))
            {
                this.Invoke(
                    "Remove",
                    new object[]{ obj.Path }
                    );
                return true;
            }
                return false;
        }
            
        public bool Add(DirectoryEntry obj)
        {
            if (!IsMember(obj))
            {
                this.Invoke(
                    "Add",
                    new object[]{ obj.Path }
                    );
                return true;
            }
                return false;
        }
            
        public bool IsMember(DirectoryEntry obj)
        {
            return (bool) this.Invoke(
                "IsMember",
                new object[]{ obj.Path }
                );
        }

        private bool Add(string memberDN)
        {
            try
            {
                this.Properties["member"].Add(memberDN);
                this.CommitChanges();
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.ErrorCode !=
                    ERR_DS_ATTRIBUTE_OR_VALUE_EXISTS)
                    throw;

                return false; //already a member
            }

            return true;
        }

        private bool Remove(string memberDN)
        {
            try
            {
                this.Properties["member"].Remove(memberDN);
                this.CommitChanges();
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.ErrorCode != ERR_DS_NO_ATTRIBUTE_OR_VALUE)
                    throw;

                return false; //not a member
            }

            return true;
        }
    }

}
