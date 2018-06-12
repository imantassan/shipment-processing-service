using System.Collections.Generic;

namespace ShipmentService.RequestProcessor.Rules
{
	public class MondialRelayProvider : ProviderBase
	{
		public MondialRelayProvider()
			: base("MR", new Dictionary<char, decimal>
			{
				{ PackageSize.S, 2.00m },
				{ PackageSize.M, 3.00m },
				{ PackageSize.L, 4.00m }
			})
		{
		}
	}
}