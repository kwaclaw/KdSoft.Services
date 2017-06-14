using System.Data.Common;

namespace KdSoft.Data
{
  public class DbConnectionSetting
  {
    public DbConnectionSetting(string name, DbProviderFactory providerFactory, string connectionString) {
      this.Name = name;
      this.ProviderFactory = providerFactory;
      this.ConnectionString = connectionString;
    }

    public string Name { get; private set; }
    public DbProviderFactory ProviderFactory { get; private set; }
    public string ConnectionString { get; private set; }
  }
}
