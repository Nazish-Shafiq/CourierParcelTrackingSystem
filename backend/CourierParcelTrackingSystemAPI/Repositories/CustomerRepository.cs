
using Microsoft.EntityFrameworkCore;
using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Extensions;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;

namespace CourierParcelTrackingSystem.API.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get customers with pagination and optional search.
    public async Task<PagedResponseDto<Customer>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search)
    {
        var query = _context.Customers
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        // Apply search before pagination.
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(c =>
                c.CustomerCode.Contains(search) ||
                c.FullName.Contains(search) ||
                (c.Email != null && c.Email.Contains(search)) ||
                c.Phone.Contains(search) ||
                (c.City != null && c.City.Contains(search))
            );
        }

        // Apply consistent ordering.
        query = query.OrderBy(c => c.CustomerId);

        // Use the common pagination function.
        return await query.ToPagedResponseAsync(pagination);
    }

    // Get customer by ID.
    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c =>
                c.CustomerId == id && !c.IsDeleted);
    }

    // Check whether email already exists.
    public async Task<bool> EmailExistsAsync(
        string email,
        int? excludeCustomerId = null)
    {
        var query = _context.Customers
            .Where(c => c.Email == email);

        if (excludeCustomerId.HasValue)
        {
            query = query.Where(c =>
                c.CustomerId != excludeCustomerId.Value);
        }

        return await query.AnyAsync();
    }

    // Add new customer.
    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
    }

    // Update existing customer.
    public async Task UpdateAsync(Customer customer)
    {
        await _context.SaveChangesAsync();
    }

    // Soft delete customer instead of permanently deleting it.
    public async Task SoftDeleteAsync(Customer customer)
    {
        customer.IsActive = false;
        customer.IsDeleted = true;

        await _context.SaveChangesAsync();
    }
}
