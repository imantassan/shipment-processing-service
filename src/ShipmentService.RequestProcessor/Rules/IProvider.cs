using System.Collections.Generic;
using ShipmentService.RequestProcessor.Dto;

namespace ShipmentService.RequestProcessor.Rules
{
	/// <summary>
	/// Provides all the needed methods to interact with a shipping provider
	/// </summary>
	public interface IProvider
	{
		/// <summary>
		/// Gets all the package sizes supported by the provider
		/// </summary>
		HashSet<char> SupportedPackageSizes { get; }

		/// <summary>
		/// Gets the price table of the provider
		/// </summary>
		IReadOnlyDictionary<char, decimal> PriceTable { get; }

		/// <summary>
		/// Gets the provider name as it should appear in requests
		/// </summary>
		string ProviderName { get; }

		/// <summary>
		/// Calculates the price for the given shipment <paramref name="request"/>
		/// </summary>
		/// <param name="request">Request to calculate the price for</param>
		ShipmentPrice GetShipmentPrice(ShipmentRequest request);

		/// <summary>
		/// Method used to allow provider be aware of the router that manages all the providers
		/// </summary>
		/// <param name="router">The router this provider is attached to</param>
		void AttachToRouter(IRouter router);
	}
}