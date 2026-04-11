using SideLearning.Application.Abstractions.SessionDesign;
using SideLearning.Domain.SessionDesign;

namespace SideLearning.Application.Features.SessionDesign.Enqueue;

public sealed class EnqueueSessionDesignJobCommandHandler(ISessionDesignJobRepository sessionDesignJobRepository)
{
    public async Task<Guid> HandleAsync(EnqueueSessionDesignJobCommand command, CancellationToken cancellationToken)
    {
        var jobId = Guid.NewGuid();
        var job = SessionDesignJob.CreateQueued(jobId, command.UserId);
        await sessionDesignJobRepository.AddAsync(job, cancellationToken);
        await sessionDesignJobRepository.SaveChangesAsync(cancellationToken);
        return jobId;
    }
}
