using CourierParcelTrackingSystem.API.DTOs.AuthenticationDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using CourierParcelTrackingSystem.API.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace CourierParcelTrackingSystem.API.Services;
public class AuthService : IAuthService
{
    private const string AdminRoleName = "Admin";
    private const string CustomerViewerRoleName = "Customer Viewer";
    private const string CourierRoleName = "Courier";
    private static readonly HashSet<string> StaffRoleNames = new(
        new[] { "Booking Agent", "Dispatcher", CourierRoleName },
        StringComparer.OrdinalIgnoreCase);
    private readonly IAuthRepository _repository;
    private readonly ICourierRepository _courierRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtSettings _jwt;
    private readonly IConfiguration _configuration;

    public AuthService(
        IAuthRepository repository,
        ICourierRepository courierRepository,
        IPasswordHasher<User> passwordHasher,
        IOptions<JwtSettings> jwtOptions, IConfiguration configuration)
    {
        _repository = repository;
        _courierRepository = courierRepository;
        _passwordHasher = passwordHasher;
        _jwt = jwtOptions.Value;
        _configuration = configuration;
    }

    public async Task<string> BootstrapAdminAsync(BootstrapAdminRequestDto request)
    {
        var configuredKey = _configuration["BootstrapAdmin:Key"];
        if (string.IsNullOrWhiteSpace(configuredKey) ||
            !string.Equals(request.BootstrapKey, configuredKey, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Invalid bootstrap key.");

        if (await _repository.AdminExistsAsync())
            throw new InvalidOperationException("An active Admin already exists. Bootstrap is disabled.");

        await EnsureUserIsUniqueAsync(request.UserName, request.Email);
        var role = await GetRequiredRoleAsync(AdminRoleName);
        var user = NewUser(request.UserName, request.Email, request.FullName, request.Phone);
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        await _repository.CreateAccountAsync(user, role, null);
        return "Initial Admin account created successfully.";
    }

    public async Task<string> RegisterStaffAsync(RegisterStaffRequestDto request, int createdBy)
    {
        var role = await _repository.GetActiveRoleByIdAsync(request.RoleId);
        if (role == null || !StaffRoleNames.Contains(role.RoleName))
            throw new InvalidOperationException(
                "Admin can create only Booking Agent, Dispatcher, or Courier accounts.");

        var isCourier = string.Equals(
            role.RoleName,
            CourierRoleName,
            StringComparison.OrdinalIgnoreCase);
        if (isCourier &&
            (string.IsNullOrWhiteSpace(request.Phone) || request.Phone.Trim().Length < 7))
            throw new ArgumentException(
                "Courier phone numbers must contain at least 7 characters.",
                nameof(request.Phone));

        await EnsureUserIsUniqueAsync(request.UserName, request.Email);
        var user = NewUser(request.UserName, request.Email, request.FullName, request.Phone);
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        Courier? courier = null;
        if (isCourier)
        {
            courier = new Courier
            {
                EmployeeCode = await _courierRepository.GenerateUniqueEmployeeCodeAsync(),
                FullName = request.FullName.Trim(),
                Phone = request.Phone!.Trim(),
                Email = Normalize(request.Email),
                VehicleType = Normalize(request.VehicleType),
                VehicleNumber = Normalize(request.VehicleNumber)
            };
        }

        await _repository.CreateAccountAsync(user, role, createdBy, courier: courier);
        return $"{role.RoleName} account created successfully.";
    }

    public async Task<IReadOnlyList<StaffRoleOptionDto>> GetStaffRolesAsync()
    {
        var roles = await _repository.GetActiveRolesAsync();
        return roles
            .Where(role => StaffRoleNames.Contains(role.RoleName))
            .Select(role => new StaffRoleOptionDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName
            })
            .ToList();
    }

    public async Task<string> RegisterCustomerAsync(RegisterCustomerRequestDto request, int createdBy)
    {
        await EnsureUserIsUniqueAsync(request.UserName, request.Email);
        var role = await GetRequiredRoleAsync(CustomerViewerRoleName);
        var user = NewUser(request.UserName, request.Email, request.FullName, request.Phone);
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        var customer = new Customer
        {
            CustomerCode = "CUS-" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone.Trim(),
            AddressLine = request.AddressLine?.Trim(),
            City = request.City?.Trim(),
            Province = request.Province?.Trim(),
            PostalCode = request.PostalCode?.Trim()
        };
        await _repository.CreateAccountAsync(user, role, createdBy, customer);
        return "Customer account created successfully.";
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var identifier = request.UserNameOrEmail.Trim();
        var user = await _repository.GetUserForLoginAsync(identifier);
        if (user == null) return null;

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed) return null;

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _repository.UpdatePasswordHashAsync(user);
        }

        user = await _repository.GetUserWithRolesAsync(user.UserId);
        if (user == null) return null;
        var roles = user.UserRoleUsers
            .Where(x => x.Role.IsActive && !x.Role.IsDeleted)
            .Select(x => x.Role.RoleName).Distinct().ToList();
        if (roles.Count == 0) return null;
        return CreateJwtResponse(user, roles);
    }

    public async Task<LoginResponseDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _repository.GetUserWithRolesAsync(userId);
        if (user == null) return null;
        var roles = user.UserRoleUsers
            .Where(x => x.Role.IsActive && !x.Role.IsDeleted)
            .Select(x => x.Role.RoleName).Distinct().ToList();
        return new LoginResponseDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roles,
            RoleIds = user.UserRoleUsers
                .Where(x => x.Role.IsActive && !x.Role.IsDeleted)
                .Select(x => x.RoleId)
                .Distinct()
                .ToList()
        };
    }

    private async Task EnsureUserIsUniqueAsync(string username, string email)
    {
        if (await _repository.UserExistsAsync(username.Trim(), email.Trim().ToLowerInvariant()))
            throw new InvalidOperationException("Username or email already exists.");
    }

    private async Task<Role> GetRequiredRoleAsync(string roleName)
    {
        var role = await _repository.GetActiveRoleByNameAsync(roleName);
        if (role == null || !string.Equals(role.RoleName, roleName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"The {roleName} role is unavailable or inactive.");
        return role;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static User NewUser(string username, string email, string fullName, string? phone) => new()
    {
        UserName = username.Trim(),
        Email = email.Trim().ToLowerInvariant(),
        FullName = fullName.Trim(),
        Phone = phone?.Trim(),
        IsActive = true,
        IsDeleted = false
    };

    private LoginResponseDto CreateJwtResponse(User user, List<string> roles)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email)
        };
        var activeUserRoles = user.UserRoleUsers
            .Where(x => x.Role.IsActive && !x.Role.IsDeleted)
            .ToList();

        claims.AddRange(activeUserRoles.Select(x =>
            new Claim(ClaimTypes.Role, x.Role.RoleName)));

        claims.AddRange(activeUserRoles.Select(x =>
            new Claim("role_id", x.RoleId.ToString())));
        // This secret signs JWTs only. It is never used to hash or verify passwords.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims,
            expires: expires, signingCredentials: credentials);
        return new LoginResponseDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roles,
            RoleIds = activeUserRoles.Select(x => x.RoleId).Distinct().ToList(),
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires
        };
    }
}
