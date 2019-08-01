using System.Collections.Generic;
using System.Net;

namespace ProxyChecker {
	public class ProxyCheckProgressReport {
		public int NumTotal { get; set; }
		public int NumChecked { get; set; }
		public WebProxy ProxyChecked { get; set; }
		public ProxyCheckResult ProxyCheckResult { get; set; }
	}
}