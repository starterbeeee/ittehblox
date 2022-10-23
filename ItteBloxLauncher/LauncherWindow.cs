using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ItteBloxLauncher
{
    public partial class LauncherWindow : Form
    {
        string[] args = Program.args;
        BackgroundWorker bg;

        public LauncherWindow()
        {
            InitializeComponent();
            bg = new BackgroundWorker();
            bg.DoWork += ExtractTask;
            bg.ProgressChanged += UpdateProgress;
            bg.WorkerReportsProgress = true;
        }

        public string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private void UpdateProgress(object sender, ProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
        }

        private void ExtractTask(object sender, DoWorkEventArgs e)
        {
            string downloadserver = Get("http://ittblox.gay/api/launcher/setupsite.php");
            string year = args[0].Substring(6, 7).Replace("/", string.Empty).Replace(":", string.Empty);
            string vrsn = Get("http://ittblox.gay/api/launcher/version" + year);
            string dir = Application.StartupPath + "\\" + year;
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
            using (var client = new System.Net.Http.HttpClient())
            using (var stream = client.GetStreamAsync(downloadserver + "/" + year + ".zip").Result)
            {
                var basepath = Path.Combine(dir);
                System.IO.Directory.CreateDirectory(basepath);

                var ar = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read);
                var i = 0.0;
                foreach (var entry in ar.Entries)
                {
                    i += 1.0;
                    int percentage = Convert.ToInt32(i / (double)ar.Entries.Count * 100);
                    bg.ReportProgress(percentage);
                    //MessageBox.Show(percentage.ToString());
                    var path = Path.Combine(basepath, entry.FullName);

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));
                        continue;
                    }

                    using (var entryStream = entry.Open())
                    {
                        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));
                        using (var file = File.Create(path))
                        {
                            entryStream.CopyTo(file);
                        }
                    }
                }
            }
            File.WriteAllText(Application.StartupPath + "\\version" + year + ".txt", vrsn);
            LaunchClient(year);
            return;
        }

        void UpgradeAndLaunch(string year, string vrsn)
        {
            string downloadserver = Get("http://ittblox.gay/api/launcher/setupsite.php");
            if (downloadserver == "")
            {
                MessageBox.Show("Download server is not available");
                Application.Exit();
            }
            else
            {
                ///Status.Text = "Downloading...";
                bg.RunWorkerAsync();
            }
            return;
        }

        public async Task LaunchClient(string year)
        {
            Status.Text = "Launching";
            string port = args[0].Substring(13, 5);
            string token = args[0].Substring(18, 50);
            string placeid = args[0].Substring(68).Replace("/", string.Empty);
            string path = (string)Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\itblox").GetValue(year);
            try
            {
                switch (year)
                {
                    case "2013":
                        var proc13 = Process.Start(path, "-a \"http://ittblox.gay/\" -t 1 -j \"http://ittblox.gay/game/join.php?port=" + port + "&token=" + token + "\"");
                        while (string.IsNullOrEmpty(proc13.MainWindowTitle) && !proc13.HasExited)
                        {
                            await Task.Delay(500);
                            proc13.Refresh();
                        }
                        break;
                    case "216c":
                        var proc16c = Process.Start(path, "-a \"http://ittblox.gay/\" -t 1 -j \"http://ittblox.gay/game/2016/join.php?placeId=" + placeid + "&port=" + port + "&token=" + token + "\"");
                        while (string.IsNullOrEmpty(proc16c.MainWindowTitle) && !proc16c.HasExited)
                        {
                            await Task.Delay(500);
                            proc16c.Refresh();
                        }
                        break;
                    case "2016":
                        var proc16 = Process.Start(path, "-a \"http://ittblox.gay/\" -t 1 -j \"http://ittblox.gay/game/2016/join.php?placeId=" + placeid + "&port=" + port + "&token=" + token + "\"");
                        while (string.IsNullOrEmpty(proc16.MainWindowTitle) && !proc16.HasExited)
                        {
                            await Task.Delay(500);
                            proc16.Refresh();
                        }
                        break;
                    case "s16c":
                        var procst16c = Process.Start(path);
                        while (string.IsNullOrEmpty(procst16c.MainWindowTitle) && !procst16c.HasExited)
                        {
                            await Task.Delay(500);
                            procst16c.Refresh();
                        }
                        break;
                    case "ibox":
                        var procbox = Process.Start(path, args[0]);
                        while (string.IsNullOrEmpty(procbox.MainWindowTitle) && !procbox.HasExited)
                        {
                            await Task.Delay(500);
                            procbox.Refresh();
                        }
                        break;
                    case "st16":
                        var procst16 = Process.Start(path);
                        while (string.IsNullOrEmpty(procst16.MainWindowTitle) && !procst16.HasExited)
                        {
                            await Task.Delay(500);
                            procst16.Refresh();
                        }
                        break;
                    default:
                        MessageBox.Show("Invalid year!");
                        Application.Exit();
                        break;
                }
            }
            catch (Exception a)
            {
                MessageBox.Show(a.ToString());
            }
            Application.Exit();
            return;
        }

        private void LauncherWindow_Load(object sender, EventArgs e)
        {

            if (args.Length > 0)
            {
                if (Uri.TryCreate(args[0], UriKind.Absolute, out var uri) && string.Equals(uri.Scheme, "itblox", StringComparison.OrdinalIgnoreCase))
                {
                    Status.Text = "Checking for updates";
                    string year = args[0].Substring(6, 7).Replace("/", string.Empty).Replace(":", string.Empty);
                    string vrsnpath = Application.StartupPath + "\\version" + year + ".txt";
                    string vrsn = Get("http://ittblox.gay/api/launcher/version" + year);
                    if (File.Exists(vrsnpath))
                    {
                        if (vrsn == File.ReadAllText(vrsnpath))
                        {
                            Progress.Value = 100;
                            LaunchClient(year);
                        } else {
                            Status.Text = "Upgrading ItteBlox";
                            UpgradeAndLaunch(year, vrsn);
                            return;
                        }
                    } else {
                        Status.Text = "Upgrading ItteBlox";
                        UpgradeAndLaunch(year, vrsn);
                        return;
                    }
                }
            }
            else
            {
                Status.Text = "Join from the website to play!";
                Progress.Value = 100;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
