using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

namespace ProxyChecker {
	public class ProxyCheckerForm : Form {
		private TableLayoutPanel layoutPanel;
		private TableLayoutPanel listViewLayoutPanel;
		private TableLayoutPanel controlLayoutPanel;
		private OpenFileDialog proxyFileDialog;
		private Button openProxyFileDialog;
		private ProgressBar proxyFileProgress;
		private ListView proxyCheckedList;
		private Button exportWorkingProxies;
		private NumericUpDown timeoutThreshold;
		private TextBox targetWebsite;
		private Button cancelProxyCheck;
		private CancellationTokenSource cancellationToken;

		public ProxyCheckerForm() {
			this.Width = 600;
			this.Height = 400;

			layoutPanel = new TableLayoutPanel();
			layoutPanel.Dock = DockStyle.Fill;
			layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 95));
			layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 5));

			listViewLayoutPanel = new TableLayoutPanel();
			listViewLayoutPanel.Dock = DockStyle.Fill;
			listViewLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
			listViewLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
			listViewLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

			controlLayoutPanel = new TableLayoutPanel();
			controlLayoutPanel.Dock = DockStyle.Fill;
			controlLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			controlLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
			controlLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
			controlLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
			controlLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
			controlLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

			proxyFileDialog = new OpenFileDialog();
			proxyFileDialog.Title = "Select proxy list";
			proxyFileDialog.DefaultExt = "txt";
			proxyFileDialog.FileOk += proxyFileDialog_FileOk;

			// Disable control while proxy check is underway
			openProxyFileDialog = new Button();
			openProxyFileDialog.Dock = DockStyle.Fill;
			openProxyFileDialog.Text = "Open proxy list";
			openProxyFileDialog.AutoSize = true;
			openProxyFileDialog.Click += openProxyFileDialog_Click;

			proxyFileProgress = new ProgressBar();
			proxyFileProgress.Dock = DockStyle.Fill;
			proxyFileProgress.Width = 300;
			proxyFileProgress.Left = 120;

			proxyCheckedList = new ListView();
			proxyCheckedList.View = View.Details;
			proxyCheckedList.Dock = DockStyle.Fill;
			proxyCheckedList.Columns.Add("#", 0);
			proxyCheckedList.Columns.Add("Address", 220, HorizontalAlignment.Center);
			proxyCheckedList.Columns.Add("Status", -2, HorizontalAlignment.Center);
			proxyCheckedList.FullRowSelect = true;
			proxyCheckedList.GridLines = true;

			// Disable control until proxy check is complete
			exportWorkingProxies = new Button();
			exportWorkingProxies.Dock = DockStyle.Fill;
			exportWorkingProxies.Text = "Export working proxies";
			exportWorkingProxies.AutoSize = true;
			exportWorkingProxies.Top = 300;

			// Disable control while proxy check is underway
			timeoutThreshold = new NumericUpDown();
			timeoutThreshold.Dock = DockStyle.Fill;
			timeoutThreshold.Minimum = 1;
			timeoutThreshold.Maximum = 600;
			timeoutThreshold.Value = 10;
			timeoutThreshold.Left = 500;

			// Disable control while proxy check is underway
			targetWebsite = new TextBox();
			targetWebsite.Dock = DockStyle.Fill;
			targetWebsite.Text = "http://google.com";
			targetWebsite.Left = 500;
			targetWebsite.Top = 24;
			targetWebsite.Width = timeoutThreshold.Width;

			// Disable control until proxy check is underway
			cancelProxyCheck = new Button();
			cancelProxyCheck.Dock = DockStyle.Fill;
			cancelProxyCheck.Text = "Cancel";
			cancelProxyCheck.Top = 330;
			cancelProxyCheck.Click += cancelProxyCheck_Click;

			layoutPanel.Controls.Add(listViewLayoutPanel, 0, 0);
			layoutPanel.Controls.Add(proxyFileProgress, 0, 1);
			listViewLayoutPanel.Controls.Add(proxyCheckedList, 0, 0);
			listViewLayoutPanel.Controls.Add(controlLayoutPanel, 1, 0);
			controlLayoutPanel.Controls.Add(openProxyFileDialog, 0, 0);
			controlLayoutPanel.Controls.Add(cancelProxyCheck, 0, 1);
			controlLayoutPanel.Controls.Add(exportWorkingProxies, 0, 2);
			controlLayoutPanel.Controls.Add(timeoutThreshold, 0, 3);
			controlLayoutPanel.Controls.Add(targetWebsite, 0, 4);

			Controls.Add(layoutPanel);
		}

		private void openProxyFileDialog_Click(object sender, EventArgs e) {
			proxyFileDialog.ShowDialog();
		}

		private void proxyFileProgress_ProgressChanged(object sender, ProxyCheckProgressReport progreport) {
			if (cancellationToken == null || cancellationToken.IsCancellationRequested) {
				return;
			}

			proxyFileProgress.Value = (progreport.NumChecked * 100) / progreport.NumTotal;

			ListViewItem proxyRow = new ListViewItem();
			proxyRow.SubItems.Add(progreport.ProxyChecked.Address.ToString());
			proxyRow.SubItems.Add(progreport.ProxyCheckResult.ToString());

			proxyCheckedList.Items.Add(proxyRow);
			proxyCheckedList.Columns[2].Width = -2; // Resize status column to fill
		}

		private async void proxyFileDialog_FileOk(object sender, CancelEventArgs e) {
			Console.WriteLine("Proxy list selected");

			Stream filestream = proxyFileDialog.OpenFile();

			using (StreamReader reader = new StreamReader(filestream)) {
				List<WebProxy> proxies = ProxyListParser.Parse(reader.ReadToEnd());

				Progress<ProxyCheckProgressReport> progress = new Progress<ProxyCheckProgressReport>();
				progress.ProgressChanged += proxyFileProgress_ProgressChanged;

				cancellationToken = new CancellationTokenSource();

				changeRunningState(true);
				proxyCheckedList.Items.Clear();

				await ProxyChecker.CheckProxiesAsync(proxies, targetWebsite.Text, (int)timeoutThreshold.Value, progress, cancellationToken.Token);
			}
		}

		private void cancelProxyCheck_Click(object sender, EventArgs e) {
			cancellationToken.Cancel();
			changeRunningState(false);
		}

		private void changeRunningState(bool isRunning) {
			if (isRunning) {
				cancelProxyCheck.Enabled = true;
				openProxyFileDialog.Enabled = false;
				exportWorkingProxies.Enabled = false;
				timeoutThreshold.Enabled = false;
				targetWebsite.Enabled = false;
			}
			else {
				cancelProxyCheck.Enabled = false;
				openProxyFileDialog.Enabled = true;
				exportWorkingProxies.Enabled = true;
				timeoutThreshold.Enabled = true;
				targetWebsite.Enabled = true;
				proxyFileProgress.Value = 0;
			}
		}
	}
}