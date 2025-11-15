using FSH.Framework.Core.Identity.Users.Dtos;
using FSH.Framework.Core.Identity.Users.Features.SearchUsers;
using FSH.Framework.Core.Paging;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace FSH.Framework.Infrastructure.Identity.Users.Services;

internal sealed partial class UserService
{
    public async Task<PagedList<UserDetail>> SearchAsync(SearchUsersCommand request, CancellationToken cancellationToken)
    {
        EnsureValidTenant();

        var query = userManager.Users.AsQueryable();

        // Apply keyword search
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.ToLower();
            query = query.Where(u =>
                u.FirstName!.ToLower().Contains(keyword) ||
                u.LastName!.ToLower().Contains(keyword) ||
                u.Email!.ToLower().Contains(keyword) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(keyword)));
        }

        // Apply IsActive filter
        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply ordering
        if (request.OrderBy?.Any() == true)
        {
            foreach (var orderBy in request.OrderBy)
            {
                var isDescending = orderBy.StartsWith('-');
                var propertyName = isDescending ? orderBy[1..] : orderBy;

                query = propertyName.ToLower() switch
                {
                    "firstname" => isDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                    "lastname" => isDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                    "email" => isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                    "phonenumber" => isDescending ? query.OrderByDescending(u => u.PhoneNumber) : query.OrderBy(u => u.PhoneNumber),
                    "isactive" => isDescending ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
                    _ => query
                };
            }
        }
        else
        {
            // Default ordering by email
            query = query.OrderBy(u => u.Email);
        }

        // Apply pagination
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize == int.MaxValue ? 10 : request.PageSize;

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var userDetails = users.Adapt<List<UserDetail>>();

        return new PagedList<UserDetail>(userDetails, pageNumber, pageSize, totalCount);
    }
}
