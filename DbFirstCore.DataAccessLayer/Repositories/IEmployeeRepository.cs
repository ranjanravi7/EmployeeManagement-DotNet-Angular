using DbFirstCore.DataAccessLayer.Models;

namespace DbFirstCore.DataAccessLayer.Repositories;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee> CreateAsync(Employee employee);
    Task<IEnumerable<Employee>> SearchByNameAsync(string name);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(int id);
}
