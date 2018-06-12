using System;
using System.Collections.Generic;
using System.Linq;

using ShipmentService.RequestProcessor.Dto;

namespace ShipmentService.RequestProcessor.Rules
{
	public abstract class ProviderBase : IProvider
	{
		protected ProviderBase(string providerName, IReadOnlyDictionary<char, decimal> pricePerPackageSize)
		{
			ProviderName = providerName;
			PriceTable = pricePerPackageSize;
			SupportedPackageSizes = pricePerPackageSize.Keys.ToHashSet();
		}

		protected IRouter Router { get; set; }

		public HashSet<char> SupportedPackageSizes { get; }

		public IReadOnlyDictionary<char, decimal> PriceTable { get; set; }
		
		protected int GetDateHash(DateTime date)
		{
			return date.Year * 100 + date.Month;
		}

		public string ProviderName { get; }

		public virtual ShipmentPrice GetShipmentPrice(ShipmentRequest request)
		{
			if (!request.IsValid || !request.ProviderName.Equals(ProviderName, StringComparison.OrdinalIgnoreCase) || !SupportedPackageSizes.Contains(request.PackageSize))
			{
				throw new NotSupportedException($"This provider ({ProviderName}) doesn't support processing {request.ProviderName} provider and/or {request.PackageSize} package size.");
			}

			if (request.PackageSize == PackageSize.S)
			{
				var originalPrice = PriceTable[request.PackageSize];
				var lowestPrice = Router.GetLowestPriceForPackageSize(request.PackageSize).Item1;
				if (lowestPrice > 0)
				{
					return new ShipmentPrice
					{
						Price = lowestPrice,
						Discount = originalPrice - lowestPrice
					};
				}
			}

			return new ShipmentPrice
			{
				Discount = 0,
				Price = PriceTable[request.PackageSize]
			};
		}

		public virtual void AttachToRouter(IRouter router)
		{
			Router = router;
		}
	}
}