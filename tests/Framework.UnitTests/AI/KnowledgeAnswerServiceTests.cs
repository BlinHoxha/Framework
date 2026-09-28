using BuildingBlocks.AI.Core.Abstractions;
using BuildingBlocks.AI.Core.Models;
using Framework.Application.AI.Services;
using Framework.Contracts.AI;

namespace Framework.UnitTests.AI;

public sealed class KnowledgeAnswerServiceTests
{
    [Fact]
    public async Task AnswerAsync_WhenRetrievalIsEmpty_DoesNotCallModel()
    {
        StubChatModel chatModel = new();
        KnowledgeAnswerService service = new(
            new StubProfileCatalog(),
            new EmptyRetriever(),
            chatModel);

        GroundedAnswerResponse result = await service.AnswerAsync(new AskKnowledgeRequest
        {
            Question = "What is the deadline?",
            Profile = "test",
            TenantId = Guid.NewGuid()
        });

        Assert.False(result.Grounded);
        Assert.Empty(result.Citations);
        Assert.False(chatModel.WasCalled);
    }

    [Fact]
    public async Task AnswerAsync_WithEmptyTenant_RejectsRequestBeforeRetrieval()
    {
        KnowledgeAnswerService service = new(
            new StubProfileCatalog(),
            new EmptyRetriever(),
            new StubChatModel());

        await Assert.ThrowsAsync<ArgumentException>(() => service.AnswerAsync(new AskKnowledgeRequest
        {
            Question = "What is the deadline?",
            Profile = "test",
            TenantId = Guid.Empty
        }));
    }

    private sealed class StubProfileCatalog : IDocumentProfileCatalog
    {
        private static readonly DocumentProfile Profile =
            new("test", "Test profile", ["question-answering"], ["en"], false);

        public IReadOnlyCollection<DocumentProfile> GetAll() => [Profile];

        public DocumentProfile GetRequired(string name) => Profile;
    }

    private sealed class EmptyRetriever : IKnowledgeRetriever
    {
        public Task<IReadOnlyCollection<KnowledgeChunk>> SearchAsync(
            KnowledgeSearchQuery query,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<KnowledgeChunk>>([]);
    }

    private sealed class StubChatModel : IChatModel
    {
        public bool WasCalled { get; private set; }

        public Task<GroundedGenerationResult> GenerateAsync(
            GroundedGenerationRequest request,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult(new GroundedGenerationResult("answer", "stub"));
        }
    }
}

