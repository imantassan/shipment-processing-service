using System;
using ShipmentService.RequestProcessor.Dto;

namespace ShipmentService.RequestProcessor.FileParser
{
	/// <summary>
	/// Provides all the necessary methods needed for parsing a single input file
	/// </summary>
	public interface IFileParser : IDisposable
	{
		/// <summary>
		/// Opens the file for parsing
		/// </summary>
		/// <param name="filename">Name of full path to the file to open</param>
		void OpenFile(string filename);

		/// <summary>
		/// Reads a single request from the file opened if one is still available
		/// </summary>
		ShipmentRequest GetNextRequest();
	}
}