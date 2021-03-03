using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

using NUnit.Framework;

namespace DotNetDevGuide.DirectoryServices.Chapter4
{
    class Chapter4
    {
        //we use this just to get app.config in
        static void Main(string[] args) { }

        /// <summary>
        /// Listing 4.2
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string BuildFilterOctetString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                sb.AppendFormat(
                    "\\{0}",
                    bytes[i].ToString("X2")
                    );
            }
            return sb.ToString();
        }

        /// <summary>
        /// Taken from Listing 4.3
        /// </summary>
        [Test]
        public void FormatGuidFilterString()
        {
            Guid objectGuid = new Guid("4a5a0fa7-1200-4198-a3a7-31ee9ba10fc9");

            string filter = string.Format(
                "(objectGUID={0})",
                BuildFilterOctetString(objectGuid.ToByteArray())
                );

            Console.WriteLine(filter);

            Assert.IsTrue(filter == @"(objectGUID=\A7\0F\5A\4A\00\12\98\41\A3\A7\31\EE\9B\A1\0F\C9)");
        }

        /// <summary>
        /// Listing 4.4
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public string GetUtcFilter(DateTime date)
        {
            return date.ToString("yyyyMMddhhmmss.0Z");
        }

        /// <summary>
        /// Also Listing 4.4
        /// </summary>
        /// <param name="date"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public string GetGeneralizedFilter(
            DateTime date,
            TimeSpan offset
            )
        {
            string sign = TimeSpan.Compare(offset, TimeSpan.Zero) == -1 ? "" : "+";

            return string.Format(
                "{0}.0{1}{2}{3}",
                date.ToString("yyyyMMddhhmmss"),
                sign,
                offset.Hours.ToString("00"),
                offset.Minutes.ToString("00")
                );
        }

        /// <summary>
        /// How to use Listing 4.4
        /// </summary>
        [Test]
        public void BuildUtcFilter()
        {
            DateTime bookRelease = new DateTime(2006, 5, 8, 12, 00, 00);

            string utcFormat = GetUtcFilter(bookRelease);

            Console.WriteLine(utcFormat);

            Assert.IsTrue(utcFormat == "20060508120000.0Z");
        }

        /// <summary>
        /// How to use Listing 4.4 cont...
        /// </summary>
        [Test]
        public void BuildGeneralizedTimeFilter()
        {
            DateTime bookRelease = new DateTime(2006, 5, 8, 12, 00, 00);

            string gtFormat = GetGeneralizedFilter(bookRelease, TimeSpan.FromHours(-5));

            Console.WriteLine(gtFormat);

            Assert.IsTrue(gtFormat == "20060508120000.0-0500");
        }

        /// <summary>
        /// Listing 4.5 modified
        /// </summary>
        [Test]
        public void LargeIntegerFormatSearch()
        {
            //Explicitly create our SearchRoot
            DirectoryEntry searchRoot = TestUtils.GetDefaultPartition();

            using (searchRoot) //we are responsible for Disposing
            {
                //find anything with a password older than 30 days
                string qry = String.Format(
                    "(pwdLastSet<={0})",
                    DateTime.Now.AddDays(-30).ToFileTime() //30 days ago
                    );

                DirectorySearcher ds = new DirectorySearcher(
                    searchRoot,
                    qry
                    );

                using (SearchResultCollection src = ds.FindAll())
                {
                    Console.WriteLine("Returning {0}", src.Count);

                    foreach (SearchResult sr in src)
                    {
                        Console.WriteLine(sr.Path);
                    }
                }
            }
        }

        /// <summary>
        /// Listing 4.6 modified
        /// </summary>
        [Test]
        public void BitwiseFilterSearch()
        {
            //Explicitly create our SearchRoot
            DirectoryEntry searchRoot = TestUtils.GetDefaultPartition();

            using (searchRoot)
            {
                //UF_ACCOUNTDISABLE = 0x2, which is 2 decimal
                //find all disabled accounts
                string filter =
                   "(userAccountControl:1.2.840.113556.1.4.803:=2)";

                DirectorySearcher ds = new DirectorySearcher(
                    searchRoot,
                    filter
                    );

                using (SearchResultCollection src = ds.FindAll())
                {
                    Console.WriteLine("Returning {0}", src.Count);

                    foreach (SearchResult sr in src)
                    {
                        Console.WriteLine(sr.Path);
                    }
                }
            }
        }

        /// <summary>
        /// Listing 4.11 Modified
        /// </summary>
        [Test]
        public void FindSingleResult()
        {
            DirectoryEntry root = null;
            DirectorySearcher ds = null;
            SearchResult result = null;

            using (root = TestUtils.GetDefaultPartition())
            using (ds = new DirectorySearcher(root))
            {
                ds.Filter = "(sAMAccountName=jkaplan)";
                ds.SizeLimit = 1;
                result = ds.FindOne();
            }

            Assert.IsNotNull(result, "Did not find result, try changing the filter");

            if (result != null)
                Console.WriteLine(result.Path);

        }

        /// <summary>
        /// Listing 4.12 Modified
        /// </summary>
        [Test]
        public void FindMultipleResults()
        {
            DirectoryEntry root = null;
            DirectorySearcher ds = null;
            SearchResultCollection results = null;

            using (root = TestUtils.GetDefaultPartition())
            using (ds = new DirectorySearcher(root))
            {
                ds.Filter = "(mail=*)";
                ds.SizeLimit = 100;
                ds.PropertiesToLoad.Add("mail");
        
                using (results = ds.FindAll())
                {
                    foreach (SearchResult result in results)
                    {
                        Console.WriteLine(result.Properties["mail"][0]);
                    }
                    
                    Assert.IsTrue(results.Count > 0, "no objects found");
                }
            }
        }

        /// <summary>
        /// Listing 4.13 Modified
        /// </summary>
        [Test]
        public void PagingSearch()
        {
            DirectoryEntry searchRoot = TestUtils.GetDefaultPartition();

            using (searchRoot) //we are responsible for Disposing
            {
                DirectorySearcher ds = new DirectorySearcher(
                    searchRoot,
                    "(&(objectCategory=person)(objectClass=user))" //any user
                    );

                //enable paging support
                ds.PageSize = 1000;

                //wait a maximum of 2 seconds per page
                ds.ServerPageTimeLimit = TimeSpan.FromSeconds(2);

                using (SearchResultCollection src = ds.FindAll())
                {
                    Console.WriteLine("Returning {0}", src.Count);

                    foreach (SearchResult sr in src)
                    {
                        Console.WriteLine(sr.Path);
                    }
                }
            }
        }

        /// <summary>
        /// Listing 4.14 Modified
        /// </summary>
        [Test]
        public void ServerSideSorting()
        {
            DirectoryEntry searchRoot = TestUtils.GetDefaultPartition();

            using (searchRoot)
            {
                DirectorySearcher ds = new DirectorySearcher(
                    searchRoot,
                    "(&(objectCategory=person)(objectClass=user))"
                    );

                //sort by last name starting with 'Z'
                ds.Sort = new SortOption(
                    "sn",
                    SortDirection.Descending
                    );

                using (SearchResultCollection src = ds.FindAll())
                {
                    Console.WriteLine("Returning {0}", src.Count);

                    foreach (SearchResult sr in src)
                    {
                        Console.WriteLine(sr.Path);
                    }
                }
            }
        }
    }
}
