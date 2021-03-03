using System;
using System.DirectoryServices;

namespace DotNetDevGuide.DirectoryServices.Chapter10
{
    /// <summary>
    /// Listing 10.7 in full
    /// </summary>
    public class DomainPolicy
    {
        readonly ResultPropertyCollection _attribs;

        public DomainPolicy(DirectoryEntry domainRoot)
        {
            string[] policyAttributes =
            {
                "maxPwdAge", "minPwdAge", "minPwdLength", 
                "lockoutDuration", "lockOutObservationWindow", 
                "lockoutThreshold", "pwdProperties", 
                "pwdHistoryLength", "objectClass", 
                "distinguishedName"
            };

            //we take advantage of the marshaling with
            //the DirectorySearcher for Large Integer values...
            DirectorySearcher ds = new DirectorySearcher(
                domainRoot,
                "(objectClass=domainDNS)",
                policyAttributes,
                SearchScope.Base
                );

            SearchResult result = ds.FindOne();

            //do some quick validation...	   
            if (result == null)
            {
                throw new ArgumentException(
                    "domainRoot is not a domainDNS object."
                    );
            }

            _attribs = result.Properties;
        }

        //for some odd reason, the intervals are all stored
        //as negative numbers.  We use this to "invert" them
        private static long GetAbsValue(object longInt)
        {
            return Math.Abs((long)longInt);
        }

        public string DomainDistinguishedName
        {
            get
            {
                const string val = "distinguishedName";
                if (_attribs.Contains(val))
                {
                    return (string)_attribs[val][0];
                }
                //default return value
                return String.Empty;
            }
        }

        public TimeSpan MaxPasswordAge
        {
            get
            {
                const string val = "maxPwdAge";
                if (_attribs.Contains(val))
                {
                    long ticks = GetAbsValue(
                        _attribs[val][0]
                        );

                    if (ticks > 0)
                        return TimeSpan.FromTicks(ticks);
                }

                return TimeSpan.MaxValue;
            }
        }

        public TimeSpan MinPasswordAge
        {
            get
            {
                const string val = "minPwdAge";
                if (_attribs.Contains(val))
                {
                    long ticks = GetAbsValue(
                        _attribs[val][0]
                        );

                    if (ticks > 0)
                        return TimeSpan.FromTicks(ticks);
                }

                return TimeSpan.MinValue;
            }
        }

        public TimeSpan LockoutDuration
        {
            get
            {
                const string val = "lockoutDuration";
                if (_attribs.Contains(val))
                {
                    long ticks = GetAbsValue(
                        _attribs[val][0]
                        );

                    if (ticks > 0)
                        return TimeSpan.FromTicks(ticks);
                }

                return TimeSpan.MaxValue;
            }
        }

        public TimeSpan LockoutObservationWindow
        {
            get
            {
                const string val = "lockoutObservationWindow";
                if (_attribs.Contains(val))
                {
                    long ticks = GetAbsValue(
                        _attribs[val][0]
                        );

                    if (ticks > 0)
                        return TimeSpan.FromTicks(ticks);
                }

                return TimeSpan.MaxValue;
            }
        }

        public int LockoutThreshold
        {
            get
            {
                const string val = "lockoutThreshold";
                if (_attribs.Contains(val))
                {
                    return (int)_attribs[val][0];
                }

                return 0;
            }
        }

        public int MinPasswordLength
        {
            get
            {
                const string val = "minPwdLength";
                if (_attribs.Contains(val))
                {
                    return (int)_attribs[val][0];
                }

                return 0;
            }
        }

        public int PasswordHistoryLength
        {
            get
            {
                const string val = "pwdHistoryLength";
                if (_attribs.Contains(val))
                {
                    return (int)_attribs[val][0];
                }

                return 0;
            }
        }

        public PasswordPolicy PasswordProperties
        {
            get
            {
                const string val = "pwdProperties";
                //this should fail if not found
                return (PasswordPolicy)_attribs[val][0];
            }
        }
    }
}
