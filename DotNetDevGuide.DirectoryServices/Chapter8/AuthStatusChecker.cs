using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

namespace DotNetDevGuide.DirectoryServices.Chapter8
{
    [Flags()]
    public enum IscRetFlags
    {
        ISC_RET_DELEGATE = 0x00000001,
        ISC_RET_MUTUAL_AUTH = 0x00000002,
        ISC_RET_REPLAY_DETECT = 0x00000004,
        ISC_RET_SEQUENCE_DETECT = 0x00000008,
        ISC_RET_CONFIDENTIALITY = 0x00000010,
        ISC_RET_USE_SESSION_KEY = 0x00000020,
        ISC_RET_USED_COLLECTED_CREDS = 0x00000040,
        ISC_RET_USED_SUPPLIED_CREDS = 0x00000080,
        ISC_RET_ALLOCATED_MEMORY = 0x00000100,
        ISC_RET_USED_DCE_STYLE = 0x00000200,
        ISC_RET_DATAGRAM = 0x00000400,
        ISC_RET_CONNECTION = 0x00000800,
        ISC_RET_INTERMEDIATE_RETURN = 0x00001000,
        ISC_RET_CALL_LEVEL = 0x00002000,
        ISC_RET_EXTENDED_ERROR = 0x00004000,
        ISC_RET_STREAM = 0x00008000,
        ISC_RET_INTEGRITY = 0x00010000,
        ISC_RET_IDENTIFY = 0x00020000,
        ISC_RET_NULL_SESSION = 0x00040000,
        ISC_RET_MANUAL_CRED_VALIDATION = 0x00080000,
        ISC_RET_RESERVED1 = 0x00100000,
        ISC_RET_FRAGMENT_ONLY = 0x00200000
    }

    public class AuthStatusChecker
    {
        private enum AdsOption
        {
            ADS_OPTION_ACCUMULATIVE_MODIFICATION = 8,
            ADS_OPTION_MUTUAL_AUTH_STATUS = 4,
            ADS_OPTION_PAGE_SIZE = 2,
            ADS_OPTION_PASSWORD_METHOD = 7,
            ADS_OPTION_PASSWORD_PORTNUMBER = 6,
            ADS_OPTION_QUOTA = 5,
            ADS_OPTION_REFERRALS = 1,
            ADS_OPTION_SECURITY_MASK = 3,
            ADS_OPTION_SERVERNAME = 0
        }

        public static bool IsMutuallyAuthenticated(DirectoryEntry entry)
        {
            IscRetFlags authStatus = GetAuthStatus(entry);

            if ((IscRetFlags.ISC_RET_MUTUAL_AUTH & authStatus) ==
                IscRetFlags.ISC_RET_MUTUAL_AUTH)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static IscRetFlags GetAuthStatus(DirectoryEntry entry)
        {
            object val = entry.Invoke(
                "GetOption",
                new object[] { AdsOption.ADS_OPTION_MUTUAL_AUTH_STATUS }
                );

            return (IscRetFlags)val;
        }
    }

}
