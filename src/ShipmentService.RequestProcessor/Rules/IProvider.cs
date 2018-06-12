using System.Collections.Generic;

using ShipmentService.RequestProcessor.Dto;

namespace ShipmentService.RequestProcessor.Rules
{
	public interface IProvider
	{
		HashSet<char> SupportedPackageSizes { get; }

		IReadOnlyDictionary<char, decimal> PriceTable { get; set; }

		string ProviderName { get; }

		ShipmentPrice GetShipmentPrice(ShipmentRequest request);

		void AttachToRouter(IRouter router);
	}
}