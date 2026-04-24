using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public sealed class SampleProfileDbContext : SampleProfile.DbContextBase
{
    private readonly SqliteConnection _connection;

    public SampleProfileDbContext(SqliteConnection connection)
    {
        _connection = connection;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(_connection);
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
