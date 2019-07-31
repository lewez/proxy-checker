using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;

namespace ProxyChecker {
	public class ProxyChecker {
		public static void Main() {
			Application.Run(new ProxyCheckerForm());
		}

		public static async Task CheckProxies(List<WebProxy> proxies) {
			List<Task<ProxyCheckResult>> tasks = new List<Task<ProxyCheckResult>>();

			foreach (WebProxy proxy in proxies) {
				tasks.Add(Task.Run(() => CheckProxy(proxy)));
			}

			ProxyCheckResult[] results = await Task.WhenAll(tasks);
		}

		public static async Task<ProxyCheckResult> CheckProxy(WebProxy proxy) {
			HttpClientHandler clienthandler = new HttpClientHandler() {
				Proxy = proxy,
				UseProxy = true
			};
			HttpClient client = new HttpClient(clienthandler) {
				Timeout = new TimeSpan(0, 0, 10)
			};
			Console.WriteLine("yeet");

			try {
				HttpResponseMessage resp = await client.GetAsync("http://google.co.uk");

				Console.WriteLine("resp: " + resp.StatusCode);
			}
			catch (Exception ex) {
				if (ex is TaskCanceledException || ex is HttpRequestException) {
					return ProxyCheckResult.TIMED_OUT;
				}
			}

			// TODO: Report progress

			return ProxyCheckResult.UNKNOWN;
		}
	}
}