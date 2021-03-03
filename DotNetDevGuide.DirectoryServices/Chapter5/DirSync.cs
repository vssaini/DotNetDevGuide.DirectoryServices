using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace DotNetDevGuide.DirectoryServices.Chapter5
{
    public class DirSync
    {
        string _username;
        string _password;
        string _adsPath;

        public DirSync()
            : this(null, null, null)
        {
        }

        public DirSync(string adsPath)
            : this(null, null, adsPath)
        {
        }

        public DirSync(
            string username,
            string password,
            string adsPath
            )
        {
            _username = username;
            _password = password;
            _adsPath = adsPath;

            if (_adsPath == null)
            {
                _adsPath = GetDefaultContext();
            }
        }

        //Grab the current domain context
        //if available
        private string GetDefaultContext()
        {
            DirectoryEntry root = new DirectoryEntry(
                "LDAP://rootDSE",
                null,
                null,
                AuthenticationTypes.Secure
                );

            using (root)
            {
                return String.Format(
                    "LDAP://{0}/{1}",
                    root.Properties["dnsHostName"][0].ToString(),
                    root.Properties["defaultNamingContext"][0].ToString()
                    );
            }
        }

        public void InitializeCookie(string qry)
        {
            //this is our searchroot
            DirectoryEntry entry = new DirectoryEntry(
                    _adsPath,
                    _username,
                    _password,
                    AuthenticationTypes.None //.Secure
                    );

            using (entry)
            {
                //we want to track all attributes (use null)
                string[] attribs = null;

                DirectorySearcher ds = new DirectorySearcher(
                        entry,
                        qry,
                        attribs
                        );

                //we must use Subtree scope
                ds.SearchScope = SearchScope.Subtree;

                //pass in the flags we wish here
                DirectorySynchronization dSynch = new DirectorySynchronization(
                        DirectorySynchronizationOptions.None
                        );

                ds.DirectorySynchronization = dSynch;

                using (SearchResultCollection src = ds.FindAll())
                {
                    Console.WriteLine(
                        "Initially Found {0} objects",
                        src.Count
                        );

                    //get and store the cookie
                    StoreCookie(
                            dSynch.GetDirectorySynchronizationCookie()
                            );
                }
            }
        }

        public void GetSynchedChanges(string qry, bool saveState)
        {
            //this is our searchroot
            DirectoryEntry entry = new DirectoryEntry(
                    _adsPath,
                    _username,
                    _password,
                    AuthenticationTypes.None
                    );

            using (entry)
            {
                string[] attribs = null;

                DirectorySearcher ds = new DirectorySearcher(
                        entry,
                        qry,
                        attribs
                        );

                //we must use Subtree scope
                ds.SearchScope = SearchScope.Subtree;

                //pass back in our saved cookie
                DirectorySynchronization dSynch = new DirectorySynchronization(
                        DirectorySynchronizationOptions.None,
                        RestoreCookie()
                        );

                ds.DirectorySynchronization = dSynch;

                using (SearchResultCollection src = ds.FindAll())
                {
                    Console.WriteLine(
                        "Subsequently Changed: {0} objects",
                        src.Count
                        );

                    //return each object that has changed
                    //and what attributes have changed.
                    //keeping in mind that the attributes:
                    //'objectGuid', 'instanceType', and
                    //'adsPath' will always be returned as well
                    foreach (SearchResult sr in src)
                    {
                        Console.WriteLine(
                                "Detected Change in {0}",
                                sr.Properties["AdsPath"][0]
                                );

                        Console.WriteLine("Changed Values:");
                        Console.WriteLine("===============================:");

                        foreach (string prop in sr.Properties.PropertyNames)
                        {
                            foreach (object o in sr.Properties[prop])
                            {
                                Console.WriteLine(
                                        "\t {0} : {1}",
                                        prop,
                                        o
                                        );
                            }
                        }
                    }

                    if (saveState)
                    {
                        //get and store the cookie again
                        StoreCookie(
                                dSynch.GetDirectorySynchronizationCookie()
                                );
                    }
                }
            }
        }

        //quick and nasty method to save the byte[] array
        //note, there is no error checking here
        private void StoreCookie(byte[] cookieBytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream fs = new FileStream(
                    "..\\..\\cookie.bin",
                    FileMode.Create
                    );

            using (fs)
            {
                formatter.Serialize(fs, cookieBytes);
            }
        }

        //quick and nasty method to restore the byte[] array
        //note, there is no error checking here
        private byte[] RestoreCookie()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream fs = new FileStream(
                    "..\\..\\cookie.bin",
                    FileMode.Open
                    );

            using (fs)
            {
                return (byte[])formatter.Deserialize(fs);
            }
        }
    }
}
