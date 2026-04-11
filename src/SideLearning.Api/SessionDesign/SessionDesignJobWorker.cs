using Microsoft.Extensions.Options;
using SideLearning.Application.Abstractions.SessionDesign;
using SideLearning.Application.Configuration;

namespace SideLearning.Api.SessionDesign;

public sealed class SessionDesignJobWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<SessionDesignerOptions> options,
    ILogger<SessionDesignJobWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.EnableWorker)
        {
            logger.LogInformation("Session design background worker is disabled (SessionDesigner:EnableWorker).");
            return;
        }

        logger.LogInformation("Session design background worker started.");
        // TODO: optional sweeper for jobs stuck in Running (e.g. designer crash) past a timeout.

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var repo = scope.ServiceProvider.GetRequiredService<ISessionDesignJobRepository>();
                var processor = scope.ServiceProvider.GetRequiredService<ISessionDesignJobDispatchProcessor>();

                var jobId = await repo.TryClaimNextQueuedAsync(stoppingToken);
                if (jobId is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                await processor.TryPostToDesignerAsync(jobId.Value, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Session design worker iteration failed");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
