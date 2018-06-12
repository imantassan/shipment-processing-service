using System;
using System.Collections.Generic;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

using ShipmentService.RequestProcessor.Dto;
using ShipmentService.RequestProcessor.Rules;

namespace ShipmentService.Tests
{
	[TestClass]
	public class RouterTests
	{
		private IProvider provider1;

		private IProvider provider2;

		[TestInitialize]
		public void Setup()
		{
			var prices1 = new Dictionary<char, decimal>
			{
				{ PackageSize.S, 5.00m },
				{ PackageSize.M, 7.00m }
			};
			provider1 = Substitute.For<IProvider>();
			provider1.ProviderName.Returns("P1");
			provider1.PriceTable.Returns(prices1);
			provider1.GetShipmentPrice(Arg.Any<ShipmentRequest>())
				.Returns(_ => new ShipmentPrice
				{
					Price = 3,
					Discount = 3
				});
			provider1.SupportedPackageSizes.Returns(
				new HashSet<char>
				{
					PackageSize.S,
					PackageSize.M
				});

			var prices2 = new Dictionary<char, decimal>
			{
				{ PackageSize.S, 2.00m },
				{ PackageSize.M, 8.00m }
			};
			provider2 = Substitute.For<IProvider>();
			provider2.ProviderName.Returns("P2");
			provider2.PriceTable.Returns(prices2);
			provider2.GetShipmentPrice(Arg.Any<ShipmentRequest>())
				.Returns(_ => new ShipmentPrice
				{
					Price = 4,
					Discount = 4
				});
			provider2.SupportedPackageSizes.Returns(
				new HashSet<char>
				{
					PackageSize.S,
					PackageSize.M
				});
		}

		[TestMethod]
		public void Router_GetLowestPriceForPackageSize_FirstProviderRegistered_GetsCorrectLowestPrice()
		{
			// arrange
			var router = new Router();
			router.RegisterProvider(provider1);

			// act
			var lowestPriceS = router.GetLowestPriceForPackageSize(PackageSize.S);
			var lowestPriceM = router.GetLowestPriceForPackageSize(PackageSize.M);

			// assert
			lowestPriceS.Item1.Should().Be(5.00m);
			lowestPriceS.Item2.Should().Be("P1");
			lowestPriceM.Item1.Should().Be(7.00m);
			lowestPriceM.Item2.Should().Be("P1");
		}

		[TestMethod]
		public void Router_GetLowestPriceForPackageSize_BothProvidersRegistered_GetsCorrectLowestPrice()
		{
			// arrange
			var router = new Router();
			router.RegisterProvider(provider1);
			router.RegisterProvider(provider2);

			// act
			var lowestPriceS = router.GetLowestPriceForPackageSize(PackageSize.S);
			var lowestPriceM = router.GetLowestPriceForPackageSize(PackageSize.M);

			// assert
			lowestPriceS.Item1.Should().Be(2.00m);
			lowestPriceS.Item2.Should().Be("P2");
			lowestPriceM.Item1.Should().Be(7.00m);
			lowestPriceM.Item2.Should().Be("P1");
		}

		[TestMethod]
		public void Router_TryGetShipmentPrice_UnexistingProvider_Fails()
		{
			// arrange
			var router = new Router();
			router.RegisterProvider(provider1);
			router.RegisterProvider(provider2);

			// act
			var success = router.TryGetShipmentPrice(
				new ShipmentRequest
				{
					IsValid = true,
					ProviderName = "P3",
					PackageSize = PackageSize.S
				},
				out ShipmentPrice price);

			// assert
			success.Should().BeFalse();
			price.Price.Should().Be(0);
			price.Discount.Should().Be(0);
		}

		[TestMethod]
		public void Router_TryGetShipmentPrice_UnexistingPackageSize_Fails()
		{
			// arrange
			var router = new Router();
			router.RegisterProvider(provider1);

			// act
			var success = router.TryGetShipmentPrice(
				new ShipmentRequest
				{
					IsValid = true,
					ProviderName = "P1",
					PackageSize = PackageSize.L
				},
				out ShipmentPrice price);

			// assert
			success.Should().BeFalse();
			price.Price.Should().Be(0);
			price.Discount.Should().Be(0);
		}

		[TestMethod]
		public void Router_TryGetShipmentPrice_ValidData_ReturnsCorrectPrice()
		{
			// arrange
			var router = new Router();
			router.RegisterProvider(provider1);

			// act
			var success = router.TryGetShipmentPrice(
				new ShipmentRequest
				{
					IsValid = true,
					ProviderName = "P1",
					PackageSize = PackageSize.M,
					Date = DateTime.Today
				},
				out ShipmentPrice price);

			// assert
			success.Should().BeTrue();
			price.Price.Should().Be(3);
			price.Discount.Should().Be(3);
		}

		[TestMethod]
		public void Router_TryGetShipmentPrice_AccumulatedDiscount_DoesntExceedLimit()
		{
			// arrange
			var router = new Router();
			router.RegisterProvider(provider1);
			var successCount = new Dictionary<bool, int>
			{
				{ true, 0 },
				{ false, 0 }
			};
			ShipmentPrice lastPrice;
			var request = new ShipmentRequest
			{
				IsValid = true,
				ProviderName = "P1",
				PackageSize = PackageSize.M,
				Date = DateTime.Today
			};

			// act
			successCount[router.TryGetShipmentPrice(request, out lastPrice)]++;
			successCount[router.TryGetShipmentPrice(request, out lastPrice)]++;
			successCount[router.TryGetShipmentPrice(request, out lastPrice)]++;
			successCount[router.TryGetShipmentPrice(request, out lastPrice)]++;

			// assert
			successCount[true].Should().Be(4);
			successCount[false].Should().Be(0);

			lastPrice.Price.Should().Be(5);
			lastPrice.Discount.Should().Be(1);
		}

		[TestMethod]
		public void Router_TryGetShipmentPrice_AccumulatedDiscount_DoesntAffectDifferentMonth()
		{
			// arrange
			var router = new Router();
			router.RegisterProvider(provider1);
			var successCount = new Dictionary<bool, int>
			{
				{ true, 0 },
				{ false, 0 }
			};
			ShipmentPrice lastPrice1;
			ShipmentPrice lastPrice2;
			var request = new ShipmentRequest
			{
				IsValid = true,
				ProviderName = "P1",
				PackageSize = PackageSize.M,
				Date = DateTime.Today
			};

			// act
			successCount[router.TryGetShipmentPrice(request, out lastPrice1)]++;
			successCount[router.TryGetShipmentPrice(request, out lastPrice1)]++;
			successCount[router.TryGetShipmentPrice(request, out lastPrice1)]++;
			successCount[router.TryGetShipmentPrice(request, out lastPrice1)]++;
			successCount[router.TryGetShipmentPrice(request, out lastPrice1)]++;

			request.Date = request.Date.AddMonths(1);
			successCount[router.TryGetShipmentPrice(request, out lastPrice2)]++;

			// assert
			successCount[true].Should().Be(6);
			successCount[false].Should().Be(0);

			lastPrice1.Price.Should().Be(6);
			lastPrice1.Discount.Should().Be(0);

			lastPrice2.Price.Should().Be(3);
			lastPrice2.Discount.Should().Be(3);
		}
	}
}