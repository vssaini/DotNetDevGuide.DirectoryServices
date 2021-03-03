using System;
using System.Collections;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection;

using NUnit.Framework;

namespace DotNetDevGuide.DirectoryServices.Chapter7
{
    class Chapter7
    {
        /// <summary>
        /// Listing 7.1 Modified.  This pulls the schema for any object
        /// </summary>
        [Test]
        public void GetSchemaViaReflection()
        {
            //point this to any object (I chose a user)
            DirectoryEntry user = TestUtils.CreateDirectoryEntry(
                "CN=User1,OU=Users," + TestUtils.Settings.DefaultPartition);

            using (user)
            using (DirectoryEntry schema = user.SchemaEntry)
            {
                Type t = schema.NativeObject.GetType();

                object inferiors = t.InvokeMember(
                    "Containment",
                    BindingFlags.Public | BindingFlags.GetProperty,
                    null,
                    schema.NativeObject,
                    null
                    );

                if (inferiors is ICollection)
                {
                    Console.WriteLine("Possible Inferiors");
                    Console.WriteLine("=====================");
                    foreach (string s in ((ICollection)inferiors))
                    {
                        Console.WriteLine(s);
                    }
                }

                object optional = t.InvokeMember(
                    "OptionalProperties",
                    BindingFlags.Public | BindingFlags.GetProperty,
                    null,
                    schema.NativeObject,
                    null
                    );

                if (optional is ICollection)
                {
                    foreach (string s in ((ICollection)optional))
                    {
                        Console.WriteLine("Optional: {0}", s);
                    }
                }

                object mand = t.InvokeMember(
                    "MandatoryProperties",
                    BindingFlags.Public | BindingFlags.GetProperty,
                    null,
                    schema.NativeObject,
                    null
                    );

                if (mand is ICollection)
                {
                    foreach (string s in ((ICollection)mand))
                    {
                        Console.WriteLine("Mandatory: {0}", s);
                    }
                }
            }
        }

        /// <summary>
        /// Listing 7.2 Modified. This uses SDS.AD to inspect some schema.
        /// </summary>
        [Test]
        public void GetSchemaViaSDSAD()
        {
            ActiveDirectorySchemaClass schema = ActiveDirectorySchemaClass.FindByName(
                new DirectoryContext(
                    DirectoryContextType.DirectoryServer,
                    TestUtils.Settings.Server), //note that SDS.AD doesn't like 'localhost'
                "organization"
                );

            Console.WriteLine("Possible Inferiors");
            Console.WriteLine("=============");
            foreach (ActiveDirectorySchemaClass adsc in
            schema.PossibleInferiors)
            {
                Console.WriteLine("{0}", adsc.Name);
            }
            Console.WriteLine("=============");

            foreach (ActiveDirectorySchemaProperty prop in schema.MandatoryProperties)
            {
                Console.WriteLine("=============");
                Console.WriteLine("Attribute {0}", prop.Name);
                Console.WriteLine("Syntax: {0}", prop.Syntax);
                Console.WriteLine("Indexed: {0}", prop.IsIndexed);
                Console.WriteLine("In GC: {0}", prop.IsInGlobalCatalog);
            }

            foreach (ActiveDirectorySchemaProperty prop in schema.OptionalProperties)
            {
                Console.WriteLine("=============");
                Console.WriteLine("Attribute {0}", prop.Name);
                Console.WriteLine("Syntax: {0}", prop.Syntax);
                Console.WriteLine("Indexed: {0}", prop.IsIndexed);
                Console.WriteLine("In GC: {0}", prop.IsInGlobalCatalog);
            }
        }

        /// <summary>
        /// Listing 7.3 Modified.  Shows all possible allowed classes and attributes
        /// </summary>
        [Test]
        public void GetAllAllowedAttributes()
        {
            DirectoryEntry root = TestUtils.GetDefaultPartition();

            using (root)
            {
                root.RefreshCache(
                    new string[] { "allowedChildClasses", "allowedAttributes" }
                );

                Console.WriteLine("Possible Inferiors");
                Console.WriteLine("=====================");
                foreach (string s in
                    root.Properties["allowedChildClasses"])
                {
                    Console.WriteLine(s);
                }
                Console.WriteLine("=====================");

                Console.WriteLine("Available Attributes");
                Console.WriteLine("=====================");
                foreach (string s in
                    root.Properties["allowedAttributes"])
                {
                    Console.WriteLine(s);
                }
            }
        }

        /// <summary>
        /// Listing 7.4 Modified.  Shows all classes and attributes
        /// that can be created or modified with current permissions.
        /// </summary>
        [Test]
        public void GetAllAllowedAttributesEffective()
        {
            DirectoryEntry root = TestUtils.GetDefaultPartition();

            using (root)
            {
                root.RefreshCache(
                    new string[] 
            {"allowedChildClassesEffective",
              "allowedAttributesEffective"}
                );

                Console.WriteLine("Effective Child Classes");
                Console.WriteLine("=====================");
                foreach (string s in
                    root.Properties["allowedChildClassesEffective"])
                {
                    Console.WriteLine(s);
                }
                Console.WriteLine("=====================");

                Console.WriteLine("Effective Attributes");
                Console.WriteLine("=====================");
                foreach (string s in
                    root.Properties["allowedAttributesEffective"])
                {
                    Console.WriteLine(s);
                }
            }

        }
    }
}
