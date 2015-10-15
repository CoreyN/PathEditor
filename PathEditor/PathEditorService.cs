using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace PathEditor
{
    class PathEditorService
    {
        bool _readOnlyMode;
        RegistryKey systemEnvironmentKey;

        public bool IsReadOnly { get { return _readOnlyMode; } }

        public PathEditorService()
        {
            try
            {
                OpenPathRegistryKey();
            }
            catch(SecurityException)
            {
                OpenPathRegistryKeyReadOnly();
            }

        }

        public IEnumerable<string> GetSystemPath()
        {
            string path = systemEnvironmentKey.GetValue("Path") as string;
            Debug.WriteLine(path);
            return path.Split(new char[] { ';' });
        }

        public void SetSystemPath(IEnumerable<string> paths)
        {
            if (_readOnlyMode)
                throw new InvalidOperationException("Cannot set the system path when in read only mode");

            string pathValue = String.Join(";", paths.ToArray());
            systemEnvironmentKey.SetValue("Path", pathValue);
            TellAllWindowsThatEnvironmentHasChanged();
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void TellAllWindowsThatEnvironmentHasChanged() //todo: rename
        {
            IntPtr HWND_BROADCAST = new IntPtr(0xffff);
            IntPtr dwReturnValue;
            StringBuilder txt = new StringBuilder("Environment");
            SendMessageTimeoutText(HWND_BROADCAST, 0x001A, txt.Length, txt, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 5000, out dwReturnValue);
        }

        private void OpenPathRegistryKey()
        {
            systemEnvironmentKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", true);
            _readOnlyMode = false;
        }

        private void OpenPathRegistryKeyReadOnly()
        {
            systemEnvironmentKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");
            _readOnlyMode = true;
        }

        [Flags]
        enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
            SMTO_ERRORONEXIT = 0x20
        }

        [DllImport("user32.dll", EntryPoint = "SendMessageTimeout", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint SendMessageTimeoutText(
            IntPtr hWnd,
            int Msg,              // Use WM_GETTEXT
            int countOfChars,
            StringBuilder text,
            SendMessageTimeoutFlags flags,
            uint uTImeoutj,
            out IntPtr result);
    }
}
