using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;

namespace KdSoft.AspNet.Configuration
{
    /// <summary>
    /// Represents a <see cref="JObject"/> instance as an <see cref="IConfigurationSource"/>.
    /// Also a <see cref="JObject"/> based <see cref="IConfigurationProvider"/>.
    /// </summary>
    public class JsonConfigurationObjectSource: ConfigurationProvider, IConfigurationSource
    {
        JObject jsonObject;
        /// <summary>The <see cref="JObject"/> instance used as configuration source.</summary>
        public JObject JsonObject {
            get => jsonObject;
            set => jsonObject = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <param name="jsonObject">JSON configuration object.</param>
        public JsonConfigurationObjectSource(JObject jsonObject) {
            this.JsonObject = jsonObject;
        }

        /// <summary>
        /// Builds the <see cref="IConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>This instance as an <see cref="IConfigurationProvider"/></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder) {
            return this;
        }

        ///<inheritdoc />
        public override void Load() {
            var parser = new JsonConfigurationObjectParser();
            Data = parser.Parse(JsonObject);
            OnReload();
        }

        /// <summary>
        /// Reloads data using new source object.
        /// </summary>
        /// <param name="jsonObject">New <see cref="JObject"/> instance to use as source.</param>
        public void Reload(JObject jsonObject) {
            this.JsonObject = jsonObject;
            Load();
        }
    }
}