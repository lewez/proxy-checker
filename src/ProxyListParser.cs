using System.Net;
using System;
using System.Collections.Generic;

namespace ProxyChecker {
	public class ProxyListParser {
		public static List<WebProxy> Parse(string proxies) {
			List<WebProxy> webproxies = new List<WebProxy>();

			foreach (string proxy in proxies.Split('\n')) {
				string formattedProxy = proxy.Trim();

				if (String.IsNullOrEmpty(formattedProxy)) {
					continue;
				}

				try {
					webproxies.Add(new WebProxy(formattedProxy));
				}
				catch (UriFormatException e) {
					Console.WriteLine("Badly foramtted proxy: " + formattedProxy);
				}
			}

			return webproxies;
		}
	}
}