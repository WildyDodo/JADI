using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JADI___Just_Another_DLL_Injector
{
    public partial class Form1 : Form
    {
        Advanced dialog = new Advanced();
        Process selectedProcess;
        string selectedFile;
        List<string> modules = new List<string>();

        enum LogType
        {
            Info,
            Warn,
            Error
        }

        private void addLog(string text, LogType type)
        {
            if (type == LogType.Info)
                textBox1.AppendText("(" + DateTime.Now.ToLocalTime().ToString() + ")[INFO]: " + text + "\n");
            else if (type == LogType.Warn)
                textBox1.AppendText("(" + DateTime.Now.ToLocalTime().ToString() + ")[WARN]: " + text + "\n");
            else
                textBox1.AppendText("(" + DateTime.Now.ToLocalTime().ToString() + ")[ERROR]: " + text + "\n");
        }

        [Flags]
        enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000,
            ReadControl = 0x00020000
        }

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        private static extern bool QueryFullProcessImageName(IntPtr hprocess, int dwFlags,
               StringBuilder lpExeName, out int size);
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess,
                       bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);

        private static string getExecutablePath(int dwProcessId)
        {
            StringBuilder buffer = new StringBuilder(1024);
            IntPtr hprocess = OpenProcess(ProcessAccessFlags.QueryInformation, false, dwProcessId);
            if (hprocess != IntPtr.Zero)
            {
                try
                {
                    int size = buffer.Capacity;
                    if (QueryFullProcessImageName(hprocess, 0, buffer, out size))
                    {
                        return buffer.ToString();
                    }
                }
                finally
                {
                    CloseHandle(hprocess);
                }
            }
            return string.Empty;
        }

        [DllImport("kernel32.dll")]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

        private static bool is64bit(Process process)
        {
            bool retVal = false;
            IsWow64Process(process.Handle, out retVal);
            return retVal;
        }

        private static List<string> getProcessList()
        {
            Process[] list = Process.GetProcesses();
            List<string> processes = new List<string>();
            foreach (Process proc in list)
            {

                try
                {
                    string processName = Path.GetFileName(getExecutablePath(proc.Id));
                    if (processName.Length > 0)
                    {
                        string arch = is64bit(proc) ? "x64" : "x86";
                        processes.Add(processName + " [" + arch + "] (" + proc.Id + ")");
                    }
                        
                }
                catch { }
            }
            return processes;
        }

        static Process runWithoutParent(string procName)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = @"cmd",
                Arguments = "/C \"" + procName + "\" & exit",
                WindowStyle = ProcessWindowStyle.Hidden
            };
            return Process.Start(psi);
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = Application.ProductName + " | v" + Application.ProductVersion + " [x" + (Environment.Is64BitProcess ? "64" : "86") + "]";
            foreach (string process in getProcessList())
                comboBox1.Items.Add(process);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("JADI - Just Another DLL Injector, a tool made by Alessandro Nava just for fun.", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string proc = comboBox1.GetItemText(comboBox1.SelectedItem);
            proc = proc.Remove(0, proc.IndexOf('(') + 1);
            proc = proc.TrimEnd(')');
            selectedProcess = Process.GetProcessById(Int32.Parse(proc));
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                comboBox1.Enabled = true;
                button6.Enabled = false;
                button7.Enabled = true;
                comboBox1.Text = String.Empty;
            }
            else
            {
                comboBox1.Enabled = false;
                button6.Enabled = true;
                button7.Enabled = false;
            }
                
        }

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedFile = openFileDialog1.FileName;
                comboBox1.Text = Path.GetFileName(openFileDialog1.FileName);
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                comboBox1.Enabled = false;
                button6.Enabled = true;
            }
            else
            {
                comboBox1.Enabled = true;
                button6.Enabled = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                comboBox1.Enabled = false;
                button6.Enabled = true;
            }
            else
            {
                comboBox1.Enabled = true;
                button6.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Add(Path.GetFileName(openFileDialog2.FileName));
                modules.Add(openFileDialog2.FileName);
            }  
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Remove(listBox1.SelectedItem);
            foreach (var module in modules)
                if (module.EndsWith(listBox1.GetItemText(listBox1.SelectedItem)))
                {
                    modules.Remove(module);
                    break;
                }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;

            if (listBox1.Items.Count > 0)
            {
                if (radioButton1.Checked)
                {
                    if (comboBox1.Items.Count > 0)
                    {
                        DllInjector inj = new DllInjector();

                        foreach (var module in modules)
                        {
                            try
                            {
                                addLog("Parsing process name...", LogType.Info);
                                string shortModule = Path.GetFileName(module);
                                string processName = Path.GetFileName(getExecutablePath(selectedProcess.Id));
                                addLog("Adding module '" + shortModule + "' to [" + (is64bit(selectedProcess) ? "x64" : "x86") + "] '" + processName + "'.", LogType.Info);

                                if (dialog.InjDelay > 0)
                                    Thread.Sleep(dialog.InjDelay);

                                switch (inj.Inject(selectedProcess.Id, module))
                                {
                                    case DllInjectionResult.DllNotFound:
                                        addLog("Module '" + shortModule + "' not found.", LogType.Error);
                                        break;

                                    case DllInjectionResult.CreateRemoteThreadFailed:
                                        addLog("Failed CreateRemoteThread for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.GetProcAddressFailed:
                                        addLog("Failed GetProcAddress for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.OpenProcessFailed:
                                        addLog("Failed OpenProcess for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.VirtualAllocExFailed:
                                        addLog("Failed VirtualAllocEx for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.WriteProcessMemoryFailed:
                                        addLog("Failed WriteProcessMemory for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.Success:
                                        addLog("Module '" + shortModule + "' has been injected into '" + processName + "'.", LogType.Info);
                                        break;
                                }
                                if (dialog.hide)
                                    ShowWindow(selectedProcess.MainWindowHandle, 0);
                                if (dialog.closeInjector)
                                    Application.Exit();
                            }
                            catch (Exception ex)
                            {
                                addLog(ex.Message, LogType.Error);
                            }
                        }
                    }
                    else
                    {
                        addLog("Select a running process to inject DLL into.", LogType.Error);
                    }
                }else if (radioButton2.Checked)
                {
                    if (!String.IsNullOrEmpty(selectedFile))
                    {
                        DllInjector inj = new DllInjector();

                        foreach (var module in modules)
                        {
                            try
                            {
                                addLog("Parsing process name...", LogType.Info);
                                string shortModule = Path.GetFileName(module);
                                string processName = Path.GetFileName(selectedFile);
                                Process proc = runWithoutParent(selectedFile);
                                int procid = proc.Id;

                                addLog("Adding module '" + shortModule + "' to [" + (is64bit(proc) ? "x64" : "x86") + "] '" + processName + "'.", LogType.Info);

                                if (dialog.InjDelay > 0)
                                    Thread.Sleep(dialog.InjDelay);

                                switch (inj.Inject(procid, module))
                                {
                                    case DllInjectionResult.DllNotFound:
                                        addLog("Module '" + shortModule + "' not found.", LogType.Error);
                                        break;

                                    case DllInjectionResult.CreateRemoteThreadFailed:
                                        addLog("Failed CreateRemoteThread for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.GetProcAddressFailed:
                                        addLog("Failed GetProcAddress for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.OpenProcessFailed:
                                        addLog("Failed OpenProcess for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.VirtualAllocExFailed:
                                        addLog("Failed VirtualAllocEx for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.WriteProcessMemoryFailed:
                                        addLog("Failed WriteProcessMemory for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.Success:
                                        addLog("Module '" + shortModule + "' has been injected into '" + processName + "'.", LogType.Info);
                                        break;
                                }
                                if (dialog.hide)
                                    ShowWindow(selectedProcess.MainWindowHandle, 0);
                                if (dialog.closeInjector)
                                    Application.Exit();
                            }
                            catch (Exception ex)
                            {
                                addLog(ex.Message, LogType.Error);
                            }
                        }
                    }
                    else
                    {
                        addLog("Select a process to run to inject DLL into.", LogType.Error);
                    }
                }else if (radioButton3.Checked)
                {
                    if (!String.IsNullOrEmpty(selectedFile))
                    {
                        addLog("Waiting for process '" + Path.GetFileName(selectedFile) + "' to launch...", LogType.Info);
                        while (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(selectedFile)).Length == 0)
                        {
                            Application.DoEvents();
                        }

                        DllInjector inj = new DllInjector();
                        foreach (var module in modules)
                        {
                            try
                            {
                                addLog("Parsing process name...", LogType.Info);
                                string shortModule = Path.GetFileName(module);
                                string processName = Path.GetFileName(selectedFile);
                                Process proc = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName))[0];
                                int procid = proc.Id;

                                addLog("Adding module '" + shortModule + "' to [" + (is64bit(proc) ? "x64" : "x86") + "] '" + processName + "'.", LogType.Info);

                                if (dialog.InjDelay > 0)
                                    Thread.Sleep(dialog.InjDelay);

                                switch (inj.Inject(procid, module))
                                {
                                    case DllInjectionResult.DllNotFound:
                                        addLog("Module '" + shortModule + "' not found.", LogType.Error);
                                        break;

                                    case DllInjectionResult.CreateRemoteThreadFailed:
                                        addLog("Failed CreateRemoteThread for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.GetProcAddressFailed:
                                        addLog("Failed GetProcAddress for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.OpenProcessFailed:
                                        addLog("Failed OpenProcess for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.VirtualAllocExFailed:
                                        addLog("Failed VirtualAllocEx for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.WriteProcessMemoryFailed:
                                        addLog("Failed WriteProcessMemory for module '" + shortModule + "' to '" + processName + "'.", LogType.Error);
                                        break;

                                    case DllInjectionResult.Success:
                                        addLog("Module '" + shortModule + "' has been injected into '" + processName + "'.", LogType.Info);
                                        break;
                                }
                                if (dialog.hide)
                                    ShowWindow(selectedProcess.MainWindowHandle, 0);
                                if (dialog.closeInjector)
                                    Application.Exit();
                            }
                            catch (Exception ex)
                            {
                                addLog(ex.Message, LogType.Error);
                            }
                        }
                    }
                    else
                    {
                        addLog("Select a process to run to inject DLL into.", LogType.Error);
                    }
                }
            }
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            modules.Clear();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            foreach (string process in getProcessList())
                comboBox1.Items.Add(process);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            dialog.ShowDialog();
        }
    }
}
