using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

namespace DotNetDevGuide.DirectoryServices.Chapter8
{
    class SchemaGuidConverter
    {
        public static string GetNameForRightsGuid(
            Guid rightsGuid,
            DirectoryEntry extendedRightsRoot
        )
        {
            string filter = String.Format(
                "(rightsGuid={0})", 
                rightsGuid.ToString("D")
                );
            return GetNameForGuid(
                filter,
                "cn",
                extendedRightsRoot
                );
        }
            
        public static string GetNameForSchemaGuid(
            Guid schemaIDGuid, 
            DirectoryEntry schemaRoot
            )
        {
            string filter = String.Format(
                "(schemaIDGUID={0})", 
                BuildFilterOctetString(
                    schemaIDGuid.ToByteArray()
                    )
                );
            return GetNameForGuid(
                filter, 
                "ldapDisplayName", 
                schemaRoot
                );
        }
        
        public static string GetNameForGuid(
            string filter,
            string targetAttribute,
            DirectoryEntry searchRoot
            )
        {
            string attributeName = null;
            SearchResult result;
            DirectorySearcher searcher = 
                new DirectorySearcher(searchRoot);
            searcher.SearchScope = SearchScope.OneLevel;
            searcher.PropertiesToLoad.Add(targetAttribute);
            searcher.Filter = filter;
            
            using (searcher)
            {
                result = searcher.FindOne();
            
                if (result != null)
                {
                    attributeName = (string) 
                        result.Properties[targetAttribute][0];
                }
            }
                
            return attributeName;        
        }
        
        public static Guid GetRightsGuid(
            string rightsName,
            DirectoryEntry extendedRightsRoot
            )
        {
            return GetGuidForName(
                "cn", 
                rightsName,
                "rightsGuid",
                extendedRightsRoot
                );
        }
        
        public static Guid GetSchemaIDGuid(
            string ldapDisplayName, 
            DirectoryEntry schemaRoot
            )
        {
            return GetGuidForName(
                "ldapDisplayName", 
                 ldapDisplayName,
                 "schemaIDGUID",
                schemaRoot
                );
        }
        
        private static Guid GetGuidForName(
            string attributeName,
            string attributeValue,
            string targetAttribute,
            DirectoryEntry root        
            )
        {
            Guid targetGuid = Guid.Empty;        
            SearchResult result;
            object guidValue;
            DirectorySearcher searcher = 
                new DirectorySearcher(root);
            searcher.SearchScope = SearchScope.OneLevel;        
                
            searcher.PropertiesToLoad.Add(targetAttribute);
            searcher.Filter = String.Format(
                "({0}={1})", 
                attributeName,
                attributeValue
                );
            
            using (searcher)
            {
                result = searcher.FindOne();
            
                if (result != null)
                {
                    guidValue =
                        result.Properties[targetAttribute][0];
                    if (guidValue is string)
                        targetGuid =  new Guid((string) guidValue);
                    else
                        targetGuid = new Guid((byte[]) guidValue);
                }
            }
                
            return targetGuid;
        }

        public static string BuildFilterOctetString(byte[] bytes)
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
    }
}
