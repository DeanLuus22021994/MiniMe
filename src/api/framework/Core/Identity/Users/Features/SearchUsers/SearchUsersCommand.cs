using FSH.Framework.Core.Identity.Users.Dtos;
using FSH.Framework.Core.Paging;
using MediatR;

namespace FSH.Framework.Core.Identity.Users.Features.SearchUsers;

public class SearchUsersCommand : PaginationFilter, IRequest<PagedList<UserDetail>>
{
    public bool? IsActive { get; set; }
}
