using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace DotNetDevGuide.DirectoryServices
{
    internal class Constants
    {
        //Group Constants
        internal const uint ADS_GROUP_TYPE_BUILTIN_LOCAL_GROUP = 0x00000001;
        internal const uint ADS_GROUP_TYPE_GLOBAL_GROUP = 0x00000002;
        internal const uint ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP = 0x00000004;
        internal const uint ADS_GROUP_TYPE_SECURITY_ENABLED = 0x80000000;
        internal const uint ADS_GROUP_TYPE_UNIVERSAL_GROUP = 0x00000008;

        //DsGetDcName Flag Values
        internal const uint DS_FORCE_REDISCOVERY = 0x00000001;
        internal const uint DS_DIRECTORY_SERVICE_REQUIRED = 0x00000010;
        internal const uint DS_DIRECTORY_SERVICE_PREFERRED = 0x00000020;
        internal const uint DS_GC_SERVER_REQUIRED = 0x00000040;
        internal const uint DS_PDC_REQUIRED = 0x00000080;
        internal const uint DS_BACKGROUND_ONLY = 0x00000100;
        internal const uint DS_IP_REQUIRED = 0x00000200;
        internal const uint DS_KDC_REQUIRED = 0x00000400;
        internal const uint DS_TIMESERV_REQUIRED = 0x00000800;
        internal const uint DS_WRITABLE_REQUIRED = 0x00001000;
        internal const uint DS_GOOD_TIMESERV_PREFERRED = 0x00002000;
        internal const uint DS_AVOID_SELF = 0x00004000;
        internal const uint DS_ONLY_LDAP_NEEDED = 0x00008000;

    }

    [Flags]
    internal enum DsGetDcFlags : uint
    {
        DS_FORCE_REDISCOVERY = 0x00000001,
        DS_DIRECTORY_SERVICE_REQUIRED = 0x00000010,
        DS_DIRECTORY_SERVICE_PREFERRED = 0x00000020,
        DS_GC_SERVER_REQUIRED = 0x00000040,
        DS_PDC_REQUIRED = 0x00000080,
        DS_BACKGROUND_ONLY = 0x00000100,
        DS_IP_REQUIRED = 0x00000200,
        DS_KDC_REQUIRED = 0x00000400,
        DS_TIMESERV_REQUIRED = 0x00000800,
        DS_WRITABLE_REQUIRED = 0x00001000,
        DS_GOOD_TIMESERV_PREFERRED = 0x00002000,
        DS_AVOID_SELF = 0x00004000,
        DS_ONLY_LDAP_NEEDED = 0x00008000,
        DS_IS_FLAT_NAME = 0x00010000,
        DS_IS_DNS_NAME = 0x00020000,
        DS_RETURN_DNS_NAME = 0x40000000,
        DS_RETURN_FLAT_NAME = 0x80000000
    }

    [Flags]
    public enum GroupType : uint
    {
        LocalDistribution = Constants.ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP,
        LocalSecurity = Constants.ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP | Constants.ADS_GROUP_TYPE_SECURITY_ENABLED,
        GlobalDistribution = Constants.ADS_GROUP_TYPE_GLOBAL_GROUP,
        GlobalSecurity = Constants.ADS_GROUP_TYPE_GLOBAL_GROUP | Constants.ADS_GROUP_TYPE_SECURITY_ENABLED,
        UniversalDistribution = Constants.ADS_GROUP_TYPE_UNIVERSAL_GROUP,
        UniversalSecurity = Constants.ADS_GROUP_TYPE_UNIVERSAL_GROUP | Constants.ADS_GROUP_TYPE_SECURITY_ENABLED
    }

    public enum DS_NAME_FORMAT
    {
        /// <summary>
        /// Unknown Name Type
        /// </summary>
        DS_UNKNOWN_NAME = 0,
        /// <summary>
        /// eg: CN=User Name,OU=Users,DC=Example,DC=Microsoft,DC=Com
        /// </summary>
        DS_FQDN_1779_NAME = 1,
        /// <summary>
        /// eg: Example\UserN
        /// Domain-only version includes trailing '\\'.
        /// </summary>
        DS_NT4_ACCOUNT_NAME = 2,
        /// <summary>
        /// Taken from 'displayName' attribute in AD.
        /// e.g.: First Last or Last, First
        /// </summary>
        DS_DISPLAY_NAME = 3,
        /// <summary>
        /// Reported obsolete - returns SPN form
        /// e.g.: sAMAccountName@fqdn.com
        /// </summary>
        DS_DOMAIN_SIMPLE_NAME = 4,
        /// <summary>
        /// Reported obsolete - returns SPN form
        /// e.g.: sAMAccountName@fqdn.com
        /// </summary>
        DS_ENTERPRISE_SIMPLE_NAME = 5,
        /// <summary>
        /// String-ized GUID as returned by IIDFromString().
        /// eg: {4fa050f0-f561-11cf-bdd9-00aa003a77b6}
        /// </summary>
        DS_UNIQUE_ID_NAME = 6,
        /// <summary>
        /// eg: example.microsoft.com/software/user name
        /// Domain-only version includes trailing '/'.
        /// </summary>
        DS_CANONICAL_NAME = 7,
        /// <summary>
        /// SPN format of name (taken from userPrincipalName attrib)
        /// eg: usern@example.microsoft.com
        /// </summary>
        DS_USER_PRINCIPAL_NAME = 8,
        /// <summary>
        /// Same as DS_CANONICAL_NAME except that rightmost '/' is
        /// replaced with '\n' - even in domain-only case.
        /// eg: example.microsoft.com/software\nuser name
        /// </summary>
        DS_CANONICAL_NAME_EX = 9,
        /// <summary>
        /// eg: www/www.microsoft.com@example.com - generalized service principal
        /// names.
        /// </summary>
        DS_SERVICE_PRINCIPAL_NAME = 10,
        /// <summary>
        /// This is the string representation of a SID.  Invalid for formatDesired.
        /// See sddl.h for SID binary <--> text conversion routines.
        /// eg: S-1-5-21-397955417-626881126-188441444-501
        /// </summary>
        DS_SID_OR_SID_HISTORY_NAME = 11,
        /// <summary>
        /// Pseudo-name format so GetUserNameEx can return the DNS domain name to
        /// a caller.  This level is not supported by the DS APIs.
        /// </summary>
        DS_DNS_DOMAIN_NAME = 12
    }

    public enum DS_NAME_ERROR
    {
        DS_NAME_NO_ERROR = 0,

        // Generic processing error.
        DS_NAME_ERROR_RESOLVING = 1,

        // Couldn't find the name at all - or perhaps caller doesn't have
        // rights to see it.
        DS_NAME_ERROR_NOT_FOUND = 2,

        // Input name mapped to more than one output name.
        DS_NAME_ERROR_NOT_UNIQUE = 3,

        // Input name found, but not the associated output format.
        // Can happen if object doesn't have all the required attributes.
        DS_NAME_ERROR_NO_MAPPING = 4,

        // Unable to resolve entire name, but was able to determine which
        // domain object resides in.  Thus DS_NAME_RESULT_ITEM?.pDomain
        // is valid on return.
        DS_NAME_ERROR_DOMAIN_ONLY = 5,

        // Unable to perform a purely syntactical mapping at the client
        // without going out on the wire.
        DS_NAME_ERROR_NO_SYNTACTICAL_MAPPING = 6,

        // The name is from an external trusted forest.
        DS_NAME_ERROR_TRUST_REFERRAL = 7
    }

    [Flags]
    public enum DS_NAME_FLAGS
    {
        DS_NAME_NO_FLAGS = 0x0,

        // Perform a syntactical mapping at the client (if possible) without
        // going out on the wire.  Returns DS_NAME_ERROR_NO_SYNTACTICAL_MAPPING
        // if a purely syntactical mapping is not possible.
        DS_NAME_FLAG_SYNTACTICAL_ONLY = 0x1,

        // Force a trip to the DC for evaluation, even if this could be
        // locally cracked syntactically.
        DS_NAME_FLAG_EVAL_AT_DC = 0x2,

        // The call fails if the DC is not a GC
        DS_NAME_FLAG_GCVERIFY = 0x4,

        // Enable cross forest trust referral
        DS_NAME_FLAG_TRUST_REFERRAL = 0x8
    }

    public enum WellKnownContainer
    {
        [XmlEnum("a9d1ca15768811d1aded00c04fd8d5cd")]
        Users,
        [XmlEnum("aa312825768811d1aded00c04fd8d5cd")]
        Computers,
        [XmlEnum("ab1d30f3768811d1aded00c04fd8d5cd")]
        System,
        [XmlEnum("a361b2ffffd211d1aa4b00c04fd7d83a")]
        DomainControllers,
        [XmlEnum("2fbac1870ade11d297c400c04fd8d5cd")]
        Infrastructure,
        [XmlEnum("18e2ea80684f11d2b9aa00c04f79f805")]
        DeletedObjects,
        [XmlEnum("ab8153b7768811d1aded00c04fd8d5cd")]
        LostAndFound
    }

    internal enum ADS_OPTION_ENUM
    {
        ADS_OPTION_SERVERNAME = 0,
        ADS_OPTION_REFERRALS = 1,
        ADS_OPTION_PAGE_SIZE = 2,
        ADS_OPTION_SECURITY_MASK = 3,
        ADS_OPTION_MUTUAL_AUTH_STATUS = 4,
        ADS_OPTION_QUOTA = 5,
        ADS_OPTION_PASSWORD_PORTNUMBER = 6,
        ADS_OPTION_PASSWORD_METHOD = 7,
        ADS_OPTION_ACCUMULATIVE_MODIFICATION = 8
    }

    [Flags]
    public enum AdsUserFlags
    {
        Script = 1,                                    // 0x1
        AccountDisabled = 2,                           // 0x2
        HomeDirectoryRequired = 8,                     // 0x8 
        AccountLockedOut = 16,                         // 0x10
        PasswordNotRequired = 32,                      // 0x20
        PasswordCannotChange = 64,                     // 0x40
        EncryptedTextPasswordAllowed = 128,            // 0x80
        TempDuplicateAccount = 256,                    // 0x100
        NormalAccount = 512,                           // 0x200
        InterDomainTrustAccount = 2048,                // 0x800
        WorkstationTrustAccount = 4096,                // 0x1000
        ServerTrustAccount = 8192,                     // 0x2000
        PasswordDoesNotExpire = 65536,                 // 0x10000
        MnsLogonAccount = 131072,                      // 0x20000
        SmartCardRequired = 262144,                    // 0x40000
        TrustedForDelegation = 524288,                 // 0x80000
        AccountNotDelegated = 1048576,                 // 0x100000
        UseDesKeyOnly = 2097152,                       // 0x200000
        DontRequirePreauth = 4194304,                  // 0x400000
        PasswordExpired = 8388608,                     // 0x800000
        TrustedToAuthenticateForDelegation = 16777216, // 0x1000000
        NoAuthDataRequired = 33554432                  // 0x2000000
    }

    [Flags]
    public enum PasswordPolicy
    {
        DOMAIN_PASSWORD_COMPLEX = 1,
        DOMAIN_PASSWORD_NO_ANON_CHANGE = 2,
        DOMAIN_PASSWORD_NO_CLEAR_CHANGE = 4,
        DOMAIN_LOCKOUT_ADMINS = 8,
        DOMAIN_PASSWORD_STORE_CLEARTEXT = 16,
        DOMAIN_REFUSE_PASSWORD_CHANGE = 32
    }
}
