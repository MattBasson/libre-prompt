using NSubstitute;
using PromptPortal.Application.Interfaces;
using PromptPortal.Application.Prompts.CreatePrompt;
using PromptPortal.Domain.Entities;

namespace PromptPortal.Application.Tests;

public class CreatePromptCommandHandlerTests
{
    private readonly IPromptRepository _repository = Substitute.For<IPromptRepository>();
    private readonly IPromptSearchIndexer _indexer = Substitute.For<IPromptSearchIndexer>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IIdGenerator _idGenerator = Substitute.For<IIdGenerator>();

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndIndexesPrompt()
    {
        // Arrange
        _currentUser.TenantId.Returns("local-dev");
        _currentUser.UserId.Returns("dev-user@local");
        _clock.UtcNow.Returns(new DateTimeOffset(2026, 4, 10, 12, 0, 0, TimeSpan.Zero));
        _idGenerator.NewPromptId().Returns("prompt_test123");

        var handler = new CreatePromptCommandHandler(_repository, _indexer, _currentUser, _clock, _idGenerator);

        var command = new CreatePromptCommand(
            Title: "Test Prompt",
            Content: "You are a helpful assistant...",
            Summary: "A test prompt",
            Tags: ["test", "demo"],
            Categories: ["testing"],
            Models: ["claude"],
            PromptStyle: "instructional",
            Language: "en-GB",
            Visibility: "private",
            Variables: null,
            Examples: null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("prompt_test123", result.PromptId);
        Assert.Equal("local-dev", result.TenantId);
        Assert.Equal("Test Prompt", result.Title);
        Assert.Equal(1, result.Version);

        await _repository.Received(1).SaveAsync(Arg.Any<PromptDefinition>(), Arg.Any<CancellationToken>());
        await _indexer.Received(1).IndexAsync(Arg.Any<PromptDefinition>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GeneratesSlugFromTitle()
    {
        // Arrange
        _currentUser.TenantId.Returns("local-dev");
        _currentUser.UserId.Returns("dev-user@local");
        _clock.UtcNow.Returns(DateTimeOffset.UtcNow);
        _idGenerator.NewPromptId().Returns("prompt_abc");

        var handler = new CreatePromptCommandHandler(_repository, _indexer, _currentUser, _clock, _idGenerator);

        var command = new CreatePromptCommand(
            Title: "Summarise Architecture Decision Record",
            Content: "Content here",
            Summary: null,
            Tags: [],
            Categories: [],
            Models: [],
            PromptStyle: null,
            Language: null,
            Visibility: null,
            Variables: null,
            Examples: null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("summarise-architecture-decision-record", result.Slug);
    }
}
