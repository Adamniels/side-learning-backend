using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SideLearning.Application.Abstractions.Persistence;
using SideLearning.Application.Common.Models;

namespace SideLearning.Application.Features.Topics.GetTopics;

public sealed class GetTopicsQueryHandler(
    IValidator<GetTopicsQuery> validator,
    IApplicationDbContext dbContext)
{
    public async Task<PagedResult<TopicDto>> HandleAsync(GetTopicsQuery query, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        var q = dbContext.Topics.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            q = q.Where(t => t.Name.ToLower().Contains(term) || t.Slug.Contains(term));
        }

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderBy(t => t.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new TopicDto(t.Id, t.Name, t.Slug, t.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<TopicDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }
}
