using Microsoft.Extensions.Configuration;
using System.Linq;

namespace KdSoft.AspNet.Configuration
{
    /// <summary>
    /// <see cref="IConfiguration"/> related helper and extension routines.
    /// </summary>
    public static class ConfigurationHelpers
    {
        /// <summary>
        /// Overrides matching entries in a <see cref="IConfigurationSection"/>.
        /// </summary>
        /// <param name="target"><see cref="IConfigurationSection"/> whose members get overridden.</param>
        /// <param name="overrideFrom"><see cref="IConfiguration"/> instance providing the entries to override with.</param>
        public static void OverrideWith(this IConfigurationSection target, IConfiguration overrideFrom) {
            var fromChildren = overrideFrom.GetChildren();
            if (fromChildren.Any()) {
                foreach (var fromSection in fromChildren) {
                    OverrideWith(target, fromSection);
                }
            }
            else {
                var mergeFromSection = (IConfigurationSection)overrideFrom;
                target[mergeFromSection.Path] = mergeFromSection.Value;
            }
        }
    }
}
