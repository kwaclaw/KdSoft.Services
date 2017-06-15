using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace KdSoft.Configuration.AspNet
{
    /// <summary>
    /// Represents a JSON <see cref="System.IO.Stream"/> as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class JsonConfigurationStreamSource: IConfigurationSource
    {
        Stream stream;
        /// <summary>The Configuration stream.</summary>
        public Stream Stream {
            get => stream;
            set => stream = value ?? throw new ArgumentNullException(nameof(value));
        }

        Action reloaded;

        /// <param name="stream">The <see cref="System.IO.Stream"/> used as configuration source.</param>
        public JsonConfigurationStreamSource(Stream stream) {
            this.Stream = stream;
        }

        /// <summary>
        /// Builds the <see cref="IConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="JsonConfigurationStreamProvider"/></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder) {
            var result = new JsonConfigurationStreamProvider(this);
            reloaded += result.Load;
            return result;
        }

        /// <summary>
        /// Reloads data using new source stream.
        /// </summary>
        /// <param name="stream">New JSON <see cref="System.IO.Stream"/> to use as source.</param>
        public void Reload(Stream stream) {
            this.Stream = stream;
            reloaded?.Invoke();
        }
    }
}