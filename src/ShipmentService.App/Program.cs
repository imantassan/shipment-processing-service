using System;

using ShipmentService.DependencyResolver;
using ShipmentService.RequestProcessor.Dto;
using ShipmentService.RequestProcessor.FileParser;
using ShipmentService.RequestProcessor.Rules;

namespace ShipmentService.App
{
    class Program
    {
        private const string DefaultInputFileName = "input.txt";
        
        static void Main(string[] args)
        {
            // Register dependencies
            var container = new Container();
            container.Bind<IFileParser>().To<FileParser>();
            container.Bind<IRouter>().To<Router>();

            // Create all the necessary objects
            var router = container.Resolve<IRouter>();
            router.RegisterProvider(new LaPosteProvider());
            router.RegisterProvider(new MondialRelayProvider());

            var fileParser = container.Resolve<IFileParser>();
            fileParser.OpenFile(args.Length > 0 ? args[0] : DefaultInputFileName);

            // Do processing
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

            Console.WriteLine();
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }
    }
}
