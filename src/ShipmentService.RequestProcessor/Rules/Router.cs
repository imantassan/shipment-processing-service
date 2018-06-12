using System;
using System.Collections.Generic;
using System.Linq;

using ShipmentService.RequestProcessor.Dto;

namespace ShipmentService.RequestProcessor.Rules
{
	public class Router : IRouter
	{
		private const decimal MaxDiscountAmountPerMonth = 10.0m;

		private IDictionary<char, (decimal, string)> lowestPricePerPackageSize = new Dictionary<char, (decimal, string)>();

		private readonly IDictionary<string, IProvider> providers = new Dictionary<string, IProvider>();

		private readonly IDictionary<int, decimal> accumulatedDiscounts = new Dictionary<int, decimal>();

		protected int GetDateHash(DateTime date)
		{
			return date.Year * 100 + date.Month;
		}

		protected decimal AccumulateDiscount(DateTime date, decimal amount)
		{
			var dateHash = GetDateHash(date);
			if (!accumulatedDiscounts.ContainsKey(dateHash))
			{
				accumulatedDiscounts.Add(dateHash, 0.0m);
			}

			amount = Math.Min(amount, MaxDiscountAmountPerMonth - accumulatedDiscounts[dateHash]);
			accumulatedDiscounts[dateHash] += amount;

			return amount;
		}

		public bool TryGetShipmentPrice(ShipmentRequest request, out ShipmentPrice price)
		{
			if (!providers.ContainsKey(request.ProviderName) || !providers[request.ProviderName].SupportedPackageSizes.Contains(request.PackageSize))
			{
				price = new ShipmentPrice();
				return false;
			}

			price = providers[request.ProviderName].GetShipmentPrice(request);
			if (price.Discount > 0)
			{
				var accumulatedDiscount = AccumulateDiscount(request.Date, price.Discount);
				if (accumulatedDiscount < price.Discount)
				{
					price.Price += price.Discount - accumulatedDiscount;
					price.Discount = accumulatedDiscount;
				}
			}

			return true;
		}

		public void RegisterProvider(IProvider provider)
		{
			if (providers.ContainsKey(provider.ProviderName))
			{
				throw new InvalidOperationException($"Provider {provider.ProviderName} is already registered with the router");
			}

			providers.Add(provider.ProviderName, provider);
			provider.AttachToRouter(this);
			RecalculateLowestPrice();
		}

		public (decimal, string) GetLowestPriceForPackageSize(char packageSize)
		{
			if (!lowestPricePerPackageSize.ContainsKey(packageSize))
			{
				return (0, null);
			}

			return lowestPricePerPackageSize[packageSize];
		}

		private void RecalculateLowestPrice()
		{
			lowestPricePerPackageSize = providers
				.Values
				.SelectMany(provider => provider.PriceTable.Select(price => new { provider.ProviderName, PackageSize = price.Key, Price = price.Value }))
				.GroupBy(item => item.PackageSize)
				.Select(
					group =>
						group.OrderBy(g => g.Price).First())
				.ToDictionary(x => x.PackageSize, x => (x.Price, x.ProviderName));
		}
	}
}