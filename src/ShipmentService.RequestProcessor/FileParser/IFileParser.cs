using System;

using ShipmentService.RequestProcessor.Dto;

namespace ShipmentService.RequestProcessor.FileParser
{
	public interface IFileParser : IDisposable
	{
		void OpenFile(string filename);

		ShipmentRequest GetNextRequest();
	}
}