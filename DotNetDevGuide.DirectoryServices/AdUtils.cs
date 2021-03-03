using System;
using System.Net;
using System.Runtime.InteropServices;
using System.DirectoryServices;

namespace DotNetDevGuide.DirectoryServices
{
	using DWORD = System.UInt32;
	using BOOL = System.Int32;
	using HANDLE = System.IntPtr;

	internal class AdUtils
	{
		private AdUtils(){}

		private static AuthenticationTypes SecureBinding = AuthenticationTypes.Secure | AuthenticationTypes.Signing | AuthenticationTypes.Sealing;
		private static AuthenticationTypes SecureServerBinding = SecureBinding | AuthenticationTypes.ServerBind;

		internal static string ParseAdsPathForServer(string adsPath)
		{
			string[] parts = adsPath.Split(new char[]{'/'});

			if (parts.Length > 3)
			{
				return parts[2]; //should be the server info
			}
			return null;
		}

		internal static SearchResult FindOne(DirectorySearcher ds)
		{
			SearchResult sr = null;

			using (SearchResultCollection src = ds.FindAll())
			{
				if (src.Count > 0)
				{
					sr = src[0];
				}
			}
			return sr;
		}

		internal static string FormatUserName(NetworkCredential credential)
		{

			if (credential.UserName != null && credential.UserName != String.Empty)
			{
				if (credential.UserName.IndexOf("@") != -1) //upn format not specified
				{
					string domainPart = String.Empty;

					if (credential.Domain != null && credential.Domain != String.Empty)
					{
						domainPart = String.Format(@"{0}\", credential.Domain);
					}
					
					return String.Format("{0}{1}", domainPart, credential.UserName);
				}
				else
					return credential.UserName;
			}
			return null;
		}

		internal static string FormatUserPassword(NetworkCredential credential)
		{
			if (credential.Password != null && credential.Password.Length  != 0)
				return credential.Password;

			return null;
		}

		#region GetAttributeString

		internal static string GetAttributeString(string attributeName, DirectoryEntry de, int idx)
		{
			if(de.Properties.Contains(attributeName))
			{
				if((idx > -1) && (idx < de.Properties[attributeName].Count))
				{
					return de.Properties[attributeName][idx].ToString();
				}
			}
			return null;
		}

		internal static string GetAttributeString(string attributeName, DirectoryEntry de)
		{
			return GetAttributeString(attributeName, de, 0);
		}

		#endregion 
		
		#region GetAttribute

		internal static object GetAttribute(string attributeName, DirectoryEntry de, int idx)
		{
			if(de.Properties.Contains(attributeName))
			{
				if((idx > -1) && (idx < de.Properties[attributeName].Count))
				{
					return de.Properties[attributeName][idx];
				}
			}
			return null;
		}

		internal static object GetAttribute(string attributeName, DirectoryEntry de)
		{
			return GetAttribute(attributeName, de, 0);
		}
		#endregion 

		#region SetAttribute

		/// <summary>
		/// Sets an attribute on a DirectoryEntry.  You must still call .CommitChanges()
		/// to write the results to the directory.
		/// </summary>
		/// <param name="attributeName"></param>
		/// <param name="value"></param>
		/// <param name="de"></param>
		internal static void SetAttribute (string attributeName, object value, DirectoryEntry de)
		{
			SetAttribute(attributeName, value, de, 0);
		}

		internal static void SetAttribute(string attributeName, object val, DirectoryEntry entry, int idx)
		{
			//if the attribute already exists then update
			if (entry.Properties.Contains(attributeName))
			{
				//remove the attribute if the value is null or ""
				if ((val == null) || (val.ToString() == String.Empty))
				{
					entry.Properties[attributeName].RemoveAt(idx);
				}
				else
				{
					//we are trying to modify an existing item
					if (idx < entry.Properties[attributeName].Count)
					{
						//update it at the index
						entry.Properties[attributeName][idx] = val;
					}
					else //we are trying to add to multi-value attribute
					{
						entry.Properties[attributeName].Add(val);
					}
				}
			}
			else
			{
				if ((val != null) && (val.ToString() != String.Empty))
				{
					//the attribute did not exist, so add it to object
					entry.Properties[attributeName].Add(val);
				}
			}
		}

		#endregion 

		#region Binding

		internal static DirectoryEntry GetDefaultContext(NetworkCredential credential, string server)
		{
			using (DirectoryEntry entry = AdUtils.SecureBind("rootDSE", credential.UserName, credential.Password, credential.Domain))
			{
				return AdUtils.SecureBind(
					AdUtils.GetAttributeString("defaultNamingContext", entry),
					entry.Username,
					credential.Password,
					server
					);
			}
		}

		internal static DirectoryEntry GCBind(string username, string password, string server)
		{
			string adsPath = String.Empty;
			if (server == null)
				adsPath = "GC:";
			else
				adsPath = String.Format("GC://{0}", server);

			return new DirectoryEntry (
				adsPath,
				username,
				password,
				(server == null) ? SecureBinding : SecureServerBinding
				);
		}

		internal static DirectoryEntry GCBind(string server)
		{
			return GCBind(null, null, server);
		}

		internal static DirectoryEntry GCBind()
		{
			return GCBind(null, null, null);
		}

		internal static DirectoryEntry SecureBind(string distinguishedName, string username, string password, string server)
		{
			string adsPath = String.Empty;
			if (server == null)
				adsPath = String.Format("LDAP://{0}", distinguishedName);
			else
				adsPath = String.Format("LDAP://{0}/{1}", server, distinguishedName);

			return new DirectoryEntry (
				adsPath,
				username,
				password,
				(server == null) ? SecureBinding : SecureServerBinding
				);
		}

		internal static DirectoryEntry SecureBind(string distinguishedName, string username, string password)
		{
			return SecureBind(distinguishedName, username, password, null);
		}

		internal static DirectoryEntry FastBind(string distinguishedName, string username, string password)
		{
			return FastBind(distinguishedName, username, password, null);
		}

		internal static DirectoryEntry FastBind(string distinguishedName, string username, string password, string server)
		{
			string adsPath = String.Empty;
			if (server == null)
				adsPath = String.Format("LDAP://{0}", distinguishedName);
			else
				adsPath = String.Format("LDAP://{0}/{1}", server, distinguishedName);

			return new DirectoryEntry (
				adsPath,
				username,
				password,
				(server == null) ? SecureBinding : SecureServerBinding
				);
		}

		#endregion

		internal static string ConvertSidToString(byte[] sid)
		{
			IntPtr pSidString = IntPtr.Zero;
			GCHandle handle = new GCHandle();

			try
			{
				//pin the sid bytes
				handle = GCHandle.Alloc(sid, GCHandleType.Pinned);

				BOOL rc = NativeMethods.ConvertSidToStringSid(handle.AddrOfPinnedObject(), out pSidString);
				Win32.CheckCall(rc);
				
				return Marshal.PtrToStringAuto(pSidString);
			}
			finally
			{
				if (!Win32.IsNullHandle(pSidString))
					Win32.LocalFree(pSidString);

				if (handle.IsAllocated)
					handle.Free();
			}
		}

		internal static void ParseUserName(ref string username, out string domain)
		{
			domain = null;

			string[] parts = username.Split(new char[]{'\\'});

			if (parts.Length > 2)
				throw new ArgumentException("Invalid Username Specified");

			if (parts.Length == 2)
			{
				domain = parts[0];
				username = parts[1];
			}
		}

		internal static string[] DsCrackNamesWrapper(
			string[] itemsToConvert,
			IntPtr hDS,
			DS_NAME_FORMAT formatOffered,
			DS_NAME_FORMAT formatDesired
			)
		{
			if (Win32.IsNullHandle(hDS))
				throw new ArgumentException("Invalid Directory Handle");

			if (itemsToConvert == null || itemsToConvert.Length == 0)
				throw new ArgumentException("No items to convert specified");

			IntPtr pResult = IntPtr.Zero;
			DS_NAME_RESULT_ITEM[] dnri = null;

			System.Collections.Specialized.StringCollection sc = new System.Collections.Specialized.StringCollection();

			try
			{			
				DWORD rc = NativeMethods.DsCrackNames(
					hDS,
					DS_NAME_FLAGS.DS_NAME_NO_FLAGS,
					formatOffered,
					formatDesired,
					(uint)itemsToConvert.Length,
					itemsToConvert,
					out pResult
					);

				if (rc != Win32.ERROR_SUCCESS)
				{
					Win32.SetLastError(rc);
					Win32.ThrowLastError();
				}

				DS_NAME_RESULT dnr = (DS_NAME_RESULT)Marshal.PtrToStructure(pResult, typeof(DS_NAME_RESULT));
				
				//define the array with size to match				
				dnri = new DS_NAME_RESULT_ITEM[dnr.cItems];

				//point to our current DS_NAME_RESULT_ITEM structure
				IntPtr pidx = dnr.rItems;

				for (int idx = 0; idx < dnr.cItems; idx++)
				{
					//marshall back the structure
					dnri[idx] = (DS_NAME_RESULT_ITEM)Marshal.PtrToStructure(pidx,typeof(DS_NAME_RESULT_ITEM));
					//update the current pointer idx to next structure
					pidx = (IntPtr)(pidx.ToInt32() + Marshal.SizeOf(dnri[idx]));
				}

				for(int i=0; i < dnri.Length; i++)
				{
					//we will intentionally ignore any that did not resolve
					if (dnri[i].status == 0)
						sc.Add(dnri[i].pName);
				}

				string[] names = new string[sc.Count];
				sc.CopyTo(names, 0);

				return names;
			}
			finally
			{
				if (!Win32.IsNullHandle(pResult))
					NativeMethods.DsFreeNameResult(pResult);
			}
		}
	}
}
