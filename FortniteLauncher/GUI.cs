using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Win32;
using FortniteLauncher.Properties;
using System.Threading.Tasks;
using FolderBrowserEx;
using DiscordRPC;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;


namespace FortniteLauncher
{
	public partial class GUI : MaterialForm
    {
		public DiscordRpcClient client;

		//Called when your application first starts.
		//For example, just before your main loop, on OnEnable for unity.
		void Initialize()
		{
#if DEBUG
#else
			materialComboBox1.Visible = false; // for hosterss false means not for hoster
#endif
			client = new DiscordRpcClient("1112565946129846273");

			//Subscribe to events
			client.OnReady += (sender, e) =>
			{
				Console.WriteLine("Received Ready from user {0}", e.User.Username);
			};

			client.OnPresenceUpdate += (sender, e) =>
			{
				Console.WriteLine("Received Update! {0}", e.Presence);
			};

			//Connect to the RPC
			client.Initialize();

			//Set the rich presence
			//Call this as many times as you want and anywhere in your code.
			client.SetPresence(new RichPresence()
			{
				Details = "Playing Flow",
				State = "In the launcher",
				Assets = new Assets()
				{
					LargeImageKey = "largeimage",
					LargeImageText = "",
					SmallImageKey = ""
				},
				Buttons = new DiscordRPC.Button[]
				{
					new DiscordRPC.Button() { Label = "Join here!", Url = "https://discord.gg/P2mnVUZfZY" }
				}
			});

		}
        public GUI()
		{
			this.Icon = Properties.Resources.Fortnite;
            InitializeComponent();
			MaterialSkinManager instance = MaterialSkinManager.Instance;
			instance.AddFormToManage(this);
			instance.Theme = MaterialSkinManager.Themes.DARK;
			instance.ColorScheme = new ColorScheme(Primary.Grey800, Primary.Grey900, Primary.Orange800, Accent.Orange700, TextShade.WHITE);
			materialTextBox1.Text = Settings.Default.Username;
			materialTextBox2.Text = Settings.Default.Path;
			materialTextBox4.Text = Settings.Default.Args;
            materialTextBox3.Text = Settings.Default.Password;
			if (!string.IsNullOrWhiteSpace(materialTextBox2.Text))
			{
				pictureBox1.Load(this.materialTextBox2.Text + "\\FortniteGame\\Content\\Splash\\Splash.bmp");
			}

			Initialize();
		}
		[DllImport("wininet.dll")]
		public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
		[DllImport("user32.dll")]
		public new static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[DllImport("user32.dll")]
		public new static extern bool ReleaseCapture();
		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);
		private bool IsValidPath(string path)
		{
			if (new Regex("^[a-zA-Z]:\\\\$").IsMatch(path.Substring(0, 3)))
			{
				string text = new string(Path.GetInvalidPathChars());
				text += ":/?*\"";
				return !new Regex("[" + Regex.Escape(text) + "]").IsMatch(path.Substring(3, path.Length - 3));
			}
			return false;
		}
		private void materialButton2_Click(object sender, EventArgs e)
		{
			FolderBrowserEx.FolderBrowserDialog diag = new FolderBrowserEx.FolderBrowserDialog();
			diag.Title = "Select a Fortnite Build";
			diag.InitialFolder = @"C:\";
			diag.AllowMultiSelect = false;

			if (diag.ShowDialog() == DialogResult.OK)
			{
				materialTextBox2.Text = diag.SelectedFolder;
				pictureBox1.Load(this.materialTextBox2.Text + "\\FortniteGame\\Content\\Splash\\Splash.bmp");
			}
		}
		private void materialButton1_Click(object sender, EventArgs e)
		{
			int num = Convert.ToInt32(Console.ReadLine());
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
			if (!new Regex("^([a-zA-Z0-9@.])*$").IsMatch(this.materialTextBox1.Text))
			{
				MessageBox.Show("Invalid email, email cannot contain any special characters.");
				return;
			}
			if (string.IsNullOrEmpty(this.materialTextBox2.Text) || this.materialTextBox1.Text.Length < 3)
			{
				MessageBox.Show("Username cannot be empty or below 3 characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			try
			{
				if (!this.IsValidPath(this.materialTextBox2.Text))
				{
					MessageBox.Show("Invalid Fortnite path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
			}
			catch
			{
				MessageBox.Show("Invalid Fortnite path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			string text = Path.Combine(this.materialTextBox2.Text, "FortniteGame\\Binaries\\Win64\\FortniteClient-Win64-Shipping.exe");
			if (!File.Exists(text))
			{
				MessageBox.Show("\"FortniteClient-Win64-Shipping.exe\" was not found, please make sure it exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}

			Settings.Default.Username = this.materialTextBox1.Text;
            Settings.Default.Path = this.materialTextBox2.Text;
            Settings.Default.Password = this.materialTextBox3.Text;
			Settings.Default.Save();

			string text2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Redirect.dll");
			if (!File.Exists(text2))
			{
				MessageBox.Show("\"Redirect.dll\" was not found, please make sure it exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			// dstring text3 = "-AUTH_LOGIN=" + this.materialTextBox1.Text + " -AUTH_PASSWORD=\"\" + this.materialTextBox3.Text + ""\\-AUTH_TYPE=epic -NOSSLPINNING";

            string user = "-AUTH_LOGIN=\"" + this.materialTextBox1.Text;
            string pass = "-AUTH_PASSWORD=\"" + this.materialTextBox3.Text;

            // user + " " + pass + ""
            // string caldera = "-caldera=eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2NvdW50X2lkIjoiYmU5ZGE1YzJmYmVhNDQwN2IyZjQwZWJhYWQ4NTlhZDQiLCJnZW5lcmF0ZWQiOjE2Mzg3MTcyNzgsImNhbGRlcmFHdWlkIjoiMzgxMGI4NjMtMmE2NS00NDU3LTliNTgtNGRhYjNiNDgyYTg2IiwiYWNQcm92aWRlciI6IkVhc3lBbnRpQ2hlYXQiLCJub3RlcyI6IiIsImZhbGxiYWNrIjpmYWxzZX0.VAWQB67RTxhiWOxx7DBjnzDnXyyEnX7OljJm-j2d88G_WgwQ9wrE6lwMEHZHjBd1ISJdUO1UVUqkfLdU5nofBQ";

            // string text3 = "-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -skippatchcheck -nobe -fromfl=eac -fltoken=3db3ba5dcbd2e16703f3978d" + caldera + " " + user + "\" " + pass + "\" -AUTH_TYPE=epic";
            string text3 = "-epicapp=Fortnite -epicenv=Prod -epicportal -AUTH_TYPE=epic " + user + "\" " + pass + "\" -epiclocale=en-us -fltoken=3db3ba5dcbd2e16703f3978d -fromfl=none -noeac -nobe -skippatchcheck ";
			text3 += "-caldera=eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2NvdW50X2lkIjoiYmU5ZGE1YzJmYmVhNDQwN2IyZjQwZWJhYWQ4NTlhZDQiLCJnZW5lcmF0ZWQiOjE2Mzg3MTcyNzgsImNhbGRlcmFHdWlkIjoiMzgxMGI4NjMtMmE2NS00NDU3LTliNTgtNGRhYjNiNDgyYTg2IiwiYWNQcm92aWRlciI6IkVhc3lBbnRpQ2hlYXQiLCJub3RlcyI6IiIsImZhbGxiYWNrIjpmYWxzZX0.VAWQB67RTxhiWOxx7DBjnzDnXyyEnX7OljJm-j2d88G_WgwQ9wrE6lwMEHZHjBd1ISJdUO1UVUqkfLdU5nofBQ";

            text3 += " " + materialTextBox4.Text;

            Console.WriteLine(text3);
            _clientProcess = new Process
			{
				StartInfo = new ProcessStartInfo(text, text3)
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = false,
					RedirectStandardError = true

                }
            };
            GUI._clientProcess.OutputDataReceived += InjectConsole;
            GUI._clientProcess.Start();
			GUI._clientProcess.BeginOutputReadLine();
            if (num != 0)
			{
				Console.WriteLine("Invalid Argument!");
				Console.ReadKey();
				return;
			}
			registryKey.SetValue("ProxyEnable", 0);
			registryKey.SetValue("ProxyServer", 0);

			bool bInjectClient = false;

			client.SetPresence(new RichPresence()
			{
				Details = "Playing Flow!",
				State = "Playing Fortnite",
				Assets = new Assets()
				{
					LargeImageKey = "largeimage",
					LargeImageText = "",
					SmallImageKey = "smallimage"
				},
				Buttons = new DiscordRPC.Button[]
				{
					new DiscordRPC.Button() { Label = "Join here!", Url = "https://discord.gg/P2mnVUZfZY" }
				}
			});

			if ((int)registryKey.GetValue("ProxyEnable", 1) != 1)
			{
				Console.WriteLine("The proxy has been turned off.");
			}
			else
			{
				Console.WriteLine("Unable to disable the proxy.");
			}
			registryKey.Close();
			GUI.InternetSetOption(IntPtr.Zero, 39, IntPtr.Zero, 0);
			GUI.InternetSetOption(IntPtr.Zero, 37, IntPtr.Zero, 0);

			return;
		}

        private void InjectConsole(object sender, DataReceivedEventArgs e)
        {
            string text2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Redirect.dll");
            string text1 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Console.dll");
			string text3 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Server.dll");

            if (e.Data != null)
            {
                if (e.Data.Contains("CheckingForPatch"))
                {
                    Console.WriteLine("CheckingForPatch logged MAYBE INJECT NOW?");

                    IntPtr hProcess = Win32.OpenProcess(1082, false, GUI._clientProcess.Id);
                    IntPtr procAddress = Win32.GetProcAddress(Win32.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                    uint num2 = (uint)((text2.Length + 1) * Marshal.SizeOf(typeof(char)));
                    IntPtr intPtr = Win32.VirtualAllocEx(hProcess, IntPtr.Zero, num2, 12288U, 4U);
                    UIntPtr uintPtr;
                    Win32.WriteProcessMemory(hProcess, intPtr, Encoding.Default.GetBytes(text2), num2, out uintPtr);
                    Win32.CreateRemoteThread(hProcess, IntPtr.Zero, 0U, procAddress, intPtr, 0U, IntPtr.Zero);

					if (materialComboBox1.Text == "Hosting")
					{
                        IntPtr hProcess1 = Win32.OpenProcess(1082, false, GUI._clientProcess.Id);
                        IntPtr procAddress1 = Win32.GetProcAddress(Win32.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                        uint num1 = (uint)((text3.Length + 1) * Marshal.SizeOf(typeof(char)));
                        IntPtr intPtr1 = Win32.VirtualAllocEx(hProcess1, IntPtr.Zero, num1, 12288U, 4U);
                        UIntPtr uintPtr1;
                        Win32.WriteProcessMemory(hProcess1, intPtr1, Encoding.Default.GetBytes(text3), num1, out uintPtr1);
                        Win32.CreateRemoteThread(hProcess1, IntPtr.Zero, 0U, procAddress1, intPtr1, 0U, IntPtr.Zero);
                    }
                    else
                    {
                        IntPtr hProcess1 = Win32.OpenProcess(1082, false, GUI._clientProcess.Id);
                        IntPtr procAddress1 = Win32.GetProcAddress(Win32.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                        uint num1 = (uint)((text1.Length + 1) * Marshal.SizeOf(typeof(char)));
                        IntPtr intPtr1 = Win32.VirtualAllocEx(hProcess1, IntPtr.Zero, num1, 12288U, 4U);
                        UIntPtr uintPtr1;
                        Win32.WriteProcessMemory(hProcess1, intPtr1, Encoding.Default.GetBytes(text1), num1, out uintPtr1);
                        Win32.CreateRemoteThread(hProcess1, IntPtr.Zero, 0U, procAddress1, intPtr1, 0U, IntPtr.Zero);
                    }

                    GUI._clientProcess.CancelOutputRead();

                }
            }

			return;
        }
        private void materialButton3_Click_1(object sender, EventArgs e)
		{
			Settings.Default.Path = this.materialTextBox2.Text;
            Settings.Default.Args = this.materialTextBox4.Text;
            Settings.Default.Save();
		}
		public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
		public const int INTERNET_OPTION_REFRESH = 37;
		private static Process _clientProcess;
		private static byte _clientAnticheat;

        private void GUI_Load(object sender, EventArgs e)
        {

        }

        private void folderBrowserDialogBrowse_HelpRequest(object sender, EventArgs e)
        {

        }

        private void materialButton4_Click(object sender, EventArgs e)
        {

        }

        private void materialButton4_Click_1(object sender, EventArgs e)
        {
			Process.Start("https://discord.gg/P2mnVUZfZY");
		}

        private void materialButton5_Click(object sender, EventArgs e)
        {
			Settings.Default.Username = this.materialTextBox1.Text;
			Settings.Default.Password = this.materialTextBox3.Text;
			Settings.Default.Save();
		}

        private void materialTabSelector1_Click(object sender, EventArgs e)
        {

        }

        private void materialTextBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void materialTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void materialTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void materialComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
