using System.Collections.Generic;

using ShipmentService.RequestProcessor.Dto;

namespace ShipmentService.RequestProcessor.Rules
{
	/// <summary>
	/// Contains all the rules and prices for the "La Poste" shipping provider
	/// </summary>
	public class LaPosteProvider : ProviderBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LaPosteProvider"/> class.
		/// </summary>
		public LaPosteProvider()
			: base("LP", new Dictionary<char, decimal>
			{
				{ PackageSize.S, 1.50m },
				{ PackageSize.M, 4.90m },
				{ PackageSize.L, 6.90m }
			})
		{
		}

		private readonly Dictionary<int, int> largePackagesPerMonth = new Dictionary<int, int>();

		/// <summary>
		/// Calculates the price for the given shipment <paramref name="request"/>
		/// </summary>
		/// <param name="request">Request to calculate the price for</param>
		public override ShipmentPrice GetShipmentPrice(ShipmentRequest request)
		{
			if (request.PackageSize == PackageSize.L)
			{
				var dateHash = GetDateHash(request.Date);
				if (!largePackagesPerMonth.ContainsKey(dateHash))
				{
					largePackagesPerMonth.Add(dateHash, 0);
				}

				if (++largePackagesPerMonth[dateHash] == 3)
				{
					var price = base.GetShipmentPrice(request);

					price.Discount = price.Price;
					price.Price = 0;

					return price;
				}
			}

			return base.GetShipmentPrice(request);
		}
	}
}