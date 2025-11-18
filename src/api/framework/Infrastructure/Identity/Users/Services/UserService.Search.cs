using System.Globalization;
using System.Linq.Expressions;
using FSH.Framework.Core.Identity.Users.Dtos;
using FSH.Framework.Core.Identity.Users.Features.SearchUsers;
using FSH.Framework.Core.Paging;
using FSH.Framework.Infrastructure.Identity.Users;
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
            var keyword = request.Keyword.ToLower(CultureInfo.InvariantCulture);
            query = query.Where(u =>
                (u.FirstName != null && u.FirstName.ToLower().Contains(keyword)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(keyword)) ||
                (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
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
            IOrderedQueryable<FshUser>? orderedQuery = null;

            foreach (var orderBy in request.OrderBy)
            {
                var isDescending = orderBy.StartsWith('-');
                var propertyName = isDescending ? orderBy[1..] : orderBy;

                Expression<Func<FshUser, object?>> keySelector = propertyName.ToLower(CultureInfo.InvariantCulture) switch
                {
                    "firstname" => u => u.FirstName,
                    "lastname" => u => u.LastName,
                    "email" => u => u.Email,
                    "phonenumber" => u => u.PhoneNumber,
                    "isactive" => u => u.IsActive,
                    _ => u => u.Email // Default to email
                };

                if (orderedQuery == null)
                {
                    orderedQuery = isDescending 
                        ? query.OrderByDescending(keySelector) 
                        : query.OrderBy(keySelector);
                }
                else
                {
                    orderedQuery = isDescending 
                        ? orderedQuery.ThenByDescending(keySelector) 
                        : orderedQuery.ThenBy(keySelector);
                }
            }

            query = orderedQuery ?? query;
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
