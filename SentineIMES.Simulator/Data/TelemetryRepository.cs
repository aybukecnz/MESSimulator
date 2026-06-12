
using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using SentinelMES.Simulator.Models;

namespace SentinelMES.Simulator.Data;

public class TelemetryRepository
{
    private readonly string _connectionString;

    // Bağlantı cümlesini appsettings.json'dan otomatik alıyoruz (Dependency Injection)
    public TelemetryRepository(IConfiguration configuration) => _connectionString = configuration.GetConnectionString("DefaultConnection");

    // Fiziksel SCADA verisini PostgreSQL'e yazan metot
    public async Task InsertTelemetryAsync(MachineTelemetry telemetry)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"INSERT INTO MachineTelemetry (Timestamp, ActivePower, WindSpeed, TheoreticalPower, WindDirection)
                    VALUES (@Timestamp, @ActivePower, @WindSpeed, @TheoreticalPower, @WindDirection)";

        await connection.ExecuteAsync(sql, telemetry);
    }

    // Siber güvenlik SOC logunu PostgreSQL'e yazan metot
    public async Task InsertAuditLogAsync(SystemAuditLog log)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"INSERT INTO ""systemauditlogs"" (""timestamp"", ""sourceip"", ""username"", ""actiontype"", ""status"", ""details"",""CountryCode"",""CountryName"")
                    VALUES (@Timestamp, @SourceIP, @UserName, @ActionType, @Status, @Details, @CountryCode, @CountryName)";

        await connection.ExecuteAsync(sql, log);
    }
}