using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DotNetDevGuide.DirectoryServices
{
    using DWORD = System.UInt32;
    using HANDLE = System.IntPtr;
    using HLOCAL = System.IntPtr;
    using BOOL = System.Int32;
    using LPCTSTR = System.String;
    using LPTSTR = System.String;

    /// <summary>
    /// Win32 Utils originally adapted from unmanaged code library found on GotDotNet here:
    /// http://www.gotdotnet.com/Community/UserSamples/Details.aspx?SampleGuid=E6098575-DDA0-48B8-9ABF-E0705AF065D9
    /// </summary>
    public class Win32
    {
        public const BOOL FALSE = 0;
        public const BOOL TRUE = 1;

        public const int SUCCESS = 0;
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_BAD_LENGTH = 24;
        public const int ERROR_INSUFFICIENT_BUFFER = 122;
        public const int ERROR_MORE_DATA = 127; //234;
        public const int ERROR_NO_TOKEN = 1008;
        public const int ERROR_NOT_ALL_ASSIGNED = 1300;
        public const int ERROR_NONE_MAPPED = 1332;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern HLOCAL LocalFree(HLOCAL hMem);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern void SetLastError(DWORD dwErrCode);

        public static bool ToBool(BOOL bValue)
        {
            return (bValue != Win32.FALSE);
        }
        public static BOOL FromBool(bool bValue)
        {
            return (bValue ? Win32.TRUE : Win32.FALSE);
        }
        public static IntPtr AllocGlobal(uint cbSize)
        {
            return AllocGlobal((int)cbSize);
        }
        public static IntPtr AllocGlobal(int cbSize)
        {
            return Marshal.AllocHGlobal(cbSize);
        }
        public static void FreeGlobal(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return;
            Marshal.FreeHGlobal(ptr);
        }

        public static DWORD GetLastError()
        {
            return (DWORD)Marshal.GetLastWin32Error();
        }

        public static void ThrowLastError()
        {
            //Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            throw new System.ComponentModel.Win32Exception();
        }

        public static void CheckCall(bool funcResult)
        {
            if (!funcResult)
            {
                ThrowLastError();
            }
        }

        public static void CheckCall(DWORD funcResult)
        {
            CheckCall(funcResult != 0);
        }

        public static void CheckCall(BOOL funcResult)
        {
            CheckCall(funcResult != 0);
        }

        public static void CheckCall(HANDLE funcResult)
        {
            CheckCall(!IsNullHandle(funcResult));
        }

        public static bool IsNullHandle(HANDLE ptr)
        {
            return (ptr == IntPtr.Zero);
        }
    }


}
