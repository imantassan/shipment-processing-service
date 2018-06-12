using System;
using System.Globalization;
using System.IO;
using ShipmentService.RequestProcessor.Dto;

namespace ShipmentService.RequestProcessor.FileParser
{
	public class FileParser : IFileParser
	{
		private StreamReader reader;

		public void Dispose()
		{
			reader.Dispose();
			reader = null;
		}

		/// <summary>
		/// Opens the file for parsing
		/// </summary>
		/// <param name="filename">Name of full path to the file to open</param>
		public void OpenFile(string filename)
		{
			if (!File.Exists(filename))
			{
				throw new FileNotFoundException($"Could not find the file {filename}");
			}

			var fileStream = new BufferedStream(File.OpenRead(filename));
			reader = new StreamReader(fileStream);
		}

		/// <summary>
		/// Reads a single request from the file opened if one is still available
		/// </summary>
		public ShipmentRequest GetNextRequest()
		{
			if (reader.EndOfStream)
			{
				return new ShipmentRequest
				{
					IsLast = true,
					IsValid = false
				};
			}

			var line = reader.ReadLine();
			var parts = (line ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);


			var result = new ShipmentRequest
			{
				IsValid = true,
				Source = line,
				IsLast = reader.EndOfStream
			};

			if (!ParseDate(parts[0], ref result) || !ParsePackageSize(parts[1], ref result) || !ParseProviderName(parts[2], ref result))
			{
				result.IsValid = false;
			}

			return result;
		}

		private static bool ParseDate(string input, ref ShipmentRequest request)
		{
			if (!DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime date))
			{
				return false;
			}

			request.Date = date;
			return true;
		}

		private static bool ParsePackageSize(string input, ref ShipmentRequest request)
		{
			if (input.Length != 1)
			{
				return false;
			}

			request.PackageSize = input[0];
			return true;
		}

		private static bool ParseProviderName(string input, ref ShipmentRequest request)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return false;
			}

			request.ProviderName = input;
			return true;
		}
	}
}