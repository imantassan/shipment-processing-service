using System.Collections.Generic;

using ShipmentService.RequestProcessor.Dto;

namespace ShipmentService.RequestProcessor.Rules
{
	public class LaPosteProvider : ProviderBase
	{
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