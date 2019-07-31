using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace ProxyChecker {
	public class ProxyCheckerForm : Form {
		private OpenFileDialog proxyFileDialog;
		private Button openProxyFileDialog;

		public ProxyCheckerForm() {
			proxyFileDialog = new OpenFileDialog();
			proxyFileDialog.FileOk += proxyFileDialog_FileOk;

			openProxyFileDialog = new Button();
			openProxyFileDialog.Text = "Open proxy list";
			openProxyFileDialog.AutoSize = true;
			openProxyFileDialog.Click += openProxyFileDialog_Click;
			
			Controls.Add(openProxyFileDialog);
		}

		private void openProxyFileDialog_Click(object sender, EventArgs e) {
			proxyFileDialog.ShowDialog();
		}

		private void proxyFileDialog_FileOk(object sender, CancelEventArgs e) {
			Console.WriteLine("Proxy list(s) selected");
		}
	}
}