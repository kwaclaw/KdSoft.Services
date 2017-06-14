using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KdSoft.AspNet.Configuration
{
    /// <summary>
    /// A JSON <see cref="System.IO.Stream"/> based <see cref="IConfigurationProvider"/>.
    /// </summary>
    public class JsonConfigurationStreamProvider: ConfigurationProvider
    {
        readonly JsonConfigurationStreamSource source;

        /// <summary>
        /// Initializes a new instance with the specified source.
        /// </summary>
        /// <param name="source">The source settings.</param>
        public JsonConfigurationStreamProvider(JsonConfigurationStreamSource source) {
            this.source = source;
        }

        ///<inheritdoc />
        public override void Load() {
            var stream = source.Stream;

            var parser = new JsonConfigurationStreamParser();
            try {
                Data = parser.Parse(stream);
            }
            catch (JsonReaderException e) {
                string errorLine = string.Empty;
                if (stream.CanSeek) {
                    stream.Seek(0, SeekOrigin.Begin);

                    IEnumerable<string> fileContent;
                    using (var streamReader = new StreamReader(stream)) {
                        fileContent = ReadLines(streamReader);
                        errorLine = RetrieveErrorContext(e, fileContent);
                    }
                }

                string errFormat = "Could not parse the JSON stream. Error on line number '{0}': '{1}'.";
                throw new FormatException(string.Format(errFormat, e.LineNumber, errorLine), e);
            }
            OnReload();
        }

        static string RetrieveErrorContext(JsonReaderException e, IEnumerable<string> fileContent) {
            string errorLine = null;
            if (e.LineNumber >= 2) {
                var errorContext = fileContent.Skip(e.LineNumber - 2).Take(2).ToList();
                // Handle situations when the line number reported is out of bounds
                if (errorContext.Count >= 2) {
                    errorLine = errorContext[0].Trim() + Environment.NewLine + errorContext[1].Trim();
                }
            }
            if (string.IsNullOrEmpty(errorLine)) {
                var possibleLineContent = fileContent.Skip(e.LineNumber - 1).FirstOrDefault();
                errorLine = possibleLineContent ?? string.Empty;
            }
            return errorLine;
        }

        static IEnumerable<string> ReadLines(StreamReader streamReader) {
            string line;
            do {
                line = streamReader.ReadLine();
                yield return line;
            } while (line != null);
        }
    }
}
