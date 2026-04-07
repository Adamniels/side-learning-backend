using SideLearning.Domain.Sessions;

namespace SideLearning.Application.Tests;

public sealed class SessionAggregateTests
{
    [Fact]
    public void Create_sets_extension_and_subject_areas()
    {
        var session = Session.Create(
            id: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            title: "JWT auth middleware",
            summary: "Build token auth for APIs.",
            goal: "Implement middleware that validates bearer tokens.",
            extension: "Next add refresh tokens and revoke flow.",
            context: new SessionContext(
                "JWT is a compact token format for claims.",
                "Helps protect API routes and identify users.",
                null,
                null),
            handsOn: new SessionHandsOn(
                "Create middleware and test valid/invalid token behavior.",
                "401/200 responses based on token validity."),
            reflection: new SessionReflection(null, null, null, null),
            subjectAreas:
            [
                SessionSubjectArea.Create("auth"),
                SessionSubjectArea.Create("api")
            ]);

        Assert.Equal("Next add refresh tokens and revoke flow.", session.Extension);
        Assert.Collection(
            session.SubjectAreas,
            item => Assert.Equal("auth", item.Value),
            item => Assert.Equal("api", item.Value));
    }

    [Fact]
    public void Create_throws_when_extension_is_blank()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Session.Create(
                id: Guid.NewGuid(),
                userId: Guid.NewGuid(),
                title: "JWT auth middleware",
                summary: "Build token auth for APIs.",
                goal: "Implement middleware that validates bearer tokens.",
                extension: " ",
                context: new SessionContext(
                    "JWT is a compact token format for claims.",
                    "Helps protect API routes and identify users.",
                    null,
                    null),
                handsOn: new SessionHandsOn(
                    "Create middleware and test valid/invalid token behavior.",
                    "401/200 responses based on token validity."),
                reflection: new SessionReflection(null, null, null, null),
                subjectAreas: [SessionSubjectArea.Create("auth")]));

        Assert.Equal("extension", ex.ParamName);
    }
}
