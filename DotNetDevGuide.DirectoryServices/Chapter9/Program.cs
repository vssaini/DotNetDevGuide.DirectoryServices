using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

using NUnit.Framework;

namespace DotNetDevGuide.DirectoryServices.Chapter9
{
    /// <summary>
    /// Most of these samples will not work with ADAM since they use Domain and Forest
    /// concepts
    /// </summary>
    class Chapter9
    {
        /// <summary>
        /// Listing 9.2 - you will need to update this one to 
        /// get it to work in your different forests.
        /// </summary>
        [Test]
        public void GetForests()
        {
            Forest currentForest = null;
            Forest alternateCurrentForest = null;
            Forest otherForest = null;

            //create a new DirectoryContext for a Forest
            //using defaults for name and credentials
            DirectoryContext context = new DirectoryContext(
                DirectoryContextType.Forest);

            using (currentForest = Forest.GetForest(context))
            {
                Console.WriteLine(currentForest.Name);
            }

            //Use the shortcut static method on the Forest class to
            //do the same thing as above
            using (alternateCurrentForest = Forest.GetCurrentForest())
            {
                Console.WriteLine(alternateCurrentForest.Name);
            }

            //Now, connect to a completely different forest 
            //specifying its name and the credentials we need to 
            //access it
            DirectoryContext otherContext = new DirectoryContext(
                DirectoryContextType.Forest,
                "other.yourforest.com", //update this stuff.
                @"other\someone",
                "MySecret!0");

            using (otherForest = Forest.GetForest(otherContext))
            {
                Console.WriteLine(otherForest.Name);
            }
            //OUT:
            //main.myforest.com
            //main.myforest.com
            //other.yourforest.com
        }

        /// <summary>
        /// Listing 9.3 - this one also needs to be modified to work
        /// correctly unfortunately.
        /// </summary>
        [Test]
        public void GetDirectoryServers()
        {
            DomainController dc = null;
            AdamInstance adam = null;

            //create a new DirectoryContext for a 
            //domain controller using a name and 
            //default credentials
            DirectoryContext dcContext = new DirectoryContext(
                DirectoryContextType.DirectoryServer,
                "mydc.mydomain.com"); //update

            using (dc = DomainController.GetDomainController(dcContext))
            {
                Console.WriteLine(dc.Name);
            }


            //Now, connect to a local ADAM instance 
            //specifying a port and local admin credentials 
            DirectoryContext adamContext = new DirectoryContext(
                DirectoryContextType.DirectoryServer,
                "127.0.0.1:50000", //update
                @"MYMACHINE\administrator",
                "MySecret!0");

            using (adam = AdamInstance.GetAdamInstance(adamContext))
            {
                Console.WriteLine(adam.Name);
            }
            //OUT:
            //mydc.mydomain.com
            //myadaminstance.mydomain.com
        }

        /// <summary>
        /// Listing 9.4
        /// </summary>
        [Test]
        public void FindDomainController()
        {
            DirectoryContext ctx = new DirectoryContext(
                DirectoryContextType.Domain,
                "mydomain.org" //update this to point to your domain
                );

            using (DomainController dc = DomainController.FindOne(ctx))
            {
                Console.WriteLine(dc.Name);
                Console.WriteLine(dc.OSVersion);
                Console.WriteLine(dc.SiteName);
                Console.WriteLine(dc.IPAddress);
                Console.WriteLine(dc.Forest);
                Console.WriteLine(dc.CurrentTime);
            }
        }

        /// <summary>
        /// Listing 9.5
        /// </summary>
        [Test]
        public void FindAllDomainControllers()
        {
            Domain domain = Domain.GetCurrentDomain();

            using (domain)
            {
                foreach (DomainController dc in
                    domain.FindAllDiscoverableDomainControllers())
                {
                    using (dc)
                    {
                        Console.WriteLine(dc.Name);
                        Console.WriteLine(dc.OSVersion);
                        Console.WriteLine(dc.SiteName);
                        Console.WriteLine(dc.IPAddress);
                        Console.WriteLine(dc.Forest);
                        Console.WriteLine(dc.CurrentTime);
                    }
                }
            }
        }

        /// <summary>
        /// Listing 9.6 - you must modify the domain name below to use.
        /// </summary>
        [Test]
        public void FindDomainControllerForceDiscovery()
        {
            DirectoryContext ctx = new DirectoryContext(
                DirectoryContextType.Domain,
                "mydomain.org"
                );

            //Notice the extra parameter here...
            using (DomainController dc = DomainController.FindOne(
                ctx, 
                LocatorOptions.ForceRediscovery)
            )
            {
                Console.WriteLine(dc.Name);
                Console.WriteLine(dc.OSVersion);
                Console.WriteLine(dc.SiteName);
                Console.WriteLine(dc.IPAddress);
                Console.WriteLine(dc.Forest);
                Console.WriteLine(dc.CurrentTime);
            }
        }

        /// <summary>
        /// Listin 9.7 - need to modify domain name and site to run
        /// </summary>
        [Test]
        public void FindControllerInSite()
        {
            DirectoryContext ctx = new DirectoryContext(
                 DirectoryContextType.Domain,
                "mydomain.org" //update this
                );

            //Now we are specifying a site name
            using (DomainController dc = DomainController.FindOne(
                ctx,
                "othersite") //and this
            )
            {
                Console.WriteLine(dc.Name);
                Console.WriteLine(dc.OSVersion);
                Console.WriteLine(dc.SiteName);
                Console.WriteLine(dc.IPAddress);
                Console.WriteLine(dc.Forest);
                Console.WriteLine(dc.CurrentTime);
            }
        }

        /// <summary>
        /// Listing 9.10 Modified.  Notice that my helper methods (TestUtil)
        /// make it pretty easy, but we still need a bunch of code to get 
        /// root default naming context.  The good thing here is that my helper
        /// methods make it a snap for either ADAM or AD.  My helper methods 
        /// were not part of the original sample in the book so the original
        /// sample only worked with AD. 
        /// </summary>
        [Test]
        public void GetDefaultNamingContext()
        {
            //getting the RootDSE normally obviously takes more code than
            //my helper method here shows, but you get the idea, right?
            DirectoryEntry root = TestUtils.CreateDirectoryEntry("RootDSE");
            string dnc;

            using (root)
            {
                string attr = "defaultNamingContext";
                dnc = root.Properties[attr][0].ToString();
            }

            DirectoryEntry entry = TestUtils.CreateDirectoryEntry(dnc);

            using (entry)
            {
                //now we are bound to default context
                Console.WriteLine(entry.Path);
            }
        }

        /// <summary>
        /// Listing 9.11 - Notice that this won't work with ADAM however (unlike 9.10).
        /// </summary>
        [Test]
        public void GetDefaultNamingContextEasy()
        {
            Domain domain = Domain.GetCurrentDomain();

            using (domain)
            using (DirectoryEntry entry = domain.GetDirectoryEntry())
            {
                //now we are bound to default context
            }
        }

        /// <summary>
        /// Listing 9.12
        /// </summary>
        [Test]
        public void GetSchemaNamingContextEasy()
        {
            ActiveDirectorySchema ads = ActiveDirectorySchema.GetCurrentSchema();

            using (ads)
            using (DirectoryEntry entry = ads.GetDirectoryEntry())
            {
                //bound to schema partition
            }
        }

        /// <summary>
        /// Listing 9.13 Modified.  Does not work with AD.  ADAM only
        /// </summary>
        [Test]
        public void EnumerateADAMInstances()
        {
            DirectoryContext ctx = new DirectoryContext(
                DirectoryContextType.DirectoryServer,
                TestUtils.Settings.Server //does not like 'localhost'
                );

            ConfigurationSet cs = ConfigurationSet.GetConfigurationSet(ctx);

            foreach (AdamInstance ai in cs.AdamInstances)
            {
                //ADAM does not have a default partition
                //by default, so this must be set explicitly
                //previously. See 9.14 for easy way to do this
                Console.WriteLine(ai.DefaultPartition);
            }
        }

        /// <summary>
        /// Listing 9.14 Modified.  This is how to set the partition
        /// so you can use 'defaultNamingContext' later.  ADAM only 
        /// sample.
        /// </summary>
        [Test]
        public void SetDefaultPartition()
        {
            AdamInstance adam;
            DirectoryContext ctx = new DirectoryContext(
                DirectoryContextType.DirectoryServer,
                TestUtils.Settings.Server //does not like localhost
                );

            using (adam = AdamInstance.GetAdamInstance(ctx))
            {
                adam.DefaultPartition = "O=My,C=Adam"; //update this
                adam.Save();
            }
        }
    }
}
