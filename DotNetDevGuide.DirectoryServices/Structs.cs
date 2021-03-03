using System;
using System.Runtime.InteropServices;

namespace DotNetDevGuide.DirectoryServices
{
	using DWORD = System.UInt32;
	using LPTSTR = System.String;
	using BOOL = System.Int32;
	using GUID = System.Guid;

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	internal struct DOMAIN_CONTROLLER_INFO 
	{
		[MarshalAs(UnmanagedType.LPTStr)]
		internal string	DomainControllerName;
		[MarshalAs(UnmanagedType.LPTStr)]
		internal string	DomainControllerAddress;
		internal uint	DomainControllerAddressType;
		internal Guid	DomainGuid;
		[MarshalAs(UnmanagedType.LPTStr)]
		internal string	DomainName;
		[MarshalAs(UnmanagedType.LPTStr)]
		internal string	DnsForestName;
		internal uint	Flags;
		[MarshalAs(UnmanagedType.LPTStr)]
		internal string	DcSiteName;
		[MarshalAs(UnmanagedType.LPTStr)]
		internal string	ClientSiteName;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	internal struct DOMAIN_CONTROLLER_INFO_2
	{
		internal LPTSTR NetbiosName;
		internal LPTSTR DnsHostName;
		internal LPTSTR SiteName;
		internal LPTSTR SiteObjectName;
		internal LPTSTR ComputerObjectName;
		internal LPTSTR ServerObjectName;
		internal LPTSTR NtdsDsaObjectName;
		internal BOOL fIsPdc;
		internal BOOL fDsEnabled;
		internal BOOL fIsGc;
		internal GUID SiteObjectGuid;
		internal GUID ComputerObjectGuid;
		internal GUID ServerObjectGuid;
		internal GUID NtdsDsaObjectGuid;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	internal struct DOMAIN_CONTROLLER_INFO_1 
	{
		internal LPTSTR NetbiosName;
		internal LPTSTR DnsHostName;
		internal LPTSTR SiteName;
		internal LPTSTR ComputerObjectName;
		internal LPTSTR ServerObjectName;
		internal BOOL fIsPdc;
		internal BOOL fDsEnabled;
	}


	[StructLayout(LayoutKind.Sequential)]
	internal struct LPSOCKET_ADDRESS
	{
		internal IntPtr lpSockaddr;
		internal int iSockaddrLength;
	}

	[StructLayout( LayoutKind.Sequential, CharSet=CharSet.Auto )]
	public struct DS_NAME_RESULT
	{
		public DWORD cItems;
		public IntPtr rItems; //  Array of pointers to DS_NAME_RESULT_ITEM
	}

	[StructLayout( LayoutKind.Sequential, CharSet=CharSet.Auto )]
	public struct DS_NAME_RESULT_ITEM
	{
		public DWORD status;
		public LPTSTR pDomain;
		public LPTSTR pName;
	}
}
