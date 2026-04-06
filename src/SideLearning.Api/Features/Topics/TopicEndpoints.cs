using Microsoft.AspNetCore.Mvc;
using SideLearning.Application.Features.Topics;
using SideLearning.Application.Features.Topics.CreateTopic;
using SideLearning.Application.Features.Topics.GetTopicById;
using SideLearning.Application.Features.Topics.GetTopics;

namespace SideLearning.Api.Features.Topics;

public static class TopicEndpoints
{
    public static void MapTopicEndpoints(this RouteGroupBuilder group)
    {
        var topics = group.MapGroup("/topics").WithTags("Topics");

        topics.MapGet("/", async Task<IResult> (
                [FromQuery] int page,
                [FromQuery] int pageSize,
                [FromQuery] string? search,
                GetTopicsQueryHandler handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetTopicsQuery(
                    page == 0 ? 1 : page,
                    pageSize == 0 ? 20 : pageSize,
                    search);
                var result = await handler.HandleAsync(query, cancellationToken);
                return Results.Ok(new PagedTopicsResponse(
                    result.Items.Select(t => TopicResponse.FromDto(t)).ToList(),
                    result.Page,
                    result.PageSize,
                    result.TotalCount));
            })
            .AllowAnonymous()
            .Produces<PagedTopicsResponse>();

        topics.MapGet("/{id:guid}", async Task<IResult> (
                Guid id,
                GetTopicByIdQueryHandler handler,
                CancellationToken cancellationToken) =>
            {
                var topic = await handler.HandleAsync(new GetTopicByIdQuery(id), cancellationToken);
                return Results.Ok(TopicResponse.FromDto(topic));
            })
            .AllowAnonymous()
            .Produces<TopicResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        topics.MapPost("/", async Task<IResult> (
                [FromBody] CreateTopicRequest request,
                CreateTopicCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                var created = await handler.HandleAsync(new CreateTopicCommand(request.Name), cancellationToken);
                return Results.Created($"/api/v1/topics/{created.Id}", TopicResponse.FromDto(created));
            })
            .RequireAuthorization()
            .Produces<TopicResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    public sealed record CreateTopicRequest(string Name);

    public sealed record TopicResponse(Guid Id, string Name, string Slug, DateTimeOffset CreatedAtUtc)
    {
        public static TopicResponse FromDto(TopicDto dto) =>
            new(dto.Id, dto.Name, dto.Slug, dto.CreatedAtUtc);
    }

    public sealed record PagedTopicsResponse(
        IReadOnlyList<TopicResponse> Items,
        int Page,
        int PageSize,
        int TotalCount);
}
