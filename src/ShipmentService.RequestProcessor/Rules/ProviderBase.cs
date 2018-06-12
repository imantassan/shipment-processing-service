using System;
using System.Collections.Generic;
using System.Linq;

using ShipmentService.RequestProcessor.Dto;

namespace ShipmentService.RequestProcessor.Rules
{
	/// <summary>
	/// Represents all the common functionality of shipping providers
	/// </summary>
	public abstract class ProviderBase : IProvider
	{
		protected ProviderBase(string providerName, IReadOnlyDictionary<char, decimal> pricePerPackageSize)
		{
			ProviderName = providerName;
			PriceTable = pricePerPackageSize;
			SupportedPackageSizes = pricePerPackageSize.Keys.ToHashSet();
		}

		protected IRouter Router { get; set; }

		/// <summary>
		/// Gets all the package sizes supported by the provider
		/// </summary>
		public HashSet<char> SupportedPackageSizes { get; }

		/// <summary>
		/// Gets the price table of the provider
		/// </summary>
		public IReadOnlyDictionary<char, decimal> PriceTable { get; set; }

		/// <summary>
		/// Gets the provider name as it should appear in requests
		/// </summary>
		public string ProviderName { get; }

		protected int GetDateHash(DateTime date)
		{
			return date.Year * 100 + date.Month;
		}

		/// <summary>
		/// Calculates the price for the given shipment <paramref name="request"/>
		/// </summary>
		/// <param name="request">Request to calculate the price for</param>
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

		/// <summary>
		/// Method used to allow provider be aware of the router that manages all the providers
		/// </summary>
		/// <param name="router">The router this provider is attached to</param>
		public virtual void AttachToRouter(IRouter router)
		{
			Router = router;
		}
	}
}