using System;

namespace ShipmentService.RequestProcessor.Dto
{
	/// <summary>
	/// Represents a single sequential shipment request
	/// </summary>
	public struct ShipmentRequest
	{
		/// <summary>
		/// Gets or sets a flag indicating whether the given request is valid
		/// </summary>
		public bool IsValid { get; set; }

		/// <summary>
		/// Gets or sets a flag indicating whether the given request is the last in sequence
		/// </summary>
		public bool IsLast { get; set; }

		/// <summary>
		/// Gets or sets the date this request is scheduled for
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		/// Gets or sets the preferred provider to be used for this shipment
		/// </summary>
		public string ProviderName { get; set; }

		/// <summary>
		/// Gets or sets the package size this shipment will contain
		/// </summary>
		public char PackageSize { get; set; }

		/// <summary>
		/// Gets or sets the original request line from the original data source it was parsed from
		/// </summary>
		public string Source { get; set; }
	}
}