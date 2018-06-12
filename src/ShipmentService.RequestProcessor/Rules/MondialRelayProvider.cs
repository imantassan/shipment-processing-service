using System.Collections.Generic;

namespace ShipmentService.RequestProcessor.Rules
{
	/// <summary>
	/// Contains all the rules and prices for the "Mondial Relay" shipping provider
	/// </summary>
	public class MondialRelayProvider : ProviderBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MondialRelayProvider"/> class.
		/// </summary>
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