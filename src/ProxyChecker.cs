using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;

namespace ProxyChecker {
	public class ProxyChecker {
		private const int ChunkSize = 25;

		public static void Main() {
			Application.Run(new ProxyCheckerForm());
		}

		public static async Task CheckProxies(List<WebProxy> proxies, IProgress<ProxyCheckProgressReport> progress) {
			int numTotal = proxies.Count;
			int numChecked = 0;
			int chunkSize = Math.Min(ChunkSize, proxies.Count);

			foreach (List<WebProxy> splitProxies in ListExtensions.ChunkBy(proxies, chunkSize)) {
				List<Task<ProxyCheckResult>> tasks = new List<Task<ProxyCheckResult>>();

				foreach (WebProxy proxy in splitProxies) {
					tasks.Add(Task.Run(() => {
						Task<ProxyCheckResult> result = CheckProxy(proxy);

						progress.Report(new ProxyCheckProgressReport() {
							NumTotal = numTotal,
							NumChecked = ++numChecked
						});

						return result;
					}));
				}

				await Task.WhenAll(tasks);
			}

			Console.WriteLine("All tasks complete");
		}

		public static async Task<ProxyCheckResult> CheckProxy(WebProxy proxy) {
			HttpClientHandler clienthandler = new HttpClientHandler() {
				Proxy = proxy,
				UseProxy = true
			};
			HttpClient client = new HttpClient(clienthandler) {
				Timeout = new TimeSpan(0, 0, 10)
			};

			ProxyCheckResult result = ProxyCheckResult.UNKNOWN;

			try {
				HttpResponseMessage resp = await client.GetAsync("http://google.com");

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

			clienthandler.Dispose();
			client.Dispose();

			return result;
		}
	}
}