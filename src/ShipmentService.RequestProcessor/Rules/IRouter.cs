using ShipmentService.RequestProcessor.Dto;

namespace ShipmentService.RequestProcessor.Rules
{
	public interface IRouter
	{
		bool TryGetShipmentPrice(ShipmentRequest request, out ShipmentPrice price);

		void RegisterProvider(IProvider provider);

		(decimal, string) GetLowestPriceForPackageSize(char packageSize);
	}
}