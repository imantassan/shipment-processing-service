using System;

namespace ShipmentService.RequestProcessor.Dto
{
	public struct ShipmentRequest
	{
		public bool IsValid { get; set; }

		public bool IsLast { get; set; }

		public DateTime Date { get; set; }

		public string ProviderName { get; set; }

		public char PackageSize { get; set; }

		public string Source { get; set; }
	}
}