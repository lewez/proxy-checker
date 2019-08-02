using System.Net;
using System;
using System.Collections.Generic;

namespace ProxyChecker {
	public class ProxyListParser {
		// TODO: Remove duplicate proxies

		public static List<WebProxy> ToWebProxy(string proxies) {
			List<WebProxy> webproxies = new List<WebProxy>();

			foreach (string proxy in new HashSet<string>(proxies.Split('\n'))) {
				string formattedProxy = proxy.Trim();

				if (String.IsNullOrEmpty(formattedProxy)) {
					continue;
				}

				try {
					webproxies.Add(new WebProxy(formattedProxy));
				}
				catch (UriFormatException) {
					Console.WriteLine("Badly foramtted proxy: " + formattedProxy);
				}
			}

			return webproxies;
		}

		public static string ToProxyList(List<WebProxy> proxies) {
			string str = "";

			foreach (WebProxy proxy in proxies) {
				str += proxy.Address + "\n";
			}

			return str;
		}
	}
}