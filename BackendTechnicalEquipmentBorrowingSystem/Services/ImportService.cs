using System.Data;
using BackendTechnicalEquipmentBorrowingSystem.DTOs;
using BackendTechnicalEquipmentBorrowingSystem.Entities;
using BackendTechnicalEquipmentBorrowingSystem.IRepository;
using BackendTechnicalEquipmentBorrowingSystem.IService;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;

namespace BackendTechnicalEquipmentBorrowingSystem.Services;

// Bulk item ingestion from an .xlsx sheet. Header row (first row) required, case-sensitive:
//   Name | SerialNumber | Category | Condition | Location | Description
// Name + SerialNumber are required; the rest optional. Bad rows are reported and skipped —
// a partial import still commits the good rows.
public class ImportService : IImportService
{
    private readonly IRepository<Item> _items;
    public ImportService(IRepository<Item> items) => _items = items;

    public async Task<ImportResult> ImportItemsAsync(Stream xlsx)
    {
        using var reader = ExcelReaderFactory.CreateReader(xlsx);
        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
        });

        if (dataSet.Tables.Count == 0)
            return new ImportResult(0, new() { "The workbook has no sheets." });

        var table = dataSet.Tables[0];
        foreach (var required in new[] { "Name", "SerialNumber" })
            if (!table.Columns.Contains(required))
                return new ImportResult(0, new() { $"Missing required column '{required}'." });

        // Serials already in the DB, plus serials seen earlier in this batch — one set, case-insensitive.
        var seen = (await _items.Query().Select(i => i.SerialNumber).ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var errors = new List<string>();
        var toAdd = new List<Item>();

        for (var r = 0; r < table.Rows.Count; r++)
        {
            var row = table.Rows[r];
            var line = r + 2; // +1 for the header row, +1 to 1-base for humans

            var name = Cell(row, "Name")?.Trim();
            var serial = Cell(row, "SerialNumber")?.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(serial))
            {
                errors.Add($"Row {line}: Name and SerialNumber are required.");
                continue;
            }
            if (!seen.Add(serial))
            {
                errors.Add($"Row {line}: duplicate serial number '{serial}'.");
                continue;
            }

            var condition = Enum.TryParse<ItemCondition>(Cell(row, "Condition"), ignoreCase: true, out var c)
                ? c : ItemCondition.Good;

            toAdd.Add(new Item
            {
                Name = name,
                SerialNumber = serial,
                Category = Cell(row, "Category")?.Trim() ?? string.Empty,
                Condition = condition,
                Location = Cell(row, "Location")?.Trim(),
                Description = Cell(row, "Description")?.Trim(),
                Status = ItemStatus.Available,
                CreatedAt = DateTime.UtcNow
            });
        }

        foreach (var item in toAdd) await _items.AddAsync(item);
        if (toAdd.Count > 0) await _items.SaveChangesAsync();

        return new ImportResult(toAdd.Count, errors);
    }

    private static string? Cell(DataRow row, string column)
        => row.Table.Columns.Contains(column) && row[column] != DBNull.Value
            ? row[column].ToString()
            : null;
}
