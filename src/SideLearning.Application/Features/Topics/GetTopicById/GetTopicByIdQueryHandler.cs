using Microsoft.EntityFrameworkCore;
using SideLearning.Application.Abstractions.Persistence;
using SideLearning.Application.Common.Exceptions;
using SideLearning.Application.Features.Topics;

namespace SideLearning.Application.Features.Topics.GetTopicById;

public sealed class GetTopicByIdQueryHandler(IApplicationDbContext dbContext)
{
    public async Task<TopicDto> HandleAsync(GetTopicByIdQuery query, CancellationToken cancellationToken)
    {
        var topic = await dbContext.Topics.AsNoTracking()
            .Where(t => t.Id == query.Id)
            .Select(t => new TopicDto(t.Id, t.Name, t.Slug, t.CreatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        if (topic is null)
        {
            throw new NotFoundException("topic_not_found", "The topic was not found.");
        }

        return topic;
    }
}
