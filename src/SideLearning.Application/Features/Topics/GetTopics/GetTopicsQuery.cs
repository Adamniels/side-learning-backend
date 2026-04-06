namespace SideLearning.Application.Features.Topics.GetTopics;

public sealed record GetTopicsQuery(int Page, int PageSize, string? Search);
