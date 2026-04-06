using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SideLearning.Application.Abstractions.Persistence;
using SideLearning.Application.Common;
using SideLearning.Application.Common.Exceptions;
using SideLearning.Domain.Topics;

namespace SideLearning.Application.Features.Topics.CreateTopic;

public sealed class CreateTopicCommandHandler(
    IValidator<CreateTopicCommand> validator,
    IApplicationDbContext dbContext)
{
    public async Task<TopicDto> HandleAsync(CreateTopicCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var slug = SlugHelper.Slugify(command.Name);
        var exists = await dbContext.Topics.AnyAsync(t => t.Slug == slug, cancellationToken);
        if (exists)
        {
            throw new ConflictException("topic_slug_conflict", "A topic with this name already exists.");
        }

        var topic = Topic.Create(command.Name, slug);
        dbContext.Topics.Add(topic);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TopicDto(topic.Id, topic.Name, topic.Slug, topic.CreatedAtUtc);
    }
}
