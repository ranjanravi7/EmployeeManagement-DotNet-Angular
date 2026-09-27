using DbFirstCore.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DbFirstCore.DataAccessLayer.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;

    public EmployeeRepository(AppDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();
        return employee;
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await _db.Employees.FindAsync(id);
        if (existing is null) return;
        _db.Employees.Remove(existing);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        try
        {
            return await _db.Employees.AsNoTracking().ToListAsync();
        }
        catch (Exception ex)
        {
            // If DB access fails, log and return empty list so API can respond gracefully.
            Console.Error.WriteLine($"EmployeeRepository.GetAllAsync error: {ex}");
            return Enumerable.Empty<Employee>();
        }
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        try
        {
            return await _db.Employees.FindAsync(id);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"EmployeeRepository.GetByIdAsync({id}) error: {ex}");
            return null;
        }
    }

    public async Task UpdateAsync(Employee employee)
    {
        // Use a tracked entity update to avoid issues with concurrency tokens
        var existing = await _db.Employees.FindAsync(employee.Id);
        if (existing is null) throw new KeyNotFoundException($"Employee {employee.Id} not found");

        // Copy scalar properties (preserves navigation and tracking metadata)
        _db.Entry(existing).CurrentValues.SetValues(employee);

        await _db.SaveChangesAsync();
    }
}
