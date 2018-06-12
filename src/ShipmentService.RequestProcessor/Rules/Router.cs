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

		/// <summary>
		/// Tries getting a shipment price for the given <paramref name="request"/>
		/// </summary>
		/// <param name="request">Request to get the shipment price for</param>
		/// <param name="price">Reference for a shipment price to be set if it was resolved</param>
		/// <returns>A flag indicating whether the price calculation was successful</returns>
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

		/// <summary>
		/// Registers a new shipping provider with the router
		/// </summary>
		/// <param name="provider">Provider to register</param>
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

		/// <summary>
		/// Gets value pairs (price, provider name) that represent the lowest available price for given <paramref name="packageSize"/>
		/// </summary>
		/// <param name="packageSize">Package size to get the lowest price for</param>
		/// <returns>Value pair (price, provider name) that contain the lowest price, (0, <c>null</c>) if one could not be found</returns>
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
			// Group all prices from all providers by package sizes and then select the lowest price from each group
			// also save it to variable so it doesn't need to be calculated on each request
			lowestPricePerPackageSize = providers
				.Values
				.SelectMany(provider => provider.PriceTable.Select(price => new { provider.ProviderName, PackageSize = price.Key, Price = price.Value }))
				.GroupBy(item => item.PackageSize)
				.Select(group => group.OrderBy(g => g.Price).First())
				.ToDictionary(x => x.PackageSize, x => (x.Price, x.ProviderName));
		}
	}
}