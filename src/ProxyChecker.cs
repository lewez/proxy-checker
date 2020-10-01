using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace ProxyChecker {
	public class ProxyChecker {
		private const int ChunkSize = 50;

		[STAThread]
		public static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ProxyCheckerForm());
		}

		public static async Task CheckProxiesAsync(
				List<WebProxy> proxies,
				string website,
				int timeoutSecs,
				IProgress<ProxyCheckProgressReport> progress,
				CancellationToken cancellationToken)
		{
			int numTotal = proxies.Count;
			int chunkSize = Math.Min(ChunkSize, proxies.Count);

			foreach (List<WebProxy> splitProxies in ListExtensions.ChunkBy(proxies, chunkSize)) {
				if (cancellationToken.IsCancellationRequested) {
					break;
				}

				List<Task> tasks = new List<Task>();

				foreach (WebProxy proxy in splitProxies) {
					tasks.Add(Task.Run(async () => {
						ProxyCheckResult result = await CheckProxyAsync(proxy, website, timeoutSecs);

						progress.Report(new ProxyCheckProgressReport() {
							NumTotal = numTotal,
							ProxyChecked = proxy,
							ProxyCheckResult = result
						});
					}));
				}

				await Task.WhenAll(tasks);
			}
		}

		public static async Task<ProxyCheckResult> CheckProxyAsync(
				WebProxy proxy,
				string website,
				int timeoutSecs)
		{
			using (HttpClientHandler clienthandler = new HttpClientHandler() {
					Proxy = proxy, UseProxy = true}) {
				using (HttpClient httpClient = new HttpClient(clienthandler) {
						Timeout = new TimeSpan(0, 0, timeoutSecs)}) {
					ProxyCheckResult result = ProxyCheckResult.UNKNOWN;

					try {
						HttpResponseMessage resp = await httpClient.GetAsync(website);

						switch (resp.StatusCode) {
							case HttpStatusCode.OK:
								result = ProxyCheckResult.OK;
								break;
							default:
								result = ProxyCheckResult.UNKNOWN;
								break;
						}
					}
					catch (Exception ex) {
						if (ex is TaskCanceledException || ex is HttpRequestException) {
							result = ProxyCheckResult.TIMED_OUT;
						}
					}

					return result;
				}
			}
		}
	}
}
