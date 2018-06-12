namespace ShipmentService.RequestProcessor.Rules
{
	public class Router : IRouter
	{
		private readonly IProvider provider;

		public Router(IProvider provider)
		{
			this.provider = provider;
		}
	}
}