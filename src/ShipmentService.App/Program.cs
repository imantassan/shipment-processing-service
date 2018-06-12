using System;

using ShipmentService.DependencyResolver;
using ShipmentService.RequestProcessor.Dto;
using ShipmentService.RequestProcessor.FileParser;
using ShipmentService.RequestProcessor.Rules;

namespace ShipmentService.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container();
            container.Bind<IFileParser>().To<FileParser>();
            container.Bind<IRouter>().To<Router>();

	        var router = container.Resolve<IRouter>();
			router.RegisterProvider(new LaPosteProvider());
			router.RegisterProvider(new MondialRelayProvider());

	        var fileParser = container.Resolve<IFileParser>();
			fileParser.OpenFile("input.txt");

	        ShipmentRequest request;
	        do
	        {
		        request = fileParser.GetNextRequest();
		        if (request.IsValid && router.TryGetShipmentPrice(request, out ShipmentPrice price))
		        {
			        Console.WriteLine($"{request.Source} {price.Price:#,##0.00} {price.Discount:#,##0.00;'—';'—'}");
		        }
		        else
		        {
			        Console.WriteLine($"{request.Source} Ignored");
		        }
	        } while (!request.IsLast);

	        Console.ReadLine();
        }
    }
}
