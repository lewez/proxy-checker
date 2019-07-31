using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyChecker {
	public class ProxyCheckerForm : Form {
		private OpenFileDialog proxyFileDialog;
		private Button openProxyFileDialog;
		private ProgressBar proxyFileProgress;

		public ProxyCheckerForm() {
			proxyFileDialog = new OpenFileDialog();
			proxyFileDialog.Title = "Select proxy list";
			proxyFileDialog.DefaultExt = "txt";
			proxyFileDialog.FileOk += proxyFileDialog_FileOk;

			openProxyFileDialog = new Button();
			openProxyFileDialog.Text = "Open proxy list";
			openProxyFileDialog.AutoSize = true;
			openProxyFileDialog.Click += openProxyFileDialog_Click;

			proxyFileProgress = new ProgressBar();
			proxyFileProgress.Left = 120;
			
			Controls.Add(openProxyFileDialog);
			Controls.Add(proxyFileProgress);
		}

		private void openProxyFileDialog_Click(object sender, EventArgs e) {
			proxyFileDialog.ShowDialog();
		}

		private void proxyFileDialog_FileOk(object sender, CancelEventArgs e) {
			Console.WriteLine("Proxy list selected");

			Stream filestream = proxyFileDialog.OpenFile();

			using (StreamReader reader = new StreamReader(filestream)) {
				List<WebProxy> proxies = ProxyListParser.Parse(reader.ReadToEnd());

				ProxyChecker.CheckProxies(proxies);
			}
		}
	}
}