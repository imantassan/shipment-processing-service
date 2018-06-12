namespace ShipmentService.RequestProcessor.Dto
{
	/// <summary>
	/// Represents an object holding info about the price of a shipment
	/// </summary>
	public struct ShipmentPrice
	{
		/// <summary>
		/// Gets or sets the final price of the shipment
		/// </summary>
		public decimal Price { get; set; }

		/// <summary>
		/// Gets or sets the discount that was applied to <see cref="Price"/>
		/// </summary>
		public decimal Discount { get; set; }
	}
}