using ShipmentService.RequestProcessor.FileParser;

namespace ShipmentService.RequestProcessor.Rules
{
	public class Provider : IProvider
	{
		private readonly IFileParser fileParser;

		public Provider(IFileParser fileParser)
		{
			this.fileParser = fileParser;
		}
	}
}