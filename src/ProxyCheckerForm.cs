using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;

namespace ProxyChecker {
	public class ProxyCheckerForm : Form {
		private TableLayoutPanel layoutPanel;
		private TableLayoutPanel listViewLayoutPanel;
		private TableLayoutPanel controlLayoutPanel;
		private TableLayoutPanel timeoutLayoutPanel;
		private TableLayoutPanel targetWebsiteLayoutPanel;
		private OpenFileDialog proxyFileDialog;
		private Button openProxyFileDialog;
		private ProgressBar proxyFileProgress;
		private ListView proxyCheckedList;
		private Button exportWorkingProxies;
		private SaveFileDialog exportWorkingProxiesSaveDialog;
		private Label timeoutLabel;
		private NumericUpDown timeoutThreshold;
		private Label targetWebsiteLabel;
		private TextBox targetWebsite;
		private Button cancelProxyCheck;
		private CancellationTokenSource cancellationToken;
		private Dictionary<WebProxy, ProxyCheckResult> proxiesChecked;

		public ProxyCheckerForm() {
			this.Width = 600;
			this.Height = 400;
			this.Text = "Proxy Checker";

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
			controlLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 24));
			controlLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 24));
			controlLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 24));
			controlLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 14));
			controlLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 14));

			timeoutLayoutPanel = new TableLayoutPanel();
			timeoutLayoutPanel.Dock = DockStyle.Fill;
			timeoutLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			timeoutLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
			timeoutLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 65));

			targetWebsiteLayoutPanel = new TableLayoutPanel();
			targetWebsiteLayoutPanel.Dock = DockStyle.Fill;
			targetWebsiteLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
			targetWebsiteLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
			targetWebsiteLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 65));

			proxyFileDialog = new OpenFileDialog();
			proxyFileDialog.Title = "Select proxy list";
			proxyFileDialog.DefaultExt = "txt";
			proxyFileDialog.FileOk += proxyFileDialog_FileOk;

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

			exportWorkingProxies = new Button();
			exportWorkingProxies.Dock = DockStyle.Fill;
			exportWorkingProxies.Text = "Export working proxies";
			exportWorkingProxies.AutoSize = true;
			exportWorkingProxies.Top = 300;
			exportWorkingProxies.Click += exportWorkingProxies_Click;

			exportWorkingProxiesSaveDialog = new SaveFileDialog();
			exportWorkingProxiesSaveDialog.Title = "Export working proxies";
			exportWorkingProxiesSaveDialog.DefaultExt = "txt";
			exportWorkingProxiesSaveDialog.FileName = "exported_proxies";

			timeoutThreshold = new NumericUpDown();
			timeoutThreshold.Dock = DockStyle.Fill;
			timeoutThreshold.Minimum = 1;
			timeoutThreshold.Maximum = 600;
			timeoutThreshold.Value = 10;
			timeoutThreshold.Left = 500;

			timeoutLabel = new Label();
			timeoutLabel.AutoSize = true;
			timeoutLabel.Text = "Timeout (secs)";

			targetWebsite = new TextBox();
			targetWebsite.Dock = DockStyle.Fill;
			targetWebsite.Text = "http://google.com";
			targetWebsite.Left = 500;
			targetWebsite.Top = 24;
			targetWebsite.Width = timeoutThreshold.Width;

			targetWebsiteLabel = new Label();
			targetWebsiteLabel.AutoSize = true;
			targetWebsiteLabel.Text = "Testing Website";

			cancelProxyCheck = new Button();
			cancelProxyCheck.Dock = DockStyle.Fill;
			cancelProxyCheck.Text = "Cancel";
			cancelProxyCheck.Top = 330;
			cancelProxyCheck.Click += cancelProxyCheck_Click;

			layoutPanel.Controls.Add(listViewLayoutPanel, 0, 0);
			layoutPanel.Controls.Add(proxyFileProgress, 0, 1);
			listViewLayoutPanel.Controls.Add(proxyCheckedList, 0, 0);
			listViewLayoutPanel.Controls.Add(controlLayoutPanel, 1, 0);
			timeoutLayoutPanel.Controls.Add(timeoutLabel, 0, 0);
			timeoutLayoutPanel.Controls.Add(timeoutThreshold, 0, 1);
			targetWebsiteLayoutPanel.Controls.Add(targetWebsiteLabel, 0, 0);
			targetWebsiteLayoutPanel.Controls.Add(targetWebsite, 0, 1);
			controlLayoutPanel.Controls.Add(openProxyFileDialog, 0, 0);
			controlLayoutPanel.Controls.Add(cancelProxyCheck, 0, 1);
			controlLayoutPanel.Controls.Add(exportWorkingProxies, 0, 2);
			controlLayoutPanel.Controls.Add(timeoutLayoutPanel, 0, 3);
			controlLayoutPanel.Controls.Add(targetWebsiteLayoutPanel, 0, 4);

			Controls.Add(layoutPanel);

			changeRunningState(ProxyCheckerState.Initial);
		}

		private void openProxyFileDialog_Click(object sender, EventArgs e) {
			proxyFileDialog.ShowDialog();
		}

		private void proxyFileProgress_ProgressChanged(object sender, ProxyCheckProgressReport progreport) {
			if (cancellationToken == null || cancellationToken.IsCancellationRequested) {
				return;
			}

			proxiesChecked.Add(progreport.ProxyChecked, progreport.ProxyCheckResult);

			proxyFileProgress.Value = (proxiesChecked.Count * 100) / progreport.NumTotal;

			ListViewItem proxyRow = new ListViewItem();
			proxyRow.SubItems.Add(progreport.ProxyChecked.Address.ToString());
			proxyRow.SubItems.Add(progreport.ProxyCheckResult.ToString());

			proxyCheckedList.Items.Add(proxyRow);
			proxyCheckedList.Columns[2].Width = -2; // Resize status column to fill
		}

		private async void proxyFileDialog_FileOk(object sender, CancelEventArgs e) {
			Stream filestream = proxyFileDialog.OpenFile();

			using (StreamReader reader = new StreamReader(filestream)) {
				List<WebProxy> proxies = ProxyListParser.ToWebProxy(reader.ReadToEnd());

				Console.WriteLine("Proxy list selected. ({0}) proxies", proxies.Count);

				Progress<ProxyCheckProgressReport> progress = new Progress<ProxyCheckProgressReport>();
				progress.ProgressChanged += proxyFileProgress_ProgressChanged;

				cancellationToken = new CancellationTokenSource();

				changeRunningState(ProxyCheckerState.Running);
				proxiesChecked = new Dictionary<WebProxy, ProxyCheckResult>();

				await ProxyChecker.CheckProxiesAsync(proxies, targetWebsite.Text, (int)timeoutThreshold.Value, progress, cancellationToken.Token);

				changeRunningState(ProxyCheckerState.Finished);
			}
		}

		private void cancelProxyCheck_Click(object sender, EventArgs e) {
			cancellationToken.Cancel();
			changeRunningState(ProxyCheckerState.Cancelled);
		}

		private void exportWorkingProxies_Click(object sender, EventArgs e) {
			if (exportWorkingProxiesSaveDialog.ShowDialog() == DialogResult.OK) {
				Stream filestream;

				if ((filestream = exportWorkingProxiesSaveDialog.OpenFile()) != null) {
					using (StreamWriter writer = new StreamWriter(filestream)) {
						List<WebProxy> workingProxies = new List<WebProxy>(proxiesChecked.Where(
							kv => kv.Value == ProxyCheckResult.OK
						).ToDictionary(x => x.Key, x => x.Value).Keys);

						writer.Write(ProxyListParser.ToProxyList(workingProxies));
					}

					filestream.Close();
				}
			}
		}

		private void changeRunningState(ProxyCheckerState state) {
			switch (state) {
				case ProxyCheckerState.Initial:
					cancelProxyCheck.Enabled = false;
					openProxyFileDialog.Enabled = true;
					exportWorkingProxies.Enabled = false;
					timeoutThreshold.Enabled = true;
					targetWebsite.Enabled = true;
					proxyFileProgress.Value = 0;
					break;
				case ProxyCheckerState.Running:
					proxyCheckedList.Items.Clear();
					cancelProxyCheck.Enabled = true;
					openProxyFileDialog.Enabled = false;
					exportWorkingProxies.Enabled = false;
					timeoutThreshold.Enabled = false;
					targetWebsite.Enabled = false;
					proxyFileProgress.Value = 0;
					break;
				case ProxyCheckerState.Cancelled:
					cancelProxyCheck.Enabled = false;
					openProxyFileDialog.Enabled = true;
					exportWorkingProxies.Enabled = true;
					timeoutThreshold.Enabled = true;
					targetWebsite.Enabled = true;
					proxyFileProgress.Value = 0;
					break;
				case ProxyCheckerState.Finished:
					cancelProxyCheck.Enabled = false;
					openProxyFileDialog.Enabled = true;
					exportWorkingProxies.Enabled = true;
					timeoutThreshold.Enabled = true;
					targetWebsite.Enabled = true;
					proxyFileProgress.Value = 100;
					break;
			}
		}
	}
}
