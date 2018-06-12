using ShipmentService.RequestProcessor.Dto;

namespace ShipmentService.RequestProcessor.Rules
{
	/// <summary>
	/// Provides methods for a router that directs requests to respective providers based on the data contained within the request
	/// </summary>
	public interface IRouter
	{
		/// <summary>
		/// Tries getting a shipment price for the given <paramref name="request"/>
		/// </summary>
		/// <param name="request">Request to get the shipment price for</param>
		/// <param name="price">Reference for a shipment price to be set if it was resolved</param>
		/// <returns>A flag indicating whether the price calculation was successful</returns>
		bool TryGetShipmentPrice(ShipmentRequest request, out ShipmentPrice price);

		/// <summary>
		/// Registers a new shipping provider with the router
		/// </summary>
		/// <param name="provider">Provider to register</param>
		void RegisterProvider(IProvider provider);

		/// <summary>
		/// Gets value pairs (price, provider name) that represent the lowest available price for given <paramref name="packageSize"/>
		/// </summary>
		/// <param name="packageSize">Package size to get the lowest price for</param>
		/// <returns>Value pair (price, provider name) that contain the lowest price, (0, <c>null</c>) if one could not be found</returns>
		(decimal, string) GetLowestPriceForPackageSize(char packageSize);
	}
}