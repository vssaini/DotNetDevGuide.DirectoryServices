using System;
using System.Runtime.InteropServices;

namespace DotNetDevGuide.DirectoryServices
{
	using BOOL = System.Int32;
	using DWORD = System.UInt32;
	using HANDLE = System.IntPtr;
	using PSID = System.IntPtr;
	using LPCTSTR = System.String;
	using LPTSTR = System.String;
	using RPC_AUTH_IDENTITY_HANDLE = System.IntPtr;

	internal class NativeMethods
	{
		internal NativeMethods()
		{
		}

		private const string Advapi32 = "advapi32.dll";
		private const string ntdsapi = "ntdsapi.dll";
		private const string Netapi32 = "Netapi32.dll";

		[DllImport(Advapi32, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern BOOL ConvertSidToStringSid(
			PSID pSID,
			out IntPtr pStringSid
			);

		[DllImport(ntdsapi, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern DWORD DsCrackNames(
			HANDLE hDS,
			DS_NAME_FLAGS flags,
			DS_NAME_FORMAT formatOffered,
			DS_NAME_FORMAT formatDesired,
			DWORD cNames,
			LPCTSTR[] rpNames,
			out IntPtr ppResult //pointer to pointer of DS_NAME_RESULT
			);

		[DllImport(ntdsapi, CharSet=CharSet.Auto)]
		internal static extern void DsFreeNameResult(
			IntPtr pResult //DS_NAME_RESULTW*
			);

		[DllImport(ntdsapi, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern DWORD DsBind(
			LPCTSTR DomainControllerName,
			LPCTSTR DnsDomainName,
			out HANDLE phDS
			);

		[DllImport(ntdsapi, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern DWORD DsBindWithCred(
			LPCTSTR DomainControllerName,
			LPCTSTR DnsDomainName,
			RPC_AUTH_IDENTITY_HANDLE AuthIdentity,
			out HANDLE phDS
			);

		[DllImport(ntdsapi, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern DWORD DsMakePasswordCredentials(
			LPCTSTR User,
			LPCTSTR Domain,
			LPCTSTR Password,
			out RPC_AUTH_IDENTITY_HANDLE pAuthIdentity
			);

		[DllImport(ntdsapi, CharSet=CharSet.Auto)]
		internal static extern void DsFreePasswordCredentials(
			RPC_AUTH_IDENTITY_HANDLE AuthIdentity
			);

		[DllImport(ntdsapi, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern DWORD DsUnBind(
			ref HANDLE phDS
			);

		[DllImport(ntdsapi, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern DWORD DsGetDomainControllerInfo(
			HANDLE hDs,
			LPTSTR DomainName,
			DWORD InfoLevel,
			ref DWORD pcOut,
			out IntPtr ppInfo
			);

		[DllImport(ntdsapi, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern void DsFreeDomainControllerInfo(
			DWORD InfoLevel,
			DWORD cInfo,
			IntPtr pInfo
			);

		[DllImport(ntdsapi, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern DWORD DsListSites(
			HANDLE hDs,
			out IntPtr ppSites //PDS_NAME_RESULT* need to marshal this one manually
			);

		//////////////////////////
		///NetApi32 Calls

		[DllImport(Netapi32, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern DWORD NetApiBufferFree(IntPtr pBufferToFree);
		
		[DllImport(Netapi32, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern DWORD DsGetDcName(
            [MarshalAs(UnmanagedType.LPTStr)]
			LPCTSTR ComputerName,
			[MarshalAs(UnmanagedType.LPTStr)]
			LPCTSTR DomainName,
			IntPtr DomainGuid,
			[MarshalAs(UnmanagedType.LPTStr)]
			LPCTSTR SiteName,
			DsGetDcFlags Flags,
			out IntPtr pDOMAIN_CONTROLLER_INFO //caller must free this struct with NetApiBufferFree
			);
		
		[DllImport(Netapi32, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern DWORD DsGetDcOpen(
			[MarshalAs(UnmanagedType.LPTStr)]
			string DnsName,
			int OptionFlags,
			[MarshalAs(UnmanagedType.LPTStr)]
			string SiteName,
			IntPtr DomainGuid,
			[MarshalAs(UnmanagedType.LPTStr)]
			string DnsForestName,
			uint DcFlags,
			out IntPtr RetGetDcContext
			);

		[DllImport(Netapi32, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern DWORD DsGetDcNext(
			HANDLE GetDcContextHandle,
			IntPtr SockAddressCount,
			IntPtr SockAddresses, //must free this if using (SocketAddressCount > 1)
			out IntPtr DnsHostName //will have to marshal this one manually
			);

		[DllImport(Netapi32, CharSet=CharSet.Auto, SetLastError=true)]
		internal static extern DWORD DsGetDcClose(HANDLE GetDcContextHandle);
	}
}
