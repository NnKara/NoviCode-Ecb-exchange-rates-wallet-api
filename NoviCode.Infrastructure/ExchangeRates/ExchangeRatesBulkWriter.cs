using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NoviCode.Application.ExchangeRates.DTOs;
using NoviCode.Application.ExchangeRates.Interfaces;

namespace NoviCode.Infrastructure.ExchangeRates;

public sealed class ExchangeRatesBulkWriter : IExchangeRatesBulkWriter
{
    //batch size per merge
    private const int MaxRowsPerMerge = 400;

    private readonly string _connectionString;

    public ExchangeRatesBulkWriter(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ExchangeRateDb")
            ?? throw new InvalidOperationException("Connection string 'ExchangeRateDb' is missing.");
    }

    public async Task MergeAsync(IReadOnlyList<ExchangeRateRow> rows, CancellationToken cancellationToken = default)
    {
        if (rows.Count == 0)
            return;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            for (var offset = 0; offset < rows.Count; offset += MaxRowsPerMerge)
            {
                var take = Math.Min(MaxRowsPerMerge, rows.Count - offset);
                var batch = rows.Skip(offset).Take(take).ToList();

                var sql = BuildMergeSql(batch, out var parameters);

                await using var cmd = new SqlCommand(sql, connection, (SqlTransaction)transaction);
                cmd.Parameters.AddRange(parameters);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static string BuildMergeSql(IReadOnlyList<ExchangeRateRow> exchangeRateRows, out SqlParameter[] mergeCommandParameters)
    {

        if (exchangeRateRows.Count == 0)
            throw new InvalidOperationException("At least one exchange rate row is required.");

        var sqlCommandText = new StringBuilder();

        sqlCommandText.AppendLine("MERGE INTO dbo.ExchangeRates AS target");
        sqlCommandText.Append("USING (VALUES ");

        var sqlParameters = new List<SqlParameter>();

        for (var rowIndex = 0; rowIndex < exchangeRateRows.Count; rowIndex++)
        {
            var currentRow = exchangeRateRows[rowIndex];

            if (rowIndex > 0)
                sqlCommandText.Append(", ");

            var rateDateParameterName = $"@RateDate_{rowIndex}";
            var baseCurrencyParameterName = $"@BaseCurrency_{rowIndex}";
            var targetCurrencyParameterName = $"@TargetCurrency_{rowIndex}";
            var exchangeRateParameterName = $"@ExchangeRate_{rowIndex}";

            sqlCommandText.Append($"({rateDateParameterName}, {baseCurrencyParameterName}, {targetCurrencyParameterName}, {exchangeRateParameterName})");

            var normalizedBaseCurrency = NormalizeCurrency(currentRow.BaseCurrency, nameof(currentRow.BaseCurrency));
            var normalizedTargetCurrency = NormalizeCurrency(currentRow.TargetCurrency, nameof(currentRow.TargetCurrency));

            sqlParameters.Add(new SqlParameter(rateDateParameterName, SqlDbType.Date)
            {
                Value = currentRow.RateDate
            });

            sqlParameters.Add(new SqlParameter(baseCurrencyParameterName, SqlDbType.Char, 3)
            {
                Value = normalizedBaseCurrency
            });

            sqlParameters.Add(new SqlParameter(targetCurrencyParameterName, SqlDbType.Char, 3)
            {
                Value = normalizedTargetCurrency
            });

            sqlParameters.Add(new SqlParameter(exchangeRateParameterName, SqlDbType.Decimal)
            {
                Precision = 18,
                Scale = 8,
                Value = currentRow.Rate
            });
        }

        sqlCommandText.AppendLine(") AS sourceRows (RateDate, BaseCurrency, TargetCurrency, Rate)");

        sqlCommandText.AppendLine("ON target.RateDate = sourceRows.RateDate");
        sqlCommandText.AppendLine("   AND target.BaseCurrency = sourceRows.BaseCurrency");
        sqlCommandText.AppendLine("   AND target.TargetCurrency = sourceRows.TargetCurrency");

        sqlCommandText.AppendLine("WHEN MATCHED THEN");
        sqlCommandText.AppendLine("  UPDATE SET Rate = sourceRows.Rate, UpdatedAt = SYSUTCDATETIME()");

        sqlCommandText.AppendLine("WHEN NOT MATCHED BY TARGET THEN");
        sqlCommandText.AppendLine("  INSERT (RateDate, BaseCurrency, TargetCurrency, Rate, CreatedAt, UpdatedAt)");
        sqlCommandText.AppendLine("  VALUES (sourceRows.RateDate, sourceRows.BaseCurrency, sourceRows.TargetCurrency, sourceRows.Rate, SYSUTCDATETIME(), SYSUTCDATETIME());");

        mergeCommandParameters = sqlParameters.ToArray();
        return sqlCommandText.ToString();
    }

    private static string NormalizeCurrency(string currency, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidOperationException($"{fieldName} is null or empty.");

        var trimmedCurrency = currency.Trim().ToUpperInvariant();

        if (trimmedCurrency.Length != 3)
            throw new InvalidOperationException($"{fieldName} must be a 3-letter currency code, got: '{currency}'.");

        return trimmedCurrency;
    }
}